# 🗄️ PostgreSQL Setup & Testing Guide

**Status**: ✅ Auth Service Configured for PostgreSQL  
**Build**: ✅ Verified (0 errors)

---

## Quick Start

### 1️⃣ Install PostgreSQL (macOS)
```bash
brew install postgresql
brew services start postgresql
```

### 2️⃣ Create Databases
```bash
psql -U postgres -c "CREATE DATABASE techbirdsfly_auth;"
psql -U postgres -c "CREATE DATABASE techbirdsfly_auth_dev;"
```

### 3️⃣ Verify Connection
```bash
psql -U postgres -h localhost -d techbirdsfly_auth -c "SELECT 1;"
```

Expected output:
```
 ?column?
----------
        1
(1 row)
```

### 4️⃣ Run Migrations
```bash
cd services/auth-service/src
dotnet ef database update
```

### 5️⃣ Start Auth Service
```bash
cd services/auth-service/src
dotnet run
```

### 6️⃣ Test in Browser
```
http://localhost:5000/swagger
```

---

## Configuration Reference

### Production (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=techbirdsfly_auth;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  }
}
```

### Development (`appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=techbirdsfly_auth_dev;Username=postgres;Password=postgres"
  }
}
```

---

## Database Status Commands

```bash
# List all databases
psql -U postgres -l

# Connect to auth database
psql -U postgres -d techbirdsfly_auth

# Show all tables
\dt

# Exit psql
\q
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| PostgreSQL not running | `brew services start postgresql` |
| Database doesn't exist | `psql -U postgres -c "CREATE DATABASE techbirdsfly_auth;"` |
| Connection refused | Check `lsof -i :5432` |
| Wrong password | Reset via `pg_ctl` or Homebrew utilities |

---

## Migration Flow (EF Core)

```bash
# Add new migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Rollback migration
dotnet ef database update <previous-migration>
```

---

## ✅ Ready for Phase 2C: Runtime Testing

All systems configured and ready to test! 🚀

