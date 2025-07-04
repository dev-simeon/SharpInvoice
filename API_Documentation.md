# SharpInvoice API Documentation

## Overview

SharpInvoice is a comprehensive invoice management system built with .NET using Clean Architecture principles. The system is organized into modules handling Authentication, User Management, Invoicing, and Payments. This documentation covers all public APIs, DTOs, interfaces, and usage examples.

## Architecture

The application follows a modular monolith pattern with:
- **API Layer**: Controllers and endpoints
- **Application Layer**: Business logic, commands, queries, and DTOs
- **Domain Layer**: Entities and business rules
- **Infrastructure Layer**: Data access and external services

## Base Configuration

- **Base URL**: `https://api.sharpinvoice.com`
- **Content-Type**: `application/json`
- **Authentication**: JWT Bearer tokens
- **API Versioning**: URI versioning (e.g., `/api/v1/`)

---

## Authentication Module

### Overview
Handles user registration, login, password management, external authentication, and two-factor authentication.

### API Endpoints

#### 1. User Registration
**POST** `/api/auth/register`

Registers a new user and creates an initial business.

**Request Body:**
```json
{
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePassword123!",
  "businessName": "John's Consulting",
  "country": "United States"
}
```

**Response (201 Created):**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedrefreshtoken",
  "requiresProfileCompletion": false
}
```

**Usage Example:**
```csharp
var registerCommand = new RegisterUserCommand(
    "user@example.com",
    "John",
    "Doe", 
    "SecurePassword123!",
    "John's Consulting",
    "United States"
);

var response = await authService.RegisterAndCreateBusinessAsync(registerCommand);
```

#### 2. User Login
**POST** `/api/auth/login`

Authenticates a user with email and password. Returns JWT tokens or 2FA challenge.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK) - No 2FA:**
```json
{
  "authResponse": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64encodedrefreshtoken",
    "requiresProfileCompletion": false
  }
}
```

**Response (200 OK) - 2FA Required:**
```json
{
  "isTwoFactorRequired": true,
  "message": "Two-factor authentication code has been sent to your email."
}
```

#### 3. Two-Factor Authentication Verification
**POST** `/api/auth/login/verify-two-factor`

Completes login process for users with 2FA enabled.

**Request Body:**
```json
{
  "email": "user@example.com",
  "code": "123456"
}
```

**Response (200 OK):**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedrefreshtoken",
  "requiresProfileCompletion": false
}
```

#### 4. Token Refresh
**POST** `/api/auth/refresh-token`

Refreshes an expired JWT using a valid refresh token.

**Request Body:**
```json
{
  "refreshToken": "base64encodedrefreshtoken"
}
```

**Response (200 OK):**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "newbase64encodedrefreshtoken",
  "requiresProfileCompletion": false
}
```

#### 5. Email Confirmation
**GET** `/api/auth/confirm-email?userId={userId}&token={token}`

Confirms a user's email address using the token sent via email.

**Parameters:**
- `userId` (query): User's unique identifier
- `token` (query): Email confirmation token

**Response (200 OK):**
```json
{
  "message": "Email confirmed successfully."
}
```

#### 6. External Authentication (Google/Facebook)
**GET** `/api/auth/external-login?provider={provider}&returnUrl={returnUrl}`

Initiates external authentication flow.

**Parameters:**
- `provider` (query): Authentication provider ("Google", "Facebook")
- `returnUrl` (query, optional): URL to redirect after authentication

**Response:** HTTP 302 redirect to external provider

#### 7. External Authentication Callback
**GET** `/api/auth/external-callback?returnUrl={returnUrl}&remoteError={error}`

Handles callback from external authentication provider.

**Response (200 OK):**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedrefreshtoken",
  "requiresProfileCompletion": false
}
```

#### 8. Forgot Password
**POST** `/api/auth/forgot-password`

Sends password reset link to user's email.

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response (200 OK):**
```json
{
  "message": "If an account with this email exists, a password reset link has been sent."
}
```

#### 9. Reset Password
**POST** `/api/auth/reset-password`

Resets password using a valid reset token.

**Request Body:**
```json
{
  "email": "user@example.com",
  "token": "reset-token-from-email",
  "newPassword": "NewSecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "message": "Password has been reset successfully."
}
```

### Authentication Interfaces

#### IAuthService
Main authentication service interface providing comprehensive authentication functionality.

```csharp
public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAndCreateBusinessAsync(RegisterUserCommand command);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl);
    Task<AuthResponseDto> HandleExternalLoginAsync();
    Task ForgotPasswordAsync(ForgotPasswordCommand command);
    Task<bool> ResetPasswordAsync(ResetPasswordCommand command);
    Task<bool> ConfirmEmailAsync(Guid userId, string token);
    
    // 2FA Methods
    Task<LoginResponseDto> LoginAsync(LoginUserCommand command);
    Task<AuthResponseDto> VerifyTwoFactorCodeAsync(VerifyTwoFactorCommand command);
    Task EnableTwoFactorAuthenticationAsync(Guid userId);
    Task DisableTwoFactorAuthenticationAsync(Guid userId);
}
```

#### Supporting Interfaces

```csharp
public interface IJwtTokenGenerator
{
    string GenerateToken(string userId, string email, string businessId);
}

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlMessage);
}

public interface IEmailTemplateRenderer
{
    Task<string> RenderEmailConfirmationTemplateAsync(string userName, string confirmationLink);
    Task<string> RenderPasswordResetTemplateAsync(string userName, string resetLink);
    Task<string> RenderInvitationTemplateAsync(string businessName, string inviterName, string acceptInvitationLink);
}
```

### Authentication DTOs

#### AuthResponseDto
```csharp
public record AuthResponseDto(
    string UserId,           // The unique identifier for the user
    string Email,            // The user's email address
    string Token,            // JWT access token
    string RefreshToken,     // Refresh token for getting new JWT
    bool RequiresProfileCompletion = false
);
```

#### LoginResponseDto
```csharp
public record LoginResponseDto(
    bool IsTwoFactorRequired,
    string? Message,
    AuthResponseDto? AuthResponse
);
```

---

## User Management Module

### Overview
Handles user profiles, business management, and team member operations.

### API Endpoints

#### User Profile Management

#### 1. Get Current User Profile
**GET** `/api/user/me`

Retrieves the authenticated user's profile information.

**Headers:**
```
Authorization: Bearer {jwt-token}
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "firstName": "John",
  "lastName": "Doe",
  "email": "user@example.com",
  "avatarUrl": "https://example.com/avatars/user.jpg",
  "phoneNumber": "+1-555-123-4567"
}
```

#### 2. Update Current User Profile
**PUT** `/api/user/me`

Updates the authenticated user's profile information.

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "avatarUrl": "https://example.com/avatars/new-user.jpg",
  "phoneNumber": "+1-555-987-6543"
}
```

**Response:** 204 No Content

### Business Management

#### 1. Create Business
**POST** `/api/businesses`

Creates a new business for the authenticated user.

**Request Body:**
```json
{
  "name": "New Business Name",
  "email": "business@example.com",
  "country": "United States"
}
```

**Response (201 Created):**
```json
{
  "id": "business-guid",
  "name": "New Business Name",
  "isActive": true
}
```

#### 2. Get User's Businesses
**GET** `/api/businesses`

Retrieves all businesses associated with the authenticated user.

**Response (200 OK):**
```json
[
  {
    "id": "business-guid-1",
    "name": "Business One",
    "isActive": true
  },
  {
    "id": "business-guid-2", 
    "name": "Business Two",
    "isActive": false
  }
]
```

#### 3. Get Business Details
**GET** `/api/businesses/{businessId}`

Retrieves comprehensive information about a specific business.

**Response (200 OK):**
```json
{
  "id": "business-guid",
  "name": "My Business",
  "email": "contact@mybusiness.com",
  "phoneNumber": "+1-555-123-4567",
  "website": "https://mybusiness.com",
  "address": "123 Business St",
  "city": "Business City",
  "state": "BC",
  "zipCode": "12345",
  "country": "United States",
  "logoUrl": "https://example.com/logos/business.png",
  "isActive": true
}
```

#### 4. Update Business Details
**PUT** `/api/businesses/{businessId}`

Updates information for a specific business.

**Request Body:**
```json
{
  "name": "Updated Business Name",
  "email": "new-email@business.com",
  "phoneNumber": "+1-555-999-8888",
  "website": "https://newbusiness.com",
  "address": "456 New St",
  "city": "New City",
  "state": "NC",
  "zipCode": "54321",
  "country": "United States"
}
```

**Response:** 204 No Content

#### 5. Upload Business Logo
**POST** `/api/businesses/{businessId}/logo`

Uploads a logo image for a business.

**Request:** 
- Content-Type: `multipart/form-data`
- Body: Form data with logo file

**Response:** 204 No Content

#### 6. Activate/Deactivate Business
**POST** `/api/businesses/{businessId}/activate`
**POST** `/api/businesses/{businessId}/deactivate`

Activates or deactivates a business.

**Response:** 204 No Content

### Team Management

#### 1. Invite Team Member
**POST** `/api/businesses/{businessId}/team/invite`

Invites a new member to join the business team.

**Request Body:**
```json
{
  "email": "newmember@example.com",
  "roleId": "role-guid"
}
```

**Response:** 200 OK

#### 2. Accept Team Invitation
**GET** `/api/businesses/{businessId}/team/accept-invitation?token={invitationToken}`

Processes an invitation token to add user to business team.

**Response (200 OK):**
```json
"Invitation accepted successfully."
```

#### 3. Get Team Members
**GET** `/api/businesses/{businessId}/team`

Retrieves all team members for a business.

**Response (200 OK):**
```json
[
  {
    "id": "member-guid",
    "userId": "user-guid",
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane@example.com",
    "roleId": "role-guid",
    "roleName": "Admin",
    "isActive": true,
    "joinedAt": "2024-01-15T10:30:00Z"
  }
]
```

#### 4. Remove Team Member
**DELETE** `/api/businesses/{businessId}/team/{teamMemberId}`

Removes a member from the business team.

**Response:** 204 No Content

#### 5. Update Team Member Role
**PUT** `/api/businesses/{businessId}/team/{teamMemberId}/role`

Updates a team member's role and permissions.

**Request Body:**
```json
{
  "newRoleId": "new-role-guid"
}
```

**Response:** 204 No Content

### User Management Interfaces

#### IProfileService
```csharp
public interface IProfileService
{
    Task<ProfileDto> GetProfileAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
}
```

#### IBusinessService
```csharp
public interface IBusinessService
{
    Task<BusinessDto> CreateBusinessForUserAsync(Guid userId, string businessName, string userEmail, string country);
    Task<BusinessDetailsDto> GetBusinessDetailsAsync(Guid businessId);
    Task<IEnumerable<BusinessDto>> GetBusinessesForUserAsync(Guid userId);
    Task UpdateBusinessDetailsAsync(Guid businessId, UpdateBusinessDetailsDto dto);
    Task UpdateBusinessLogoAsync(Guid businessId, Stream logoStream, string fileName);
    Task<Guid> GetBusinessIdByOwnerAsync(Guid userId);
    Task DeactivateBusinessAsync(Guid businessId);
    Task ActivateBusinessAsync(Guid businessId);
}
```

#### ITeamMemberService
```csharp
public interface ITeamMemberService
{
    Task InviteTeamMemberAsync(Guid businessId, string email, Guid roleId);
    Task AcceptInvitationAsync(string token);
    Task<IEnumerable<TeamMemberDto>> GetTeamMembersForBusinessAsync(Guid businessId);
    Task RemoveTeamMemberAsync(Guid teamMemberId);
    Task UpdateTeamMemberRoleAsync(Guid teamMemberId, Guid newRoleId);
}
```

### User Management DTOs

#### ProfileDto
```csharp
public record ProfileDto(
    Guid Id,                 // User's unique identifier
    string FirstName,        // User's first name
    string LastName,         // User's last name
    string Email,            // User's email address
    string? AvatarUrl,       // Profile picture URL (optional)
    string? PhoneNumber      // Contact phone number (optional)
);
```

#### BusinessDetailsDto
```csharp
public record BusinessDetailsDto(
    Guid Id,                 // Business unique identifier
    string Name,             // Business name
    string? Email,           // Business contact email
    string? PhoneNumber,     // Business phone number
    string? Website,         // Business website URL
    string? Address,         // Street address
    string? City,            // City
    string? State,           // State/Province
    string? ZipCode,         // Postal/ZIP code
    string? Country,         // Country
    string? LogoUrl,         // Business logo URL
    bool IsActive            // Whether business is active
);
```

#### TeamMemberDto
```csharp
public record TeamMemberDto(
    Guid Id,                 // Team member unique identifier
    Guid UserId,             // Associated user ID
    string FirstName,        // Member's first name
    string LastName,         // Member's last name
    string Email,            // Member's email
    Guid RoleId,             // Assigned role ID
    string RoleName,         // Role display name
    bool IsActive,           // Whether member is active
    DateTime JoinedAt        // When member joined the team
);
```

---

## Shared Components

### Overview
Common interfaces, utilities, and base classes used across all modules.

### Core Interfaces

#### ICurrentUserProvider
Provides access to authenticated user context.

```csharp
public interface ICurrentUserProvider
{
    Guid GetCurrentUserId();                    // Gets current user ID (throws if not authenticated)
    bool TryGetCurrentUserId(out Guid userId);  // Safely tries to get current user ID
    Guid GetCurrentBusinessId();                // Gets current business ID from claims
}
```

**Usage Example:**
```csharp
public class SomeService
{
    private readonly ICurrentUserProvider _currentUserProvider;
    
    public async Task DoSomething()
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var businessId = _currentUserProvider.GetCurrentBusinessId();
        
        // Use IDs for business logic
    }
}
```

#### IFileStorageService
```csharp
public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName);
    Task DeleteFileAsync(string fileUrl);
}
```

### Domain Base Classes

#### Entity<TId>
Base class for all domain entities.

```csharp
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; }
    
    protected Entity(TId id) => Id = id;
    protected Entity() { } // EF Core constructor
}
```

#### AuditableEntity<TId>
Base class for entities requiring audit information.

```csharp
public abstract class AuditableEntity<TId> : Entity<TId> where TId : notnull
{
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }
}
```

---

## Error Handling

### Standard Error Response Format

All API endpoints return consistent error responses:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Specific error description",
  "instance": "/api/endpoint",
  "errors": {
    "field1": ["Validation error message"],
    "field2": ["Another validation error"]
  }
}
```

### Common HTTP Status Codes

- **200 OK**: Request successful
- **201 Created**: Resource created successfully
- **204 No Content**: Request successful, no content returned
- **400 Bad Request**: Invalid request data
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **422 Unprocessable Entity**: Validation failed
- **500 Internal Server Error**: Server error

---

## Authentication & Authorization

### JWT Token Structure

Tokens include these claims:
- `sub`: User ID
- `email`: User email
- `business_id`: Current business ID
- `role`: User role
- `exp`: Expiration timestamp
- `iat`: Issued at timestamp

### Rate Limiting

Certain endpoints have rate limiting applied:
- Login endpoints: 5 attempts per minute per IP
- Password reset: 3 attempts per hour per email
- Registration: 2 attempts per minute per IP

### Security Headers

All responses include security headers:
- `X-Frame-Options: DENY`
- `X-Content-Type-Options: nosniff`
- `X-XSS-Protection: 1; mode=block`
- `Strict-Transport-Security: max-age=31536000`

---

## Usage Examples

### Complete Authentication Flow

```csharp
// 1. Register new user
var registerCommand = new RegisterUserCommand(
    "user@example.com",
    "John",
    "Doe",
    "SecurePassword123!",
    "John's Business",
    "United States"
);
var authResponse = await authService.RegisterAndCreateBusinessAsync(registerCommand);

// 2. Login
var loginCommand = new LoginUserCommand("user@example.com", "SecurePassword123!");
var loginResponse = await authService.LoginAsync(loginCommand);

// 3. Use JWT token in subsequent requests
var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", loginResponse.AuthResponse.Token);

// 4. Refresh token when expired
var newAuthResponse = await authService.RefreshTokenAsync(authResponse.RefreshToken);
```

### Business Management Flow

```csharp
// 1. Get user's businesses
var businesses = await businessService.GetBusinessesForUserAsync(userId);

// 2. Create new business
var newBusiness = await businessService.CreateBusinessForUserAsync(
    userId, 
    "New Business", 
    "contact@newbusiness.com", 
    "Canada"
);

// 3. Update business details
var updateDto = new UpdateBusinessDetailsDto
{
    Name = "Updated Business Name",
    Email = "updated@business.com",
    PhoneNumber = "+1-555-999-8888"
};
await businessService.UpdateBusinessDetailsAsync(newBusiness.Id, updateDto);

// 4. Upload business logo
using var logoStream = File.OpenRead("logo.png");
await businessService.UpdateBusinessLogoAsync(newBusiness.Id, logoStream, "logo.png");
```

### Team Management Flow

```csharp
// 1. Invite team member
await teamMemberService.InviteTeamMemberAsync(
    businessId, 
    "newmember@example.com", 
    adminRoleId
);

// 2. Get team members
var teamMembers = await teamMemberService.GetTeamMembersForBusinessAsync(businessId);

// 3. Update member role
await teamMemberService.UpdateTeamMemberRoleAsync(memberId, newRoleId);

// 4. Remove team member
await teamMemberService.RemoveTeamMemberAsync(memberId);
```

---

## Future Modules

The following modules are planned but not yet implemented:

### Invoicing Module
- Invoice creation and management
- Invoice templates
- PDF generation
- Invoice numbering system
- Multi-currency support
- Recurring invoices

### Payments Module
- Stripe integration
- Payment tracking
- Refund processing
- Payment reminders
- Transaction history

### Analytics Module
- Revenue reporting
- Client analytics
- Accounts receivable aging
- Export functionality

---

## API Testing

### Swagger/OpenAPI

The API includes comprehensive Swagger documentation available at:
- Development: `https://localhost:5001/swagger`
- Production: `https://api.sharpinvoice.com/swagger`

### Example Test Requests

#### Register and Login Test
```bash
# Register
curl -X POST "https://api.sharpinvoice.com/api/auth/register" \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test@example.com",
       "firstName": "Test",
       "lastName": "User",
       "password": "TestPassword123!",
       "businessName": "Test Business",
       "country": "United States"
     }'

# Login
curl -X POST "https://api.sharpinvoice.com/api/auth/login" \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test@example.com",
       "password": "TestPassword123!"
     }'
```

#### Business Operations Test
```bash
# Get user profile (requires auth token)
curl -X GET "https://api.sharpinvoice.com/api/user/me" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Get businesses
curl -X GET "https://api.sharpinvoice.com/api/businesses" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

This documentation provides comprehensive coverage of all public APIs, interfaces, and components currently implemented in the SharpInvoice system, along with practical usage examples and integration guidance.