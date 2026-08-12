# Fundo — Loan Application

**Video walkthrough:** _(link to be added)_

A small loan application flow: a Next.js form collects an application, a rule engine on
the .NET backend approves or denies it, approved applications are written to the database
in a single transaction, and a background worker pushes them to an external service over
HTTP.

Architecture and the reasoning behind each decision: [ARCHITECTURE.md](ARCHITECTURE.md).

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)

No database to install and no containers to start: the API uses SQLite and creates the
file itself on first run.

## Run it

Three processes, one per terminal, from the repository root.

```bash
# 1. Mock external service — http://localhost:5081
dotnet run --project src/Fundo.Loans.ExternalService.Mock

# 2. API — http://localhost:5080
dotnet run --project src/Fundo.Loans.Api

# 3. Web — http://localhost:3000
cd web
npm install
npm run dev
```

Then open <http://localhost:3000>.

The API applies its migrations on start-up, so there is no setup step. To start from an
empty database, delete `src/Fundo.Loans.Api/fundo-loans.db` and restart it.

## Run the tests

```bash
dotnet test
```

33 tests. They cover the rule engine, the submit endpoint, the returning-customer path
and the transaction, and need nothing running: the integration tests host the API over an
in-memory SQLite database.

## Test data

The form accepts any name, company, street and city. What changes the outcome:

| To get | Enter |
| --- | --- |
| **Approved** | Any state except `NY`, and an SSN that is not on the list below. For example state `CA`, SSN `444-55-6666`. |
| **Denied — restricted state** | State `NY`, any SSN. |
| **Denied — blacklisted SSN** | `111-11-1111`, `222-22-2222` or `333-33-3333`, any state. |
| **Returning customer** | Submit an approved application, then submit again with **the same SSN** and a different amount, company or address. |

SSNs may be typed with or without dashes. Both lists live in
[`src/Fundo.Loans.Api/appsettings.json`](src/Fundo.Loans.Api/appsettings.json) under
`DecisionRules`.

### Watching it work

- **Approved** goes to a confirmation page showing the application reference.
- **Denied** goes to a denial page. Nothing is written.
- **Returning customer** returns *the same* application reference as the first submission,
  and the customer and application rows are updated rather than duplicated.
- **The external service** receives the data a second or two later, from the background
  worker rather than from the request. Check it at <http://localhost:5081/customers>: a
  new customer appears once, and a returning customer updates that same record.
- **The external service being down** does not fail an application. Stop it, submit an
  approved application, and the API logs a retry warning; start it again and the record
  arrives on the next attempt.

## Layout

```
src/
  Fundo.Loans.Domain                 entities, value objects, the rule engine
  Fundo.Loans.Application            the use case and the ports it needs
  Fundo.Loans.Infrastructure         EF Core, the outbox, the HTTP client
  Fundo.Loans.Api                    minimal API, request validation
  Fundo.Loans.ExternalService.Mock   the partner system, stubbed
tests/
  Fundo.Loans.Tests                  unit and integration tests
web/                                 Next.js app
```

## Configuration

Everything has a working default in
[`appsettings.json`](src/Fundo.Loans.Api/appsettings.json); nothing needs to be set to run
locally.

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings:Loans` | SQLite database file. |
| `SsnHashing:Key` | HMAC key the SSN is hashed with. A committed development value; in production it belongs in a secret store. |
| `DecisionRules:RestrictedStates` | States that are denied. |
| `DecisionRules:BlacklistedSsns` | SSNs that are denied. |
| `ExternalService:BaseUrl` | Where the background worker delivers to. |
| `Outbox:*` | Polling interval, batch size, retry attempts and backoff. |

The web app reads `LOANS_API_URL` (see [`web/.env.example`](web/.env.example)) and
defaults to `http://localhost:5080`.
