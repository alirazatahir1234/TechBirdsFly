# User Profile Schema - Complete Reference

## 📋 Overview

The User Profile Schema is a composite system that manages user information across multiple entities in the TechBirdsFly platform. It includes the core User entity, UserProfile details, and related preferences and subscription data.

---

## 🗂️ Entity Structure

### 1. User Entity (Core)

**File**: `/services/user-service/src/UserService/Domain/Entities/UserEntities.cs`  
**Database Table**: `Users`  
**Database**: PostgreSQL

```csharp
public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public EmailAddress Email { get; private set; } = new EmailAddress(string.Empty);
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public PhoneNumber? Phone { get; private set; }
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public bool EmailVerified { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public string? Bio { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int LoginAttempts { get; private set; }
    public DateTime? LockoutUntil { get; private set; }
}
```

#### User Properties

| Property | Type | Constraints | Purpose |
|----------|------|-----------|---------|
| **Id** | Guid | Primary Key, NOT NULL | Unique user identifier |
| **Username** | string | UNIQUE, Max 100 | Display name / login identifier |
| **Email** | EmailAddress (VO) | UNIQUE, Max 255 | Email address (value object) |
| **PasswordHash** | string | Required, Max 255 | Hashed password (bcrypt/argon2) |
| **FullName** | string | Required, Max 200 | User's full name |
| **Phone** | PhoneNumber? (VO) | Optional, Max 20 | Phone number (value object) |
| **Role** | UserRole (enum) | Required | user, admin, moderator |
| **Status** | UserStatus (enum) | Required | active, inactive, suspended, deleted |
| **EmailVerified** | bool | Required | Email confirmation status |
| **ProfileImageUrl** | string? | Optional, Max 500 | Profile picture URL |
| **Bio** | string? | Optional, Max 1000 | User biography / description |
| **CreatedAt** | DateTime | Required | Account creation timestamp |
| **UpdatedAt** | DateTime? | Optional | Last modification timestamp |
| **LastLoginAt** | DateTime? | Optional | Last successful login |
| **LoginAttempts** | int | Required, Default 0 | Failed login counter |
| **LockoutUntil** | DateTime? | Optional | Account lockout deadline |

#### User Enums

**UserRole**:
```csharp
public enum UserRole
{
    User = 0,      // Regular user
    Admin = 1,     // Administrator
    Moderator = 2  // Moderator
}
```

**UserStatus**:
```csharp
public enum UserStatus
{
    Pending = 0,     // Email not verified
    Active = 1,      // Active account
    Inactive = 2,    // Deactivated
    Suspended = 3,   // Temporarily suspended
    Deleted = 4      // Soft deleted
}
```

#### Database Schema

```sql
CREATE TABLE "Users" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "Username" TEXT NOT NULL UNIQUE,
    "Email" TEXT NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "FullName" TEXT NOT NULL,
    "Phone" TEXT,
    "Role" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "EmailVerified" INTEGER NOT NULL,
    "ProfileImageUrl" TEXT,
    "Bio" TEXT,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT,
    "LastLoginAt" TEXT,
    "LoginAttempts" INTEGER NOT NULL DEFAULT 0,
    "LockoutUntil" TEXT
);

-- Indexes for performance
CREATE INDEX "IX_Users_Email_Unique" ON "Users"("Email") UNIQUE;
CREATE INDEX "IX_Users_Username_Unique" ON "Users"("Username") UNIQUE;
CREATE INDEX "IX_Users_Status" ON "Users"("Status");
CREATE INDEX "IX_Users_Role" ON "Users"("Role");
CREATE INDEX "IX_Users_CreatedAt" ON "Users"("CreatedAt");
CREATE INDEX "IX_Users_LastLoginAt" ON "Users"("LastLoginAt");
CREATE INDEX "IX_Users_LockoutUntil" ON "Users"("LockoutUntil");
```

---

### 2. UserProfile Entity (Extended Profile)

**File**: `/services/user-service/src/UserService/Domain/Entities/UserEntities.cs`  
**Database Table**: `UserProfiles`  
**Database**: PostgreSQL  
**Relationship**: One-to-One with User (CASCADE delete)

```csharp
public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? CompanyName { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? SocialMediaLinks { get; set; }
    public string? Preferences { get; set; }
    public bool NotificationsEnabled { get; set; }
    public bool EmailNotifications { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public UserProfile() { }

    public UserProfile(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        NotificationsEnabled = true;
        EmailNotifications = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(
        string? companyName,
        string? department,
        string? jobTitle,
        string? location,
        string? website,
        string? preferences)
    {
        CompanyName = companyName ?? CompanyName;
        Department = department ?? Department;
        JobTitle = jobTitle ?? JobTitle;
        Location = location ?? Location;
        Website = website ?? Website;
        Preferences = preferences ?? Preferences;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotificationPreferences(bool notifications, bool emailNotifications)
    {
        NotificationsEnabled = notifications;
        EmailNotifications = emailNotifications;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

#### UserProfile Properties

| Property | Type | Constraints | Purpose |x
|----------|------|-----------|---------|
| **Id** | Guid | Primary Key, NOT NULL | Unique profile identifier |
| **UserId** | Guid | Foreign Key, NOT NULL, UNIQUE | Link to User (1:1) |
| **CompanyName** | string? | Optional, Max 255 | Company/Organization name |
| **Department** | string? | Optional, Max 255 | Department name |
| **JobTitle** | string? | Optional, Max 255 | Job title/position |
| **Location** | string? | Optional, Max 255 | City, region, or address |
| **Website** | string? | Optional, Max 500 | Personal/company website URL |
| **SocialMediaLinks** | string? | Optional, JSON | Social media URLs (JSON format) |
| **Preferences** | string? | Optional, Max 1000 | User preferences (JSON format) |
| **NotificationsEnabled** | bool | Required, Default true | Enable notifications |
| **EmailNotifications** | bool | Required, Default true | Enable email notifications |
| **CreatedAt** | DateTime | Required | Profile creation timestamp |
| **UpdatedAt** | DateTime? | Optional | Profile last update timestamp |

#### Database Schema

```sql
CREATE TABLE "UserProfiles" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "UserId" TEXT NOT NULL UNIQUE,
    "CompanyName" TEXT,
    "Department" TEXT,
    "JobTitle" TEXT,
    "Location" TEXT,
    "Website" TEXT,
    "SocialMediaLinks" TEXT,
    "Preferences" TEXT,
    "NotificationsEnabled" INTEGER NOT NULL DEFAULT 1,
    "EmailNotifications" INTEGER NOT NULL DEFAULT 1,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT,
    CONSTRAINT "FK_UserProfiles_Users_UserId" FOREIGN KEY ("UserId") 
        REFERENCES "Users"("Id") ON DELETE CASCADE
);

-- Indexes
CREATE INDEX "IX_UserProfiles_UserId_Unique" ON "UserProfiles"("UserId") UNIQUE;
CREATE INDEX "IX_UserProfiles_CreatedAt" ON "UserProfiles"("CreatedAt");
```

---

### 3. DTOs (Data Transfer Objects)

**File**: `/services/user-service/src/UserService/Application/DTOs/UserDtos.cs`

#### UserProfileDto

```csharp
public class UserProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? CompanyName { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? SocialMediaLinks { get; set; }
    public string? Preferences { get; set; }
    public bool NotificationsEnabled { get; set; }
    public bool EmailNotifications { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

#### UpdateProfileRequestDto

```csharp
public class UpdateProfileRequestDto
{
    public string? CompanyName { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? SocialMediaLinks { get; set; }
    public string? Preferences { get; set; }
    [Range(typeof(bool), "false", "true")]
    public bool NotificationsEnabled { get; set; }
    [Range(typeof(bool), "false", "true")]
    public bool EmailNotifications { get; set; }
}
```

#### UserDto

```csharp
public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
```

---

## 📊 Complete Data Model Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     Users Table                              │
├─────────────────────────────────────────────────────────────┤
│ PK  Id (GUID)                                                │
│     Username (UNIQUE)                                        │
│     Email (UNIQUE) ◄─ Value Object (EmailAddress)          │
│     PasswordHash                                             │
│     FullName                                                 │
│     Phone (Optional) ◄─ Value Object (PhoneNumber)          │
│     Role (enum: User, Admin, Moderator)                     │
│     Status (enum: Pending, Active, Inactive, Suspended)     │
│     EmailVerified (bool)                                     │
│     ProfileImageUrl (Optional)                               │
│     Bio (Optional, 1000 chars max)                           │
│     CreatedAt (DateTime)                                     │
│     UpdatedAt (DateTime, Optional)                           │
│     LastLoginAt (DateTime, Optional)                         │
│     LoginAttempts (int, default 0)                           │
│     LockoutUntil (DateTime, Optional)                        │
└──────────────────────┬──────────────────────────────────────┘
                       │ FK (1:1)
                       │ CASCADE DELETE
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                  UserProfiles Table                          │
├─────────────────────────────────────────────────────────────┤
│ PK  Id (GUID)                                                │
│ FK  UserId (GUID, UNIQUE)                                    │
│     CompanyName (Optional, 255 chars max)                    │
│     Department (Optional, 255 chars max)                     │
│     JobTitle (Optional, 255 chars max)                       │
│     Location (Optional, 255 chars max)                       │
│     Website (Optional, 500 chars max)                        │
│     SocialMediaLinks (Optional, JSON)                        │
│     Preferences (Optional, 1000 chars max, JSON)             │
│     NotificationsEnabled (bool, default true)                │
│     EmailNotifications (bool, default true)                  │
│     CreatedAt (DateTime)                                     │
│     UpdatedAt (DateTime, Optional)                           │
└─────────────────────────────────────────────────────────────┘

Indexes:
├── Users
│   ├── Email (UNIQUE)
│   ├── Username (UNIQUE)
│   ├── Status
│   ├── Role
│   ├── CreatedAt
│   ├── LastLoginAt
│   └── LockoutUntil
│
└── UserProfiles
    ├── UserId (UNIQUE) - Foreign Key
    └── CreatedAt
```

---

## 🔄 Entity Relationships

### User → UserProfile (1:1)

```csharp
// In User entity (not explicitly shown, but implied)
// One User has exactly one UserProfile

// In UserProfile entity
public Guid UserId { get; set; }  // Foreign key to Users

// Entity Framework Configuration
modelBuilder.Entity<User>(entity =>
{
    entity.HasMany<UserProfile>()
        .WithOne()
        .HasForeignKey(p => p.UserId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

**Behavior**:
- When a User is deleted, the corresponding UserProfile is automatically deleted (CASCADE)
- Each User can have exactly one UserProfile
- UserProfile is created automatically when a User registers

---

## 📝 Value Objects

### EmailAddress (Value Object)

```csharp
public class EmailAddress
{
    public string Value { get; private set; }

    public EmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty");
        
        if (!email.Contains("@"))
            throw new ArgumentException("Invalid email format");
        
        Value = email.ToLower();
    }

    public static implicit operator string(EmailAddress email) => email.Value;
    public override string ToString() => Value;
}
```

**Mapping in EF Core**:
```csharp
entity.OwnsOne(e => e.Email, email =>
{
    email.Property(e => e.Value)
        .IsRequired()
        .HasMaxLength(255)
        .HasColumnName("Email");

    email.HasIndex(e => e.Value)
        .IsUnique()
        .HasDatabaseName("IX_Users_Email_Unique");
});
```

### PhoneNumber (Value Object)

```csharp
public class PhoneNumber
{
    public string Value { get; private set; }

    public PhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be empty");
        
        // Remove non-digits
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        
        if (digits.Length < 10)
            throw new ArgumentException("Phone must have at least 10 digits");
        
        Value = digits;
    }

    public static implicit operator string(PhoneNumber phone) => phone.Value;
    public override string ToString() => Value;
}
```

**Mapping in EF Core**:
```csharp
entity.OwnsOne(e => e.Phone, phone =>
{
    phone.Property(e => e.Value)
        .HasMaxLength(20)
        .HasColumnName("Phone");

    phone.HasIndex(e => e.Value)
        .HasDatabaseName("IX_Users_Phone");
});
```

---

## 🛠️ Database Migrations

### Migration File

**File**: `/services/user-service/src/UserService/Migrations/20251110232311_InitialCreate.cs`

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Create Users table
    migrationBuilder.CreateTable(
        name: "Users",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
            Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
            Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
            Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
            Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
            EmailVerified = table.Column<bool>(type: "INTEGER", nullable: false),
            ProfileImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
            Bio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
            CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
            UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
            LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
            LoginAttempts = table.Column<int>(type: "INTEGER", nullable: false),
            LockoutUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Users", x => x.Id);
        });

    // Create UserProfiles table
    migrationBuilder.CreateTable(
        name: "UserProfiles",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            UserId = table.Column<Guid>(type: "TEXT", nullable: false),
            CompanyName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            Department = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            JobTitle = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            Location = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            Website = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
            SocialMediaLinks = table.Column<string>(type: "TEXT", nullable: true),
            Preferences = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
            NotificationsEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
            EmailNotifications = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
            CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
            UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_UserProfiles", x => x.Id);
            table.ForeignKey(
                name: "FK_UserProfiles_Users_UserId",
                column: x => x.UserId,
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        });

    // Create indexes
    migrationBuilder.CreateIndex(
        name: "IX_Users_Email_Unique",
        table: "Users",
        column: "Email",
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_Users_Username_Unique",
        table: "Users",
        column: "Username",
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_Users_Status",
        table: "Users",
        column: "Status");

    migrationBuilder.CreateIndex(
        name: "IX_Users_Role",
        table: "Users",
        column: "Role");

    migrationBuilder.CreateIndex(
        name: "IX_Users_CreatedAt",
        table: "Users",
        column: "CreatedAt");

    migrationBuilder.CreateIndex(
        name: "IX_Users_LastLoginAt",
        table: "Users",
        column: "LastLoginAt");

    migrationBuilder.CreateIndex(
        name: "IX_Users_LockoutUntil",
        table: "Users",
        column: "LockoutUntil");

    migrationBuilder.CreateIndex(
        name: "IX_UserProfiles_UserId_Unique",
        table: "UserProfiles",
        column: "UserId",
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_UserProfiles_CreatedAt",
        table: "UserProfiles",
        column: "CreatedAt");
}
```

---

## 🔐 Constraints & Validation

### Unique Constraints

| Constraint | Table | Column(s) | Purpose |
|-----------|-------|-----------|---------|
| `PK_Users` | Users | Id | Primary key |
| `IX_Users_Email_Unique` | Users | Email | Prevent duplicate emails |
| `IX_Users_Username_Unique` | Users | Username | Prevent duplicate usernames |
| `PK_UserProfiles` | UserProfiles | Id | Primary key |
| `IX_UserProfiles_UserId_Unique` | UserProfiles | UserId | One profile per user |

### Max Length Constraints

| Column | Table | Max Length | Reason |
|--------|-------|-----------|--------|
| Username | Users | 100 | Reasonable identifier length |
| Email | Users | 255 | RFC 5321 email standard |
| PasswordHash | Users | 255 | Bcrypt/Argon2 hash length |
| FullName | Users | 200 | Typical name length |
| Phone | Users | 20 | International format |
| ProfileImageUrl | Users | 500 | URL length limit |
| Bio | Users | 1000 | Bio/description length |
| CompanyName | UserProfiles | 255 | Company name length |
| Department | UserProfiles | 255 | Department name length |
| JobTitle | UserProfiles | 255 | Job title length |
| Location | UserProfiles | 255 | Location string length |
| Website | UserProfiles | 500 | Website URL length |
| Preferences | UserProfiles | 1000 | JSON preferences |

---

## 📡 API Endpoints

### User Profile Endpoints

**GET** `/api/users/{userId}/profile`
- Retrieve user profile
- Returns: `UserProfileDto`

**PUT** `/api/users/{userId}/profile`
- Update user profile
- Request: `UpdateProfileRequestDto`
- Returns: `UserProfileDto`

**GET** `/api/users/{userId}`
- Get user information
- Returns: `UserDto`

**PUT** `/api/users/{userId}`
- Update user information
- Request: `UpdateUserRequestDto`
- Returns: `UserDto`

---

## 🔄 Entity Lifecycle

### User Creation Flow

```
1. Register (Auth Service)
   ├─ Validate email/username unique
   ├─ Hash password (bcrypt/argon2)
   ├─ Create User entity
   └─ Create default UserProfile (1:1)

2. Email Verification
   ├─ Send verification email
   ├─ Update User.EmailVerified = true
   └─ Update User.Status = Active

3. Profile Update
   ├─ User updates profile info
   ├─ Update UserProfile properties
   └─ Update User.UpdatedAt timestamp

4. Account Deactivation
   ├─ Set User.Status = Inactive
   ├─ Clear sensitive data (optional)
   └─ UserProfile remains (soft delete)

5. Hard Delete
   ├─ Delete User (cascades to UserProfile)
   ├─ All profile data deleted
   └─ Cannot be recovered (audit logs preserve history)
```

---

## 🛡️ Security Considerations

### Password Security

- **Storage**: Bcrypt/Argon2 hashing (not reversible)
- **Min Length**: 8 characters
- **Complexity**: Uppercase, lowercase, numbers, special chars
- **Update**: Only through authenticated endpoint
- **Reset**: Via forgot password flow with token validation

### Email Security

- **Uniqueness**: Database unique constraint + application validation
- **Verification**: Email verification required for account activation
- **Privacy**: Never exposed in logs or error messages

### Profile Data Security

- **Authorization**: Users can only view/edit own profile
- **Admin Access**: Admins can view/moderate any profile
- **Audit Trail**: All changes logged for compliance

### Lockout Mechanism

```csharp
// After 5 failed login attempts
if (user.LoginAttempts >= 5)
{
    user.LockoutUntil = DateTime.UtcNow.AddMinutes(15);
}

// After lockout period expires
if (user.LockoutUntil <= DateTime.UtcNow)
{
    user.LoginAttempts = 0;
    user.LockoutUntil = null;
}
```

---

## 📊 Query Examples

### Get User Profile

```sql
SELECT 
    u.Id, u.Email, u.FullName, u.Role, u.Status,
    p.CompanyName, p.JobTitle, p.Location
FROM Users u
LEFT JOIN UserProfiles p ON u.Id = p.UserId
WHERE u.Id = @UserId;
```

### Find Users by Role

```sql
SELECT Id, Email, FullName, Role, Status
FROM Users
WHERE Role = @Role
AND Status = 'Active'
ORDER BY CreatedAt DESC;
```

### Search Users

```sql
SELECT Id, Email, FullName, Role
FROM Users
WHERE (Email LIKE @SearchTerm OR FullName LIKE @SearchTerm)
AND Status != 'Deleted'
ORDER BY CreatedAt DESC;
```

### Get Active Users Created This Month

```sql
SELECT COUNT(*) as ActiveUsers
FROM Users
WHERE Status = 'Active'
AND CreatedAt >= DATE_TRUNC('month', CURRENT_DATE);
```

---

## 🔗 Related Entities

Future extensions that would link to User Profile:

- **UserSubscription**: Subscription plan and usage
- **UserPreferences**: Theme, language, notifications
- **UserAuditLog**: Login history, profile changes
- **UserDevice**: Trusted devices for security
- **UserSocialLinks**: Connected social profiles
- **UserSettings**: Feature flags and preferences

---

## 📦 Entity Mapping

### User → UserDto

```csharp
public static UserDto ToDto(User user)
{
    return new UserDto
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email.Value,
        FullName = user.FullName,
        Phone = user.Phone?.Value,
        Role = user.Role.ToString(),
        Status = user.Status.ToString(),
        EmailVerified = user.EmailVerified,
        ProfileImageUrl = user.ProfileImageUrl,
        Bio = user.Bio,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        LastLoginAt = user.LastLoginAt
    };
}
```

### UserProfile → UserProfileDto

```csharp
public static UserProfileDto ToDto(UserProfile profile)
{
    return new UserProfileDto
    {
        Id = profile.Id,
        UserId = profile.UserId,
        CompanyName = profile.CompanyName,
        Department = profile.Department,
        JobTitle = profile.JobTitle,
        Location = profile.Location,
        Website = profile.Website,
        SocialMediaLinks = profile.SocialMediaLinks,
        Preferences = profile.Preferences,
        NotificationsEnabled = profile.NotificationsEnabled,
        EmailNotifications = profile.EmailNotifications,
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt
    };
}
```

---

## 🎯 Best Practices

### Do's ✅

- ✅ Always validate email format before storing
- ✅ Hash passwords using strong algorithms
- ✅ Use transactions for multi-entity updates
- ✅ Index frequently queried columns
- ✅ Implement soft deletes for audit trail
- ✅ Use value objects for domain concepts
- ✅ Log all profile modifications
- ✅ Implement role-based access control

### Don'ts ❌

- ❌ Never store plaintext passwords
- ❌ Don't expose sensitive data in APIs
- ❌ Avoid synchronous operations for non-critical tasks
- ❌ Don't skip email verification
- ❌ Avoid hardcoding constraints in code
- ❌ Don't trust user input without validation
- ❌ Avoid storing passwords in profiles
- ❌ Don't skip proper error handling

---

## 📈 Performance Optimization

### Index Strategy

**Frequently Used Queries**:
```sql
-- Find user by email (authentication)
CREATE INDEX IX_Users_Email_Unique ON Users(Email);

-- List users by status
CREATE INDEX IX_Users_Status ON Users(Status);

-- Recent registrations
CREATE INDEX IX_Users_CreatedAt ON Users(CreatedAt);

-- Last active users
CREATE INDEX IX_Users_LastLoginAt ON Users(LastLoginAt);
```

### Query Optimization

```csharp
// Good: Single query with eager loading
var users = await _context.Users
    .Include(u => u.Profile)
    .Where(u => u.Status == UserStatus.Active)
    .OrderByDescending(u => u.CreatedAt)
    .Take(100)
    .ToListAsync();

// Avoid: N+1 queries
var users = await _context.Users.ToListAsync();
foreach (var user in users)
{
    var profile = await _context.UserProfiles
        .FirstOrDefaultAsync(p => p.UserId == user.Id);
}
```

---

## 📝 Summary

The User Profile Schema consists of:

1. **User Entity** - Core user account (accounts, authentication, status)
2. **UserProfile Entity** - Extended profile (job, location, preferences)
3. **Value Objects** - EmailAddress, PhoneNumber (domain logic)
4. **DTOs** - UserDto, UserProfileDto (data transfer)
5. **Database** - PostgreSQL with proper indexes and constraints
6. **Security** - Password hashing, email verification, access control
7. **Relationships** - One-to-One with CASCADE delete
8. **Audit Trail** - Created/Updated timestamps

This schema provides a solid foundation for user management in microservices architecture.
