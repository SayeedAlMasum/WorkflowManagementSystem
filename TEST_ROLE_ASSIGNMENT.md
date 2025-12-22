# Role Assignment Testing Guide

## Problem Description
You registered with email: `swagger@example.com` and manually assigned Admin role in database. But when trying to assign role to users via API, you're getting errors.

## Solution Steps

### Step 1: Verify Database Roles
Run this SQL query in your database to check roles:

```sql
-- Check if roles exist
SELECT * FROM AspNetRoles;

-- Expected output (5 roles):
-- Admin, Manager, HR, Reviewer, Employee
```

### Step 2: Check Your User and Their Roles
```sql
-- Find your user
SELECT Id, Email, UserName, EmailConfirmed FROM AspNetUsers WHERE Email = 'swagger@example.com';

-- Check your roles (use your UserId from above)
SELECT u.Email, r.Name as RoleName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.RoleId
WHERE u.Email = 'swagger@example.com';
```

### Step 3: Login and Get JWT Token
**API Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
"email": "swagger@example.com",
  "password": "Swagger123456789"
}
```

**Response (save the token):**
```json
{
  "success": true,
  "message": "Login successful",
  "userId": "1435458b-eb9c-4edd-87c6-88e31bce40c7",
  "email": "swagger@example.com",
  "token": "eyJhbGc...", 
  "tokenExpiry": "2024-12-14T10:30:00Z",
  "roles": ["Admin", "Employee"]
}
```

### Step 4: Authorize in Swagger
1. Click the **Authorize** button (lock icon) at the top right in Swagger
2. Enter: `Bearer {your-token}` (replace {your-token} with actual token)
3. Click **Authorize**
4. Click **Close**

### Step 5: Test Auth (Verify Token Works)
**API Endpoint:** `GET /api/auth/test-auth`

This should return your user info and confirm you're authenticated.

### Step 6: Get All Users
**API Endpoint:** `GET /api/auth/users`

This will list all users with their roles.

### Step 7: Assign Role to Another User
**API Endpoint:** `POST /api/auth/assign-role`

**Request Body:**
```json
{
  "userId": "TARGET_USER_ID_HERE",
  "role": "Admin"
}
```

**Expected Success Response:**
```json
{
  "success": true,
  "message": "Role 'Admin' assigned successfully to user user@example.com",
  "userId": "user-id",
  "email": "user@example.com",
  "roles": ["Employee", "Admin"]
}
```

## Common Errors and Fixes

### Error 1: "401 Unauthorized"
**Cause:** JWT token is missing or invalid

**Fix:**
1. Make sure you clicked "Authorize" in Swagger
2. Token format should be: `Bearer eyJhbGc...` (with "Bearer " prefix)
3. Token might have expired - login again to get new token

### Error 2: "403 Forbidden"
**Cause:** Your user doesn't have Admin role

**Fix:** Run this SQL to add Admin role:
```sql
-- First, get role and user IDs
DECLARE @AdminRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin');
DECLARE @UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'swagger@example.com');

-- Add Admin role to user (if not exists)
IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @AdminRoleId)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @AdminRoleId);
    PRINT 'Admin role added successfully';
END
ELSE
BEGIN
    PRINT 'User already has Admin role';
END
```

### Error 3: "Role 'Admin' does not exist"
**Cause:** Role not seeded in database

**Fix:** Restart the API - roles are seeded on startup in `Program.cs`

Or manually add:
```sql
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES 
    (NEWID(), 'Admin', 'ADMIN', NEWID()),
    (NEWID(), 'Manager', 'MANAGER', NEWID()),
 (NEWID(), 'HR', 'HR', NEWID()),
    (NEWID(), 'Reviewer', 'REVIEWER', NEWID()),
    (NEWID(), 'Employee', 'EMPLOYEE', NEWID());
```

### Error 4: "User with ID 'xxx' not found"
**Cause:** Invalid userId provided

**Fix:** 
- Use `GET /api/auth/users` to get correct user IDs
- Make sure the userId is the GUID from AspNetUsers table

## Testing Workflow

### 1. Register New Test User
**POST /api/auth/register**
```json
{
  "email": "testuser@example.com",
  "password": "Test123456",
  "confirmPassword": "Test123456",
  "fullName": "Test User"
}
```

### 2. Login as Admin
**POST /api/auth/login**
```json
{
  "email": "swagger@example.com",
  "password": "Swagger123456789"
}
```

### 3. Authorize with Token
Click Authorize ? Enter: `Bearer {token}`

### 4. Get New User's ID
**GET /api/auth/users**

Find the testuser@example.com and copy their `id`

### 5. Assign Manager Role
**POST /api/auth/assign-role**
```json
{
  "userId": "PASTE_USER_ID_HERE",
  "role": "Manager"
}
```

### 6. Verify Assignment
**GET /api/auth/users/{userId}**

Should show roles: ["Employee", "Manager"]

## Database Verification

After assigning role, verify in database:
```sql
-- Check role assignment
SELECT 
    u.Email,
    u.UserName,
    r.Name as RoleName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.RoleId
WHERE u.Email = 'testuser@example.com';
```

## Troubleshooting Tips

1. **Always check API logs** - they show detailed error messages
2. **Verify token expiry** - default is 60 minutes, login again if expired
3. **Check database directly** - sometimes easier to spot issues
4. **Test with Swagger first** - before trying with Angular frontend
5. **Clear browser cache** - old tokens might be cached

## Available Roles
- Admin (full access)
- Manager (approves documents, leave requests)
- HR (approves leave requests)
- Reviewer (reviews documents)
- Employee (default role, submits requests)

## Quick SQL Scripts

### Remove all role assignments from a user
```sql
DELETE FROM AspNetUserRoles 
WHERE UserId = 'USER_ID_HERE';
```

### Add multiple roles at once
```sql
DECLARE @UserId NVARCHAR(450) = 'USER_ID_HERE';
DECLARE @AdminRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin');
DECLARE @ManagerRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Manager');

INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @AdminRoleId);
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @ManagerRoleId);
```

### List all users with their roles
```sql
SELECT 
    u.Email,
  u.FullName,
    STRING_AGG(r.Name, ', ') as Roles
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.RoleId
GROUP BY u.Email, u.FullName
ORDER BY u.Email;
```

---

## Expected Flow Summary

1. ? Register user ? Gets "Employee" role automatically
2. ? Login as Admin user ? Get JWT token
3. ? Authorize in Swagger with token
4. ? Call `/api/auth/assign-role` ? Assign additional roles
5. ? Verify with `/api/auth/users` or database

**Note:** The issue you faced was likely because:
- Token wasn't properly authorized in Swagger
- Or Admin role wasn't properly assigned to your user
- Or the token had expired

Follow the steps above and it should work! ??
