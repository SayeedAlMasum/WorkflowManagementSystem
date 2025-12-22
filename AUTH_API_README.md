# Authentication API Documentation

## Overview

The Authentication API provides complete user management and authentication functionality through RESTful endpoints. This replaces the need for Razor Pages UI and allows you to test all APIs programmatically.

## Base URL

```
https://localhost:<port>/api/auth
```

## Endpoints

### 1. Register a New User

**POST** `/api/auth/register`

Register a new user account. Newly registered users are automatically assigned the "Employee" role.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123",
  "confirmPassword": "Password123",
  "fullName": "John Doe"
}
```

**Response (Success - 200):**
```json
{
  "success": true,
"message": "User registered successfully",
  "userId": "user-id-guid",
  "email": "user@example.com",
  "roles": ["Employee"]
}
```

**Response (Error - 400):**
```json
{
  "success": false,
  "message": "User with this email already exists",
  "errors": null
}
```

---

### 2. Login

**POST** `/api/auth/login`

Authenticate a user and create a session cookie.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123"
}
```

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Login successful",
  "userId": "user-id-guid",
  "email": "user@example.com",
  "roles": ["Employee", "Manager"]
}
```

**Response (Error - 401):**
```json
{
  "success": false,
  "message": "Invalid email or password"
}
```

**Note:** After successful login, a session cookie is automatically set. Include this cookie in subsequent API requests.

---

### 3. Logout

**POST** `/api/auth/logout`

**Authorization:** Required (Bearer token or session cookie)

Logout the current user and invalidate the session.

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Logout successful"
}
```

---

### 4. Get Current User

**GET** `/api/auth/me`

**Authorization:** Required

Get the currently authenticated user's information.

**Response (Success - 200):**
```json
{
  "userId": "user-id-guid",
  "email": "user@example.com",
  "fullName": "John Doe",
  "roles": ["Employee", "Manager"]
}
```

---

### 5. Change Password

**POST** `/api/auth/change-password`

**Authorization:** Required

Change the password for the currently authenticated user.

**Request Body:**
```json
{
  "currentPassword": "OldPassword123",
  "newPassword": "NewPassword123",
  "confirmPassword": "NewPassword123"
}
```

**Response (Success - 200):**
```json
{
  "success": true,
"message": "Password changed successfully"
}
```

**Response (Error - 400):**
```json
{
  "success": false,
  "message": "Failed to change password",
  "errors": ["Incorrect password."]
}
```

---

### 6. Get All Roles

**GET** `/api/auth/roles`

**Authorization:** Required (Admin role)

Get a list of all available roles in the system.

**Response (Success - 200):**
```json
[
  {
"id": "role-id-guid",
    "name": "Admin"
  },
  {
    "id": "role-id-guid",
    "name": "Manager"
},
  {
    "id": "role-id-guid",
    "name": "HR"
  },
  {
    "id": "role-id-guid",
    "name": "Reviewer"
  },
  {
    "id": "role-id-guid",
    "name": "Employee"
  }
]
```

---

### 7. Assign Role to User

**POST** `/api/auth/assign-role`

**Authorization:** Required (Admin role)

Assign a role to a specific user.

**Request Body:**
```json
{
  "userId": "user-id-guid",
  "role": "Manager"
}
```

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Role 'Manager' assigned successfully"
}
```

**Response (Error - 404):**
```json
{
  "success": false,
  "message": "User not found"
}
```

---

### 8. Get All Users

**GET** `/api/auth/users`

**Authorization:** Required (Admin role)

Get a list of all users with their roles.

**Response (Success - 200):**
```json
[
  {
    "id": "user-id-guid",
    "email": "admin@example.com",
  "fullName": "Admin User",
    "userName": "admin@example.com",
    "roles": ["Admin", "Manager"]
  },
  {
    "id": "user-id-guid",
    "email": "user@example.com",
    "fullName": "John Doe",
    "userName": "user@example.com",
    "roles": ["Employee"]
  }
]
```

---

## Testing with Swagger

1. **Navigate to Swagger UI:**
   ```
   https://localhost:<port>/swagger
   ```

2. **Register a new user:**
   - Expand `POST /api/auth/register`
   - Click "Try it out"
   - Fill in the request body
   - Click "Execute"

3. **Login:**
   - Expand `POST /api/auth/login`
   - Use the credentials from registration
   - Click "Execute"
   - The session cookie will be automatically stored

4. **Test Protected Endpoints:**
   - After logging in, you can now access protected endpoints
   - Try `GET /api/auth/me` to get your user info
   - Try other API endpoints like `/api/documents` or `/api/workflows`

---

## Testing with Postman/cURL

### Register
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123",
    "confirmPassword": "Test123",
    "fullName": "Test User"
  }'
```

### Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -c cookies.txt \
  -d '{
    "email": "test@example.com",
    "password": "Test123"
  }'
```

### Get Current User (with cookie)
```bash
curl -X GET https://localhost:5001/api/auth/me \
  -b cookies.txt
```

---

## Default Roles

The system automatically creates the following roles on startup:

- **Admin**: Full system access, can manage users and assign roles
- **Manager**: Can manage workflows and approve/reject tasks
- **HR**: Can handle leave requests and HR-related workflows
- **Reviewer**: Can review and comment on documents and tasks
- **Employee**: Basic user access, can create workflows and submit requests

---

## Creating an Admin User

Since new users are registered as "Employee" by default, you need to manually promote a user to Admin:

### Option 1: Direct Database Update

```sql
-- Find the user ID
SELECT Id, Email FROM AspNetUsers WHERE Email = 'admin@example.com';

-- Find the Admin role ID
SELECT Id, Name FROM AspNetRoles WHERE Name = 'Admin';

-- Assign Admin role to user
INSERT INTO AspNetUserRoles (UserId, RoleId) 
VALUES ('user-id-from-step-1', 'role-id-from-step-2');
```

### Option 2: Update Seed Method in Program.cs

You can modify the `SeedRoles` method to also create an admin user:

```csharp
static async Task SeedRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
    
    // Seed roles
    string[] roles = { "Admin", "Manager", "HR", "Reviewer", "Employee" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
      {
            await roleManager.CreateAsync(new IdentityRole(role));
  }
    }
    
    // Create admin user if doesn't exist
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

---

## Password Requirements

- Minimum 6 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- Special characters are optional

---

## Authentication Flow

1. **Register** ? User created with "Employee" role
2. **Login** ? Session cookie created
3. **Access APIs** ? Cookie automatically included in requests
4. **Logout** ? Session invalidated

---

## Error Handling

All endpoints return consistent error responses:

```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

HTTP Status Codes:
- `200 OK`: Success
- `400 Bad Request`: Invalid input
- `401 Unauthorized`: Not authenticated
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

---

## Security Notes

1. **Cookie-based Authentication**: The API uses ASP.NET Core Identity's cookie authentication
2. **HTTPS**: Always use HTTPS in production
3. **CORS**: Currently configured to allow all origins (update for production)
4. **Password Hashing**: Passwords are automatically hashed using Identity's default hasher
5. **Session Management**: Sessions are managed by ASP.NET Core Identity

---

## Next Steps

1. Test the authentication endpoints in Swagger
2. Register a user and login
3. Promote a user to Admin (using database update)
4. Test role-based endpoints
5. Test the existing workflow, document, and task APIs with authentication

---

## Files Created/Modified

### Created:
- `Workflow.Api/Controllers/AuthController.cs` - Main authentication controller
- `Workflow.Api/DTOs/AuthDtos.cs` - Data transfer objects for authentication
- `AUTH_API_README.md` - This documentation

### Modified:
- `Workflow.Api/Program.cs` - Removed Razor Pages, kept cookie authentication

### Removed:
- All files in `Areas/Identity` directory
- All files in `Pages` directory
- `Workflow.Api/Services/EmailSender.cs`
- `ScaffoldingReadMe.txt`
