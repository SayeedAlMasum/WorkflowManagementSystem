using System.ComponentModel.DataAnnotations;

namespace Workflow.Application.DTOs;

/// <summary>
/// DTO for user registration
/// </summary>
public class RegisterDto
{
  [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

  [Required]
[StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
  public string ConfirmPassword { get; set; } = string.Empty;

    public string? FullName { get; set; }
}

/// <summary>
/// DTO for user login
/// </summary>
public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
  public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for changing password
/// </summary>
public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// DTO for assigning/removing roles
/// </summary>
public class AssignRoleDto
{
  [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for authentication operations
/// </summary>
public class AuthResponseDto
{
    public bool Success { get; set; }
  public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Token { get; set; }
    public DateTime? TokenExpiry { get; set; }
    public List<string>? Roles { get; set; }
    public List<string>? Errors { get; set; }
}
