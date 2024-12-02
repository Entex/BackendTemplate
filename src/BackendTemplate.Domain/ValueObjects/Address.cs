using System.Text.RegularExpressions;

namespace BackendTemplate.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street1 { get; }
    public string Street2 { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private static readonly string PostalCodePattern = @"^\d{5}(-\d{4})?$";

    private Address(string street1, string street2, string city, string postalCode, string country)
    {
        Street1 = street1;
        Street2 = street2;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    public static Result<Address> Create(
        string street1,
        string? street2,
        string city,
        string postalCode,
        string country
    )
    {
        if (string.IsNullOrWhiteSpace(street1))
        {
            return Result<Address>.Failure("Street1 is required.");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return Result<Address>.Failure("City is required.");
        }

        if (string.IsNullOrWhiteSpace(postalCode) || !Regex.IsMatch(postalCode, PostalCodePattern))
        {
            return Result<Address>.Failure(
                "Invalid postal code format. Expected formats: 12345 or 12345-6789."
            );
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            return Result<Address>.Failure("Country is required.");
        }

        return Result<Address>.Success(
            new Address(street1, street2 ?? string.Empty, city, postalCode, country)
        );
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street1;
        yield return Street2;
        yield return City;
        yield return PostalCode;
        yield return Country;
    }
}
