# Aspekt - Aspect-Oriented Programming for .NET

[![Build status](https://ci.appveyor.com/api/projects/status/ysr9ebr6dwaqamus?svg=true)](https://ci.appveyor.com/project/mvpete/aspekt)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%208.0%20%7C%209.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Version](https://img.shields.io/badge/version-3.0.0--AI%20Era-brightgreen)](https://github.com/mvpete/aspekt/releases)

Aspekt is a lightweight, powerful Aspect-Oriented Programming (AOP) foundation library for .NET that allows you to implement cross-cutting concerns using attributes. It supports modern .NET versions including .NET 6.0, 8.0, and 9.0, with async/await patterns and comprehensive Design by Contract capabilities.

## 🚀 Key Features

- **Attribute-Based AOP**: Apply aspects declaratively using C# attributes
- **Async Support**: Full support for async/await patterns with `ValueTask` methods
- **Design by Contract**: Comprehensive contract system with preconditions, postconditions, and invariants
- **Return Value Interception**: Modify return values using `IAspectExitHandler<T>`
- **Modern .NET Support**: Compatible with .NET 6.0, 8.0, 9.0, and .NET Standard 2.1
- **Post-Compilation Weaving**: IL manipulation using Mono.Cecil for zero-overhead aspect application
- **Built-in Logging**: Ready-to-use logging aspects with customizable formatters
- **Thread-Safe**: Designed for multi-threaded applications

## 📦 Installation

```bash
# Core AOP functionality
Install-Package Aspekt -Version 3.0.0

# Design by Contract support
Install-Package Aspekt.Contracts -Version 3.0.0

# Logging aspects
Install-Package Aspekt.Logging -Version 3.0.0
```

## 🏃‍♂️ Quick Start

### Basic Logging Aspect

```csharp
using Aspekt;

public class LoggingAspect : Aspect
{
    public override void OnEntry(MethodArguments args)
    {
        Console.WriteLine($"Entering: {args.FullName}");
    }

    public override void OnExit(MethodArguments args)
    {
        Console.WriteLine($"Exiting: {args.FullName}");
    }

    public override void OnException(MethodArguments args, Exception ex)
    {
        Console.WriteLine($"Exception in {args.FullName}: {ex.Message}");
    }
}
```

### Usage

```csharp
public class Calculator
{
    [Logging]
    public int Add(int x, int y)
    {
        return x + y;
    }

    [Logging]
    public async Task<string> GetDataAsync()
    {
        await Task.Delay(100);
        return "Hello, World!";
    }
}
```

### Advanced: Return Value Modification

```csharp
public class ResultModifierAspect : Aspect, IAspectExitHandler<string>
{
    public string OnExit(MethodArguments args, string result)
    {
        return $"Modified: {result}";
    }
}

public class Service
{
    [ResultModifier]
    public string GetMessage() => "Original";
    // Returns: "Modified: Original"
}
```

### Design by Contract

```csharp
using Aspekt.Contracts;

public class BankAccount
{
    private decimal _balance;
    
    [Invariant(nameof(_balance), Contract.Comparison.GreaterThanEqualTo, 0)]
    public decimal Balance => _balance;
    
    [Require(nameof(amount), Contract.Comparison.GreaterThan, 0)]
    [Ensure(Contract.Comparison.GreaterThanEqualTo, 0)]
    public decimal Deposit(decimal amount)
    {
        _balance += amount;
        return _balance;
    }
}
```

## 📚 Documentation

- **[Getting Started Guide](docs/GETTING_STARTED.md)** - Complete guide from installation to advanced features
- **[Contracts Documentation](docs/CONTRACTS.md)** - Design by Contract with preconditions, postconditions, and invariants
- **[API Reference](docs/API.md)** - Detailed API documentation for all components
- **[Examples](docs/EXAMPLES.md)** - Real-world usage examples and patterns
- **[Troubleshooting Guide](docs/TROUBLESHOOTING.md)** - Common issues and solutions

## 🔧 How It Works

Aspekt uses post-compilation IL weaving via Mono.Cecil. When you apply an aspect attribute to a method:

1. **Build Time**: Your code compiles normally
2. **Post-Build**: Aspekt.Bootstrap.Host processes your assembly
3. **IL Weaving**: Aspect calls are injected into your methods
4. **Runtime**: Aspects execute seamlessly with your code

```csharp
// Your code:
[LoggingAspect]
public void DoWork() { /* your logic */ }

// Becomes (conceptually):
public void DoWork()
{
    var aspect = new LoggingAspect();
    var args = new MethodArguments(/* method info */);
    
    aspect.OnEntry(args);
    try
    {
        /* your original logic */
        aspect.OnExit(args);
    }
    catch (Exception ex)
    {
        aspect.OnException(args, ex);
        throw;
    }
}
```

## 🏗️ Project Structure

- **Aspekt**: Core AOP functionality and base `Aspect` class
- **Aspekt.Contracts**: Design by Contract implementation
- **Aspekt.Logging**: Built-in logging aspects with multiple formatters
- **Aspekt.Bootstrap**: IL weaving engine using Mono.Cecil
- **Aspekt.Test**: Comprehensive test suite with 100+ tests

## 🛠️ Build Requirements

- .NET SDK 6.0 or later
- Visual Studio 2022 or VS Code
- MSBuild 17.0+

```bash
# Clone and build
git clone https://github.com/mvpete/aspekt.git
cd aspekt
dotnet build
dotnet test
```

## 🚀 CI/CD Pipeline

This project uses GitHub Actions for continuous integration and deployment:

- **Automated Testing**: Multi-version testing across .NET 6.0, 8.0, and 9.0
- **Code Quality**: Automated formatting, analysis, and security scanning
- **Package Publishing**: Automatic NuGet releases on tagged versions
- **Documentation**: Link validation and spell checking

See [Pipeline Documentation](.github/PIPELINE.md) for detailed information about the build and release process.

## ⚡ Performance

Aspekt is designed for minimal runtime overhead:
- **Zero reflection** at runtime
- **Compile-time weaving** means no performance impact from AOP infrastructure
- **Selective application** - only methods with aspects are modified
- **Async-aware** - proper support for async/await patterns

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

1. Fork the repository
2. Create a feature branch
3. Add tests for your changes
4. Ensure all tests pass
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [Mono.Cecil](https://github.com/jbevain/cecil) for IL manipulation
- Inspired by PostSharp and other AOP frameworks
- Thanks to all contributors and users

---

**Get started today**: Check out the [Getting Started Guide](docs/GETTING_STARTED.md) to begin using Aspekt in your projects!


