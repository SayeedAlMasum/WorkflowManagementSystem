# JWT Authentication for Angular Frontend

## ? What Changed: Cookie ? JWT Token

### Before (Cookie-Based):
```
? Not ideal for Angular
? CORS issues across domains
? Doesn't work with mobile apps
? Session-based (stateful)
```

### Now (JWT Token-Based):
```
? Perfect for Angular/React/Vue
? CORS-friendly
? Works with mobile apps
? Stateless and scalable
? Industry standard
```

---

## How JWT Authentication Works

```
???????????????????????????????
?   Angular   ?     ?  .NET API    ?
?   Frontend  ?  ?  Backend   ?
???????????????     ????????????????
       ?              ?
       ?  1. POST /api/auth/login   ?
       ???????????????????????????????>?
     ?     {email, password}          ?
       ?  ?
       ?  2. Return JWT Token         ?
       ?<???????????????????????????????
       ?  {token: "eyJhbG...", ...}     ?
       ?     ?
       ?  Store token in localStorage   ?
       ?      ?
       ?  3. GET /api/workflows         ?
       ?  Header: Authorization:        ?
       ?  Bearer eyJhbG...         ?
       ???????????????????????????????>?
    ?       ?
       ?  4. Validate token & return    ?
  ?<???????????????????????????????
       ?  { workflows: [...] }     ?
```

---

## API Endpoints (Updated)

### 1. Register

**POST** `/api/auth/register`

**Request:**
```json
{
  "email": "user@example.com",
  "password": "Password123",
  "confirmPassword": "Password123",
  "fullName": "John Doe"
}
```

**Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "userId": "guid-here",
  "email": "user@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenExpiry": "2025-01-20T15:30:00Z",
  "roles": ["Employee"]
}
```

### 2. Login

**POST** `/api/auth/login`

**Request:**
```json
{
  "email": "user@example.com",
  "password": "Password123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "userId": "guid-here",
  "email": "user@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenExpiry": "2025-01-20T15:30:00Z",
  "roles": ["Employee", "Manager"]
}
```

### 3. Using Protected Endpoints

**All protected endpoints require the JWT token in the Authorization header:**

```http
GET /api/workflows
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Testing in Swagger

### Step 1: Login
1. Go to `https://localhost:5001/swagger`
2. Expand **POST /api/auth/login**
3. Click "Try it out"
4. Enter your credentials
5. Click "Execute"
6. **Copy the token** from the response

### Step 2: Authorize
1. Click the **"Authorize"** button at the top (?? icon)
2. Enter: `Bearer your-token-here`
3. Click "Authorize"
4. Click "Close"

### Step 3: Test Protected Endpoints
- Now all requests will automatically include your token!
- Try GET `/api/workflows`, GET `/api/documents`, etc.

---

## Angular Implementation

### 1. Create Auth Service

```typescript
// src/app/services/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  userId?: string;
  email?: string;
  token?: string;
  tokenExpiry?: string;
  roles?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:5001/api/auth';
  private tokenKey = 'jwt_token';
  private currentUserSubject = new BehaviorSubject<AuthResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    // Load user from localStorage on init
const token = this.getToken();
  if (token) {
      this.loadCurrentUser();
    }
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => {
          if (response.success && response.token) {
            localStorage.setItem(this.tokenKey, response.token);
            this.currentUserSubject.next(response);
          }
        })
      );
  }

  register(data: any): Observable<AuthResponse> {
  return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data)
      .pipe(
      tap(response => {
          if (response.success && response.token) {
            localStorage.setItem(this.tokenKey, response.token);
            this.currentUserSubject.next(response);
          }
        })
      );
}

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    
    // Check if token is expired
    const expiry = this.getTokenExpiry(token);
    return expiry > new Date();
  }

  private getTokenExpiry(token: string): Date {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return new Date(payload.exp * 1000);
  }

  private loadCurrentUser(): void {
    this.http.get<any>(`${this.apiUrl}/me`).subscribe(
      user => this.currentUserSubject.next(user),
      () => this.logout()
    );
  }
}
```

### 2. Create HTTP Interceptor

```typescript
// src/app/interceptors/auth.interceptor.ts
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();
    
    if (token) {
      req = req.clone({
     setHeaders: {
 Authorization: `Bearer ${token}`
     }
      });
    }
    
    return next.handle(req);
  }
}
```

### 3. Register Interceptor in app.config.ts

```typescript
// src/app/app.config.ts
import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AuthInterceptor } from './interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withInterceptors([AuthInterceptor])
    ),
    // ... other providers
  ]
};
```

### 4. Create Auth Guard

```typescript
// src/app/guards/auth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): boolean {
    if (this.authService.isLoggedIn()) {
      return true;
    }
    
    this.router.navigate(['/login']);
    return false;
  }
}
```

### 5. Login Component Example

```typescript
// src/app/components/login/login.component.ts
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit() {
 this.authService.login({ email: this.email, password: this.password })
      .subscribe({
        next: (response) => {
      if (response.success) {
            this.router.navigate(['/dashboard']);
 }
  },
        error: (err) => {
  this.error = 'Invalid email or password';
        }
   });
  }
}
```

```html
<!-- login.component.html -->
<form (ngSubmit)="onSubmit()">
  <div>
    <label>Email:</label>
    <input type="email" [(ngModel)]="email" name="email" required>
  </div>
  <div>
    <label>Password:</label>
    <input type="password" [(ngModel)]="password" name="password" required>
  </div>
  <div *ngIf="error" class="error">{{ error }}</div>
  <button type="submit">Login</button>
</form>
```

### 6. Protected Route Example

```typescript
// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { 
    path: 'dashboard', 
    component: DashboardComponent,
    canActivate: [AuthGuard]  // Protected route
  },
  { 
    path: 'workflows', 
    component: WorkflowsComponent,
    canActivate: [AuthGuard]  // Protected route
  }
];
```

---

## JWT Token Structure

Your JWT token contains:

```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "jti": "unique-token-id",
  "nameid": "user-id",
  "name": "user@example.com",
  "given_name": "John Doe",
"role": ["Employee", "Manager"],
  "exp": 1705761000,
  "iss": "WorkflowManagementSystem",
  "aud": "WorkflowManagementSystemUsers"
}
```

---

## Security Configuration

### Current Settings (`appsettings.json`):

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

### ?? Production Recommendations:

1. **Change the JWT Key** to a truly random, secure key
2. **Use Environment Variables** instead of hardcoding
3. **Enable HTTPS** (set `RequireHttpsMetadata = true`)
4. **Implement Refresh Tokens** for better UX
5. **Add Token Revocation** (blacklist)

---

## Comparison: JWT vs Cookie

| Feature | JWT Token | Cookie |
|---------|-----------|--------|
| **For Angular** | ? Perfect | ? Problematic |
| **CORS** | ? No issues | ? CORS restrictions |
| **Mobile Apps** | ? Works | ? Doesn't work |
| **Scalability** | ? Stateless | ? Stateful |
| **Storage** | localStorage | Browser cookie |
| **Security** | Token in headers | httpOnly cookie |
| **Industry Standard for SPAs** | ? Yes | ? No |

---

## What You Need to Know

1. **JWT is the industry standard** for SPAs like Angular
2. **Token stored in localStorage** (Angular handles this)
3. **Automatically sent in headers** (via HTTP interceptor)
4. **Expires after 60 minutes** (configurable)
5. **Includes user roles** (for authorization)

---

## Next Steps for Your Angular App

1. ? Test JWT endpoints in Swagger first
2. ? Create AuthService in Angular
3. ? Create HTTP Interceptor for tokens
4. ? Implement login/register components
5. ? Add AuthGuard for protected routes
6. ? Build your workflow UI components

---

## Summary

**You now have BOTH authentication systems:**
- ? **JWT Tokens** - For Angular frontend (RECOMMENDED)
- ? **ASP.NET Core Identity** - For user management
- ? **Role-based Authorization** - Admin, Manager, HR, etc.
- ? **Industry-standard implementation** - Used by Google, Microsoft, Amazon

**This is the CORRECT choice for Angular!** ??
