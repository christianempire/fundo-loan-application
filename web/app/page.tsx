import { LoanApplicationForm } from "@/components/LoanApplicationForm";

export default function ApplyPage() {
  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Apply for working capital</h1>
        <p className="mt-2 text-sm text-muted">
          One page, no documents to upload. You will get a decision as soon as you submit.
        </p>
      </div>

      <LoanApplicationForm />
    </div>
  );
}
