# Quick Start Guide - Authentication API

## Step-by-Step Testing Guide

### 1. Start the Application

```bash
cd Workflow.Api
dotnet run
```

The application will start at `https://localhost:5001` (or the port shown in console).

### 2. Open Swagger UI

Navigate to: `https://localhost:5001/swagger`

### 3. Register Your First User

1. Find and expand **POST /api/auth/register**
2. Click **"Try it out"**
3. Enter the following JSON:

```json
{
  "email": "test@example.com",
  "password": "Test123",
  "confirmPassword": "Test123",
  "fullName": "Test User"
}
```

4. Click **"Execute"**
5. You should see a 200 response with success message

### 4. Login

1. Find and expand **POST /api/auth/login**
2. Click **"Try it out"**
3. Enter:

```json
{
  "email": "test@example.com",
  "password": "Test123"
}
```

4. Click **"Execute"**
5. You should see a 200 response with your user info
6. **Important:** The session cookie is now stored in your browser!

### 5. Test Protected Endpoint

1. Find and expand **GET /api/auth/me**
2. Click **"Try it out"**
3. Click **"Execute"**
4. You should see your user information (because you're logged in)

### 6. Test Other APIs

Now you can test all other endpoints that require authentication:

- **GET /api/documents** - List documents
- **POST /api/documents/upload** - Upload a document
- **GET /api/workflows** - List workflows
- **POST /api/workflows** - Create a workflow
- etc.

All these endpoints will work because you're authenticated!

### 7. Create an Admin User (Important!)

To test admin-only endpoints (like assigning roles), you need an admin user.

**Option A: Direct Database Update**

1. Open SQL Server Management Studio
2. Connect to your database
3. Run these queries:

```sql
-- Find your user
SELECT Id, Email FROM AspNetUsers WHERE Email = 'test@example.com';
-- Copy the Id

-- Find Admin role
SELECT Id, Name FROM AspNetRoles WHERE Name = 'Admin';
-- Copy the Id

-- Assign Admin role (replace the GUIDs with the ones from above)
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('your-user-id-here', 'your-admin-role-id-here');
```

4. Logout and login again in Swagger
5. Now you can test admin endpoints like **POST /api/auth/assign-role**

**Option B: Automatic Admin Creation (Recommended)**

Add this to your `Program.cs` after the `SeedRoles` method:

```csharp
static async Task SeedRoles(IServiceProvider serviceProvider)
{
  var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
    
    // Existing role seeding code...
    string[] roles = { "Admin", "Manager", "HR", "Reviewer", "Employee" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
 await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    
    // Add this: Create default admin user
 var adminEmail = "admin@workflowsystem.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
      FullName = "System Administrator",
            EmailConfirmed = true
        };
 
      await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
```

Then restart the application and login with:
- Email: `admin@workflowsystem.com`
- Password: `Admin@123`

### 8. Test Admin Endpoints

Once you're logged in as admin:

1. **GET /api/auth/users** - See all users
2. **GET /api/auth/roles** - See all roles
3. **POST /api/auth/assign-role** - Assign roles to users

Example: Assign Manager role to a user:
```json
{
  "userId": "user-guid-from-users-list",
  "role": "Manager"
}
```

### 9. Test Role-Based Access

1. Create another user (as Employee)
2. Login as that user
3. Try to access **GET /api/auth/users** (should get 403 Forbidden)
4. Login as admin
5. Access **GET /api/auth/users** (should work)

## Common Issues

### Issue: "User not authenticated" error
**Solution:** Make sure you're logged in. Run POST /api/auth/login first.

### Issue: Session not persisting in Swagger
**Solution:** Swagger automatically handles cookies. After login, just continue using the same browser tab.

### Issue: 403 Forbidden on admin endpoints
**Solution:** Your user doesn't have admin role. Follow step 7 to create/promote an admin user.

### Issue: Can't test from Postman
**Solution:** In Postman, enable "Automatically follow redirects" and "Send cookies with requests" in Settings.

## Testing Checklist

- [ ] Register a new user
- [ ] Login successfully
- [ ] View current user info (GET /api/auth/me)
- [ ] Change password
- [ ] Logout
- [ ] Create admin user
- [ ] Login as admin
- [ ] View all users
- [ ] View all roles
- [ ] Assign role to a user
- [ ] Test document upload with authentication
- [ ] Test workflow creation with authentication
- [ ] Test role-based authorization

## What's Different from Razor Pages UI?

**Before (Razor Pages):**
- Had to navigate to web pages
- UI-based testing
- Difficult to automate

**Now (API):**
- Pure REST API endpoints
- Test directly in Swagger
- Easy to automate with scripts
- Can integrate with any frontend (React, Angular, Vue, mobile apps)
- Better for API testing and development

## Next Steps

1. ? Test authentication API endpoints
2. ? Create admin user
3. ? Test role-based access
4. Test your existing workflow APIs with authentication
5. Test document management APIs
6. Test leave request APIs
7. Consider adding JWT tokens if needed for mobile/SPA applications

## Need Help?

- Check `AUTH_API_README.md` for detailed API documentation
- All authentication logic is in `Workflow.Api/Controllers/AuthController.cs`
- DTOs are in `Workflow.Api/DTOs/AuthDtos.cs`

Happy Testing! ??
