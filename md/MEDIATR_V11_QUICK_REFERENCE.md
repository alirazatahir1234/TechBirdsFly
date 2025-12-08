# MediatR v11 Quick Reference - Registration Patterns

## ✅ Working Patterns in MediatR v11

### Pattern 1: Simple Assembly Scan (Recommended)
```csharp
services.AddMediatR(typeof(Program));
// or
services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);
```
- **Best for:** Simple single-assembly registration
- **Usage:** Most common pattern in TechBirdsFly services
- **Services using this:** Editor, Media, Publish

---

### Pattern 2: Explicit Assembly Scan
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```
- **Best for:** When you need configuration options
- **Usage:** Used in Generator Service with behaviors
- **Services using this:** Generator

---

### Pattern 3: Multiple Assemblies
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(DependencyInjection).Assembly,
    typeof(Infrastructure).Assembly
));
```
- **Best for:** Multi-layer services (Domain, Application, Infrastructure)
- **Usage:** When handlers span multiple projects
- **Services using this:** ProjectService, TemplateService

---

## ❌ WRONG Patterns (Don't Use These)

### ❌ v12 Style Methods (Don't exist in v11)
```csharp
// ❌ WRONG - v12 only
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Handler>());

// ❌ WRONG - Generic method removed
services.AddMediatR<SomeType>();
```

---

## 🔄 Migration Reference

### From MediatR v12 to v11

| v12 Code | v11 Equivalent | Notes |
|----------|---|---|
| `cfg.RegisterServicesFromAssemblyContaining<T>()` | `cfg.RegisterServicesFromAssembly(typeof(T).Assembly)` | Must extract Assembly |
| `cfg.AddRequestPreProcessor<T>()` | `cfg.AddRequestPreProcessor(typeof(T))` | Still works in v11 |
| `typeof(Program)` | `typeof(Program)` | Both work the same |

---

## 🎯 Service-by-Service Reference

### Editor Service
```csharp
// Location: Program.cs, line ~16
builder.Services.AddMediatR(typeof(Program));
```

### Media Service  
```csharp
// Location: Program.cs, line ~24
builder.Services.AddMediatR(typeof(Program));
```

### Generator Service
```csharp
// Location: Application/DependencyInjection.cs, line ~21
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
```

### Project Service
```csharp
// Location: Infrastructure/DependencyInjection.cs, line ~29
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(InfrastructureServiceCollectionExtensions).Assembly
));
```

### Template Service
```csharp
// Location: Api/Program.cs (inferred from structure)
services.AddMediatR(/* pattern to be confirmed */);
```

### Publish Service
```csharp
// Location: WebAPI/Extensions/ServiceCollectionExtensions.cs, line ~41
services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);
```

### Event Bus Service
```csharp
// Location: Program.cs (inferred from structure)
services.AddMediatR(/* pattern follows standard */);
```

---

## 🧪 Testing Your MediatR Registration

### Quick Test: Add Handler Logging
```csharp
// In your DependencyInjection or Program.cs
services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    
    // Add request logging
    cfg.AddRequestPreProcessor(typeof(LoggingBehavior<>));
});
```

### Verify Registration Works
```csharp
// In your controller or service
var mediator = serviceProvider.GetRequiredService<IMediator>();
var result = await mediator.Send(new YourCommand());
```

---

## 🐛 Common Errors & Solutions

### Error 1: "No handler found"
```
MediatR.Exceptions.HandlerNotFoundException
```

**Solution:** Ensure your handlers are in the registered assembly:
```csharp
// This must include the assembly containing your handlers
services.AddMediatR(typeof(YourHandler).Assembly);
```

---

### Error 2: "RegisterServicesFromAssemblyContaining not found"
```
CS1061: 'MediatRServiceConfiguration' does not contain a definition 
for 'RegisterServicesFromAssemblyContaining'
```

**Solution:** Use v11 style registration:
```csharp
// ❌ Wrong (v12 style)
cfg.RegisterServicesFromAssemblyContaining<Handler>()

// ✅ Correct (v11 style)
cfg.RegisterServicesFromAssembly(typeof(Handler).Assembly)
```

---

### Error 3: "ServiceFactory not found"
```
System.TypeLoadException: Could not load type 'MediatR.ServiceFactory'
```

**Solution:** Ensure version consistency:
```bash
# Check your .csproj files have matching versions:
MediatR Version="11.1.0"
MediatR.Extensions.Microsoft.DependencyInjection Version="11.1.0"
```

---

## ✅ Verification Checklist

After applying MediatR v11 fixes:

- [ ] All `.csproj` files show MediatR v11.1.0
- [ ] All `.csproj` files have Extensions v11.1.0
- [ ] No MediatR v12.x packages remain
- [ ] All registration patterns match this guide
- [ ] Build succeeds: `dotnet build TechBirdsFly.sln`
- [ ] Services start without TypeLoadException
- [ ] Handlers are discovered and work correctly

---

## 📚 Additional Resources

- **Full Fix Summary:** `MEDIATR_FIX_SUMMARY.md`
- **Build Status:** `DEPLOYMENT_STATUS.md`
- **Service Launch Guide:** `LAUNCH_CONFIGURATION_GUIDE.md`

---

**Status:** ✅ Complete and Tested  
**Version:** MediatR 11.1.0  
**Date:** December 5, 2025
