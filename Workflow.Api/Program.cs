using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Workflow.Api.Services;
using Workflow.Application.Interfaces;
using Workflow.Application.Mappings;
using Workflow.Domain.Entities;
using Workflow.Infrastructure.Persistence;
using Workflow.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
maxRetryCount: 5,
     maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)
    ));

// Add Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,  // Re-enable for production
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero, // Remove delay of token expiry
        
        // CRITICAL FIX: Map standard JWT claims to ASP.NET Identity claims
        NameClaimType = System.Security.Claims.ClaimTypes.Name,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };

    // Add event handlers for debugging
    options.Events = new JwtBearerEvents
    {
   OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
  logger.LogError("Authentication failed: {Error}", context.Exception.Message);
        
  if (context.Exception is SecurityTokenExpiredException)
          {
          context.Response.Headers.Add("Token-Expired", "true");
   logger.LogError("Token expired at {ExpiredAt}", context.Exception.Message);
 }
            else if (context.Exception is SecurityTokenInvalidSignatureException)
  {
         logger.LogError("Invalid token signature");
  }
          else if (context.Exception is SecurityTokenInvalidIssuerException)
     {
         logger.LogError("Invalid token issuer. Expected: {Expected}", jwtSettings["Issuer"]);
          }
  else if (context.Exception is SecurityTokenInvalidAudienceException)
            {
        logger.LogError("Invalid token audience. Expected: {Expected}", jwtSettings["Audience"]);
     }
          
  return Task.CompletedTask;
},
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
      var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
       logger.LogInformation("Token validated successfully for user: {UserId}", userId);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
     logger.LogWarning("Authorization challenge: {Error}, {ErrorDescription}", 
      context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (!string.IsNullOrEmpty(token))
    {
            logger.LogInformation("JWT token received in request");
    }
   return Task.CompletedTask;
        }
    };
});

// Register JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Register Application Services
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IWorkflowInstanceService, WorkflowInstanceService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IWorkflowHistoryService, WorkflowHistoryService>();

// Register Storage Service (file upload path)
var uploadPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadPath); // Ensure directory exists
builder.Services.AddSingleton<IStorageService>(new LocalStorageService(uploadPath));

// Add Controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Workflow Management API", 
    Version = "v1",
    Description = "A comprehensive workflow management system with document approval and leave request workflows.\n\n" +
        "**Authentication:**\n" +
  "1. Register a new user via POST /api/auth/register\n" +
     "2. Login via POST /api/auth/login to get JWT token\n" +
     "3. Click 'Authorize' button and paste ONLY THE TOKEN (without 'Bearer')\n" +
      "4. All subsequent requests will include the token automatically\n\n" +
  "**Default Roles:** Admin, Manager, HR, Reviewer, Employee"
 });

    // Add JWT Authentication to Swagger
   c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
      Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
 BearerFormat = "JWT",
In = ParameterLocation.Header,
   Description = "JWT Authorization header using the Bearer scheme.\n\n" +
"Enter ONLY your token in the text input below.\n\n" +
               "Example: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\n\n" +
          "DO NOT include 'Bearer' - it will be added automatically!"
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

    // Enable file upload support in Swagger
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
Format = "binary"
    });

    // Enable XML comments if available
var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
  {
c.IncludeXmlComments(xmlPath);
 }
});

// Add CORS (if needed for frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workflow Management API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Serve static files (for uploaded documents)
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed roles on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedRoles(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles");
    }
}

app.Run();

// Helper method to seed roles
static async Task SeedRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    string[] roles = { "Admin", "Manager", "HR", "Reviewer", "Employee" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
