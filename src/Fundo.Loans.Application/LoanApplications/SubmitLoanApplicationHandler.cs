using Fundo.Loans.Application.Abstractions;
using Fundo.Loans.Application.IntegrationEvents;
using Fundo.Loans.Domain.Applications;
using Fundo.Loans.Domain.Customers;
using Fundo.Loans.Domain.Decisions;

namespace Fundo.Loans.Application.LoanApplications;

/// <summary>
/// The whole use case: decide, then persist and publish as one unit of work.
/// </summary>
public sealed class SubmitLoanApplicationHandler
{
    private readonly DecisionEngine _decisionEngine;
    private readonly ICustomerRepository _customers;
    private readonly ILoanApplicationRepository _applications;
    private readonly IIntegrationEventPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISsnHasher _ssnHasher;

    public SubmitLoanApplicationHandler(
        DecisionEngine decisionEngine,
        ICustomerRepository customers,
        ILoanApplicationRepository applications,
        IIntegrationEventPublisher publisher,
        IUnitOfWork unitOfWork,
        ISsnHasher ssnHasher)
    {
        _decisionEngine = decisionEngine;
        _customers = customers;
        _applications = applications;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
        _ssnHasher = ssnHasher;
    }

    public async Task<SubmitLoanApplicationResult> HandleAsync(
        SubmitLoanApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var applicant = new Applicant(
            command.FirstName,
            command.LastName,
            command.Address,
            command.CompanyName,
            command.RequestedAmount,
            command.Ssn);

        var decision = _decisionEngine.Evaluate(applicant);
        if (!decision.IsApproved)
        {
            // Nothing is written for a denial, so there is nothing to roll back.
            return SubmitLoanApplicationResult.Denied(decision.Denial!);
        }

        var applicationId = Guid.Empty;

        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var ssnHash = _ssnHasher.Hash(command.Ssn);
            var customer = await _customers.FindBySsnHashAsync(ssnHash, token);

            var isReturningCustomer = customer is not null;
            if (customer is null)
            {
                customer = Customer.Register(
                    ssnHash,
                    command.Ssn.Last4,
                    command.FirstName,
                    command.LastName,
                    command.CompanyName,
                    command.Address);

                _customers.Add(customer);
            }
            else
            {
                customer.UpdateDetails(
                    command.FirstName,
                    command.LastName,
                    command.CompanyName,
                    command.Address);
            }

            var application = isReturningCustomer
                ? await _applications.FindByCustomerIdAsync(customer.Id, token)
                : null;

            if (application is null)
            {
                application = LoanApplication.Open(customer.Id, command.RequestedAmount);
                _applications.Add(application);
            }
            else
            {
                application.UpdateRequestedAmount(command.RequestedAmount);
            }

            applicationId = application.Id;

            await _publisher.PublishAsync(
                new CustomerSyncRequested(
                    isReturningCustomer ? CustomerSyncOperation.Update : CustomerSyncOperation.Create,
                    customer.Id,
                    customer.FirstName,
                    customer.LastName,
                    customer.CompanyName,
                    new CustomerSyncAddress(
                        customer.Address.Street,
                        customer.Address.City,
                        customer.Address.State,
                        customer.Address.PostalCode),
                    customer.SsnLast4,
                    application.Id,
                    application.RequestedAmount),
                token);
        }, cancellationToken);

        return SubmitLoanApplicationResult.Approved(applicationId);
    }
}
