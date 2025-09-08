using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Million.PropertiesService.Api.Models;
using Million.PropertiesService.Api.Services;
using Million.PropertiesService.Api.Web;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Million.PropertiesService.Api.Controllers;

/// <summary>
/// Authentication endpoints for user login and token generation
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[SwaggerTag("User authentication and authorization")]
public class AuthController : ApiControllerBase
{
    private readonly IJwtService _jwtService;

    public AuthController(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="loginRequest">User login credentials</param>
    /// <returns>JWT token and user information</returns>
    /// <remarks>
    /// Admin User:
    /// - Email: admin@million.com
    /// - Password: admin123
    /// - Role: Admin
    /// 
    /// Manager User:
    /// - Email: manager@million.com  
    /// - Password: manager123
    /// - Role: Manager
    /// 
    /// Regular User:
    /// - Email: user@million.com
    /// - Password: user123
    /// - Role: User
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1.0/auth/login
    ///     {
    ///         "email": "admin@million.com",
    ///         "password": "admin123"
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">Login successful, returns JWT token</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "User login",
        Description = "Authenticates a user with email and password, returns JWT token for API access",
        OperationId = "Login",
        Tags = new[] { "Authentication" }
    )]
    [SwaggerResponse((int)HttpStatusCode.OK, "Login successful", typeof(LoginResponse))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request data")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Invalid credentials")]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, "Internal server error")]
    [ProducesResponseType(typeof(LoginResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest loginRequest)
    {
        var (isValid, userId, role) = ValidateUser(loginRequest.Email, loginRequest.Password);
        
        if (!isValid)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var token = _jwtService.GenerateToken(userId, loginRequest.Email, role);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var response = new LoginResponse(
            Token: token,
            UserId: userId,
            Email: loginRequest.Email,
            Role: role,
            ExpiresAt: expiresAt
        );

        return Ok(response);
    }

    private static (bool IsValid, string UserId, string Role) ValidateUser(string email, string password)
    {
        var demoUsers = new Dictionary<string, (string Password, string UserId, string Role)>
        {
            { "admin@million.com", ("admin123", Guid.NewGuid().ToString(), "Admin") },
            { "manager@million.com", ("manager123", Guid.NewGuid().ToString(), "Manager") },
            { "user@million.com", ("user123", Guid.NewGuid().ToString(), "User") }
        };

        if (demoUsers.TryGetValue(email.ToLower(), out var user) && user.Password == password)
        {
            return (true, user.UserId, user.Role);
        }

        return (false, string.Empty, string.Empty);
    }
}