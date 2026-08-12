# Architecture

## The shape

Four backend projects, dependencies pointing inward:

```
Api ──────► Infrastructure ──────► Application ──────► Domain
```

| Project | Holds | Knows about |
| --- | --- | --- |
| **Domain** | `Customer`, `LoanApplication`, `Ssn`, `Address`, the rule engine and its rules | Nothing. No EF Core, no ASP.NET, no configuration. |
| **Application** | The `SubmitLoanApplicationHandler` use case and the ports it needs (`ICustomerRepository`, `ILoanApplicationRepository`, `IUnitOfWork`, `IIntegrationEventPublisher`, `ISsnHasher`) | Domain only. |
| **Infrastructure** | EF Core, the repositories, the outbox, the HTTP client, the hashing, the config-backed blacklist | Application and Domain. |
| **Api** | Minimal API endpoint, request contract, validation | Infrastructure, for composition. |

The ports are declared by the layer that *uses* them and implemented by the layer
outside it, so the use case has no idea there is a database, an outbox or an HTTP client
behind them. That is what the transaction test exploits: it swaps the real publisher for
one that throws, without touching the code under test.

`web/` is a separate Next.js app. It talks to a route handler on its own server, which
forwards to the API — see [The frontend](#the-frontend).

## The rule engine

A rule answers about itself and nothing else:

```csharp
public interface IDenialRule
{
    Denial? Evaluate(Applicant applicant);   // null = no objection
}
```

`DecisionEngine` takes every registered rule and returns the first denial it finds, or an
approval if there is none. Two rules exist: `RestrictedStateRule` and `BlacklistedSsnRule`.

**To add a rule**, write the class and register it. That is the whole change:

```csharp
// src/Fundo.Loans.Infrastructure/DependencyInjection.cs, AddDecisionRules
services.AddSingleton<IDenialRule, MinimumAmountRule>();
```

No existing rule changes, and neither does the engine. `DecisionEngineTests` proves this
by running the engine with a rule it has never seen.

Neither rule hardcodes its data: restricted states and blacklisted SSNs come from
configuration, so changing *who* is denied is not a code change. The blacklist sits behind
an `ISsnBlacklist` port, so moving it to a table later replaces one adapter and leaves the
rule alone.

## The transaction, and the background event

The requirement is that saving the customer, saving the application and publishing the
event are one unit of work — if any fails, none of them happened — while the event is
*processed* outside the request. Those two pull in opposite directions: an HTTP call
cannot be inside a database transaction and also be undone by it.

The resolution is a **transactional outbox**. Publishing does not call anyone; it inserts
a row:

```
BEGIN
  insert/update customer
  insert/update loan_application
  insert outbox_messages          ← "publish"
COMMIT
```

All three go through the same `DbContext`, so they commit or roll back together, with no
distributed transaction. `UnitOfWork.ExecuteInTransactionAsync` owns that boundary: it
runs the handler's whole block, saves once at the end and commits. Anything that throws
inside — including the publisher — rolls the lot back.

`OutboxProcessor`, a `BackgroundService`, then polls the table and delivers each message
over HTTP, well away from the request that answered the form.

**What happens when things fail**

| Failure | Result |
| --- | --- |
| Database write fails | Nothing is written and no event is queued. The endpoint returns 500. |
| Publishing fails | Same. The customer and the application are rolled back with it. Covered by `TransactionalWriteTests`. |
| External service is down or slow | The application is already approved and saved. The message stays in the outbox and is retried: five attempts, backing off 5s, 10s, 20s, 40s. |
| Still failing after five attempts | The row stays unprocessed with its last error. It is the record of an approved customer the partner never received, so it is kept for someone to look at rather than deleted. |
| The API dies mid-delivery | The message was never marked processed, so it is picked up again on restart. |

Delivery is **at-least-once**. Marking a message processed before sending it would be
at-most-once, which can silently drop an approved customer — the worse failure. The
duplicate that at-least-once implies is absorbed by the receiving end being idempotent on
the customer id.

## The returning customer

The SSN identifies a person across submissions. On an approved application the handler
looks the customer up by it and either inserts or updates; then either opens their
application or updates the existing one; then publishes `Create` or `Update` accordingly.
Both uniqueness rules are enforced by the database as well — a unique index on the
customer's SSN hash and another on the application's customer id — so a race cannot
produce the duplicate the code is trying to avoid.

## The SSN

The raw SSN never reaches the database. A customer stores:

- `SsnHash` — HMAC-SHA256 under a configured key, and the unique index used for lookups.
- `SsnLast4` — for display.

The hash has to be deterministic, because it *is* the lookup key; a per-row salt would
make the returning-customer query impossible. Keyed rather than a bare SHA-256 because
nine digits is a small enough space to precompute a rainbow table for in minutes, so the
key is what makes the stored column useless on its own. The external service is sent only
the last four digits — it has no need for the rest.

Rotating the key would require rehashing the column, which is the cost of a searchable
hash and is the reason it lives in configuration rather than in the code.

## The frontend

Next.js App Router. The form is a client component; the two outcome pages are server
components that read the outcome from the query string.

The browser **never calls the API directly**. It posts to `app/api/loan-applications/route.ts`,
which forwards to the .NET API from the server. That keeps the SSN off any cross-origin
request, leaves the API's address a server-side detail, and means there is no CORS
configuration to get wrong. The route handler is a proxy and nothing else — it makes no
decisions and interprets nothing.

Validation lives on the server too. The API returns problem details keyed by field name
(`FirstName`, `Address.State`), and the form maps those names back onto its inputs, so the
rules are not duplicated in the browser. The denial wording comes from the decision for
the same reason: the rules live on the server, so the words describing them should too.

## Choices, and what was left out

**SQLite.** The challenge needs real transactions; SQLite has them, and it keeps the whole
thing to two commands with nothing to install. Nothing above Infrastructure knows the
provider, so PostgreSQL is a package and a connection string away. The visible cost is
that timestamps are stored as UTC `DateTime` rather than `DateTimeOffset`: SQLite has no
native date type and EF Core cannot translate a `DateTimeOffset` comparison against it,
which the outbox's retry query needs.

**No message broker.** The outbox is one table and one hosted service. A broker would add
an operational dependency to a problem that does not have one yet; the publisher sits
behind `IIntegrationEventPublisher`, so introducing one later does not reach the use case.

**No Docker, no CI.** Neither earns its place at this size. `dotnet run` and `npm run dev`
are the whole story, and adding a compose file would make the setup longer, not shorter.

**Denied applications are not stored.** The brief says to create the records on approval,
so a denial writes nothing at all. A real lender would want the audit trail — adverse
action rules effectively require it — and that would be a `DecisionRecord` written on both
paths. It is out of scope here, and out of scope loudly rather than silently.

**No authentication**, as the brief states. Note that the endpoint takes an SSN and is
open, so in anything real it would sit behind auth and rate limiting.

**Validation by data annotations**, with a small helper that walks nested objects, since
minimal APIs do not check model state on their own. It is about thirty lines and covers
everything this form needs, which is why there is no validation library in the project.

**One application per customer**, because the brief defines the returning-customer flow
that way ("same SSN means one customer and one application"). A real product would keep a
history and add a new application per submission; that is a change to the repository and
the handler, not to the shape of the system.
