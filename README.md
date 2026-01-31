# User Authentication API with Email OTP

A simple ASP.NET Core Web API that implements **secure email-based OTP registration** with rate limiting and abuse prevention.  
This project is designed as a **learning-focused backend mini project**, emphasizing correctness, security basics, and clean structure.

---

## 🚀 Features Implemented

### 🔐 Email OTP Registration
- Generate and send a 6-digit OTP to user email
- OTP expiry time (5 minutes)
- Single active OTP per email

### 🛡️ Security & Abuse Prevention
- **Rate Limiting**: Max 5 OTP requests per email in 15 minutes
- **Cooldown Period**: Prevents OTP spamming by enforcing a wait time between requests
- **OTP Revocation**: Regenerating OTP invalidates all previous unused OTPs
- **One-Time Use**: OTP is marked as used after successful verification

### 🧱 Database
- Entity Framework Core
- Code-first migrations
- OTP audit data stored with timestamps

### 📧 Email Service
- SMTP-based email sending
- Configurable sender and credentials
- Supports `noreply` email usage (recommended)

---

## 🗂️ Project Structure

```
src/
 └── UserAuth.Api/
     ├── Controllers/
     ├── Services/
     ├── Interfaces/
     ├── Entities/
     ├── Data/
     ├── DTOs/
     └── Program.cs
```

---

## 🛠️ Technologies Used

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- SMTP (Email)
- Dependency Injection
- Logging (ILogger)

---

## 🔧 Setup Instructions

1. Clone the repository
2. Configure email settings in `appsettings.json`
3. Apply database migrations
4. Run the API

```bash
dotnet ef database update
dotnet run
```

---

## 🧪 API Flow (OTP Registration)

1. User submits email
2. OTP is generated and emailed
3. Previous OTPs are revoked
4. User verifies OTP
5. OTP is marked as used

---

## 🔮 Upcoming Features (Planned)

### 🔑 JWT-Based Authentication
- Email + password login
- Role-based authorization (User / Admin)
- Secure JWT access tokens

### 🔄 Refresh Token Security
- Refresh tokens stored as **hashed values**
- Refresh token rotation
- Automatic revocation of previous refresh tokens
- Detection of **refresh token reuse**
  - If an old refresh token is reused → all sessions revoked
  - User is forced to re-login
- Rate limiting on refresh token endpoint

### 🧩 Combined Registration & Login Flow
- Register → Send OTP
- OTP verification
- User sets password after OTP verification
- Backend securely links password to verified email
- Login → Issue access & refresh tokens

### 🛡️ Additional Security Enhancements
- Session invalidation on suspicious activity
- Token revocation on password change
- Improved audit logging

---

## 📚 Purpose of This Project

This project is built as a **learning exercise** to understand:
- Real-world OTP flows
- Security considerations in authentication
- Backend best practices at a beginner–intermediate level

---

## 🧑‍💻 Author

Built by a BCA student learning backend development with ASP.NET Core.

