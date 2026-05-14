using FluentValidation;
using FluentValidation.Results;
using Microsoft.Data.SqlClient;
using NatarakiCarRental.Data;
using NatarakiCarRental.Exceptions;
using NatarakiCarRental.Models;
using NatarakiCarRental.Repositories;
using NatarakiCarRental.Validators;

namespace NatarakiCarRental.Services;

public sealed class CustomerService
{
    private readonly CustomerRepository _customerRepository;
    private readonly ActivityLogService _activityLogService;
    private readonly DbConnectionFactory _connectionFactory;

    public CustomerService()
        : this(new DbConnectionFactory())
    {
    }

    private CustomerService(DbConnectionFactory connectionFactory)
        : this(new CustomerRepository(connectionFactory), new ActivityLogService(connectionFactory), connectionFactory)
    {
    }

    public CustomerService(CustomerRepository customerRepository, ActivityLogService activityLogService)
        : this(customerRepository, activityLogService, new DbConnectionFactory())
    {
    }

    public CustomerService(
        CustomerRepository customerRepository,
        ActivityLogService activityLogService,
        DbConnectionFactory connectionFactory)
    {
        _customerRepository = customerRepository;
        _activityLogService = activityLogService;
        _connectionFactory = connectionFactory;
    }

    public Task<Customer?> GetCustomerByIdAsync(int customerId)
    {
        return _customerRepository.GetCustomerByIdAsync(customerId);
    }

    public Task<IReadOnlyList<Customer>> SearchCustomersAsync(string searchText, bool includeArchived)
    {
        return _customerRepository.SearchCustomersAsync(searchText, includeArchived);
    }

    public Task<CustomerCounts> GetCustomerCountsAsync()
    {
        return _customerRepository.GetCustomerCountsAsync();
    }

    public Task<bool> PhoneNumberExistsAsync(string phoneNumber, int? excludingCustomerId = null)
    {
        return _customerRepository.PhoneNumberExistsAsync(phoneNumber, excludingCustomerId);
    }

    public async Task<int> AddCustomerAsync(Customer customer)
    {
        NormalizeCustomer(customer);
        ValidateCustomer(customer);

        bool phoneExists = await _customerRepository.PhoneNumberExistsAsync(customer.PhoneNumber);

        if (phoneExists)
        {
            throw CreateDuplicatePhoneValidationException();
        }

        await using SqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync();
        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            int customerId = await _customerRepository.AddCustomerAsync(customer, transaction);
            await _activityLogService.LogAsync(
                "Add customer",
                "Customer",
                customerId,
                $"Added customer {customer.FirstName} {customer.LastName} ({customer.PhoneNumber}).",
                transaction: transaction);

            transaction.Commit();
            return customerId;
        }
        catch (SqlException exception) when (IsUniqueConstraintViolation(exception))
        {
            RollbackQuietly(transaction);
            throw CreateDuplicatePhoneValidationException();
        }
        catch
        {
            RollbackQuietly(transaction);
            throw;
        }
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        NormalizeCustomer(customer);
        ValidateCustomer(customer);

        bool phoneExists = await _customerRepository.PhoneNumberExistsAsync(customer.PhoneNumber, customer.CustomerId);

        if (phoneExists)
        {
            throw CreateDuplicatePhoneValidationException();
        }

        await using SqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync();
        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            int affectedRows = await _customerRepository.UpdateCustomerAsync(customer, transaction);

            if (affectedRows == 0)
            {
                throw new RecordNotFoundException($"Customer record #{customer.CustomerId} was not found.");
            }

            await _activityLogService.LogAsync(
                "Edit customer",
                "Customer",
                customer.CustomerId,
                $"Edited customer {customer.FirstName} {customer.LastName} ({customer.PhoneNumber}).",
                transaction: transaction);

            transaction.Commit();
        }
        catch (SqlException exception) when (IsUniqueConstraintViolation(exception))
        {
            RollbackQuietly(transaction);
            throw CreateDuplicatePhoneValidationException();
        }
        catch
        {
            RollbackQuietly(transaction);
            throw;
        }
    }

    public async Task ArchiveCustomerAsync(int customerId)
    {
        Customer? customer = await _customerRepository.GetCustomerByIdAsync(customerId);
        await using SqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync();
        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            int affectedRows = await _customerRepository.ArchiveCustomerAsync(customerId, transaction);

            if (affectedRows == 0)
            {
                throw new RecordNotFoundException($"Customer record #{customerId} was not found.");
            }

            await _activityLogService.LogAsync(
                "Archive customer",
                "Customer",
                customerId,
                $"Archived customer {DescribeCustomer(customer, customerId)}.",
                transaction: transaction);

            transaction.Commit();
        }
        catch
        {
            RollbackQuietly(transaction);
            throw;
        }
    }

    public async Task ToggleBlacklistAsync(int customerId, bool isBlacklisted)
    {
        Customer? customer = await _customerRepository.GetCustomerByIdAsync(customerId);
        await using SqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync();
        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            int affectedRows = await _customerRepository.ToggleBlacklistAsync(customerId, isBlacklisted, transaction);

            if (affectedRows == 0)
            {
                throw new RecordNotFoundException($"Customer record #{customerId} was not found or is archived.");
            }

            string action = isBlacklisted ? "Blacklist customer" : "Remove customer blacklist";
            string description = isBlacklisted
                ? $"Blacklisted customer {DescribeCustomer(customer, customerId)}."
                : $"Removed blacklist flag from customer {DescribeCustomer(customer, customerId)}.";

            await _activityLogService.LogAsync(
                action,
                "Customer",
                customerId,
                description,
                transaction: transaction);

            transaction.Commit();
        }
        catch
        {
            RollbackQuietly(transaction);
            throw;
        }
    }

    private static void NormalizeCustomer(Customer customer)
    {
        customer.FirstName = customer.FirstName?.Trim() ?? string.Empty;
        customer.LastName = customer.LastName?.Trim() ?? string.Empty;
        customer.Email = NullIfWhiteSpace(customer.Email);
        customer.PhoneNumber = customer.PhoneNumber?.Trim() ?? string.Empty;
        customer.Address = NullIfWhiteSpace(customer.Address);
        customer.DriverLicensePath = NullIfWhiteSpace(customer.DriverLicensePath);
        customer.ProofOfBillingPath = NullIfWhiteSpace(customer.ProofOfBillingPath);
    }

    private static void ValidateCustomer(Customer customer)
    {
        CustomerValidator validator = new();
        validator.ValidateAndThrow(customer);
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string DescribeCustomer(Customer? customer, int customerId)
    {
        return customer is null
            ? $"#{customerId}"
            : $"{customer.FirstName} {customer.LastName} ({customer.PhoneNumber})";
    }

    private static bool IsUniqueConstraintViolation(SqlException exception)
    {
        return exception.Number is 2601 or 2627;
    }

    private static ValidationException CreateDuplicatePhoneValidationException()
    {
        return new ValidationException(
            [new ValidationFailure(nameof(Customer.PhoneNumber), "Phone number already exists.")]);
    }

    private static void RollbackQuietly(SqlTransaction transaction)
    {
        try
        {
            transaction.Rollback();
        }
        catch
        {
            // Preserve the original exception that caused the rollback.
        }
    }
}
