export type LoanApplicationForm = {
  firstName: string;
  lastName: string;
  companyName: string;
  address: {
    street: string;
    city: string;
    state: string;
    postalCode: string;
  };
  requestedAmount: number;
  ssn: string;
};

export type LoanDecision = {
  decision: "Approved" | "Denied";
  applicationId: string | null;
  denialCode: string | null;
  denialReason: string | null;
};

/** RFC 9457 problem details, as the backend returns them for a malformed form. */
export type ValidationProblem = {
  errors: Record<string, string[]>;
};

export const emptyForm: LoanApplicationForm = {
  firstName: "",
  lastName: "",
  companyName: "",
  address: { street: "", city: "", state: "", postalCode: "" },
  requestedAmount: 0,
  ssn: "",
};

export const currency = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  maximumFractionDigits: 0,
});
