import Link from "next/link";

export default async function ApprovedPage({ searchParams }: PageProps<"/approved">) {
  const { application } = await searchParams;
  const applicationId = typeof application === "string" ? application : null;

  return (
    <div className="rounded-lg border border-border bg-surface p-8">
      <p className="text-sm font-medium text-accent">Approved</p>

      <h1 className="mt-2 text-2xl font-semibold tracking-tight">
        Your application has been approved
      </h1>

      <p className="mt-3 text-sm text-muted">
        We have your details on file and a specialist will be in touch about next steps. If you
        apply again with the same SSN, this application is updated rather than duplicated.
      </p>

      {applicationId ? (
        <dl className="mt-6 border-t border-border pt-6">
          <dt className="text-xs uppercase tracking-wide text-muted">Application reference</dt>
          <dd className="mt-1 font-mono text-sm break-all">{applicationId}</dd>
        </dl>
      ) : null}

      <Link
        href="/"
        className="mt-8 inline-block rounded-md border border-border px-4 py-2 text-sm font-medium transition hover:border-accent hover:text-accent"
      >
        Submit another application
      </Link>
    </div>
  );
}
