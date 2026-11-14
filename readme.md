# Course Management API

A backend REST API built using **ASP.NET Core 10**, **Entity Framework Core**, and **MySQL**. The project follows a layered architecture to separate HTTP handling, business logic, and database operations.

## Tech Stack

- **Framework:** ASP.NET Core 10 Web API
- **Language:** C#
- **ORM:** Entity Framework Core
- **Database:** MySQL 8
- **Database Containerization:** Docker
- **Authentication:** JWT (Access & Refresh Tokens)
- **Password Security:** BCrypt
- **Email Security:** HMAC-SHA256 and AES-256-GCM

## Architecture

The application follows a layered architecture:

```text
Controllers
    ↓
Services
    ↓
Repositories
    ↓
DbContext
    ↓
MySQL
```

- **Controllers:** Handle HTTP requests and responses.
- **Services:** Contain business logic and validation.
- **Repositories:** Handle database operations.
- **DbContext:** Manages database access through Entity Framework Core.
- **DTOs:** Define request and response data structures.

## Features

### Course Management
- Create, retrieve, update, and delete courses.
- Unique course title validation.
- Input validation and appropriate HTTP status codes.

### User Management
- User registration and login.
- Retrieve user profiles and user lists.
- Update user profiles.
- Delete users.
- Role-based access control (Student, Teacher, Admin).

### Authentication & Security
- JWT-based authentication using access and refresh tokens.
- Tokens stored in HTTP-only cookies.
- Password hashing using BCrypt.
- Email lookup using HMAC-SHA256.
- Email encryption using AES-256-GCM.
- Sensitive configuration stored in environment variables.

## Prerequisites

Make sure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Git
- Postman or another API testing tool

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd CourseManagement
```

### 2. Configure environment variables

Create a `.env` file in the project root (next to the `.csproj` file).

Use `.env.example` as a reference and provide your own values.

Required environment variables:

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | MySQL connection string |
| `HMAC_EMAIL_SECRET` | Base64-encoded HMAC secret |
| `EMAIL_ENCRYPTION_KEY` | Base64-encoded AES encryption key |
| `JWT_ACCESS_SECRET` | Base64-encoded JWT access-token secret |
| `JWT_REFRESH_SECRET` | Base64-encoded JWT refresh-token secret |

Generate separate random secrets for each key. Do not reuse keys or commit your `.env` file.

### 3. Start MySQL using Docker

Start Docker Desktop, then run:

```bash
docker compose up -d
```

Make sure the connection string in `.env` matches your Docker MySQL configuration.

### 4. Restore dependencies

```bash
dotnet restore
```

### 5. Apply database migrations

```bash
dotnet ef database update
```

If the EF Core CLI is not installed:

```bash
dotnet tool install --global dotnet-ef
```

### 6. Run the application

```bash
dotnet run
```

Alternatively, use hot reload during development:

```bash
dotnet watch
```

## API Endpoints

The API uses the following route structure:

### Courses

| Method | Endpoint | Description |
|---|---|---|
| GET | `/Courses` | Get all courses |
| GET | `/Courses/{id}` | Get a course by ID |
| POST | `/Courses` | Create a course |
| PATCH | `/Courses/{id}` | Update a course |
| DELETE | `/Courses/{id}` | Delete a course |

### Users

| Method | Endpoint | Description |
|---|---|---|
| POST | `/User` | Register a user |
| POST | `/User/login` | Login |
| POST | `/User/refresh` | Refresh authentication tokens |
| POST | `/User/logout` | Logout |
| GET | `/User` | Get all users (Admin) |
| GET | `/User/{id}` | Get a user by ID |
| PATCH | `/User/{id}` | Update a user |
| DELETE | `/User/{id}` | Delete a user (Admin) |

**Note:** Protected endpoints require valid authentication cookies and appropriate permissions. Registration currently allows the client to select a role for development/testing; this should be restricted before production deployment.

## Database Management

The project uses MySQL running in Docker.

If Adminer is configured in `docker-compose.yml`, it can be accessed at:

```text
http://localhost:8080
```

Use the MySQL service name as the server when connecting from Adminer inside Docker Compose.


## Project Status

The project is under active development. Additional features and security improvements may be added.