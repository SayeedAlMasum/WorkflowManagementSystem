using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;
using Workflow.Domain.Entities;

namespace Workflow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtService _jwtService;
private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole> roleManager,
   IJwtService jwtService,
        ILogger<AuthController> logger,
    IConfiguration configuration)
    {
      _userManager = userManager;
    _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
     _logger = logger;
    _configuration = configuration;
    }

    /// <summary>
  /// Register a new user
 /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        try
        {
     if (!ModelState.IsValid)
         return BadRequest(new AuthResponseDto
    {
  Success = false,
                    Message = "Invalid input",
         Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
   });

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
          return BadRequest(new AuthResponseDto
                {
      Success = false,
     Message = "User with this email already exists"
        });

         var user = new AppUser
    {
         UserName = model.Email,
   Email = model.Email,
            FullName = model.FullName
         };

    var result = await _userManager.CreateAsync(user, model.Password);

      if (!result.Succeeded)
        return BadRequest(new AuthResponseDto
         {
         Success = false,
      Message = "Failed to create user",
        Errors = result.Errors.Select(e => e.Description).ToList()
            });

  // Assign default "Employee" role
    await _userManager.AddToRoleAsync(user, "Employee");

     // Generate JWT token
  var roles = await _userManager.GetRolesAsync(user);
    var token = _jwtService.GenerateToken(user, roles);
         var tokenExpiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]));

      _logger.LogInformation("User {Email} registered successfully", model.Email);

        return Ok(new AuthResponseDto
       {
    Success = true,
   Message = "User registered successfully",
             UserId = user.Id,
      Email = user.Email,
  Token = token,
    TokenExpiry = tokenExpiry,
 Roles = roles.ToList()
            });
     }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
    return StatusCode(500, new AuthResponseDto
       {
     Success = false,
     Message = "An error occurred while registering user"
    });
     }
    }

    /// <summary>
    /// Login a user
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        try
        {
if (!ModelState.IsValid)
  return BadRequest(new AuthResponseDto
    {
    Success = false,
   Message = "Invalid input",
Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
           });

        var user = await _userManager.FindByEmailAsync(model.Email);
   if (user == null)
      return Unauthorized(new AuthResponseDto
             {
        Success = false,
 Message = "Invalid email or password"
     });

  var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

if (!result.Succeeded)
  return Unauthorized(new AuthResponseDto
 {
          Success = false,
Message = result.IsLockedOut ? "Account is locked out" :
      result.IsNotAllowed ? "Account is not allowed to sign in" :
             "Invalid email or password"
          });

            var roles = await _userManager.GetRolesAsync(user);
     
    // Generate JWT token
       var token = _jwtService.GenerateToken(user, roles);
   var tokenExpiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]));

  _logger.LogInformation("User {Email} logged in successfully", model.Email);

return Ok(new AuthResponseDto
     {
   Success = true,
 Message = "Login successful",
      UserId = user.Id,
 Email = user.Email,
 Token = token,
      TokenExpiry = tokenExpiry,
 Roles = roles.ToList()
    });
    }
        catch (Exception ex)
  {
     _logger.LogError(ex, "Error during login");
        return StatusCode(500, new AuthResponseDto
        {
 Success = false,
   Message = "An error occurred during login"
  });
        }
    }

    /// <summary>
    /// Logout the current user (JWT tokens should be removed on client-side)
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
     try
        {
            // With JWT, logout is handled on the client-side by removing the token
     // This endpoint is optional but useful for logging purposes
   var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      _logger.LogInformation("User {UserId} logged out", userId);

   return Ok(new AuthResponseDto
     {
  Success = true,
      Message = "Logout successful. Please remove the token from client storage."
   });
     }
    catch (Exception ex)
        {
  _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new AuthResponseDto
     {
     Success = false,
       Message = "An error occurred during logout"
    });
      }
    }

    /// <summary>
    /// Get current user information
    /// </summary>
 [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
       return Unauthorized(new AuthResponseDto
 {
         Success = false,
     Message = "User not authenticated"
             });

      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
   return NotFound(new AuthResponseDto
    {
           Success = false,
         Message = "User not found"
                });

            var roles = await _userManager.GetRolesAsync(user);

     return Ok(new
            {
       userId = user.Id,
         email = user.Email,
                fullName = user.FullName,
      roles = roles.ToList()
     });
        }
        catch (Exception ex)
   {
       _logger.LogError(ex, "Error getting current user");
   return StatusCode(500, new AuthResponseDto
       {
 Success = false,
        Message = "An error occurred while retrieving user information"
       });
        }
    }

    /// <summary>
    /// Change password for the current user
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
    {
        try
{
          if (!ModelState.IsValid)
   return BadRequest(new AuthResponseDto
         {
    Success = false,
         Message = "Invalid input",
                 Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
           });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
          return Unauthorized(new AuthResponseDto
                {
   Success = false,
        Message = "User not authenticated"
  });

    var user = await _userManager.FindByIdAsync(userId);
     if (user == null)
  return NotFound(new AuthResponseDto
                {
   Success = false,
 Message = "User not found"
     });

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

      if (!result.Succeeded)
      return BadRequest(new AuthResponseDto
   {
             Success = false,
            Message = "Failed to change password",
          Errors = result.Errors.Select(e => e.Description).ToList()
      });

            _logger.LogInformation("User {UserId} changed password successfully", userId);

    return Ok(new AuthResponseDto
         {
       Success = true,
       Message = "Password changed successfully"
  });
     }
        catch (Exception ex)
        {
         _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new AuthResponseDto
  {
      Success = false,
 Message = "An error occurred while changing password"
  });
        }
    }

    /// <summary>
    /// Get all available roles (Admin only)
    /// </summary>

    [Authorize(Roles ="Admin")]
    [HttpGet("roles")]
    public IActionResult GetRoles()
    {
        try
        {
  var roles = _roleManager.Roles.Select(r => new
         {
         id = r.Id,
    name = r.Name
            }).ToList();

            return Ok(roles);
        }
   catch (Exception ex)
        {
     _logger.LogError(ex, "Error getting roles");
  return StatusCode(500, new AuthResponseDto
            {
     Success = false,
           Message = "An error occurred while retrieving roles"
      });
}
    }

    /// <summary>
    /// Assign role to user (Admin only)
    /// </summary>

    [Authorize(Roles ="Admin")]
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
    {
      try
  {
       _logger.LogInformation("Attempting to assign role {Role} to user {UserId}", model.Role, model.UserId);

       if (!ModelState.IsValid)
   {
           _logger.LogWarning("Invalid model state for role assignment");
    return BadRequest(new AuthResponseDto
            {
        Success = false,
          Message = "Invalid input",
      Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
           });
            }

    // Check if user exists
            var user = await _userManager.FindByIdAsync(model.UserId);
   if (user == null)
          {
       _logger.LogWarning("User not found: {UserId}", model.UserId);
    return NotFound(new AuthResponseDto
         {
Success = false,
           Message = $"User with ID '{model.UserId}' not found"
                });
         }

        // Check if role exists
    var roleExists = await _roleManager.RoleExistsAsync(model.Role);
 if (!roleExists)
     {
     _logger.LogWarning("Role does not exist: {Role}", model.Role);
           
   // List available roles for debugging
       var availableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
      _logger.LogInformation("Available roles: {Roles}", string.Join(", ", availableRoles));
  
      return BadRequest(new AuthResponseDto
 {
                    Success = false,
           Message = $"Role '{model.Role}' does not exist. Available roles: {string.Join(", ", availableRoles)}"
       });
      }

     // Check if user already has the role
   var isInRole = await _userManager.IsInRoleAsync(user, model.Role);
   if (isInRole)
    {
       _logger.LogInformation("User {UserId} already has role {Role}", model.UserId, model.Role);
    
        // Get current roles
     var currentRoles = await _userManager.GetRolesAsync(user);
      
      return Ok(new AuthResponseDto
                {
        Success = true,
         Message = $"User already has the '{model.Role}' role",
  Roles = currentRoles.ToList()
                });
            }

   // Assign the role
       var result = await _userManager.AddToRoleAsync(user, model.Role);

          if (!result.Succeeded)
            {
                _logger.LogError("Failed to assign role {Role} to user {UserId}. Errors: {Errors}", 
   model.Role, model.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
           
              return BadRequest(new AuthResponseDto
    {
   Success = false,
         Message = "Failed to assign role",
         Errors = result.Errors.Select(e => e.Description).ToList()
    });
            }

            // Get updated roles
      var updatedRoles = await _userManager.GetRolesAsync(user);
       
   _logger.LogInformation("Role {Role} successfully assigned to user {UserId} ({Email})", 
 model.Role, model.UserId, user.Email);

      return Ok(new AuthResponseDto
            {
    Success = true,
          Message = $"Role '{model.Role}' assigned successfully to user {user.Email}",
                UserId = user.Id,
         Email = user.Email,
         Roles = updatedRoles.ToList()
    });
        }
        catch (Exception ex)
 {
            _logger.LogError(ex, "Error assigning role {Role} to user {UserId}", model.Role, model.UserId);
            return StatusCode(500, new AuthResponseDto
      {
Success = false,
        Message = $"An error occurred while assigning role: {ex.Message}"
 });
        }
    }
    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
    try
 {
     var users = _userManager.Users.Select(u => new
         {
   id = u.Id,
    email = u.Email,
       fullName = u.FullName,
      userName = u.UserName
     }).ToList();

     var usersWithRoles = new List<object>();
         foreach (var user in users)
  {
var appUser = await _userManager.FindByIdAsync(user.id);
     var roles = await _userManager.GetRolesAsync(appUser!);
      usersWithRoles.Add(new
{
    user.id,
user.email,
   user.fullName,
        user.userName,
       roles = roles.ToList()
   });
          }

        return Ok(usersWithRoles);
        }
        catch (Exception ex)
        {
        _logger.LogError(ex, "Error getting users");
 return StatusCode(500, new AuthResponseDto
            {
          Success = false,
  Message = "An error occurred while retrieving users"
       });
     }
    }

    /// <summary>
    /// Remove role from user (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto model)
    {
    try
        {
_logger.LogInformation("Attempting to remove role {Role} from user {UserId}", model.Role, model.UserId);

            if (!ModelState.IsValid)
            {
          return BadRequest(new AuthResponseDto
      {
    Success = false,
               Message = "Invalid input",
             Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
        });
}

         var user = await _userManager.FindByIdAsync(model.UserId);
      if (user == null)
    {
          return NotFound(new AuthResponseDto
 {
       Success = false,
            Message = $"User with ID '{model.UserId}' not found"
        });
            }

            var isInRole = await _userManager.IsInRoleAsync(user, model.Role);
         if (!isInRole)
            {
    return BadRequest(new AuthResponseDto
       {
    Success = false,
    Message = $"User does not have the '{model.Role}' role"
            });
 }

   var result = await _userManager.RemoveFromRoleAsync(user, model.Role);

            if (!result.Succeeded)
     {
                return BadRequest(new AuthResponseDto
 {
              Success = false,
           Message = "Failed to remove role",
    Errors = result.Errors.Select(e => e.Description).ToList()
         });
      }

            var updatedRoles = await _userManager.GetRolesAsync(user);
     
         _logger.LogInformation("Role {Role} successfully removed from user {UserId}", model.Role, model.UserId);

   return Ok(new AuthResponseDto
            {
      Success = true,
    Message = $"Role '{model.Role}' removed successfully from user {user.Email}",
          UserId = user.Id,
  Email = user.Email,
 Roles = updatedRoles.ToList()
            });
        }
     catch (Exception ex)
{
            _logger.LogError(ex, "Error removing role {Role} from user {UserId}", model.Role, model.UserId);
return StatusCode(500, new AuthResponseDto
     {
                Success = false,
                Message = $"An error occurred while removing role: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        try
        {
        var user = await _userManager.FindByIdAsync(userId);
       if (user == null)
          {
   return NotFound(new AuthResponseDto
     {
         Success = false,
             Message = "User not found"
 });
   }

      var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
      {
        id = user.Id,
           email = user.Email,
        fullName = user.FullName,
 userName = user.UserName,
      roles = roles.ToList()
            });
   }
        catch (Exception ex)
 {
       _logger.LogError(ex, "Error getting user {UserId}", userId);
    return StatusCode(500, new AuthResponseDto
      {
     Success = false,
     Message = "An error occurred while retrieving user"
  });
}
    }

    /// <summary>
    /// Test authentication - verify JWT token is working
    /// </summary>
 [Authorize]
    [HttpGet("test-auth")]
  [ApiExplorerSettings(IgnoreApi = true)]  // Hide from Swagger in production
    public IActionResult TestAuth()
    {
#if DEBUG
        try
        {
         return Ok(new
    {
    authenticated = User.Identity?.IsAuthenticated ?? false,
                userName = User.Identity?.Name,
              userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
          email = User.FindFirstValue(ClaimTypes.Email),
         claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
     });
        }
    catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test-auth");
          return StatusCode(500, new { error = ex.Message });
        }
#else
     return NotFound();
#endif
    }

    /// <summary>
    /// Debug JWT configuration - shows current JWT settings (Admin only for security)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("debug-jwt")]
    [ApiExplorerSettings(IgnoreApi = true)]// Hide from Swagger in production
    public IActionResult DebugJwt()
    {
#if DEBUG
        try
{
            var jwtSettings = _configuration.GetSection("Jwt");

            return Ok(new
            {
                issuer = jwtSettings["Issuer"],
       audience = jwtSettings["Audience"],
      keyLength = jwtSettings["Key"]?.Length ?? 0,
expiryMinutes = jwtSettings["ExpiryInMinutes"],
            serverTime = DateTime.UtcNow,
       message = "JWT Configuration (Key hidden for security)"
   });
        }
    catch (Exception ex)
        {
_logger.LogError(ex, "Error in debug-jwt");
            return StatusCode(500, new { error = ex.Message });
        }
#else
      return NotFound();
#endif
    }

    /// <summary>
    /// Generate a test token without authentication (for testing only - remove in production!)
    /// </summary>
    [HttpPost("generate-test-token")]
    [ApiExplorerSettings(IgnoreApi = true)]  // Hide from Swagger in production
    public async Task<IActionResult> GenerateTestToken([FromBody] string email)
    {
#if DEBUG
        try
    {
          var user = await _userManager.FindByEmailAsync(email);
  if (user == null)
       return NotFound(new { message = "User not found" });

          var roles = await _userManager.GetRolesAsync(user);
  var token = _jwtService.GenerateToken(user, roles);
         var tokenExpiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]));

         // Also decode and show token details
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

return Ok(new
            {
       token = token,
     tokenExpiry = tokenExpiry,
       decodedToken = new
             {
              issuer = jwtToken.Issuer,
        audience = jwtToken.Audiences.FirstOrDefault(),
          expiration = jwtToken.ValidTo,
        claims = jwtToken.Claims.Select(c => new { c.Type, c.Value }).ToList()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating test token");
     return StatusCode(500, new { error = ex.Message });
        }
#else
      return NotFound();
#endif
    }

    /// <summary>
    /// Test raw Authorization header - verify token is being sent correctly (No auth required)
    /// </summary>
    [HttpGet("test-raw-header")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]  // Hide from Swagger in production
    public IActionResult TestRawAuthHeader()
    {
#if DEBUG
        try
    {
        var authHeader = Request.Headers["Authorization"].ToString();
          var hasAuthHeader = !string.IsNullOrEmpty(authHeader);

    return Ok(new
         {
     hasAuthorizationHeader = hasAuthHeader,
      authorizationHeader = hasAuthHeader ? authHeader : "No Authorization header found",
                headerCount = Request.Headers.Count,
                allHeaders = Request.Headers.Select(h => new { h.Key, Value = h.Value.ToString() }).ToList()
   });
      }
  catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test-raw-header");
 return StatusCode(500, new { error = ex.Message });
 }
#else
        return NotFound();
#endif
 }  

    /// <summary>
    /// Get just the token from login (easier to copy) - for testing only
    /// </summary>
    [HttpPost("get-token")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]  // Hide from Swagger in production
    public async Task<IActionResult> GetToken([FromBody] LoginDto model)
    {
#if DEBUG
        try
     {
    if (!ModelState.IsValid)
         return BadRequest(new { error = "Invalid input" });

   var user = await _userManager.FindByEmailAsync(model.Email);
    if (user == null)
       return Unauthorized(new { error = "Invalid credentials" });

    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
          if (!result.Succeeded)
    return Unauthorized(new { error = "Invalid credentials" });

  var roles = await _userManager.GetRolesAsync(user);
     var token = _jwtService.GenerateToken(user, roles);
 var expiryMinutes = Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]);
          var tokenExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

       // Return ONLY the token as plain text for easy copying
          return Ok(new
       {
   token = token,
  tokenExpiry = tokenExpiry,
        expiresIn = $"{expiryMinutes} minutes",
       instruction = "Copy the token value above (without quotes) and paste it in Swagger Authorize dialog"
  });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token");
   return StatusCode(500, new { error = ex.Message });
        }
#else
    return NotFound();
#endif
    }

 /// <summary>
    /// Validate if current token is still valid - for testing only
    /// </summary>
    [HttpGet("validate-token")]
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult ValidateToken()
    {
#if DEBUG
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
  var email = User.FindFirstValue(ClaimTypes.Email);
    var userName = User.Identity?.Name;

            return Ok(new
            {
                valid = true,
      message = "Token is valid",
                userId = userId,
    email = email,
      userName = userName,
   authenticated = User.Identity?.IsAuthenticated ?? false
            });
        }
        catch (Exception ex)
        {
         _logger.LogError(ex, "Error validating token");
            return StatusCode(500, new { error = ex.Message });
        }
#else
        return NotFound();
#endif
    }
}
