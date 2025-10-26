# Aspekt Troubleshooting Guide

This guide helps you diagnose and resolve common issues when using Aspekt.

## Table of Contents
- [Build Issues](#build-issues)
- [Aspect Not Being Applied](#aspect-not-being-applied)
- [Runtime Errors](#runtime-errors)
- [Performance Issues](#performance-issues)
- [Contract Issues](#contract-issues)
- [Async Issues](#async-issues)
- [Debugging Tips](#debugging-tips)

## Build Issues

### Build Fails with "Could not load file or assembly 'Aspekt.Bootstrap.Host'"

**Symptoms:**
```
Error: Could not load file or assembly 'Aspekt.Bootstrap.Host.exe' or one of its dependencies.
```

**Causes & Solutions:**

1. **Missing NuGet Package:**
   ```bash
   Install-Package Aspekt
   ```

2. **Corrupted Package Cache:**
   ```bash
   dotnet nuget locals all --clear
   dotnet restore
   ```

3. **Manual Installation Issues:**
   Ensure `Aspekt.Bootstrap.Host.exe` is in your project's output directory or build path.

### Post-Build Step Fails

**Symptoms:**
```
The command "Aspekt.Bootstrap.Host.exe MyAssembly.dll" exited with code 1.
```

**Solutions:**

1. **Check Assembly Path:**
   Verify the assembly path is correct in your post-build command:
   ```xml
   <Exec Command="$(AspektBootstrapPath) &quot;$(OutputPath)$(AssemblyName).dll&quot;" />
   ```

2. **Enable Detailed Logging:**
   Add verbose logging to see what's happening:
   ```xml
   <Exec Command="$(AspektBootstrapPath) &quot;$(OutputPath)$(AssemblyName).dll&quot; --verbose" />
   ```

3. **Check Dependencies:**
   Ensure all referenced assemblies are available in the output directory.

### .NET Version Compatibility Issues

**Symptoms:**
```
Could not load type 'System.ValueTask' from assembly 'mscorlib'
```

**Solutions:**

1. **Update Target Framework:**
   ```xml
   <TargetFramework>net8.0</TargetFramework>
   <!-- Or use multi-targeting -->
   <TargetFrameworks>net6.0;net8.0;net9.0</TargetFrameworks>
   ```

2. **Install Compatibility Packages:**
   For older frameworks:
   ```bash
   Install-Package System.Threading.Tasks.Extensions
   ```

## Aspect Not Being Applied

### Aspect Attributes Ignored

**Symptoms:**
- Methods with aspect attributes don't trigger aspect behavior
- No logging/timing/etc. occurs

**Common Causes & Solutions:**

1. **Private/Internal Methods:**
   ```csharp
   // ❌ Won't work - private method
   [LoggingAspect]
   private void DoWork() { }
   
   // ✅ Works - public method
   [LoggingAspect]
   public void DoWork() { }
   ```

2. **Static Methods (depends on aspect):**
   Some aspects don't support static methods. Check your aspect implementation.

3. **Post-Build Step Not Running:**
   Check if the weaving step executed:
   ```xml
   <Target Name="AspektWeaving" AfterTargets="Build">
     <Message Text="Running Aspekt weaver..." Importance="high" />
     <Exec Command="$(AspektBootstrapPath) &quot;$(OutputPath)$(AssemblyName).dll&quot;" />
   </Target>
   ```

4. **Assembly Loading Issues:**
   Ensure the woven assembly is being loaded, not the original:
   ```csharp
   // Check if assembly was modified
   var assembly = Assembly.GetExecutingAssembly();
   var hasAspektAttribute = assembly.GetCustomAttributes(typeof(ProcessedByAspektAttribute), false).Any();
   Console.WriteLine($"Assembly processed by Aspekt: {hasAspektAttribute}");
   ```

### Aspect Methods Not Called

**Check Aspect Implementation:**

```csharp
public class DebuggingAspect : Aspect
{
    public override void OnEntry(MethodArguments args)
    {
        // Add debug output to verify this is called
        System.Diagnostics.Debug.WriteLine($"Aspect OnEntry called for {args.Name}");
        Console.WriteLine($"Aspect OnEntry called for {args.Name}");
    }
}
```

## Runtime Errors

### NullReferenceException in Aspect Code

**Common Causes:**

1. **Uninitialized Aspect Fields:**
   ```csharp
   public class LoggingAspect : Aspect
   {
       private readonly ILogger _logger;
       
       public LoggingAspect()
       {
           // ❌ Logger not initialized
       }
       
       public LoggingAspect()
       {
           // ✅ Properly initialized
           _logger = LogManager.GetCurrentClassLogger();
       }
   }
   ```

2. **Accessing Target Instance on Static Methods:**
   ```csharp
   public override void OnEntry(MethodArguments args)
   {
       // ❌ args.Target will be null for static methods
       var instance = (MyClass)args.Target;
       
       // ✅ Check for null first
       if (args.Target is MyClass instance)
       {
           // Safe to use instance
       }
   }
   ```

### InvalidCastException When Accessing Arguments

**Problem:**
```csharp
public override void OnEntry(MethodArguments args)
{
    var stringArg = (string)args.Arguments[0]; // May throw InvalidCastException
}
```

**Solution:**
```csharp
public override void OnEntry(MethodArguments args)
{
    if (args.Arguments.Count > 0 && args.Arguments[0] is string stringArg)
    {
        // Safe to use stringArg
    }
}
```

### Aspect Constructor Exceptions

**Problem:**
Exceptions in aspect constructors can cause method calls to fail.

**Solution:**
```csharp
public class SafeAspect : Aspect
{
    private readonly ILogger? _logger;
    
    public SafeAspect()
    {
        try
        {
            _logger = LogManager.GetCurrentClassLogger();
        }
        catch (Exception ex)
        {
            // Fallback or log to console
            Console.WriteLine($"Failed to initialize logger: {ex.Message}");
        }
    }
    
    public override void OnEntry(MethodArguments args)
    {
        try
        {
            _logger?.Info($"Entering {args.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logging failed: {ex.Message}");
        }
    }
}
```

## Performance Issues

### Aspect Overhead Too High

**Measuring Performance:**
```csharp
public class PerformanceTestAspect : Aspect
{
    private static readonly ConcurrentDictionary<string, List<long>> Timings = new();
    private Stopwatch? _aspectStopwatch;
    
    public override void OnEntry(MethodArguments args)
    {
        _aspectStopwatch = Stopwatch.StartNew();
        // Your aspect logic here
    }
    
    public override void OnExit(MethodArguments args)
    {
        _aspectStopwatch?.Stop();
        
        var timings = Timings.GetOrAdd(args.Name, _ => new List<long>());
        lock (timings)
        {
            timings.Add(_aspectStopwatch?.ElapsedTicks ?? 0);
        }
    }
    
    public static void PrintAspectOverhead()
    {
        foreach (var kvp in Timings)
        {
            var avgTicks = kvp.Value.Average();
            var avgMs = TimeSpan.FromTicks((long)avgTicks).TotalMilliseconds;
            Console.WriteLine($"{kvp.Key}: {avgMs:F4}ms average aspect overhead");
        }
    }
}
```

**Optimization Strategies:**

1. **Minimize Work in Aspects:**
   ```csharp
   // ❌ Expensive operations
   public override void OnEntry(MethodArguments args)
   {
       var expensiveData = CallDatabaseForLoggingInfo();
       Console.WriteLine($"Expensive: {expensiveData}");
   }
   
   // ✅ Lightweight operations
   public override void OnEntry(MethodArguments args)
   {
       Console.WriteLine($"Entering: {args.Name}");
   }
   ```

2. **Use Conditional Logic:**
   ```csharp
   public override void OnEntry(MethodArguments args)
   {
       if (!IsLoggingEnabled) return;
       
       // Only do work when needed
       DoLogging(args);
   }
   ```

3. **Async for I/O Operations:**
   ```csharp
   public override async ValueTask OnEntryAsync(MethodArguments args, CancellationToken cancellationToken)
   {
       await LogToRemoteServiceAsync(args, cancellationToken);
   }
   ```

### Memory Leaks in Aspects

**Common Issues:**

1. **Event Handlers Not Unsubscribed:**
   ```csharp
   public class EventAspect : Aspect, IDisposable
   {
       private readonly IEventPublisher _publisher;
       
       public EventAspect()
       {
           _publisher = EventPublisher.Instance;
           _publisher.SomeEvent += OnEvent; // ❌ Never unsubscribed
       }
       
       public void Dispose()
       {
           _publisher.SomeEvent -= OnEvent; // ✅ Proper cleanup
       }
   }
   ```

2. **Static Collections Growing Indefinitely:**
   ```csharp
   // ❌ Can cause memory leaks
   private static readonly Dictionary<string, object> Cache = new();
   
   // ✅ Use expiring cache
   private static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions
   {
       SizeLimit = 1000
   });
   ```

## Contract Issues

### Contract Violations Not Detected

**Check Contract Configuration:**

1. **Ensure Contracts Package is Installed:**
   ```bash
   Install-Package Aspekt.Contracts
   ```

2. **Verify Contract Attributes:**
   ```csharp
   // ✅ Correct usage
   [Require(nameof(value), Contract.Comparison.GreaterThan, 0)]
   public void ProcessValue(int value) { }
   
   // ❌ Incorrect parameter name
   [Require("wrongName", Contract.Comparison.GreaterThan, 0)]
   public void ProcessValue(int value) { } // Contract won't work
   ```

### False Positive Contract Violations

**Common Issues:**

1. **Floating Point Comparisons:**
   ```csharp
   // ❌ Problematic due to floating point precision
   [Ensure(Contract.Comparison.EqualTo, 1.0)]
   public double Calculate() => 0.1 + 0.2 + 0.7;
   
   // ✅ Use appropriate tolerance
   [Ensure(Contract.Comparison.GreaterThan, 0.99)]
   [Ensure(Contract.Comparison.LessThan, 1.01)]
   public double Calculate() => 0.1 + 0.2 + 0.7;
   ```

2. **Race Conditions in Multi-threaded Code:**
   ```csharp
   private volatile bool _initialized;
   
   // ❌ Race condition possible
   [Require(Contract.Target.Field, nameof(_initialized), Contract.Comparison.EqualTo, true)]
   public void DoWork() { }
   
   // ✅ Use proper synchronization
   private readonly object _lock = new();
   
   [Require(Contract.Target.Field, nameof(_initialized), Contract.Comparison.EqualTo, true)]
   public void DoWork()
   {
       lock (_lock)
       {
           // Work here
       }
   }
   ```

## Async Issues

### Deadlocks with Async Aspects

**Problem:**
```csharp
public class DeadlockAspect : Aspect
{
    public override void OnEntry(MethodArguments args)
    {
        // ❌ Can cause deadlocks
        SomeAsyncMethod().Wait();
    }
}
```

**Solution:**
```csharp
public class AsyncSafeAspect : Aspect
{
    public override async ValueTask OnEntryAsync(MethodArguments args, CancellationToken cancellationToken)
    {
        // ✅ Proper async usage
        await SomeAsyncMethod().ConfigureAwait(false);
    }
}
```

### Mixed Sync/Async Aspects

**Issue:**
Mixing synchronous and asynchronous aspect methods can cause issues.

**Solution:**
Choose one approach consistently:

```csharp
// ✅ All sync
public class SyncAspect : Aspect
{
    public override void OnEntry(MethodArguments args) { }
    public override void OnExit(MethodArguments args) { }
}

// ✅ All async
public class AsyncAspect : Aspect
{
    public override ValueTask OnEntryAsync(MethodArguments args, CancellationToken cancellationToken) { }
    public override ValueTask OnExitAsync(MethodArguments args, CancellationToken cancellationToken) { }
}
```

## Debugging Tips

### Enable Aspekt Verbose Logging

Add to your project file:
```xml
<PropertyGroup>
  <AspektVerbose>true</AspektVerbose>
</PropertyGroup>
```

### Use Debug Aspects

Create a debug aspect to understand execution flow:
```csharp
public class DebugAspect : Aspect
{
    public override void OnEntry(MethodArguments args)
    {
        System.Diagnostics.Debug.WriteLine($"=== ENTRY: {args.FullName} ===");
        System.Diagnostics.Debug.WriteLine($"Target: {args.Target?.GetType().Name ?? "static"}");
        System.Diagnostics.Debug.WriteLine($"Arguments: {args.Arguments.Count}");
        
        for (int i = 0; i < args.Arguments.Count; i++)
        {
            System.Diagnostics.Debug.WriteLine($"  [{i}] {args.Arguments[i]?.GetType().Name}: {args.Arguments[i]}");
        }
    }
    
    public override void OnExit(MethodArguments args)
    {
        System.Diagnostics.Debug.WriteLine($"=== EXIT: {args.FullName} ===");
    }
    
    public override void OnException(MethodArguments args, Exception exception)
    {
        System.Diagnostics.Debug.WriteLine($"=== EXCEPTION: {args.FullName} ===");
        System.Diagnostics.Debug.WriteLine($"Exception: {exception}");
    }
}
```

### Inspect Woven Assembly

Use tools like ILSpy or dotPeek to inspect the woven assembly and verify that aspects were applied correctly.

### Test Without Aspects

Create builds with and without aspects to isolate issues:

```xml
<!-- Conditional post-build step -->
<Target Name="AspektWeaving" AfterTargets="Build" Condition="'$(EnableAspekt)' == 'true'">
  <Exec Command="$(AspektBootstrapPath) &quot;$(OutputPath)$(AssemblyName).dll&quot;" />
</Target>
```

Build with: `dotnet build -p:EnableAspekt=true`

### Common Error Messages

**"Method not found: 'Void Aspekt.Aspect..ctor()'"**
- Your aspect doesn't have a parameterless constructor, but one is required by the weaver.

**"Could not resolve type 'Aspekt.MethodArguments'"**
- Reference to Aspekt.dll is missing or incorrect version.

**"Aspect type must inherit from Aspekt.Aspect"**
- Your aspect class doesn't properly inherit from the base Aspect class.

## Getting Help

When reporting issues, include:

1. **Aspekt version**
2. **.NET version and target framework**
3. **Complete error message and stack trace**
4. **Minimal reproducible example**
5. **Build output with verbose logging enabled**

For community support:
- GitHub Issues: Report bugs and feature requests
- Stack Overflow: Use the `aspekt` tag for questions
- Documentation: Check the docs folder for detailed guides

Remember that aspect-oriented programming can introduce complexity, so start simple and gradually add more sophisticated aspects as you become comfortable with the framework.