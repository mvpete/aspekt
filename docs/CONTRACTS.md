# Aspekt Contracts - Design by Contract

Aspekt.Contracts provides Design by Contract (DbC) support for .NET applications. Design by Contract is a software development methodology that allows you to specify and verify program behavior through preconditions, postconditions, and invariants.

## Table of Contents
- [Overview](#overview)
- [Installation](#installation)
- [Core Concepts](#core-concepts)
- [Preconditions](#preconditions)
- [Postconditions](#postconditions)
- [Invariants](#invariants)
- [Built-in Contracts](#built-in-contracts)
- [Custom Contracts](#custom-contracts)
- [Examples](#examples)
- [Best Practices](#best-practices)

## Overview

Design by Contract helps you:
- **Specify behavior**: Clearly define what methods expect and guarantee
- **Catch bugs early**: Contract violations are caught at runtime
- **Improve documentation**: Contracts serve as executable documentation
- **Enhance testing**: Contracts act as automatic assertions
- **Increase reliability**: Ensure your code behaves as intended

## Installation

```bash
Install-Package Aspekt.Contracts
```

Contracts are automatically woven into your code during the build process when using the NuGet package.

## Core Concepts

### Contract Types
- **Preconditions** (`[Require]`): Conditions that must be true when a method is called
- **Postconditions** (`[Ensure]`): Conditions that must be true when a method returns
- **Invariants** (`[Invariant]`): Conditions that must always be true for a class

### Contract Elements
```csharp
using Aspekt.Contracts;

public class BankAccount
{
    private decimal _balance;
    
    [Invariant(nameof(_balance), Contract.Comparison.GreaterThanEqualTo, 0)]
    public decimal Balance => _balance;
    
    [Require(nameof(amount), Contract.Comparison.GreaterThan, 0)]
    [Ensure(Contract.Comparison.GreaterThanEqualTo, 0)] // Return value >= 0
    public decimal Withdraw(decimal amount)
    {
        _balance -= amount;
        return _balance;
    }
}
```

## Preconditions

Preconditions specify what must be true when a method is called. They're checked before the method body executes.

### Parameter Validation
```csharp
using Aspekt.Contracts;

public class Calculator
{
    // Require parameter to be positive
    [Require(nameof(number), Contract.Comparison.GreaterThan, 0)]
    public double SquareRoot(double number)
    {
        return Math.Sqrt(number);
    }
    
    // Require parameter to be non-null
    [Require(nameof(text), Contract.Constraint.NotNull)]
    public string ProcessText(string text)
    {
        return text.ToUpper();
    }
    
    // Multiple preconditions
    [Require(nameof(dividend), Contract.Comparison.NotEqualTo, 0)]
    [Require(nameof(divisor), Contract.Comparison.NotEqualTo, 0)]
    public double Divide(double dividend, double divisor)
    {
        return dividend / divisor;
    }
}
```

### State-based Preconditions
```csharp
public class FileProcessor
{
    private bool _initialized;
    
    // Require object to be in correct state
    [Require(Contract.Target.Field, nameof(_initialized), Contract.Comparison.EqualTo, true)]
    public void ProcessFile(string filename)
    {
        // Process file
    }
    
    public void Initialize()
    {
        _initialized = true;
    }
}
```

## Postconditions

Postconditions specify what must be true when a method returns. They're checked after the method body executes but before returning to the caller.

### Return Value Contracts
```csharp
public class StringProcessor
{
    // Ensure return value is not null
    [Ensure(Contract.Constraint.NotNull)]
    public string FormatString(string input)
    {
        return input?.Trim() ?? string.Empty;
    }
    
    // Ensure return value meets specific criteria
    [Ensure(Contract.Comparison.GreaterThan, 0)]
    public int GetStringLength(string input)
    {
        return input?.Length ?? 0;
    }
}
```

### State Change Contracts
```csharp
public class Counter
{
    private int _count;
    
    public int Count => _count;
    
    // Ensure state change occurred
    [Ensure(Contract.Target.Property, nameof(Count), Contract.Comparison.EqualTo, 1)]
    public void IncrementToOne()
    {
        _count = 1;
    }
}
```

## Invariants

Invariants specify conditions that must always be true for a class. They're checked after construction and after every public method call.

### Class Invariants
```csharp
public class Rectangle
{
    private double _width;
    private double _height;
    
    // Width must always be positive
    [Invariant(nameof(_width), Contract.Comparison.GreaterThan, 0)]
    public double Width
    {
        get => _width;
        set => _width = value;
    }
    
    // Height must always be positive
    [Invariant(nameof(_height), Contract.Comparison.GreaterThan, 0)]
    public double Height
    {
        get => _height;
        set => _height = value;
    }
    
    public Rectangle(double width, double height)
    {
        _width = width;
        _height = height;
    }
    
    public double Area => _width * _height;
}
```

### Complex Invariants
```csharp
public class SortedList<T> where T : IComparable<T>
{
    private List<T> _items = new List<T>();
    
    // Invariant: list must always be sorted
    [Invariant("IsSorted")]
    public bool IsSorted
    {
        get
        {
            for (int i = 1; i < _items.Count; i++)
            {
                if (_items[i - 1].CompareTo(_items[i]) > 0)
                    return false;
            }
            return true;
        }
    }
    
    public void Add(T item)
    {
        _items.Add(item);
        _items.Sort();
    }
}
```

## Built-in Contracts

### NotNull Contract
```csharp
using Aspekt.Contracts.Aspects;

public class UserService
{
    [NotNull("username")]
    public User CreateUser(string username, string email)
    {
        return new User { Username = username, Email = email };
    }
}
```

### Argument Validation
```csharp
using Aspekt.Contracts.Aspects;

public class ValidationExample
{
    // Validates string argument is not null or empty
    [RequiresArgument("name", typeof(string), "Cannot be null or empty")]
    public void ProcessName(string name)
    {
        Console.WriteLine($"Processing: {name}");
    }
    
    // Validates numeric argument is positive
    [RequiresArgument("age", typeof(int), 18)]
    public void RegisterUser(string name, int age)
    {
        Console.WriteLine($"Registering {name}, age {age}");
    }
}
```

## Custom Contracts

### Creating Custom Contract Evaluators
```csharp
using Aspekt.Contracts;

public class EmailContractEvaluator : IContractEvaluator
{
    public bool Evaluate(object obj)
    {
        if (obj is not string email)
            return false;
            
        return email.Contains('@') && email.Contains('.');
    }
}

// Usage in custom contract aspect
public class RequireValidEmailAttribute : ContractAspect
{
    public RequireValidEmailAttribute(string parameterName) 
        : base(Contract.Target.Property, parameterName)
    {
        AddContract(new EmailContractEvaluator(), parameterName);
    }
}

// Usage
public class UserRegistration
{
    [RequireValidEmail(nameof(email))]
    public void RegisterUser(string email, string name)
    {
        // Registration logic
    }
}
```

### Custom Comparison Contracts
```csharp
public class RangeContractEvaluator : IContractEvaluator
{
    private readonly double _min;
    private readonly double _max;
    
    public RangeContractEvaluator(double min, double max)
    {
        _min = min;
        _max = max;
    }
    
    public bool Evaluate(object obj)
    {
        if (obj is not double value)
            return false;
            
        return value >= _min && value <= _max;
    }
}

public class RequireRangeAttribute : ContractAspect
{
    public RequireRangeAttribute(string parameterName, double min, double max)
        : base(Contract.Target.Property, parameterName)
    {
        AddContract(new RangeContractEvaluator(min, max), parameterName);
    }
}
```

## Examples

### Complete Banking Example
```csharp
using Aspekt.Contracts;

public class BankAccount
{
    private decimal _balance;
    private readonly string _accountNumber;
    private bool _isActive = true;
    
    // Invariants - always true
    [Invariant(nameof(_balance), Contract.Comparison.GreaterThanEqualTo, 0)]
    public decimal Balance => _balance;
    
    [Invariant(nameof(_accountNumber), Contract.Constraint.NotNull)]
    public string AccountNumber => _accountNumber;
    
    public BankAccount(string accountNumber, decimal initialBalance = 0)
    {
        _accountNumber = accountNumber;
        _balance = initialBalance;
    }
    
    // Preconditions and postconditions
    [Require(nameof(amount), Contract.Comparison.GreaterThan, 0)]
    [Require(Contract.Target.Field, nameof(_isActive), Contract.Comparison.EqualTo, true)]
    [Ensure(Contract.Comparison.GreaterThanEqualTo, 0)] // Return value >= 0
    public decimal Deposit(decimal amount)
    {
        _balance += amount;
        return _balance;
    }
    
    [Require(nameof(amount), Contract.Comparison.GreaterThan, 0)]
    [Require(nameof(amount), Contract.Comparison.LessThanEqualTo, nameof(_balance))]
    [Require(Contract.Target.Field, nameof(_isActive), Contract.Comparison.EqualTo, true)]
    [Ensure(Contract.Comparison.GreaterThanEqualTo, 0)]
    public decimal Withdraw(decimal amount)
    {
        _balance -= amount;
        return _balance;
    }
    
    [Require(Contract.Target.Field, nameof(_isActive), Contract.Comparison.EqualTo, true)]
    public void CloseAccount()
    {
        _isActive = false;
        _balance = 0;
    }
}
```

### Data Validation Service
```csharp
using Aspekt.Contracts;
using Aspekt.Contracts.Aspects;

public class ValidationService
{
    [NotNull("data")]
    [Require(nameof(data), Contract.Constraint.NotNull)]
    [Ensure(Contract.Constraint.NotNull)]
    public ValidationResult ValidateUser(UserData data)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrEmpty(data.Email) || !data.Email.Contains('@'))
            result.AddError("Invalid email");
            
        if (data.Age < 0 || data.Age > 150)
            result.AddError("Invalid age");
            
        return result;
    }
    
    [RequiresArgument("email", typeof(string), "Email cannot be null")]
    [Ensure(Contract.Comparison.NotEqualTo, false)] // Must return true for valid emails
    public bool IsValidEmail(string email)
    {
        return !string.IsNullOrEmpty(email) && 
               email.Contains('@') && 
               email.Contains('.');
    }
}

public class UserData
{
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ValidationResult
{
    public List<string> Errors { get; } = new List<string>();
    public bool IsValid => Errors.Count == 0;
    
    public void AddError(string error) => Errors.Add(error);
}
```

## Best Practices

### 1. Start Simple
```csharp
// Good: Simple, clear contracts
[Require(nameof(value), Contract.Comparison.GreaterThan, 0)]
public double SquareRoot(double value) => Math.Sqrt(value);

// Avoid: Overly complex contracts
[Require(nameof(value), Contract.Comparison.GreaterThan, 0)]
[Require(nameof(value), Contract.Comparison.LessThan, double.MaxValue)]
[Ensure(Contract.Comparison.GreaterThanEqualTo, 0)]
[Ensure(Contract.Comparison.LessThan, value)] // Too many contracts
public double SquareRoot(double value) => Math.Sqrt(value);
```

### 2. Use Meaningful Names
```csharp
// Good: Clear parameter names
[Require(nameof(accountNumber), Contract.Constraint.NotNull)]
public Account GetAccount(string accountNumber)

// Avoid: Generic parameter names
[Require(nameof(param1), Contract.Constraint.NotNull)]
public Account GetAccount(string param1)
```

### 3. Don't Duplicate Business Logic
```csharp
// Good: Contracts check preconditions, business logic handles the work
[Require(nameof(balance), Contract.Comparison.GreaterThan, 0)]
public void ProcessPayment(decimal balance)
{
    // Business logic here
    var fee = CalculateFee(balance);
    // ...
}

// Avoid: Duplicating business logic in contracts
[Require(nameof(balance), Contract.Comparison.GreaterThan, CalculateFee(balance))]
public void ProcessPayment(decimal balance) // Don't call business methods in contracts
```

### 4. Handle Contract Failures Gracefully
```csharp
try
{
    account.Withdraw(amount);
}
catch (PreconditionException ex)
{
    // Handle contract violation
    Console.WriteLine($"Invalid operation: {ex.Message}");
}
catch (PostconditionException ex)
{
    // Handle postcondition failure
    Console.WriteLine($"Method failed to meet postcondition: {ex.Message}");
}
```

### 5. Use Contracts for Documentation
```csharp
/// <summary>
/// Calculates compound interest.
/// </summary>
/// <param name="principal">The initial amount (must be positive)</param>
/// <param name="rate">The interest rate (must be between 0 and 1)</param>
/// <param name="time">The time period (must be positive)</param>
/// <returns>The final amount (will be greater than principal)</returns>
[Require(nameof(principal), Contract.Comparison.GreaterThan, 0)]
[Require(nameof(rate), Contract.Comparison.GreaterThanEqualTo, 0)]
[Require(nameof(rate), Contract.Comparison.LessThanEqualTo, 1)]
[Require(nameof(time), Contract.Comparison.GreaterThan, 0)]
[Ensure(Contract.Comparison.GreaterThan, nameof(principal))]
public double CalculateCompoundInterest(double principal, double rate, double time)
{
    return principal * Math.Pow(1 + rate, time);
}
```

## Troubleshooting

### Common Issues

**Contracts not being enforced**:
- Verify Aspekt.Contracts NuGet package is installed
- Check that post-build weaving is occurring
- Ensure methods are public (contracts don't apply to private methods)

**Performance concerns**:
- Contracts add runtime overhead - use judiciously in performance-critical code
- Consider using conditional compilation for debug-only contracts
- Profile your application to measure contract impact

**Contract violations in tests**:
- This is often good! Contracts help find bugs
- Write tests that verify both valid and invalid inputs
- Use contracts to guide test case creation

### Debugging Contracts

```csharp
// Enable detailed contract violation messages
[Require(nameof(value), Contract.Comparison.GreaterThan, 0, 
         ErrorMessage = "Value must be positive, got: {0}")]
public void ProcessValue(int value)
{
    // Method implementation
}
```

## Integration with Testing

Contracts work excellently with unit testing:

```csharp
[Test]
public void Withdraw_WithInsufficientFunds_ThrowsPreconditionException()
{
    var account = new BankAccount("123", 100);
    
    Assert.Throws<PreconditionException>(() => 
        account.Withdraw(200)); // Violates balance >= amount precondition
}

[Test]
public void Deposit_ValidAmount_IncreasesBalance()
{
    var account = new BankAccount("123", 100);
    var result = account.Deposit(50);
    
    Assert.AreEqual(150, result);
    // Postcondition automatically verified: result >= 0
}
```

Contracts make your code more reliable, easier to test, and better documented. Start with simple preconditions and gradually add more sophisticated contracts as you become comfortable with the approach.
