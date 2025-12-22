# JWT Token 401 Error - Quick Checklist ?

## ??????
```
401 Unauthorized
www-authenticate: Bearer error="invalid_token"
```

## ????????? ?????? (5 ?????)

### ??? ?: API Restart ???? ??
```bash
# Terminal ? API stop ???? (Ctrl+C)
# ????? ???? run ????:
cd Workflow.Api
dotnet run
```

**????** ??? Program.cs ? enhanced logging add ????? - ??? apply ??? restart ??????

---

### ??? ?: Fresh Login ???? ??

**Swagger ? ???:** `POST /api/auth/login`

**Request:**
```json
{
  "email": "swagger@example.com",
  "password": "Swagger123456789"
}
```

**Response ???? token copy ????:**
```json
{
  "token": "eyJhbGc..." ? ??? copy ???? (??????)
}
```

---

### ??? ?: Token Verify ???? ??

1. **Go to:** https://jwt.io/
2. **Paste** ????? token
3. **Check ????:**
   - Algorithm: HS256 ?
   - Issuer: WorkflowManagementSystem ?
   - Audience: WorkflowManagementSystemUsers ?

4. **"VERIFY SIGNATURE" section ? paste ????:**
```
YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm
```

**"Signature Verified" ?????? token valid ?**

---

### ??? ?: Swagger ? Authorize ???? ??

1. Swagger UI ?? **"Authorize"** button (?? lock icon) click ????
2. **Value field ? type ????:**
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
   - ?? **"Bearer "** (capital B, ???? space) ?????? ?????
   - ?? Full token paste ????

3. **"Authorize"** click ????
4. **"Close"** click ????

---

### ??? ?: Test Authentication ??

**Endpoint:** `GET /api/auth/test-auth`

1. **Try it out** click ????
2. **Execute** click ????

**Expected Response (? Success):**
```json
{
  "authenticated": true,
  "userName": "swagger@example.com",
  "userId": "1435458b-eb9c-4edd-87c6-88e31bce40c7",
  "claims": [...]
}
```

**??? 401 error ??? (?):**
- Terminal/Console ? logs check ????
- Error message ?????:
  ```
  Authentication failed: {reason}
  Invalid token signature
  Invalid token issuer
  ```

---

### ??? ?: Assign Role ???? ??

**Endpoint:** `POST /api/auth/assign-role`

**Request:**
```json
{
  "userId": "TARGET_USER_ID_HERE",
  "role": "Admin"
}
```

**Success Response (?):**
```json
{
  "success": true,
  "message": "Role 'Admin' assigned successfully...",
  "roles": ["Employee", "Admin"]
}
```

---

## ??? ???? Error ???

### Check 1: Console Logs ????? ??

Terminal/Console ? ??? logs ????? ??????:

**Success:**
```
Token validated successfully for user: 1435458b-eb9c-4edd-87c6-88e31bce40c7
```

**Failure:**
```
Authentication failed: The token is expired
Authentication failed: The signature is invalid
Invalid token issuer. Expected: WorkflowManagementSystem
```

---

### Check 2: Token Format ??

**Correct (?):**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Wrong (?):**
```
Authorization: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9... (Bearer missing)
Authorization: bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...   (lowercase b)
Authorization: Bearer  eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...   (double space)
```

---

### Check 3: Token Expiry ??

**Login response ? check ????:**
```json
{
  "tokenExpiry": "2024-12-19T11:30:00Z"
}
```

**??? current time > tokenExpiry ???, ???? login ?????**

---

### Check 4: appsettings.json ??

**Verify ????:**
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm",
    "Issuer": "WorkflowManagementSystem",
    "Audience": "WorkflowManagementSystemUsers",
    "ExpiryInMinutes": 60
  }
}
```

**Common mistakes:**
- Key too short (< 32 characters) ?
- Trailing spaces ?
- Wrong Issuer/Audience spelling ?

---

## Visual Guide

```
1. Stop API (Ctrl+C)
   ?
2. Start API (dotnet run)
   ?
3. Login (POST /api/auth/login)
   ?
4. Copy Token
   ?
5. Verify on jwt.io (optional but recommended)
   ?
6. Authorize in Swagger (?? button)
   ?
7. Test Auth (GET /api/auth/test-auth)
   ?
   ?? Success (200 OK) ? Proceed to Step 8
   ?? Failure (401) ? Check logs, retry from Step 3
   ?
8. Assign Role (POST /api/auth/assign-role)
```

---

## Database Check (Optional)

??? ?? ??? ?????? role assign ?? ???:

```sql
-- Check your roles
SELECT * FROM AspNetRoles;

-- Check your user's current roles
SELECT 
    u.Email,
    r.Name as RoleName
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.RoleId
WHERE u.Email = 'swagger@example.com';
```

---

## Postman Alternative

??? Swagger ? issue ????:

**Postman Setup:**
1. **Method:** POST
2. **URL:** `https://localhost:7032/api/auth/assign-role`
3. **Headers:**
   - `Authorization: Bearer YOUR_TOKEN`
   - `Content-Type: application/json`
4. **Body (raw JSON):**
   ```json
   {
     "userId": "1435458b-eb9c-4edd-87c6-88e31bce40c7",
     "role": "Admin"
   }
   ```

---

## Expected Timeline

| Step | Time |
|------|------|
| 1. Restart API | 30 seconds |
| 2. Login | 10 seconds |
| 3. Verify Token (jwt.io) | 1 minute |
| 4. Authorize Swagger | 10 seconds |
| 5. Test Auth | 10 seconds |
| 6. Assign Role | 10 seconds |
| **Total** | **~3 minutes** |

---

## Final Checklist

Before you start:

- ? API is running (check terminal: "Now listening on: https://localhost:7032")
- ? Database is accessible (roles are seeded)
- ? No compilation errors
- ? Swagger UI loads successfully

During testing:

- ? Login returns token
- ? Token verified on jwt.io (signature valid)
- ? Authorized in Swagger (lock icon becomes closed ??)
- ? test-auth returns 200 OK
- ? Console shows "Token validated successfully"

If assign-role works:

- ? Returns 200 OK
- ? Response shows updated roles array
- ? Database updated (check AspNetUserRoles table)

---

## ???? ????

```bash
# 1. Stop API
Ctrl+C in terminal

# 2. Start API
cd Workflow.Api
dotnet run

# Wait for: "Now listening on: https://localhost:7032"

# 3. Open Swagger
Browser: https://localhost:7032/swagger

# 4. Login ? Copy Token ? Authorize ? Test ? Assign Role
```

---

**Status:**  
? Enhanced logging added  
? Build successful  
? Waiting for API restart and testing

**Next:** Restart API and follow checklist above ??

---

## ??? ?????? ???

Screenshot ?????:
1. Console/Terminal logs
2. Swagger authorize dialog (with token - hide sensitive parts)
3. Error response
4. JWT.io verification result

Exact problem identify ???? ?????
