using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wishlist.Application.Interfaces;
using Wishlist.Application.Models;
using Wishlist.Application.Security;
using Wishlist.Contracts.Auth;
using Wishlist.Contracts.Users;
using Wishlist.Domain.Entities;

namespace Wishlist.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _dbContext;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAppDbContext dbContext,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new ValidationAppException("user.exists", "Email address is already registered", new Dictionary<string, string[]>
            {
                ["email"] = new[] { "Email address is already registered" }
            });
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Name = request.Name,
            Locale = request.Locale,
            Country = request.Country,
            Currency = request.Currency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Email} registered", user.Email);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is null || !_passwordHasher.VerifyHashedPassword(user.PasswordHash, request.Password))
        {
            throw new AppException(System.Net.HttpStatusCode.Unauthorized, "auth.invalid", "Invalid credentials");
        }

        var roles = new List<string> { "user" };
        if (user.Email.EndsWith("@admin"))
        {
            roles.Add("admin");
        }

        var (token, expiresAt) = _tokenService.CreateAccessToken(user, roles);
        return new AuthResponse(user.Id, token, expiresAt);
    }
}
