---
name: Authentication System
overview: "Thêm hệ thống đăng nhập, user management và login history với 4 role: SAdmin, Administrator, Operator, Viewer"
todos:
  - id: auth-db
    content: Tạo AuthDb.cs - database schema và seed data
    status: completed
  - id: auth-service
    content: Tạo AuthService.cs - login/logout logic
    status: completed
  - id: auth-middleware
    content: Tạo SessionMiddleware + Authorize attribute
    status: completed
  - id: auth-endpoints
    content: Thêm auth API endpoints vào GProjectApiServer
    status: completed
  - id: auth-init
    content: Update Program.cs init AuthDb
    status: in_progress
  - id: fe-auth-context
    content: Tạo AuthContext + LoginScreen (React)
    status: pending
  - id: fe-auth-api
    content: Tạo authApi.ts service (React)
    status: pending
  - id: fe-app-auth
    content: Update App.tsx với auth flow
    status: pending
  - id: build-test
    content: Build + test
    status: pending
isProject: false
---

## Kiến trúc

```
┌─────────────────────────────────────────────────────────────┐
│                     React Frontend                          │
│  LoginScreen -> App (with auth context)                     │
└─────────────────────────┬───────────────────────────────────┘
                          │ Session Cookie (HttpOnly)
┌─────────────────────────▼───────────────────────────────────┐
│                     GProject Backend                         │
│  /api/auth/login  /api/auth/logout  /api/auth/me           │
│  /api/auth/users  (SAdmin only)                             │
│  Session middleware -> [Authorize] attribute                │
│  SQLite: C:\GProject\Auth\gauth.db                         │
└─────────────────────────────────────────────────────────────┘
```

## 1. Database Schema - SQLite Auth (`C:\GProject\Auth\gauth.db`)

```sql
-- Users table
CREATE TABLE Users (
    Id TEXT PRIMARY KEY,
    Username TEXT UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,  -- BCrypt
    DisplayName TEXT,
    Role TEXT NOT NULL,  -- 'SAdmin', 'Administrator', 'Operator', 'Viewer'
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT,
    CreatedBy TEXT
);

-- Login History
CREATE TABLE LoginHistory (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    Username TEXT NOT NULL,
    LoginTime TEXT NOT NULL,
    LogoutTime TEXT,
    IpAddress TEXT,
    UserAgent TEXT,
    Success INTEGER DEFAULT 1,
    FailureReason TEXT,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

## 2. Backend Files to Create

### `GProject/Auth/AuthDb.cs` - Database helper

```csharp
public static class AuthDb
{
    private static readonly string DbPath = @"C:\GProject\Auth\gauth.db";
    
    public static void EnsureCreated()
    {
        // Create directory and tables if not exists
        // Seed default SAdmin: admin / Admin@123
    }
}
```

### `GProject/Auth/AuthService.cs` - Business logic

- `bool ValidateLogin(username, password)` - check credentials
- `User? GetUser(id)` - get user info
- `List<User> GetAllUsers()` - list all users (SAdmin only)
- `void CreateUser/UserUpdate/Delete` - CRUD operations

### `GProject/Auth/SessionMiddleware.cs` - Session handling

- Check session cookie on each request
- Load user context
- Validate session expiry (configurable, default 8 hours)

### `GProject/Auth/AuthorizeAttribute.cs` - Authorization

- `[Authorize(Roles = "Administrator, SAdmin")]` attribute
- Check user role against endpoint requirements

## 3. API Endpoints (in GProjectApiServer.cs)

### Authentication


| Method | Path                   | Description      | Auth     |
| ------ | ---------------------- | ---------------- | -------- |
| POST   | `/api/auth/login`      | Login            | None     |
| POST   | `/api/auth/logout`     | Logout           | Required |
| GET    | `/api/auth/me`         | Get current user | Required |
| GET    | `/api/auth/history`    | Login history    | Required |
| GET    | `/api/auth/users`      | List users       | SAdmin   |
| POST   | `/api/auth/users`      | Create user      | SAdmin   |
| PUT    | `/api/auth/users/{id}` | Update user      | SAdmin   |
| DELETE | `/api/auth/users/{id}` | Delete user      | SAdmin   |


### Role Permissions


| Endpoint           | SAdmin | Administrator | Operator | Viewer |
| ------------------ | ------ | ------------- | -------- | ------ |
| View pools/codes   | Yes    | Yes           | Yes      | Yes    |
| Add/Edit codes     | Yes    | Yes           | Yes      | No     |
| Import codes       | Yes    | Yes           | Yes      | No     |
| Manage pools       | Yes    | Yes           | No       | No     |
| Manage users       | Yes    | No            | No       | No     |
| View login history | Yes    | Yes           | No       | No     |


## 4. Frontend Files

### `src/contexts/AuthContext.tsx`

```tsx
interface AuthContextType {
  user: User | null;
  login: (username, password) => Promise<void>;
  logout: () => Promise<void>;
  isLoading: boolean;
}
```

### `src/components/LoginScreen.tsx`

- Username/password form
- Error display
- Remember last username

### `src/services/authApi.ts`

```ts
export const authApi = {
  login, logout, getMe, getUsers, createUser, updateUser, deleteUser, getHistory
}
```

### Update `src/App.tsx`

- Add AuthProvider
- Show LoginScreen if not authenticated
- Pass user context to components
- Role-based nav items visibility

## 5. Files to Modify


| File                            | Changes                                |
| ------------------------------- | -------------------------------------- |
| `GProject/GProject.csproj`      | Add BCrypt.Net-Next package            |
| `GProject/GProjectApiServer.cs` | Add auth endpoints, session middleware |
| `GProject/Program.cs`           | Initialize AuthDb                      |
| `src/services/datapoolApi.ts`   | Add auth interceptor, base URL         |
| `src/App.tsx`                   | Add auth flow                          |
| `src/types/`                    | Add User, LoginHistory types           |


## 6. Default Users (seeded on first run)


| Username | Password | Role          | Description                       |
| -------- | -------- | ------------- | --------------------------------- |
| s        | s        | SAdmin        | Super Admin - full access         |
| admin    | a        | Administrator | Admin - manage data, view history |
| operator | 12345    | Operator      | Can add/edit code                 |
| viewer   | 12345    | Viewer        | Read-only access                  |


## 7. Implementation Order

1. Create AuthDb + seed data
2. Create AuthService
3. Create session middleware + authorize
4. Add auth API endpoints
5. Add Serilog logging for auth events
6. Frontend: AuthContext + LoginScreen
7. Frontend: API service + interceptor
8. Frontend: App.tsx integration
9. Frontend: Role-based UI visibility

