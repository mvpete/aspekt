# Code Formatting and Style Rules

This document describes the code formatting and style rules setup for the Aspekt project to ensure consistent code quality, automatic removal of unused usings, and proper code organization.

## 📁 Configuration Files

### 1. `.editorconfig`
- **Purpose**: Defines code style and formatting rules that work across different IDEs
- **Key Features**:
  - Enforces consistent indentation (4 spaces for C#)
  - Manages line endings (CRLF for Windows)
  - Controls using directive placement (outside namespace)
  - Removes unnecessary using statements (IDE0005)
  - Enforces naming conventions (PascalCase, camelCase, etc.)
  - Applies code analysis rules for quality improvements

### 2. `AspektCodeAnalysis.ruleset`
- **Purpose**: Comprehensive code analysis rules for maintainability, performance, and security
- **Coverage**:
  - Design Rules (CA1000-CA1065)
  - Naming Rules (CA1700-CA1726)
  - Performance Rules (CA1800-CA1824)
  - Security Rules (CA2100-CA2127)
  - Usage Rules (CA2200-CA2243)
  - IDE Rules (IDE0001-IDE0305) for code cleanup

### 3. `Directory.Build.props`
- **Purpose**: Centralized build configuration for all projects
- **Features**:
  - Enables .NET analyzers (`EnableNETAnalyzers=true`)
  - Enforces code style during build (`EnforceCodeStyleInBuild=true`)
  - References the custom ruleset file
  - Includes analyzer packages for all projects
  - Automatic removal of unused imports (`RemoveUnusedImports=true`)

### 4. `global.json`
- **Purpose**: Ensures consistent .NET SDK version across the team
- **Benefits**: Prevents version-related build issues

## 🛠️ Automation Scripts

### PowerShell Script: `format-code.ps1`
```powershell
# Usage examples:
.\format-code.ps1                    # Format entire solution
.\format-code.ps1 -FixOnly          # Only apply fixes, no analysis
.\format-code.ps1 -AnalyzeOnly      # Only run analysis, no fixes
.\format-code.ps1 -Verbose          # Show detailed output
```

**Features**:
- Cleans and restores solution
- Applies code formatting
- Removes unused using statements
- Runs code analysis
- Provides detailed progress reporting

### Batch Script: `format-code.bat`
```batch
REM Simple double-click execution for Windows users
format-code.bat
```

**Features**:
- User-friendly Windows interface
- Same functionality as PowerShell script
- No PowerShell execution policy requirements

## 🚀 Quick Start

### 1. One-Time Setup
The configuration files are already set up. Just ensure you have .NET 7+ installed.

### 2. Format Code
```bash
# Command line - full formatting
dotnet format Aspekt.sln

# Remove unused usings only
dotnet format analyzers Aspekt.sln

# Check what would be formatted (without making changes)
dotnet format Aspekt.sln --verify-no-changes
```

### 3. Use Automation Scripts
```bash
# PowerShell
.\format-code.ps1

# Batch file (Windows)
format-code.bat
```

## 📝 What Gets Fixed Automatically

### ✅ Code Style Issues
- **Unused using statements** - Automatically removed
- **Using directive ordering** - System usings first, then alphabetical
- **Indentation** - Consistent 4-space indentation for C#
- **Line endings** - CRLF for Windows compatibility
- **Braces** - Required for all control flow statements
- **Naming conventions** - PascalCase for public members, camelCase for parameters

### ✅ Code Quality Issues
- **Unnecessary casts** - Removed automatically
- **Simplified expressions** - `var` usage where appropriate
- **Null checks** - Simplified using pattern matching
- **Dead code** - Unused variables and unreachable code
- **Access modifiers** - Added where missing

### ⚠️ Issues Flagged (Manual Fix Required)
- **Security vulnerabilities** - SQL injection, unsafe operations
- **Performance issues** - Inefficient loops, boxing operations
- **Design problems** - Large methods, excessive parameters
- **Documentation** - Missing XML comments for public APIs

## 🔧 IDE Integration

### Visual Studio
1. **Tools > Options > Text Editor > C# > Code Style**
2. **Enable "Format document on save"**
3. **Enable "Remove unnecessary usings on save"**

### Visual Studio Code
1. Install **C# extension**
2. Add to `settings.json`:
```json
{
    "editor.formatOnSave": true,
    "editor.codeActionsOnSave": {
        "source.organizeImports": true,
        "source.removeUnusedImports": true
    }
}
```

### JetBrains Rider
1. **File > Settings > Editor > Code Style**
2. **Import settings from .editorconfig**
3. **Enable "Reformat code" and "Optimize imports" on save**

## 📊 Code Quality Metrics

After running the formatting tools, you can expect:

- **Zero unused using statements** - All unnecessary imports removed
- **Consistent formatting** - All files follow the same style rules  
- **Improved readability** - Proper indentation and spacing
- **Better maintainability** - Following C# coding conventions
- **Reduced warnings** - Many code analysis warnings auto-fixed

## 🚨 CI/CD Integration

Add this to your build pipeline to enforce code quality:

```yaml
# Azure DevOps / GitHub Actions example
- name: Check Code Formatting
  run: dotnet format --verify-no-changes

- name: Run Code Analysis
  run: dotnet build --configuration Release
```

This ensures that all code committed to the repository maintains consistent formatting and quality standards.

## 📚 Resources

- [.NET Code Style Rules](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/)
- [EditorConfig Documentation](https://editorconfig.org/)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/)
- [dotnet format Command](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-format)

---

## 💡 Pro Tips

1. **Run formatting regularly** - Use the automation scripts before committing code
2. **Enable format on save** - Configure your IDE for automatic formatting
3. **Review warnings** - Code analysis warnings often indicate real issues
4. **Customize rules** - Modify `.editorconfig` or `AspektCodeAnalysis.ruleset` for team preferences
5. **Team consistency** - Ensure all team members use the same configuration files