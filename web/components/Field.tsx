import type { InputHTMLAttributes } from "react";

type FieldProps = InputHTMLAttributes<HTMLInputElement> & {
  label: string;
  name: string;
  error?: string;
  hint?: string;
};

export function Field({ label, name, error, hint, className, ...input }: FieldProps) {
  const describedBy = error ? `${name}-error` : hint ? `${name}-hint` : undefined;

  return (
    <div className={className}>
      <label htmlFor={name} className="block text-sm font-medium">
        {label}
      </label>

      <input
        id={name}
        name={name}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy}
        className={`mt-1.5 w-full rounded-md border bg-surface px-3 py-2 text-sm outline-none transition
          focus:ring-2 focus:ring-accent/40 ${error ? "border-danger" : "border-border focus:border-accent"}`}
        {...input}
      />

      {error ? (
        <p id={`${name}-error`} role="alert" className="mt-1.5 text-xs text-danger">
          {error}
        </p>
      ) : hint ? (
        <p id={`${name}-hint`} className="mt-1.5 text-xs text-muted">
          {hint}
        </p>
      ) : null}
    </div>
  );
}
