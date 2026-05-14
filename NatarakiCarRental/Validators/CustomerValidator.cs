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
    }
}
