import Link from "next/link";

/**
 * The reason comes from the backend rather than being derived here: the rules live on
 * the server, and the wording of a denial is part of the decision, not of the page.
 */
export default async function DeniedPage({ searchParams }: PageProps<"/denied">) {
  const { reason } = await searchParams;
  const denialReason =
    typeof reason === "string" && reason.length > 0
      ? reason
      : "We are unable to approve this application.";

  return (
    <div className="rounded-lg border border-border bg-surface p-8">
      <p className="text-sm font-medium text-danger">Not approved</p>

      <h1 className="mt-2 text-2xl font-semibold tracking-tight">
        We could not approve your application
      </h1>

      <p className="mt-3 text-sm">{denialReason}</p>

      <p className="mt-3 text-sm text-muted">
        Nothing was saved. You can ask us to explain this decision, and a person will review it.
      </p>

      <Link
        href="/"
        className="mt-8 inline-block rounded-md border border-border px-4 py-2 text-sm font-medium transition hover:border-accent hover:text-accent"
      >
        Start a new application
      </Link>
    </div>
  );
}
