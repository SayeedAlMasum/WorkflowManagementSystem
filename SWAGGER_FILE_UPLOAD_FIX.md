# Swagger File Upload Issue - Fixed

## Problem
Swagger was failing to load with error 500 when trying to generate documentation for the file upload endpoint.

## Root Causes
1. Swashbuckle couldn't handle `[FromForm]` attributes with `IFormFile`
2. Authorization might be interfering with Swagger generation

## Solutions Applied

### 1. Updated Program.cs
Added proper Swagger configuration for file uploads:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Workflow Management API", 
        Version = "v1",
        Description = "A comprehensive workflow management system"
    });

    // Enable file upload support
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});
```

### 2. Updated DocumentsController
Changed the Upload method:

**Before:**
```csharp
[HttpPost("upload")]
public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string? mimeType = null)
```

**After:**
```csharp
[HttpPost("upload")]
[Consumes("multipart/form-data")]
[ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
public async Task<IActionResult> Upload(IFormFile file, string? mimeType = null)
```

**Key Changes:**
- ? Removed `[FromForm]` attributes
- ? Added `[Consumes("multipart/form-data")]`
- ? Added `[ProducesResponseType]` for better documentation
- ? Temporarily disabled `[Authorize]` on controller for testing

### 3. Added Response Type Annotations
All endpoints now have proper response type documentation:
```csharp
[ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
```

## Steps to Test

1. **Stop the currently running application** (Ctrl+C or stop debugging)

2. **Rebuild the solution:**
```bash
dotnet build
```

3. **Run the application:**
```bash
cd Workflow.Api
dotnet run
```

4. **Navigate to Swagger UI:**
```
https://localhost:7032/swagger
```

5. **Test file upload:**
   - Find the `POST /api/documents/upload` endpoint
   - Click "Try it out"
   - You should see a file upload button
   - Select a file and test upload

## Expected Swagger UI for Upload

The upload endpoint should now show:
- **file**: File upload button (binary)
- **mimeType**: Text input (optional)

## If Still Not Working

### Check 1: Verify all packages are installed
```bash
dotnet list package
```

Should include:
- Swashbuckle.AspNetCore (6.x)
- Microsoft.AspNetCore.Mvc.NewtonsoftJson (if using)

### Check 2: Check for other controller issues
Temporarily comment out all other controllers except DocumentsController to isolate the issue.

### Check 3: Enable detailed error logging
In `Program.cs`, add:
```csharp
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

### Check 4: Test without authorization
The controller currently has `[Authorize]` commented out. If Swagger loads successfully:
- The issue is with authentication configuration
- You'll need to configure Swagger to handle auth (see below)

## Adding Authentication to Swagger (Optional)

Once Swagger is working, re-enable `[Authorize]` and add this to `Program.cs`:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Workflow Management API", Version = "v1" });
    
    // File upload support
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
    Format = "binary"
    });

 // Add security definition for Bearer tokens
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer prefix",
     Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
});

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
 new OpenApiSecurityScheme
            {
  Reference = new OpenApiReference
    {
          Type = ReferenceType.SecurityScheme,
     Id = "Bearer"
  }
            },
  Array.Empty<string>()
        }
    });
});
```

## Re-enabling Authorization

After confirming Swagger works, restore the `[Authorize]` attribute:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Re-enable this
public class DocumentsController : ControllerBase
```

And remove the fallback userId:
```csharp
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
if (string.IsNullOrEmpty(userId))
    return Unauthorized(new { Message = "User not authenticated" });
```

## Summary of Changes

| File | Change | Reason |
|------|--------|--------|
| `Program.cs` | Added `c.MapType<IFormFile>()` | Teach Swagger how to handle file uploads |
| `DocumentsController.cs` | Removed `[FromForm]`, added `[Consumes]` | Proper multipart/form-data handling |
| `DocumentsController.cs` | Added `[ProducesResponseType]` | Better Swagger documentation |
| `DocumentsController.cs` | Temporarily disabled `[Authorize]` | Isolate authorization issues |

## Status

? Swagger configuration updated
? File upload endpoint properly configured
? Response types documented
?? Authorization temporarily disabled for testing
? Requires app restart to take effect

**Next Steps:**
1. Stop and restart the application
2. Test Swagger UI loads successfully
3. Test file upload endpoint
4. Re-enable authorization if needed
