# Background
This project is from this course: https://www.coursera.org/learn/full-stack-developer-capstone-project/home/welcome

# SkillSnap

SkillSnap is a full-stack .NET portfolio application built with a hosted Blazor WebAssembly frontend, an ASP.NET Core API backend, and a shared contract library. It allows portfolio data such as users, projects, and skills to be stored in SQLite and exposed through API endpoints, with JWT-based authentication and role-protected write operations.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Blazor WebAssembly
- Entity Framework Core
- SQLite
- ASP.NET Core Identity
- JWT authentication

## Solution Structure

- `SkillSnap.Api`
  - Backend API
  - Entity Framework Core data access
  - Identity and JWT authentication
  - Controllers for auth, projects, skills, seed data, and data retrieval
- `SkillSnap.Client`
  - Blazor WebAssembly frontend
  - UI components and pages
  - Client services for API access
- `SkillSnap.Shared`
  - Shared DTOs and contracts used by both client and server

## Current Features

- User registration and login with JWT token generation
- ASP.NET Core Identity integration
- Admin role seeding on startup
- Protected POST endpoints for project and skill creation
- Portfolio user, project, and skill persistence with SQLite
- Seed endpoint for sample portfolio data
- In-memory caching for project and skill read endpoints
- Request-duration logging middleware
- Hosted Blazor client served by the API project

## Security

The project currently includes:

- ASP.NET Core Identity for user management and password handling
- JWT bearer authentication
- Role-based authorization using the `Admin` role
- Automatic admin role and admin user seeding at startup
- HTTPS redirection and HSTS outside development
- CORS restricted to the local client origin

## Performance Improvements

The project applies a few practical performance optimizations:

- In-memory caching for `GET /api/projects` and `GET /api/skills`
- Per-item caching for `GET /api/projects/{id}` and `GET /api/skills/{id}`
- Fallback cache responses when database reads fail
- `AsNoTracking()` on read-only EF Core queries
- Request timing logs for endpoint performance visibility
- Framework-provided static asset compression and fingerprinting

## Prerequisites

- .NET 10 SDK
- SQLite support through EF Core
- A development JWT configuration

## Getting Started

1. Clone the repository.
2. Open the `SkillSnap` solution.
3. Restore packages:

```bash
dotnet restore
```

4. Apply the database migrations:
```
dotnet ef database update --project SkillSnap.Api
```
Add JWT settings to appsettings.Development.json or user secrets.
Example development configuration:
```json
{
  "Jwt": {
    "Issuer": "SkillSnap.Api",
    "Audience": "SkillSnap.Client",
    "Key": "replace-with-a-long-development-secret-key",
    "ExpiryMinutes": "60"
  }
}
```

6. Run the API project:
```
dotnet run --project SkillSnap.Api
```
## Local URLs
By default the API runs at:

https://localhost:7127
http://localhost:5268

## Default Admin User
The application seeds an admin user at startup if it does not already exist.

- Email: admin@skillsnap.local
- Password: ChangeThisDevPassword123!
- This is intended for development only.

## API Endpoints
### Authentication
POST /api/auth/register
POST /api/auth/login
### Portfolio Data
GET /api/get
POST /api/seed
### Projects
GET /api/projects
GET /api/projects/{id}
POST /api/projects Admin only
### Skills
GET /api/skills
GET /api/skills/{id}
POST /api/skills Admin only
