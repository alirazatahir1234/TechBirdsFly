# 🔧 Phase A Deployment - Critical Fixes Required

**Status:** 2 Services blocked, 2 Services running ✅, 2 Services warnings (Kafka) ⚠️

---

## 🎯 Error Analysis & Solutions

### **ERROR 1: ImageService - EF Core CustomProperties Dictionary Mapping**

**Error Message:**
```
System.InvalidOperationException: Unable to determine the relationship represented by navigation 
'ImageMetadata.CustomProperties' of type 'Dictionary<string, string>'. Either manually configure 
the relationship, or ignore this property...
```

**Root Cause:** EF Core can't map a `Dictionary<string, string>` as a navigation property. Dictionaries need to be marked as `[NotMapped]` or ignored in `OnModelCreating`.

**Fix Required:** Find the `ImageMetadata` entity and mark `CustomProperties` as not mapped.

---

### **ERROR 2: AuthService - Invalid PostgreSQL Connection String**

**Error Message:**
```
System.ArgumentException: Couldn't set data source (Parameter 'data source')
KeyNotFoundException: The given key was not present in the dictionary
```

**Root Cause:** The connection string is malformed or missing. PostgreSQL connection strings need proper format:
```
Host=localhost;Port=5432;Database=dbname;Username=user;Password=pass
```

**Fix Required:** Check/fix the connection string in appsettings.json

---

## 🔍 Let Me Scan the Files

I'll find and fix both issues now:

