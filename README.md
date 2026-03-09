# User Authentication API with Email OTP

A secure **ASP.NET Core Web API** implementing **email-based OTP registration, JWT authentication, and refresh token security**.  
This project is designed as a **learning-focused backend project** emphasizing security fundamentals, clean architecture, and proper testing practices.

---

## 🚀 Features Implemented

### 🔐 Email OTP Registration
- Generate and send a **6-digit OTP** to the user email
- OTP expiration (5 minutes)
- Only **one active OTP per email**
- OTP verification required before account activation

### 🛡️ Security & Abuse Prevention
- **Rate Limiting:** Max 5 OTP requests per email within 15 minutes
- **Cooldown Period:** Prevents OTP request spamming
- **OTP Revocation:** Regenerating OTP invalidates all previous unused OTPs
- **One-Time Use:** OTP becomes invalid once verified

### 🔑 JWT Authentication
- Email + password login
- Secure **JWT access tokens**
- Role-based authorization support (User / Admin ready)
- Token-based authentication for protected endpoints

### 🔄 Refresh Token Security
- Refresh tokens stored as **hashed values**
- **Refresh token rotation**
- Automatic revocation of previous refresh tokens
- Detection of **refresh token reuse**
  - If an old refresh token is reused → all sessions are revoked
  - User is forced to log in again
- Rate limiting applied on refresh token endpoint

### 🧩 Complete Registration Flow
- Register with email
- OTP is sent to email
- OTP verification
- User sets password after verification
- Login issues **access token + refresh token**

---

## 🗂️ Project Structure

```
src/
 └── UserAuth.Api/
     ├── Controllers/
     ├── Services/
     ├── Interfaces/
     ├── Entities/
     ├── DTOs/
     ├── Data/
     ├── Email/
     └── Program.cs

tests/
 ├── Unit/
 │   └── Service logic tests
 │
 ├── API/
 │   └── Controller endpoint tests
 │
 └── DataLayer/
     └── Repository / database interaction tests
```

---

## 🧪 Testing

The project includes **automated tests using MSTest**.

### Test Categories

**Unit Tests**
- Service logic validation
- OTP generation logic
- Business rule validation

**API Tests**
- Controller endpoint behavior
- HTTP request/response validation

**Data Layer Tests**
- Repository operations
- Database interaction verification

Run all tests:

```bash
dotnet test
```

---

## 🛠️ Technologies Used

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- SMTP Email Service
- Dependency Injection
- MSTest
- Logging (ILogger)

---

## 🔧 Setup Instructions

### 1️⃣ Clone the repository

```bash
git clone https://github.com/pawan-pathak12/dotnet-otp-auth-api.git

```

### 2️⃣ Configure Email Settings , Database and JWT Setup

Update `appsettings.json`:

```json
{
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "noreply@example.com",
    "Password": "your-email-password"
  }
}
---

 "ConnectionStrings": {
   "DefaultConnection": "Server=your_server_name;Database=UserOtpDb;User Id=your_server_id;Password=your_passowrd;Trusted_Connection=True;TrustServerCertificate=True;"
 }

---
 "Jwt": {
   "Key": "your_secret_key_should_be_more_than_32_letter",
   "Issuer": "JwtAuthLearning",
   "Audience": "JwtAuthLearningUsers",
   "ExpiresInMinutes": 15
 }

```



### 3️⃣ Apply Database Migrations

```bash
dotnet ef database update
```

### 4️⃣ Run the API

```bash
dotnet run
```

---

## 🔄 API Authentication Flow

### Registration Flow

1. User submits email
2. OTP is generated
3. OTP is sent via email
4. Previous OTPs are revoked
5. User verifies OTP
6. User sets password
---
### Login Flow

1. User logs in using email + password
2. API returns:
   - Access Token (JWT)
   - Refresh Token
---
### Refresh Token Flow

1. Client sends refresh token
2. Server verifies hashed token
3. Issues new access token
4. Rotates refresh token
5. Revokes previous token

If refresh token reuse is detected:
- All tokens are revoked
- User must login again

---

## 📚 Purpose of This Project

This project was built as a **learning-focused backend project** to understand:

- Secure OTP authentication flows
- JWT authentication and refresh token security
- Abuse prevention techniques
- Clean project structure
- Automated backend testing
- Practical backend security concepts

---

## 🔮 Possible Future Improvements

- Redis-based distributed rate limiting
- Email template rendering
- Login attempt throttling
- Password reset with OTP
- Docker containerization
- CI/CD pipeline integration

---

## 🧑‍💻 Author

Built as a backend learning project using **ASP.NET Core** to explore real-world authentication patterns and security practices.

---
## License

MIT License  
See the LICENSE file for details.

---
