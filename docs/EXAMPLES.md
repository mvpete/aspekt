# Aspekt Examples

This document provides comprehensive, real-world examples of using Aspekt for various scenarios.

## Table of Contents
- [Basic Examples](#basic-examples)
- [Logging Examples](#logging-examples)
- [Performance Examples](#performance-examples)
- [Security Examples](#security-examples)
- [Caching Examples](#caching-examples)
- [Error Handling Examples](#error-handling-examples)
- [Contract Examples](#contract-examples)
- [Async Examples](#async-examples)
- [Advanced Examples](#advanced-examples)

## Basic Examples

### Simple Method Logging

```csharp
using Aspekt;

public class SimpleLoggingAspect : Aspect
{
    public override void OnEntry(MethodArguments args)
    {
        Console.WriteLine($"[LOG] Entering method: {args.Name}");
    }
    
    public override void OnExit(MethodArguments args)
    {
        Console.WriteLine($"[LOG] Exiting method: {args.Name}");
    }
}

public class Calculator
{
    [SimpleLogging]
    public int Add(int a, int b)
    {
        return a + b;
    }
    
    [SimpleLogging]
    public double Divide(double dividend, double divisor)
    {
        return dividend / divisor;
    }
}

// Usage
var calc = new Calculator();
var result = calc.Add(5, 3); // Logs entry and exit
```

### Parameter Logging

```csharp
public class ParameterLoggingAspect : Aspect
{
    public override void OnEntry(MethodArguments args)
    {
        Console.WriteLine($"Calling {args.Name} with parameters:");
        for (int i = 0; i < args.Arguments.Count; i++)
        {
            Console.WriteLine($"  [{i}] = {args.Arguments[i] ?? "null"}");
        }
    }
}

public class UserService
{
    [ParameterLogging]
    public User CreateUser(string name, int age, string email)
    {
        return new User { Name = name, Age = age, Email = email };
    }
}

// Output:
// Calling CreateUser with parameters:
//   [0] = John Doe
//   [1] = 30
//   [2] = john@example.com
```

## Logging Examples

### Structured Logging with Serilog

```csharp
using Serilog;
using Aspekt;

public class StructuredLoggingAspect : Aspect
{
    private static readonly ILogger Logger = Log.ForContext<StructuredLoggingAspect>();
    
    public override void OnEntry(MethodArguments args)
    {
        Logger.Information("Method {MethodName} called with {ParameterCount} parameters",
            args.Name, args.Arguments.Count);
    }
    
    public override void OnExit(MethodArguments args)
    {
        Logger.Information("Method {MethodName} completed successfully", args.Name);
    }
    
    public override void OnException(MethodArguments args, Exception ex)
    {
        Logger.Error(ex, "Method {MethodName} threw exception", args.Name);
    }
}

public class OrderService
{
    [StructuredLogging]
    public async Task<Order> ProcessOrderAsync(int orderId, decimal amount)
    {
        // Simulate processing
        await Task.Delay(100);
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
            
        return new Order { Id = orderId, Amount = amount };
    }
}
```

### Conditional Logging

```csharp
public class ConditionalLoggingAspect : Aspect
{
    private readonly LogLevel _logLevel;
    private readonly Func<MethodArguments, bool>? _condition;
    
    public ConditionalLoggingAspect(LogLevel logLevel = LogLevel.Info, 
                                   Func<MethodArguments, bool>? condition = null)
    {
        _logLevel = logLevel;
        _condition = condition;
    }
    
    public override void OnEntry(MethodArguments args)
    {
        if (ShouldLog(args))
        {
            Console.WriteLine($"[{_logLevel}] Entering: {args.FullName}");
        }
    }
    
    private bool ShouldLog(MethodArguments args)
    {
        return _condition?.Invoke(args) ?? true;
    }
}

// Usage: Only log methods with more than 2 parameters
public class BusinessService
{
    [ConditionalLogging(LogLevel.Debug, args => args.Arguments.Count > 2)]
    public void ProcessData(string data) { } // Won't log
    
    [ConditionalLogging(LogLevel.Debug, args => args.Arguments.Count > 2)]
    public void ProcessComplexData(string data, int id, bool flag) { } // Will log
}
```

## Performance Examples

### Method Timing

```csharp
using System.Diagnostics;

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
        Console.WriteLine($"{args.Name} executed in {_stopwatch?.ElapsedMilliseconds}ms");
    }
}

public class DataService
{
    [Timing]
    public List<Customer> GetCustomers()
    {
        Thread.Sleep(150); // Simulate database call
        return new List<Customer>();
    }
    
    [Timing]
    public async Task<Order[]> GetOrdersAsync(int customerId)
    {
        await Task.Delay(200); // Simulate async database call
        return Array.Empty<Order>();
    }
}
```

### Performance Monitoring

```csharp
public class PerformanceMonitoringAspect : Aspect
{
    private static readonly ConcurrentDictionary<string, PerformanceMetrics> Metrics = new();
    private Stopwatch? _stopwatch;
    
    public override void OnEntry(MethodArguments args)
    {
        _stopwatch = Stopwatch.StartNew();
        
        var metrics = Metrics.GetOrAdd(args.FullName, _ => new PerformanceMetrics());
        Interlocked.Increment(ref metrics.CallCount);
    }
    
    public override void OnExit(MethodArguments args)
    {
        if (_stopwatch != null)
        {
            _stopwatch.Stop();
            var metrics = Metrics[args.FullName];
            
            lock (metrics)
            {
                metrics.TotalExecutionTime += _stopwatch.ElapsedMilliseconds;
                metrics.AverageExecutionTime = metrics.TotalExecutionTime / metrics.CallCount;
                
                if (_stopwatch.ElapsedMilliseconds > metrics.MaxExecutionTime)
                    metrics.MaxExecutionTime = _stopwatch.ElapsedMilliseconds;
            }
        }
    }
    
    public static void PrintMetrics()
    {
        foreach (var kvp in Metrics)
        {
            var method = kvp.Key;
            var metrics = kvp.Value;
            
            Console.WriteLine($"Method: {method}");
            Console.WriteLine($"  Calls: {metrics.CallCount}");
            Console.WriteLine($"  Avg Time: {metrics.AverageExecutionTime:F2}ms");
            Console.WriteLine($"  Max Time: {metrics.MaxExecutionTime}ms");
            Console.WriteLine();
        }
    }
}

public class PerformanceMetrics
{
    public long CallCount;
    public long TotalExecutionTime;
    public double AverageExecutionTime;
    public long MaxExecutionTime;
}
```

## Security Examples

### Authorization Aspect

```csharp
using System.Security.Principal;

public class RequireRoleAttribute : Aspect
{
    private readonly string[] _requiredRoles;
    
    public RequireRoleAttribute(params string[] requiredRoles)
    {
        _requiredRoles = requiredRoles;
    }
    
    public override void OnEntry(MethodArguments args)
    {
        var user = Thread.CurrentPrincipal;
        
        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("Authentication required");
        }
        
        if (_requiredRoles.Length > 0 && !_requiredRoles.Any(role => user.IsInRole(role)))
        {
            throw new UnauthorizedAccessException(
                $"User must have one of the following roles: {string.Join(", ", _requiredRoles)}");
        }
    }
}

public class AdminService
{
    [RequireRole("Administrator")]
    public void DeleteUser(int userId)
    {
        // Only administrators can execute this
    }
    
    [RequireRole("Manager", "Administrator")]
    public void ApproveOrder(int orderId)
    {
        // Managers or administrators can execute this
    }
}
```

### Audit Trail

```csharp
public class AuditAspect : Aspect
{
    private static readonly ILogger AuditLogger = LogManager.GetLogger("Audit");
    
    public override void OnEntry(MethodArguments args)
    {
        var user = Thread.CurrentPrincipal?.Identity?.Name ?? "Anonymous";
        var parameters = string.Join(", ", args.Arguments.Values.Select(v => v?.ToString() ?? "null"));
        
        AuditLogger.Info($"User {user} called {args.FullName} with parameters: {parameters}");
    }
}

public class PaymentService
{
    [Audit]
    public void ProcessPayment(decimal amount, string accountNumber)
    {
        // Payment processing logic
    }
    
    [Audit]
    public void RefundPayment(string transactionId, decimal amount)
    {
        // Refund processing logic
    }
}
```

## Caching Examples

### Simple Method Caching

```csharp
using System.Collections.Concurrent;

public class CacheAttribute : Aspect, IAspectExitHandler<object>
{
    private static readonly ConcurrentDictionary<string, CacheEntry> Cache = new();
    private readonly TimeSpan _expiration;
    
    public CacheAttribute(int expirationMinutes = 5)
    {
        _expiration = TimeSpan.FromMinutes(expirationMinutes);
    }
    
    public override void OnEntry(MethodArguments args)
    {
        var key = GenerateCacheKey(args);
        
        if (Cache.TryGetValue(key, out var entry) && 
            DateTime.UtcNow - entry.CreatedAt <= _expiration)
        {
            // Would need to handle early return here
            // This is a simplified example
        }
    }
    
    public object OnExit(MethodArguments args, object result)
    {
        var key = GenerateCacheKey(args);
        Cache[key] = new CacheEntry { Value = result, CreatedAt = DateTime.UtcNow };
        return result;
    }
    
    private string GenerateCacheKey(MethodArguments args)
    {
        var parameters = string.Join("|", args.Arguments.Values.Select(v => v?.ToString() ?? "null"));
        return $"{args.FullName}|{parameters}";
    }
    
    public static void ClearCache() => Cache.Clear();
}

public class CacheEntry
{
    public object Value { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class ProductService
{
    [Cache(expirationMinutes: 10)]
    public Product GetProduct(int productId)
    {
        // Simulate expensive database call
        Thread.Sleep(100);
        return new Product { Id = productId, Name = $"Product {productId}" };
    }
}
```

### Redis Caching

```csharp
using StackExchange.Redis;
using Newtonsoft.Json;

public class RedisCacheAttribute : Aspect, IAspectExitHandler<object>
{
    private static readonly Lazy<ConnectionMultiplexer> Connection = new(() =>
        ConnectionMultiplexer.Connect("localhost"));
    
    private readonly int _expirationMinutes;
    
    public RedisCacheAttribute(int expirationMinutes = 5)
    {
        _expirationMinutes = expirationMinutes;
    }
    
    private IDatabase Database => Connection.Value.GetDatabase();
    
    public object OnExit(MethodArguments args, object result)
    {
        var key = GenerateCacheKey(args);
        var serialized = JsonConvert.SerializeObject(result);
        
        Database.StringSet(key, serialized, TimeSpan.FromMinutes(_expirationMinutes));
        
        return result;
    }
    
    private string GenerateCacheKey(MethodArguments args)
    {
        var parameters = string.Join("|", args.Arguments.Values.Select(v => v?.ToString() ?? "null"));
        return $"cache:{args.FullName}:{parameters}";
    }
}
```

## Error Handling Examples

### Retry Logic

```csharp
public class RetryAttribute : Aspect
{
    private readonly int _maxAttempts;
    private readonly TimeSpan _delay;
    private readonly Type[] _retryOnExceptions;
    
    public RetryAttribute(int maxAttempts = 3, int delayMilliseconds = 1000, params Type[] retryOnExceptions)
    {
        _maxAttempts = maxAttempts;
        _delay = TimeSpan.FromMilliseconds(delayMilliseconds);
        _retryOnExceptions = retryOnExceptions.Length > 0 ? retryOnExceptions : new[] { typeof(Exception) };
    }
    
    public override void OnException(MethodArguments args, Exception exception)
    {
        // This is a simplified example - actual retry logic would need
        // to be implemented at the IL level for proper method re-execution
        
        if (ShouldRetry(exception))
        {
            Console.WriteLine($"Retrying {args.Name} due to {exception.GetType().Name}");
        }
    }
    
    private bool ShouldRetry(Exception exception)
    {
        return _retryOnExceptions.Any(type => type.IsAssignableFrom(exception.GetType()));
    }
}

public class NetworkService
{
    [Retry(maxAttempts: 3, delayMilliseconds: 2000, typeof(HttpRequestException), typeof(TimeoutException))]
    public async Task<string> FetchDataAsync(string url)
    {
        using var client = new HttpClient();
        return await client.GetStringAsync(url);
    }
}
```

### Circuit Breaker Pattern

```csharp
public class CircuitBreakerAttribute : Aspect
{
    private static readonly ConcurrentDictionary<string, CircuitBreakerState> States = new();
    
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    
    public CircuitBreakerAttribute(int failureThreshold = 5, int timeoutSeconds = 30)
    {
        _failureThreshold = failureThreshold;
        _timeout = TimeSpan.FromSeconds(timeoutSeconds);
    }
    
    public override void OnEntry(MethodArguments args)
    {
        var state = States.GetOrAdd(args.FullName, _ => new CircuitBreakerState());
        
        if (state.State == CircuitState.Open)
        {
            if (DateTime.UtcNow - state.LastFailureTime > _timeout)
            {
                state.State = CircuitState.HalfOpen;
            }
            else
            {
                throw new CircuitBreakerOpenException($"Circuit breaker is open for {args.Name}");
            }
        }
    }
    
    public override void OnExit(MethodArguments args)
    {
        var state = States[args.FullName];
        
        if (state.State == CircuitState.HalfOpen)
        {
            state.State = CircuitState.Closed;
            state.FailureCount = 0;
        }
    }
    
    public override void OnException(MethodArguments args, Exception exception)
    {
        var state = States[args.FullName];
        
        state.FailureCount++;
        state.LastFailureTime = DateTime.UtcNow;
        
        if (state.FailureCount >= _failureThreshold)
        {
            state.State = CircuitState.Open;
        }
    }
}

public class CircuitBreakerState
{
    public CircuitState State { get; set; } = CircuitState.Closed;
    public int FailureCount { get; set; }
    public DateTime LastFailureTime { get; set; }
}

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}

public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message) { }
}
```

## Contract Examples

### Banking System with Contracts

```csharp
using Aspekt.Contracts;

public class BankAccount
{
    private decimal _balance;
    private readonly string _accountNumber;
    private bool _isActive = true;
    
    // Invariants
    [Invariant(nameof(_balance), Contract.Comparison.GreaterThanEqualTo, 0)]
    public decimal Balance => _balance;
    
    [Invariant(nameof(_accountNumber), Contract.Constraint.NotNull)]
    public string AccountNumber => _accountNumber;
    
    public bool IsActive => _isActive;
    
    public BankAccount(string accountNumber, decimal initialBalance = 0)
    {
        _accountNumber = accountNumber ?? throw new ArgumentNullException(nameof(accountNumber));
        _balance = initialBalance;
    }
    
    [Require(nameof(amount), Contract.Comparison.GreaterThan, 0)]
    [Require(Contract.Target.Property, nameof(IsActive), Contract.Comparison.EqualTo, true)]
    [Ensure(Contract.Comparison.GreaterThanEqualTo, 0)]
    public decimal Deposit(decimal amount)
    {
        _balance += amount;
        return _balance;
    }
    
    [Require(nameof(amount), Contract.Comparison.GreaterThan, 0)]
    [Require(nameof(amount), Contract.Comparison.LessThanEqualTo, nameof(_balance))]
    [Require(Contract.Target.Property, nameof(IsActive), Contract.Comparison.EqualTo, true)]
    [Ensure(Contract.Comparison.GreaterThanEqualTo, 0)]
    public decimal Withdraw(decimal amount)
    {
        _balance -= amount;
        return _balance;
    }
    
    [Require(Contract.Target.Property, nameof(IsActive), Contract.Comparison.EqualTo, true)]
    public void CloseAccount()
    {
        _isActive = false;
        _balance = 0;
    }
}

// Usage
var account = new BankAccount("12345", 1000);

account.Deposit(500);    // OK
account.Withdraw(200);   // OK
// account.Withdraw(2000); // Throws PreconditionException
account.CloseAccount();
// account.Deposit(100);   // Throws PreconditionException (account not active)
```

### Data Validation with Contracts

```csharp
public class User
{
    private string _email = string.Empty;
    private int _age;
    
    [Invariant(nameof(_email), Contract.Constraint.NotNull)]
    public string Email
    {
        get => _email;
        set => _email = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    [Invariant(nameof(_age), Contract.Comparison.GreaterThanEqualTo, 0)]
    [Invariant(nameof(_age), Contract.Comparison.LessThan, 150)]
    public int Age
    {
        get => _age;
        set => _age = value;
    }
    
    [Require(nameof(email), Contract.Constraint.NotNull)]
    [Require(nameof(age), Contract.Comparison.GreaterThanEqualTo, 0)]
    [Ensure(Contract.Constraint.NotNull)]
    public User CreateUser(string email, int age)
    {
        return new User { Email = email, Age = age };
    }
}
```

## Async Examples

### Async Logging

```csharp
public class AsyncLoggingAspect : Aspect
{
    private static readonly SemaphoreSlim LogSemaphore = new(1, 1);
    
    public override async ValueTask OnEntryAsync(MethodArguments args, CancellationToken cancellationToken = default)
    {
        await LogAsync($"[ASYNC] Entering: {args.FullName}", cancellationToken);
    }
    
    public override async ValueTask OnExitAsync(MethodArguments args, CancellationToken cancellationToken = default)
    {
        await LogAsync($"[ASYNC] Exiting: {args.FullName}", cancellationToken);
    }
    
    public override async ValueTask OnExceptionAsync(MethodArguments args, Exception exception, CancellationToken cancellationToken = default)
    {
        await LogAsync($"[ASYNC] Exception in {args.FullName}: {exception.Message}", cancellationToken);
    }
    
    private async ValueTask LogAsync(string message, CancellationToken cancellationToken)
    {
        await LogSemaphore.WaitAsync(cancellationToken);
        try
        {
            // Simulate async logging (e.g., to database or remote service)
            await Task.Delay(1, cancellationToken);
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff} {message}");
        }
        finally
        {
            LogSemaphore.Release();
        }
    }
}

public class AsyncService
{
    [AsyncLogging]
    public async Task<string> ProcessDataAsync(string input)
    {
        await Task.Delay(100);
        return input.ToUpper();
    }
    
    [AsyncLogging]
    public async ValueTask<int> CalculateAsync(int value)
    {
        await Task.Delay(50);
        return value * 2;
    }
}
```

### Async Rate Limiting

```csharp
public class RateLimitAttribute : Aspect
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Semaphores = new();
    private readonly int _maxConcurrency;
    private readonly TimeSpan _timeout;
    
    public RateLimitAttribute(int maxConcurrency = 10, int timeoutSeconds = 30)
    {
        _maxConcurrency = maxConcurrency;
        _timeout = TimeSpan.FromSeconds(timeoutSeconds);
    }
    
    public override async ValueTask OnEntryAsync(MethodArguments args, CancellationToken cancellationToken = default)
    {
        var semaphore = Semaphores.GetOrAdd(args.FullName, _ => new SemaphoreSlim(_maxConcurrency, _maxConcurrency));
        
        var acquired = await semaphore.WaitAsync(_timeout, cancellationToken);
        if (!acquired)
        {
            throw new TimeoutException($"Rate limit exceeded for {args.Name}");
        }
    }
    
    public override ValueTask OnExitAsync(MethodArguments args, CancellationToken cancellationToken = default)
    {
        var semaphore = Semaphores[args.FullName];
        semaphore.Release();
        return ValueTask.CompletedTask;
    }
    
    public override ValueTask OnExceptionAsync(MethodArguments args, Exception exception, CancellationToken cancellationToken = default)
    {
        var semaphore = Semaphores[args.FullName];
        semaphore.Release();
        return ValueTask.CompletedTask;
    }
}

public class ApiController
{
    [RateLimit(maxConcurrency: 5, timeoutSeconds: 10)]
    public async Task<ApiResponse> ProcessRequestAsync(ApiRequest request)
    {
        // Simulate API processing
        await Task.Delay(200);
        return new ApiResponse { Success = true };
    }
}
```

## Advanced Examples

### Multi-Aspect Composition

```csharp
public class CompositeService
{
    [Timing]
    [Logging]
    [Audit]
    [Cache(expirationMinutes: 15)]
    public Customer GetCustomerById(int customerId)
    {
        // Method will be wrapped with all four aspects
        // Execution order: Timing -> Logging -> Audit -> Cache -> Method -> Cache -> Audit -> Logging -> Timing
        
        Thread.Sleep(100); // Simulate database call
        return new Customer { Id = customerId, Name = $"Customer {customerId}" };
    }
}
```

### Custom Metrics Collection

```csharp
public class MetricsAspect : Aspect
{
    private static readonly IMetricsCollector Metrics = new MetricsCollector();
    private Stopwatch? _stopwatch;
    
    public override void OnEntry(MethodArguments args)
    {
        _stopwatch = Stopwatch.StartNew();
        Metrics.IncrementCounter($"method.calls.{args.Name}");
    }
    
    public override void OnExit(MethodArguments args)
    {
        if (_stopwatch != null)
        {
            _stopwatch.Stop();
            Metrics.RecordTiming($"method.duration.{args.Name}", _stopwatch.ElapsedMilliseconds);
            Metrics.IncrementCounter($"method.success.{args.Name}");
        }
    }
    
    public override void OnException(MethodArguments args, Exception exception)
    {
        Metrics.IncrementCounter($"method.error.{args.Name}");
        Metrics.IncrementCounter($"method.error.{args.Name}.{exception.GetType().Name}");
    }
}

public interface IMetricsCollector
{
    void IncrementCounter(string name);
    void RecordTiming(string name, long milliseconds);
}

public class MetricsCollector : IMetricsCollector
{
    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, List<long>> _timings = new();
    
    public void IncrementCounter(string name)
    {
        _counters.AddOrUpdate(name, 1, (_, count) => count + 1);
    }
    
    public void RecordTiming(string name, long milliseconds)
    {
        _timings.AddOrUpdate(name, new List<long> { milliseconds }, (_, list) =>
        {
            lock (list)
            {
                list.Add(milliseconds);
                return list;
            }
        });
    }
    
    public void PrintMetrics()
    {
        Console.WriteLine("=== COUNTERS ===");
        foreach (var kvp in _counters.OrderBy(x => x.Key))
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }
        
        Console.WriteLine("\n=== TIMINGS ===");
        foreach (var kvp in _timings.OrderBy(x => x.Key))
        {
            var avg = kvp.Value.Average();
            var min = kvp.Value.Min();
            var max = kvp.Value.Max();
            Console.WriteLine($"{kvp.Key}: avg={avg:F2}ms, min={min}ms, max={max}ms, count={kvp.Value.Count}");
        }
    }
}
```

### Distributed Tracing

```csharp
using System.Diagnostics;

public class TracingAspect : Aspect
{
    private static readonly ActivitySource ActivitySource = new("MyApplication");
    private Activity? _activity;
    
    public override void OnEntry(MethodArguments args)
    {
        _activity = ActivitySource.StartActivity(args.Name);
        _activity?.SetTag("method.fullname", args.FullName);
        _activity?.SetTag("method.parameters.count", args.Arguments.Count.ToString());
        
        for (int i = 0; i < args.Arguments.Count; i++)
        {
            _activity?.SetTag($"method.parameter.{i}", args.Arguments[i]?.ToString() ?? "null");
        }
    }
    
    public override void OnExit(MethodArguments args)
    {
        _activity?.SetTag("method.result", "success");
        _activity?.Dispose();
    }
    
    public override void OnException(MethodArguments args, Exception exception)
    {
        _activity?.SetTag("method.result", "error");
        _activity?.SetTag("error.type", exception.GetType().Name);
        _activity?.SetTag("error.message", exception.Message);
        _activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
        _activity?.Dispose();
    }
}

public class OrderService
{
    [Tracing]
    public async Task<Order> CreateOrderAsync(int customerId, decimal amount)
    {
        // This method will be traced with distributed tracing
        await ProcessPaymentAsync(amount);
        return new Order { CustomerId = customerId, Amount = amount };
    }
    
    [Tracing]
    private async Task ProcessPaymentAsync(decimal amount)
    {
        await Task.Delay(100); // Simulate payment processing
    }
}
```

These examples demonstrate the power and flexibility of Aspekt for implementing cross-cutting concerns in a clean, maintainable way. Each example can be combined with others to create sophisticated application architectures with minimal code duplication.