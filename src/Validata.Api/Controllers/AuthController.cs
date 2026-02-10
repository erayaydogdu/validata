using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Validata.Application.DTOs.Auth;
using Validata.Application.DTOs.Common;
using Validata.Core.Entities;
using Validata.Core.Enums;
using Validata.Core.Interfaces;
using Validata.Infrastructure.Identity;

namespace Validata.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        JwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
            {
                return Unauthorized(new ErrorResponse { Code = "INVALID_CREDENTIALS", Message = "Invalid email or password" });
            }

            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new ErrorResponse { Code = "INVALID_CREDENTIALS", Message = "Invalid email or password" });
            }

            var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, ((UserRole)user.Role).ToString());
            var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

            user.LastLoginAt = DateTime.UtcNow;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 900,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = ((UserRole)user.Role).ToString(),
                    OrganizationId = user.OrganizationId
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for {Email}", request.Email);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred during login" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Conflict(new ErrorResponse { Code = "EMAIL_EXISTS", Message = "Email already registered" });
            }

            Organization? organization = null;
            if (!string.IsNullOrEmpty(request.OrganizationName))
            {
                organization = await _organizationRepository.GetByNameAsync(request.OrganizationName);
                if (organization == null)
                {
                    organization = new Organization
                    {
                        Name = request.OrganizationName,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _organizationRepository.AddAsync(organization);
                    await _organizationRepository.SaveChangesAsync();
                }
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = (int)UserRole.HRStaff,
                OrganizationId = organization?.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, ((UserRole)user.Role).ToString());
            var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

            return CreatedAtAction(nameof(Login), new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 900,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = ((UserRole)user.Role).ToString(),
                    OrganizationId = user.OrganizationId
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", request.Email);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred during registration" });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var principal = _jwtTokenService.ValidateToken(request.RefreshToken);
            if (principal == null)
            {
                return Unauthorized(new ErrorResponse { Code = "INVALID_TOKEN", Message = "Invalid refresh token" });
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new ErrorResponse { Code = "INVALID_TOKEN", Message = "Invalid token claims" });
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return Unauthorized(new ErrorResponse { Code = "USER_NOT_FOUND", Message = "User not found or inactive" });
            }

            var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, ((UserRole)user.Role).ToString());
            var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 900
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred during token refresh" });
        }
    }
}
