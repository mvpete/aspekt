# Aspekt API Reference

This document provides detailed API reference for all Aspekt components.

## Table of Contents
- [Core Classes](#core-classes)
- [Aspect Base Class](#aspect-base-class)
- [Method Arguments](#method-arguments)
- [Exit Handlers](#exit-handlers)
- [Contracts API](#contracts-api)
- [Logging API](#logging-api)

## Core Classes

### Aspect Class

The base class for all aspects in Aspekt.

```csharp
namespace Aspekt
{
    public abstract class Aspect
    {
        // Synchronous methods
        public virtual void OnEntry(MethodArguments args) { }
        public virtual void OnExit(MethodArguments args) { }
        public virtual void OnException(MethodArguments args, Exception exception) { }
        
        // Asynchronous methods
        public virtual ValueTask OnEntryAsync(MethodArguments args, CancellationToken cancellationToken = default) 
            => ValueTask.CompletedTask;
        public virtual ValueTask OnExitAsync(MethodArguments args, CancellationToken cancellationToken = default) 
            => ValueTask.CompletedTask;
        public virtual ValueTask OnExceptionAsync(MethodArguments args, Exception exception, CancellationToken cancellationToken = default) 
            => ValueTask.CompletedTask;
    }
}
```

#### Methods

**OnEntry(MethodArguments args)**
- Called before the method body executes
- Use for logging method entry, validation, setup, etc.
- Parameters:
  - `args`: Information about the intercepted method

**OnExit(MethodArguments args)**
- Called after the method body executes successfully
- Called before each return statement in the method
- Use for cleanup, logging method exit, etc.
- Parameters:
  - `args`: Information about the intercepted method

**OnException(MethodArguments args, Exception exception)**
- Called when the method throws an exception
- Use for error logging, exception handling, recovery, etc.
- Parameters:
  - `args`: Information about the intercepted method
  - `exception`: The exception that was thrown

**OnEntryAsync, OnExitAsync, OnExceptionAsync**
- Async versions of the above methods
- Return `ValueTask` for async operations
- Include `CancellationToken` parameter for cancellation support

## Method Arguments

### MethodArguments Class

Contains information about the intercepted method.

```csharp
public class MethodArguments
{
    public string Name { get; }           // Method name
    public string FullName { get; }       // Fully qualified method name
    public object? Target { get; }        // Instance (null for static methods)
    public Arguments Arguments { get; }   // Method parameters
}
```

#### Properties

**Name**
- Type: `string`
- The simple name of the method (e.g., "ProcessData")

**FullName**
- Type: `string`
- The fully qualified method name including namespace, class, and signature
- Example: "MyNamespace.MyClass.ProcessData(System.String, System.Int32)"

**Target**
- Type: `object?`
- The instance of the class containing the method
- `null` for static methods
- Cast to the appropriate type to access instance members

**Arguments**
- Type: `Arguments`
- Collection of method parameters and their values

### Arguments Class

Collection of method parameters.

```csharp
public class Arguments : IEnumerable<object?>
{
    public int Count { get; }
    public object? this[int index] { get; set; }
    public IReadOnlyList<object?> Values { get; }
    
    public IEnumerator<object?> GetEnumerator();
}
```

#### Usage Examples

```csharp
public override void OnEntry(MethodArguments args)
{
    Console.WriteLine($"Method: {args.Name}");
    Console.WriteLine($"Target: {args.Target?.GetType().Name ?? "Static"}");
    
    for (int i = 0; i < args.Arguments.Count; i++)
    {
        Console.WriteLine($"Arg[{i}]: {args.Arguments[i]}");
    }
}
```

## Exit Handlers

### IAspectExitHandler<T> Interface

Allows aspects to intercept and modify return values.

```csharp
public interface IAspectExitHandler<T>
{
    T OnExit(MethodArguments args, T result);
}
```

#### Usage

```csharp
public class StringModifierAspect : Aspect, IAspectExitHandler<string>
{
    public string OnExit(MethodArguments args, string result)
    {
        return $"[Modified] {result}";
    }
}

// Apply to method
[StringModifier]
public string GetMessage() => "Hello";
// Returns: "[Modified] Hello"
```

### Multiple Return Type Handlers

```csharp
public class MultiTypeAspect : Aspect, 
    IAspectExitHandler<string>, 
    IAspectExitHandler<int>
{
    public string OnExit(MethodArguments args, string result)
    {
        return result.ToUpper();
    }
    
    public int OnExit(MethodArguments args, int result)
    {
        return result * 2;
    }
}
```

## Contracts API

### Core Contract Classes

```csharp
namespace Aspekt.Contracts
{
    public static class Contract
    {
        public enum Comparison
        {
            EqualTo,
            NotEqualTo,
            GreaterThan,
            GreaterThanEqualTo,
            LessThan,
            LessThanEqualTo
        }
        
        public enum Constraint
        {
            NotNull
        }
        
        public enum Target
        {
            Property,
            Field,
            ReturnValue
        }
    }
}
```

### Contract Attributes

#### RequireAttribute

Specifies preconditions that must be true when a method is called.

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RequireAttribute : Attribute
{
    // Parameter validation
    public RequireAttribute(string parameterName, Contract.Comparison comparison, object value);
    public RequireAttribute(string parameterName, Contract.Constraint constraint);
    
    // State validation
    public RequireAttribute(Contract.Target target, string memberName, Contract.Comparison comparison, object value);
}
```

**Examples:**
```csharp
[Require(nameof(value), Contract.Comparison.GreaterThan, 0)]
public void ProcessValue(int value) { }

[Require(nameof(name), Contract.Constraint.NotNull)]
public void ProcessName(string name) { }

[Require(Contract.Target.Field, nameof(_initialized), Contract.Comparison.EqualTo, true)]
public void DoWork() { }
```

#### EnsureAttribute

Specifies postconditions that must be true when a method returns.

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class EnsureAttribute : Attribute
{
    // Return value validation
    public EnsureAttribute(Contract.Comparison comparison, object value);
    public EnsureAttribute(Contract.Constraint constraint);
    
    // State validation
    public EnsureAttribute(Contract.Target target, string memberName, Contract.Comparison comparison, object value);
}
```

**Examples:**
```csharp
[Ensure(Contract.Comparison.GreaterThan, 0)]
public int GetPositiveNumber() => 42;

[Ensure(Contract.Constraint.NotNull)]
public string GetText() => "Hello";

[Ensure(Contract.Target.Property, nameof(Count), Contract.Comparison.GreaterThan, 0)]
public void AddItem(string item) { }
```

#### InvariantAttribute

Specifies conditions that must always be true for a class.

```csharp
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class InvariantAttribute : Attribute
{
    public InvariantAttribute(string memberName, Contract.Comparison comparison, object value);
    public InvariantAttribute(string memberName, Contract.Constraint constraint);
}
```

**Examples:**
```csharp
public class Rectangle
{
    private double _width;
    
    [Invariant(nameof(_width), Contract.Comparison.GreaterThan, 0)]
    public double Width
    {
        get => _width;
        set => _width = value;
    }
}
```

### Contract Exceptions

```csharp
public class ContractException : Exception
{
    public string ContractType { get; }
    public string MemberName { get; }
    
    protected ContractException(string contractType, string memberName, string message);
}

public class PreconditionException : ContractException { }
public class PostconditionException : ContractException { }
public class InvariantException : ContractException { }
```

## Logging API

### LoggingAspect Class

Built-in logging aspect with configurable formatting.

```csharp
namespace Aspekt.Logging
{
    public class LoggingAspect : Aspect
    {
        public LoggingAspect(LogLevel level = LogLevel.Info, string? category = null);
        
        public override void OnEntry(MethodArguments args);
        public override void OnExit(MethodArguments args);
        public override void OnException(MethodArguments args, Exception exception);
    }
}
```

### LogLevel Enumeration

```csharp
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Info = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    None = 6
}
```

### ILogFormatter Interface

```csharp
public interface ILogFormatter
{
    string FormatEntry(MethodArguments args, LogLevel level);
    string FormatExit(MethodArguments args, LogLevel level);
    string FormatException(MethodArguments args, Exception exception, LogLevel level);
}
```

### Built-in Formatters

#### DefaultLogFormatter
```csharp
public class DefaultLogFormatter : ILogFormatter
{
    public DefaultLogFormatter(bool includeParameters = true, bool includeTimestamp = true);
}
```

#### JsonLogFormatter
```csharp
public class JsonLogFormatter : ILogFormatter
{
    public JsonLogFormatter(bool includeParameters = true);
}
```

### Usage Examples

```csharp
// Basic logging
[LoggingAspect]
public void DoWork() { }

// Custom log level and category
[LoggingAspect(LogLevel.Debug, "BusinessLogic")]
public void ProcessData() { }

// With custom formatter
[LoggingAspect(formatter: typeof(JsonLogFormatter))]
public void ApiCall() { }
```

## Utility Classes

### IgnoreAspectWarningAttribute

Suppresses warnings about aspects not being applied.

```csharp
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class IgnoreAspectWarningAttribute : Attribute { }
```

**Usage:**
```csharp
[LoggingAspect]
[IgnoreAspectWarning] // Suppresses warning if aspect can't be applied
private void InternalMethod() { }
```

## Build Integration

### MSBuild Targets

When using NuGet packages, these targets are automatically included:

```xml
<Target Name="AspektWeaving" AfterTargets="Build">
  <Exec Command="$(AspektBootstrapPath) &quot;$(OutputPath)$(AssemblyName).dll&quot;" 
        ContinueOnError="false" />
</Target>
```

### Manual Integration

For manual installation:

```xml
<PropertyGroup>
  <AspektBootstrapPath>path\to\Aspekt.Bootstrap.Host.exe</AspektBootstrapPath>
</PropertyGroup>

<Target Name="AspektWeaving" AfterTargets="Build">
  <Exec Command="$(AspektBootstrapPath) &quot;$(OutputPath)$(AssemblyName).dll&quot;" />
</Target>
```

## Thread Safety

### Thread-Safe Aspects

Aspects should be designed to be thread-safe:

```csharp
public class ThreadSafeAspect : Aspect
{
    private static readonly ConcurrentDictionary<string, int> CallCounts = new();
    
    public override void OnEntry(MethodArguments args)
    {
        CallCounts.AddOrUpdate(args.FullName, 1, (key, value) => value + 1);
    }
}
```

### Instance vs Static State

Be careful with instance vs static state in aspects:

```csharp
public class CountingAspect : Aspect
{
    private int _instanceCount = 0;              // New instance per method call
    private static int _staticCount = 0;         // Shared across all calls
    private static readonly object _lock = new();
    
    public override void OnEntry(MethodArguments args)
    {
        _instanceCount++;           // Thread-safe (new instance each time)
        
        lock (_lock)
        {
            _staticCount++;         // Requires synchronization
        }
    }
}
```

## Performance Considerations

### Async Best Practices

```csharp
public class AsyncAspect : Aspect
{
    public override async ValueTask OnEntryAsync(MethodArguments args, CancellationToken cancellationToken = default)
    {
        // Use ConfigureAwait(false) for library code
        await SomeAsyncOperation().ConfigureAwait(false);
    }
}
```

### Minimal Overhead Patterns

```csharp
public class PerformantAspect : Aspect
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    
    public override void OnEntry(MethodArguments args)
    {
        // Check if logging is enabled before expensive operations
        if (Logger.IsInfoEnabled)
        {
            Logger.Info($"Entering {args.Name}");
        }
    }
}
```

This API reference covers all major components of the Aspekt framework. For more examples and usage patterns, see the [Getting Started Guide](GETTING_STARTED.md) and [Examples](EXAMPLES.md).