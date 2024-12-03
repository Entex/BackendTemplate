namespace BackendTemplate.Domain.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        email = (email ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<Email>.Failure("Email should not be empty.");
        }

        if (email.Length > 256)
        {
            return Result<Email>.Failure("Email is too long.");
        }

        try
        {
            var mailAddress = new System.Net.Mail.MailAddress(email);
            return Result<Email>.Success(new Email(mailAddress.Address));
        }
        catch (FormatException)
        {
            return Result<Email>.Failure("Email is invalid.");
        }
    }

    public static implicit operator string(Email email) => email.Value;

    public static explicit operator Email(string email)
    {
        var result = Create(email);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error);
        }
        return result.Value!;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
