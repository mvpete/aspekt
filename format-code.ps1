# PowerShell script to format and clean up the Aspekt codebase
# This script removes unused usings, formats code, and runs code analysis

param(
    [switch]$FixOnly,    # Only fix issues, don't run analysis
    [switch]$AnalyzeOnly, # Only run analysis, don't fix issues
    [string]$Project = "",    # Specific project to target
    [switch]$Verbose
)

Write-Host "🧹 Aspekt Code Cleanup and Formatting Tool" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan

# Get the solution directory
$SolutionDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionFile = Join-Path $SolutionDir "Aspekt.sln"

if (-not (Test-Path $SolutionFile)) {
    Write-Error "❌ Solution file not found: $SolutionFile"
    exit 1
}

Write-Host "📁 Working in: $SolutionDir" -ForegroundColor Green

# Function to run a command and capture output
function Invoke-Command-Safe {
    param(
        [string]$Command,
        [string]$Arguments,
        [string]$Description
    )
    
    Write-Host "🔧 $Description..." -ForegroundColor Yellow
    
    try {
        $process = Start-Process -FilePath $Command -ArgumentList $Arguments -Wait -PassThru -NoNewWindow -RedirectStandardOutput "temp_output.txt" -RedirectStandardError "temp_error.txt"
        
        $output = Get-Content "temp_output.txt" -ErrorAction SilentlyContinue
        $error = Get-Content "temp_error.txt" -ErrorAction SilentlyContinue
        
        if ($Verbose -and $output) {
            Write-Host $output -ForegroundColor Gray
        }
        
        if ($process.ExitCode -eq 0) {
            Write-Host "✅ $Description completed successfully" -ForegroundColor Green
        } else {
            Write-Host "⚠️  $Description completed with warnings (Exit Code: $($process.ExitCode))" -ForegroundColor Yellow
            if ($error) {
                Write-Host $error -ForegroundColor Red
            }
        }
        
        # Clean up temp files
        Remove-Item "temp_output.txt" -ErrorAction SilentlyContinue
        Remove-Item "temp_error.txt" -ErrorAction SilentlyContinue
        
        return $process.ExitCode
    }
    catch {
        Write-Error "❌ Failed to run $Description`: $_"
        return 1
    }
}

# Determine target
$Target = if ($Project) { $Project } else { $SolutionFile }
$TargetName = if ($Project) { "project $Project" } else { "solution" }

Write-Host "🎯 Target: $TargetName" -ForegroundColor Magenta

if (-not $AnalyzeOnly) {
    Write-Host ""
    Write-Host "🧹 CLEANING AND FORMATTING" -ForegroundColor Cyan
    Write-Host "===========================" -ForegroundColor Cyan
    
    # Clean the solution
    $exitCode = Invoke-Command-Safe "dotnet" "clean `"$Target`" --configuration Debug" "Clean Debug configuration"
    $exitCode = Invoke-Command-Safe "dotnet" "clean `"$Target`" --configuration Release" "Clean Release configuration"
    
    # Restore packages
    $exitCode = Invoke-Command-Safe "dotnet" "restore `"$Target`" --force" "Restore NuGet packages"
    
    # Format code (this handles indentation, spacing, etc.)
    $exitCode = Invoke-Command-Safe "dotnet" "format `"$Target`" --verbosity diagnostic" "Format code style"
    
    # Format code with style fixes (this handles code style issues)
    $exitCode = Invoke-Command-Safe "dotnet" "format style `"$Target`" --verbosity diagnostic" "Apply code style fixes"
    
    # Format code with analyzer fixes (this handles analyzer issues like unused usings)
    $exitCode = Invoke-Command-Safe "dotnet" "format analyzers `"$Target`" --verbosity diagnostic" "Apply analyzer fixes (including unused usings)"
    
    Write-Host "✅ Code formatting and cleanup completed!" -ForegroundColor Green
}

if (-not $FixOnly) {
    Write-Host ""
    Write-Host "🔍 CODE ANALYSIS" -ForegroundColor Cyan
    Write-Host "================" -ForegroundColor Cyan
    
    # Build with full analysis
    $exitCode = Invoke-Command-Safe "dotnet" "build `"$Target`" --configuration Release --verbosity normal" "Build with code analysis"
    
    # Run additional analysis if available
    if (Get-Command "dotnet-sonarscanner" -ErrorAction SilentlyContinue) {
        Write-Host "🔍 Running SonarScanner..." -ForegroundColor Yellow
        Invoke-Command-Safe "dotnet" "sonarscanner begin /k:`"aspekt`"" "Initialize SonarScanner"
        Invoke-Command-Safe "dotnet" "build `"$Target`"" "Build for SonarScanner"
        Invoke-Command-Safe "dotnet" "sonarscanner end" "Complete SonarScanner analysis"
    }
    
    Write-Host "✅ Code analysis completed!" -ForegroundColor Green
}

Write-Host ""
Write-Host "📊 SUMMARY" -ForegroundColor Cyan
Write-Host "==========" -ForegroundColor Cyan

# Check for common code issues
$csFiles = Get-ChildItem -Path $SolutionDir -Recurse -Filter "*.cs" | Where-Object { $_.DirectoryName -notlike "*\bin\*" -and $_.DirectoryName -notlike "*\obj\*" }
$totalFiles = $csFiles.Count

Write-Host "📁 C# files processed: $totalFiles" -ForegroundColor White

# Check for potential issues
$filesWithUnusedUsings = @()
$filesWithTabs = @()
$filesWithoutFinalNewline = @()

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    
    # Check for potential unused usings (basic check)
    if ($content -match "using\s+\w+(\.\w+)*;") {
        $usingStatements = [regex]::Matches($content, "using\s+[\w\.]+;").Count
        $actualUsages = [regex]::Matches($content, "\b\w+\b").Count
        
        # This is a very basic heuristic - actual unused using detection requires semantic analysis
        if ($usingStatements -gt ($actualUsages / 50)) {
            $filesWithUnusedUsings += $file.Name
        }
    }
    
    # Check for tab characters
    if ($content -match "\t") {
        $filesWithTabs += $file.Name
    }
    
    # Check for final newline
    if (-not $content.EndsWith("`n") -and -not $content.EndsWith("`r`n")) {
        $filesWithoutFinalNewline += $file.Name
    }
}

if ($filesWithTabs.Count -gt 0) {
    Write-Host "⚠️  Files with tab characters: $($filesWithTabs.Count)" -ForegroundColor Yellow
    if ($Verbose) {
        $filesWithTabs | ForEach-Object { Write-Host "   - $_" -ForegroundColor Gray }
    }
}

if ($filesWithoutFinalNewline.Count -gt 0) {
    Write-Host "⚠️  Files without final newline: $($filesWithoutFinalNewline.Count)" -ForegroundColor Yellow
    if ($Verbose) {
        $filesWithoutFinalNewline | ForEach-Object { Write-Host "   - $_" -ForegroundColor Gray }
    }
}

Write-Host ""
Write-Host "🎉 Code cleanup and analysis completed!" -ForegroundColor Green
Write-Host ""
Write-Host "💡 Tips:" -ForegroundColor Cyan
Write-Host "   - Run this script regularly to maintain code quality" -ForegroundColor White
Write-Host "   - Use 'dotnet format' in your IDE for real-time formatting" -ForegroundColor White
Write-Host "   - Enable 'Format on Save' in your editor settings" -ForegroundColor White
Write-Host "   - Consider adding this script to your CI/CD pipeline" -ForegroundColor White