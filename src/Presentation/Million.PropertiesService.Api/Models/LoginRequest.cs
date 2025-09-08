using System.ComponentModel.DataAnnotations;

namespace Million.PropertiesService.Api.Models;

public record LoginRequest(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    string Password
);

public record LoginResponse(
    string Token,
    string UserId,
    string Email,
    string Role,
    DateTime ExpiresAt
);