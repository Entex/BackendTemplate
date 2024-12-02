using System.Text.RegularExpressions;

namespace BackendTemplate.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    public string Value { get; }

    private static readonly string PhoneNumberPattern = @"^\+?\d{10,15}$";

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static Result<PhoneNumber> Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return Result<PhoneNumber>.Failure("Phone number cannot be empty.");
        }

        if (!Regex.IsMatch(phoneNumber, PhoneNumberPattern))
        {
            return Result<PhoneNumber>.Failure("Invalid phone number format.");
        }

        return Result<PhoneNumber>.Success(new PhoneNumber(phoneNumber));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
