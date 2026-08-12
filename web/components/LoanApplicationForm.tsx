"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { Field } from "@/components/Field";
import type { LoanDecision, ValidationProblem } from "@/lib/loan-application";

/** Mirrors the backend's field names so its validation errors map straight onto inputs. */
type FieldErrors = Record<string, string>;

const initialValues = {
  firstName: "",
  lastName: "",
  companyName: "",
  street: "",
  city: "",
  state: "",
  postalCode: "",
  requestedAmount: "",
  ssn: "",
};

type Values = typeof initialValues;

export function LoanApplicationForm() {
  const router = useRouter();
  const [values, setValues] = useState<Values>(initialValues);
  const [errors, setErrors] = useState<FieldErrors>({});
  const [failure, setFailure] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const update = (field: keyof Values) => (event: React.ChangeEvent<HTMLInputElement>) => {
    setValues((current) => ({ ...current, [field]: event.target.value }));

    // Clear the server-side error for this field as soon as it is edited.
    setErrors((current) => {
      const remaining = { ...current };
      delete remaining[fieldToApiName[field]];
      return remaining;
    });
  };

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmitting(true);
    setFailure(null);

    try {
      const response = await fetch("/api/loan-applications", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          firstName: values.firstName.trim(),
          lastName: values.lastName.trim(),
          companyName: values.companyName.trim(),
          address: {
            street: values.street.trim(),
            city: values.city.trim(),
            state: values.state.trim().toUpperCase(),
            postalCode: values.postalCode.trim(),
          },
          requestedAmount: Number(values.requestedAmount || 0),
          ssn: values.ssn.trim(),
        }),
      });

      if (response.status === 400) {
        const problem = (await response.json()) as ValidationProblem;
        setErrors(flatten(problem.errors));
        return;
      }

      if (!response.ok) {
        setFailure("Something went wrong on our side. Your application was not submitted.");
        return;
      }

      const decision = (await response.json()) as LoanDecision;

      if (decision.decision === "Approved") {
        router.push(`/approved?application=${decision.applicationId}`);
        return;
      }

      const reason = encodeURIComponent(decision.denialReason ?? "");
      router.push(`/denied?reason=${reason}`);
    } catch {
      setFailure("We could not reach the application service. Please try again.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} noValidate className="space-y-8">
      <section className="rounded-lg border border-border bg-surface p-6">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted">Applicant</h2>

        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <Field
            label="First name"
            name="firstName"
            autoComplete="given-name"
            value={values.firstName}
            onChange={update("firstName")}
            error={errors.FirstName}
          />
          <Field
            label="Last name"
            name="lastName"
            autoComplete="family-name"
            value={values.lastName}
            onChange={update("lastName")}
            error={errors.LastName}
          />
          <Field
            label="Company name"
            name="companyName"
            autoComplete="organization"
            className="sm:col-span-2"
            value={values.companyName}
            onChange={update("companyName")}
            error={errors.CompanyName}
          />
          <Field
            label="Social Security Number"
            name="ssn"
            inputMode="numeric"
            placeholder="123-45-6789"
            hint="Used to look up your file. We store only the last four digits."
            className="sm:col-span-2"
            value={values.ssn}
            onChange={update("ssn")}
            error={errors.Ssn}
          />
        </div>
      </section>

      <section className="rounded-lg border border-border bg-surface p-6">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted">Address</h2>

        <div className="mt-4 grid gap-4 sm:grid-cols-6">
          <Field
            label="Street"
            name="street"
            autoComplete="address-line1"
            className="sm:col-span-6"
            value={values.street}
            onChange={update("street")}
            error={errors["Address.Street"]}
          />
          <Field
            label="City"
            name="city"
            autoComplete="address-level2"
            className="sm:col-span-3"
            value={values.city}
            onChange={update("city")}
            error={errors["Address.City"]}
          />
          <Field
            label="State"
            name="state"
            autoComplete="address-level1"
            placeholder="CA"
            maxLength={2}
            className="sm:col-span-1"
            value={values.state}
            onChange={update("state")}
            error={errors["Address.State"]}
          />
          <Field
            label="ZIP code"
            name="postalCode"
            autoComplete="postal-code"
            inputMode="numeric"
            placeholder="92101"
            className="sm:col-span-2"
            value={values.postalCode}
            onChange={update("postalCode")}
            error={errors["Address.PostalCode"]}
          />
        </div>
      </section>

      <section className="rounded-lg border border-border bg-surface p-6">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted">Loan</h2>

        <div className="mt-4 sm:max-w-xs">
          <Field
            label="Requested amount (USD)"
            name="requestedAmount"
            type="number"
            min={1}
            step={100}
            inputMode="decimal"
            placeholder="10000"
            value={values.requestedAmount}
            onChange={update("requestedAmount")}
            error={errors.RequestedAmount}
          />
        </div>
      </section>

      {failure ? (
        <p role="alert" className="rounded-md border border-danger/40 bg-danger/5 px-4 py-3 text-sm text-danger">
          {failure}
        </p>
      ) : null}

      <button
        type="submit"
        disabled={submitting}
        className="w-full rounded-md bg-accent px-4 py-2.5 text-sm font-medium text-white transition
          hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-60 sm:w-auto"
      >
        {submitting ? "Checking your application…" : "Submit application"}
      </button>
    </form>
  );
}

/** Input name to the field name the API reports errors against. */
const fieldToApiName: Record<keyof Values, string> = {
  firstName: "FirstName",
  lastName: "LastName",
  companyName: "CompanyName",
  street: "Address.Street",
  city: "Address.City",
  state: "Address.State",
  postalCode: "Address.PostalCode",
  requestedAmount: "RequestedAmount",
  ssn: "Ssn",
};

function flatten(errors: Record<string, string[]>): FieldErrors {
  return Object.fromEntries(
    Object.entries(errors).map(([field, messages]) => [field, messages[0]]),
  );
}
