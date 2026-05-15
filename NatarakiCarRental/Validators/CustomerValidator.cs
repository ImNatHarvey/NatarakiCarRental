using FluentValidation;
using NatarakiCarRental.Models;

namespace NatarakiCarRental.Validators;

public sealed class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(customer => customer.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.");

        RuleFor(customer => customer.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.");

        RuleFor(customer => customer.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.");

        RuleFor(customer => customer.Email)
            .EmailAddress()
            .When(customer => !string.IsNullOrWhiteSpace(customer.Email))
            .WithMessage("Email address is invalid.");

        When(HasAnyAddressValue, () =>
        {
            RuleFor(customer => customer.Region)
                .NotEmpty()
                .WithMessage("Region is required when entering an address.");

            RuleFor(customer => customer.Province)
                .NotEmpty()
                .WithMessage("Province is required when entering an address.");

            RuleFor(customer => customer.City)
                .NotEmpty()
                .WithMessage("City or municipality is required when entering an address.");

            RuleFor(customer => customer.Barangay)
                .NotEmpty()
                .WithMessage("Barangay is required when entering an address.");
        });
    }

    private static bool HasAnyAddressValue(Customer customer)
    {
        return !string.IsNullOrWhiteSpace(customer.Region)
            || !string.IsNullOrWhiteSpace(customer.Province)
            || !string.IsNullOrWhiteSpace(customer.City)
            || !string.IsNullOrWhiteSpace(customer.Barangay)
            || !string.IsNullOrWhiteSpace(customer.StreetAddress);
    }
}
