# 🛡️ Security Policy

## 🔐 Core Security Standards

The **Task Management API** is built with security as a first-class citizen, implementing industry-standard protection at every layer of the Modular Monolith.

### 1. Identity & Access Management
- **Authentication**: Powered by **ASP.NET Core Identity** and **JWT (JSON Web Tokens)**.
- **Validation**: Strict `TokenValidationParameters` including issuer, audience, and lifetime validation.
- **RBAC**: Role-Based Access Control enforced via `[Authorize(Roles = "...")]` and fine-grained **Authorization Policies**.

### 2. Transport Security
- **HSTS**: HTTP Strict Transport Security is enforced in Production (`UseHsts`) to prevent protocol downgrade attacks.
- **HTTPS Redirection**: Automatic redirection of all HTTP traffic to secure HTTPS endpoints.

### 3. Data Protection
- **Anti-Forgery**: Cross-Site Request Forgery (CSRF) protection is implemented for all state-changing operations via `.AddAntiforgery()`.
- **Soft Deletes**: Uses Global Query Filters in EF Core to prevent accidental or malicious data loss while maintaining an audit trail.
- **SQLi Prevention**: Leveraging EF Core's parameterized queries to eliminate SQL injection vulnerabilities.

### 4. Input Validation
- **FluentValidation**: Comprehensive validation for all incoming DTOs to ensure data integrity.
- **Cross-Site Scripting (XSS)**: Automatic output encoding by ASP.NET Core and strict content-type handling.

---

## ⚡ Reporting a Vulnerability

We take the security of this project seriously. If you believe you have found a security vulnerability, please do NOT open a public issue. Instead, follow these steps:

1. **Email us**: Send a detailed report to **security@m-said-portfolio.netlify.app**.
2. **Details**: Include a description of the vulnerability, steps to reproduce, and any potential impact.
3. **Response**: We will acknowledge receipt of your report within 48 hours and provide a timeline for a resolution.
4. **Disclosure**: We request that you do not disclose the vulnerability publicly until we have Had a chance to address it.

---

## 📅 Security Updates
Security patches and updates are documented in our **[CHANGELOG.md](CHANGELOG.md)**. We recommend keeping the .NET SDK and all NuGet dependencies updated to the latest stable versions.
