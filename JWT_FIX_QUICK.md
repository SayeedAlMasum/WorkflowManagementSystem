# JWT Fix - ???? ???? (2 ?????)

## ??? ?: API Restart ??
```
Terminal ? Ctrl+C ? Stop
?????: dotnet run
```

## ??? ?: Test Token Generate ????
**Swagger ? ???:**
```
POST /api/auth/generate-test-token
```

**Body:**
```json
"swagger@example.com"
```

**Response check ????:**
```json
{
  "token": "eyJhbGc...",  ? ??? copy ????
  "tokenExpiry": "2024-12-19T20:00:00Z",  ? ????????? time ???? check ????
  "decodedToken": {
    "issuer": "WorkflowManagementSystem",  ? Match ???? ????
    "audience": "WorkflowManagementSystemUsers",  ? Match ???? ????
    "expiration": "...",
    "claims": [...]
  }
}
```

## ??? ?: Swagger ? Authorize
1. **Authorize** button click
2. **???? token paste ????** (Bearer ??)
3. **Authorize** ? **Close**

## ??? ?: Test ????
```
GET /api/auth/test-auth
```

**Expected:**
```json
{
  "authenticated": true,
  "userName": "swagger@example.com"
}
```

## ??? ?: Role Assign ????
```
POST /api/auth/assign-role
{
  "userId": "user-id",
  "role": "Admin"
}
```

---

## ??? ???? 401 ???:

### Console Logs ?????:
```
? Token validated successfully ? Working!
? Authentication failed ? Check below
```

### Debug Endpoint use ????:
```
GET /api/auth/debug-jwt
Authorization: Bearer YOUR_TOKEN
```

??? configuration ???????

---

## Common Mistakes:

| ? Wrong | ? Correct |
|----------|------------|
| Bearer Bearer token | ???? token |
| Expired token | Fresh token generate ???? |
| Old token (before restart) | Restart ???? ?? ???? token |
| Incomplete token | Full token copy ???? (200+ chars) |

---

## ???? ????:

```bash
# 1. Stop API (Ctrl+C)
# 2. Start API
dotnet run

# 3. Generate token
POST /api/auth/generate-test-token
Body: "swagger@example.com"

# 4. Copy token from response

# 5. Authorize in Swagger (paste token)

# 6. Test
GET /api/auth/test-auth

# 7. Assign role
POST /api/auth/assign-role
```

**Time:** 2 minutes  
**Success Rate:** 99% if followed exactly

---

**New Features Added:**
- ? `generate-test-token` - Token ?? details ??
- ? `debug-jwt` - Configuration verify ???? ????
- ? Enhanced logging - Exact error messages

**Action:** API restart ???? ????!
