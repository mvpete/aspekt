# Getting Started with Aspekt

Aspekt is a lightweight Aspect-Oriented Programming (AOP) foundation library for .NET that allows you to implement cross-cutting concerns using attributes. This guide will help you get started with Aspekt quickly.

## Table of Contents
- [Installation](#installation)
- [Basic Concepts](#basic-concepts)
- [Creating Your First Aspect](#creating-your-first-aspect)
- [Using Aspects](#using-aspects)
- [Advanced Features](#advanced-features)
- [Build Integration](#build-integration)
- [Examples](#examples)

## Installation

### NuGet Package (Recommended)
```bash
Install-Package Aspekt
```

When using the NuGet package, build integration is automatically configured.

### Manual Installation
1. Download the latest release from GitHub
2. Reference `Aspekt.dll` in your project
3. Configure post-build steps (see [Build Integration](#build-integration))

## Basic Concepts

### What is Aspect-Oriented Programming?
AOP allows you to separate cross-cutting concerns (like logging, security, caching) from your business logic. Instead of cluttering your methods with these concerns, you can apply them declaratively using attributes.

### Core Components
- **Aspect**: Base class for all aspects (`Aspekt.Aspect`)
- **MethodArguments**: Contains information about the intercepted method
- **Attribute-based**: Aspects are applied using C# attributes
- **Post-compilation**: Aspect weaving happens after compilation using IL manipulation

## Creating Your First Aspect

### Simple Logging Aspect
```csharp
using Aspekt;

public class SimpleLoggingAspect : Aspect
{
    private readonly string _prefix;
    
    public SimpleLoggingAspect(string prefix = "LOG")
    {
        _prefix = prefix;
    }
    
    public override void OnEntry(MethodArguments args)
    {
        Console.WriteLine($"[{_prefix}] Entering: {args.FullName}");
    }
    
    public override void OnExit(MethodArguments args)
    {
        Console.WriteLine($"[{_prefix}] Exiting: {args.FullName}");
    }
    
    public override void OnException(MethodArguments args, Exception ex)
    {
        Console.WriteLine($"[{_prefix}] Exception in {args.FullName}: {ex.Message}");
    }
}
```

### Performance Timing Aspect
```csharp
using System.Diagnostics;
using Aspekt;

public class TimingAspect : Aspect
{
    private Stopwatch? _stopwatch;
    
    public override void OnEntry(MethodArguments args)
    {
        _stopwatch = Stopwatch.StartNew();
    }
    
    public override void OnExit(MethodArguments args)
    {
        _stopwatch?.Stop();
        Console.WriteLine($"{args.FullName} executed in {_stopwatch?.ElapsedMilliseconds}ms");
    }
}
```

## Using Aspects

### Basic Usage
```csharp
public class BusinessService
{
    [SimpleLogging("BUSINESS")]
    public string ProcessData(string input)
    {
        // Your business logic here
        return input.ToUpper();
    }
    
    [Timing]
    [SimpleLogging]
    public void ExpensiveOperation()
    {
        // Multiple aspects can be applied
        Thread.Sleep(1000);
    }
}
```

### Method Information Access
```csharp
public class DetailedLoggingAspect : Aspect
{
    public override void OnEntry(MethodArguments args)
    {
        Console.WriteLine($"Method: {args.Name}");
        Console.WriteLine($"Full Name: {args.FullName}");
        Console.WriteLine($"Target Instance: {args.Target?.GetType().Name ?? "Static"}");
        
        if (args.Arguments.Count > 0)
        {
            Console.WriteLine("Parameters:");
            for (int i = 0; i < args.Arguments.Count; i++)
            {
                Console.WriteLine($"  [{i}] = {args.Arguments[i] ?? "null"}");
            }
        }
    }
}
```

## Advanced Features

### Async Support
```csharp
public class AsyncLoggingAspect : Aspect
{
    public override async ValueTask OnEntryAsync(MethodArguments args, CancellationToken cancellationToken = default)
    {
        await LogAsync($"Entering {args.FullName}", cancellationToken);
    }
    
    public override async ValueTask OnExitAsync(MethodArguments args, CancellationToken cancellationToken = default)
    {
        await LogAsync($"Exiting {args.FullName}", cancellationToken);
    }
    
    private async ValueTask LogAsync(string message, CancellationToken cancellationToken)
    {
        // Async logging implementation
        await Task.Delay(1, cancellationToken);
        Console.WriteLine(message);
    }
}
```

### Return Value Interception
```csharp
public class ResultModifierAspect : Aspect, IAspectExitHandler<string>
{
    public string OnExit(MethodArguments args, string result)
    {
        // Modify the return value
        return $"Modified: {result}";
    }
}

// Usage
[ResultModifier]
public string GetData()
{
    return "Original Data";
}
// Returns: "Modified: Original Data"
```

### Conditional Aspects
```csharp
public class ConditionalLoggingAspect : Aspect
{
    private readonly bool _enabled;
    
    public ConditionalLoggingAspect(bool enabled = true)
    {
        _enabled = enabled;
    }
    
    public override void OnEntry(MethodArguments args)
    {
        if (_enabled)
        {
            Console.WriteLine($"Entering: {args.FullName}");
        }
    }
}
```

## Build Integration

### Automatic (NuGet)
When using the NuGet package, build integration is automatic. The package includes MSBuild targets that run the Aspekt weaver after compilation.

### Manual Integration
Add this to your project file:

```xml
<Target Name="AspektWeaving" AfterTargets="Build">
  <Exec Command="Aspekt.Bootstrap.Host.exe $(OutputPath)$(AssemblyName).dll" />
</Target>
```

### Verification
To verify that aspects are being woven:

1. Build your project
2. Check build output for Aspekt messages
3. Use a .NET decompiler to inspect the generated IL
4. Run your application and verify aspect behavior

## Examples

### Complete Example: Caching Aspect
```csharp
using System.Collections.Concurrent;
using Aspekt;

public class CachingAspect : Aspect, IAspectExitHandler<object>
{
    private static readonly ConcurrentDictionary<string, object> Cache = new();
    private readonly int _timeoutSeconds;
    private readonly ConcurrentDictionary<string, DateTime> _cacheTimestamps = new();
    
    public CachingAspect(int timeoutSeconds = 300)
    {
        _timeoutSeconds = timeoutSeconds;
    }
    
    public override void OnEntry(MethodArguments args)
    {
        var key = GenerateKey(args);
        
        if (Cache.TryGetValue(key, out var cachedValue) && 
            _cacheTimestamps.TryGetValue(key, out var timestamp) &&
            DateTime.Now - timestamp < TimeSpan.FromSeconds(_timeoutSeconds))
        {
            // Return cached value (implementation depends on return type handling)
            Console.WriteLine($"Cache hit for {args.FullName}");
        }
    }
    
    public object OnExit(MethodArguments args, object result)
    {
        var key = GenerateKey(args);
        Cache[key] = result;
        _cacheTimestamps[key] = DateTime.Now;
        
        Console.WriteLine($"Cached result for {args.FullName}");
        return result;
    }
    
    private string GenerateKey(MethodArguments args)
    {
        var parameters = string.Join("|", args.Arguments.Values.Select(v => v?.ToString() ?? "null"));
        return $"{args.FullName}|{parameters}";
    }
}

// Usage
public class DataService
{
    [Caching(timeoutSeconds: 600)]
    public string GetExpensiveData(int id)
    {
        Thread.Sleep(2000); // Simulate expensive operation
        return $"Data for ID: {id}";
    }
}
```

### Security Aspect Example
```csharp
using System.Security.Principal;
using Aspekt;

public class RequireRoleAspect : Aspect
{
    private readonly string _requiredRole;
    
    public RequireRoleAspect(string requiredRole)
    {
        _requiredRole = requiredRole;
    }
    
    public override void OnEntry(MethodArguments args)
    {
        var user = Thread.CurrentPrincipal;
        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("User must be authenticated");
        }
        
        if (!user.IsInRole(_requiredRole))
        {
            throw new UnauthorizedAccessException($"User must have role: {_requiredRole}");
        }
    }
}

// Usage
public class AdminService
{
    [RequireRole("Administrator")]
    public void DeleteUser(int userId)
    {
        // Only administrators can execute this method
    }
}
```

## Best Practices

1. **Keep Aspects Focused**: Each aspect should handle one concern
2. **Minimize Performance Impact**: Avoid heavy operations in aspects
3. **Handle Exceptions**: Always consider exception scenarios
4. **Thread Safety**: Make aspects thread-safe when needed
5. **Testing**: Test both with and without aspects applied
6. **Documentation**: Document aspect behavior and side effects

## Next Steps

- Explore [Contracts](CONTRACTS.md) for Design by Contract support
- Check out the built-in logging aspects in `Aspekt.Logging`
- Review the test projects for more examples
- Read the API documentation for advanced features

## Troubleshooting

### Common Issues

**Aspects not being applied**:
- Verify post-build step is running
- Check that methods are not private or static (unless specifically supported)
- Ensure Aspekt.Bootstrap.Host is in your build path

**Runtime exceptions**:
- Check that all dependencies are available at runtime
- Verify aspect constructors don't throw exceptions
- Ensure proper exception handling in aspect methods

**Performance issues**:
- Profile your aspects to identify bottlenecks
- Consider async versions for I/O operations
- Use conditional logic to minimize overhead
