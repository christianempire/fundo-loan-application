using System.ComponentModel.DataAnnotations;

namespace Fundo.Loans.Api.LoanApplications;

/// <summary>
/// Runs data annotations over a request, including nested objects, and returns the
/// failures in the shape <c>Results.ValidationProblem</c> expects.
/// </summary>
/// <remarks>
/// Minimal APIs do not validate model state on their own. This is the whole of what
/// this project needs, which is why there is no validation library here.
/// </remarks>
internal static class RequestValidator
{
    public static bool TryValidate(object instance, out Dictionary<string, string[]> errors)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);

        foreach (var property in instance.GetType().GetProperties())
        {
            if (property.GetValue(instance) is not { } value || IsSimple(property.PropertyType))
            {
                continue;
            }

            var nested = new List<ValidationResult>();
            Validator.TryValidateObject(value, new ValidationContext(value), nested, validateAllProperties: true);

            results.AddRange(nested.Select(result => new ValidationResult(
                result.ErrorMessage,
                result.MemberNames.Select(member => $"{property.Name}.{member}"))));
        }

        errors = results
            .SelectMany(result => result.MemberNames.Select(member => (member, result.ErrorMessage)))
            .GroupBy(failure => failure.member, failure => failure.ErrorMessage ?? "Invalid value.")
            .ToDictionary(group => group.Key, group => group.ToArray());

        return errors.Count == 0;
    }

    private static bool IsSimple(Type type) =>
        type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)
        || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(Guid);
}
