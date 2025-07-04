# SharpInvoice API Summary

## Quick Overview

SharpInvoice is a .NET-based invoice management system built with Clean Architecture and modular design. The system currently implements **Authentication** and **User Management** modules, with **Invoicing** and **Payments** modules planned for future development.

## Current Implementation Status

### ✅ Fully Implemented
- **Authentication Module**: Complete user registration, login, 2FA, password management, and external auth
- **User Management Module**: User profiles, business management, and team collaboration
- **Shared Infrastructure**: Base entities, current user context, file storage interfaces

### 🔄 Planned/In Development
- **Invoicing Module**: Invoice creation, templates, PDF generation, recurring invoices
- **Payments Module**: Stripe integration, payment tracking, refunds
- **Analytics Module**: Revenue reporting, client analytics, export functionality

## API Endpoints Summary

| Module | Endpoints | Description |
|--------|-----------|-------------|
| **Authentication** | 9 endpoints | Registration, login, 2FA, password management, external auth |
| **User Profile** | 2 endpoints | Get/update user profile information |
| **Business Management** | 7 endpoints | CRUD operations for businesses, logo upload, activation |
| **Team Management** | 5 endpoints | Invite members, manage roles, team operations |

## Key Components

### 🔐 Authentication Features
- JWT-based authentication with refresh tokens
- Two-factor authentication (email/authenticator app)
- External authentication (Google, Facebook, Azure AD B2C planned)
- Password reset and email confirmation
- Rate limiting for security

### 👥 User Management Features
- User profile management with avatars
- Multi-business support per user
- Team collaboration with role-based permissions
- Business branding (logos, themes)
- Invitation system for team members

### 🏗️ Architecture Highlights
- **Clean Architecture** with clear separation of concerns
- **Modular Monolith** design for scalability
- **Domain-Driven Design** principles
- **CQRS pattern** with commands and queries
- **Entity Framework Core** for data persistence

## Technology Stack

- **.NET 8** with ASP.NET Core
- **Entity Framework Core** for data access
- **JWT** for authentication
- **Swagger/OpenAPI** for documentation
- **Rate Limiting** for security
- **Azure services** integration ready

## Public Interfaces

### Core Services
```csharp
IAuthService              // Authentication operations
IProfileService           // User profile management
IBusinessService          // Business CRUD operations  
ITeamMemberService        // Team management
ICurrentUserProvider      // User context access
IJwtTokenGenerator        // Token generation
IEmailSender              // Email notifications
IFileStorageService       // File upload/storage
```

### Key DTOs
```csharp
AuthResponseDto           // Authentication responses
ProfileDto                // User profile data
BusinessDetailsDto        // Complete business information
TeamMemberDto             // Team member information
RegisterUserCommand       // User registration
LoginUserCommand          // User login
```

## Security Features

- **JWT Bearer Authentication** with configurable expiration
- **Refresh Token Rotation** for enhanced security
- **Rate Limiting** on sensitive endpoints
- **Email Confirmation** for account verification
- **Two-Factor Authentication** support
- **Role-Based Authorization** for team management
- **Secure Password Hashing** with modern algorithms

## API Design Principles

- **RESTful conventions** for predictable endpoints
- **Consistent error responses** with RFC 7807 Problem Details
- **Comprehensive validation** with detailed error messages
- **Swagger documentation** with examples
- **Rate limiting** for protection
- **CORS support** for web applications

## Future Roadmap

### Phase 1 (Next)
- Complete invoicing module with PDF generation
- Stripe payment integration
- Invoice templates and customization

### Phase 2
- Analytics and reporting dashboards
- Multi-currency support
- Recurring invoice automation

### Phase 3
- Advanced Azure integrations
- Mobile API optimizations
- Enterprise features

## Getting Started

1. **Authentication**: Register user → Login → Get JWT token
2. **Profile Setup**: Update user profile and business details
3. **Team Building**: Invite team members and assign roles
4. **Ready for invoicing**: System prepared for invoice management

## Documentation Links

- **Full API Documentation**: `API_Documentation.md`
- **Swagger UI**: `/swagger` endpoint
- **Authentication Guide**: See Authentication Module section
- **Integration Examples**: See Usage Examples section

---

*This summary provides a high-level overview. Refer to the complete API documentation for detailed endpoint specifications, request/response examples, and integration guidance.*