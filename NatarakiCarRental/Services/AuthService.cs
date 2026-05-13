using NatarakiCarRental.Models;
using NatarakiCarRental.Repositories;

namespace NatarakiCarRental.Services;

public sealed class AuthService
{
    private readonly UserRepository _userRepository;

    public AuthService()
        : this(new UserRepository())
    {
    }

    public AuthService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public User? Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        User? user = _userRepository.GetActiveUserByUsername(username.Trim());

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }
}
