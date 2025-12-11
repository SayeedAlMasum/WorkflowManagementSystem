using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

// Add AutoMapper - AddAuto Mapper from assemblies containing profiles
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

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Workflow Management API", Version = "v1" });
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
    app.UseSwaggerUI();
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
