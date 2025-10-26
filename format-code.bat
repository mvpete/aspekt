@echo off
REM Batch script to format and clean up the Aspekt codebase
REM This script removes unused usings, formats code, and runs analysis

echo.
echo ================================================================
echo                 Aspekt Code Cleanup and Formatting
echo ================================================================
echo.

REM Get the directory where this script is located
set SCRIPT_DIR=%~dp0
set SOLUTION_FILE=%SCRIPT_DIR%Aspekt.sln

if not exist "%SOLUTION_FILE%" (
    echo ERROR: Solution file not found: %SOLUTION_FILE%
    pause
    exit /b 1
)

echo Working directory: %SCRIPT_DIR%
echo Solution file: %SOLUTION_FILE%
echo.

echo ================================================================
echo                      CLEANING SOLUTION
echo ================================================================
echo.

echo Cleaning Debug configuration...
dotnet clean "%SOLUTION_FILE%" --configuration Debug
if %ERRORLEVEL% neq 0 (
    echo WARNING: Clean Debug failed with exit code %ERRORLEVEL%
)

echo Cleaning Release configuration...
dotnet clean "%SOLUTION_FILE%" --configuration Release
if %ERRORLEVEL% neq 0 (
    echo WARNING: Clean Release failed with exit code %ERRORLEVEL%
)

echo.
echo ================================================================
echo                    RESTORING PACKAGES
echo ================================================================
echo.

echo Restoring NuGet packages...
dotnet restore "%SOLUTION_FILE%" --force
if %ERRORLEVEL% neq 0 (
    echo ERROR: Package restore failed with exit code %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ================================================================
echo                      FORMATTING CODE
echo ================================================================
echo.

echo Formatting code style...
dotnet format "%SOLUTION_FILE%" --verbosity diagnostic
if %ERRORLEVEL% neq 0 (
    echo WARNING: Code formatting completed with warnings (exit code %ERRORLEVEL%)
)

echo Applying code style fixes...
dotnet format style "%SOLUTION_FILE%" --verbosity diagnostic
if %ERRORLEVEL% neq 0 (
    echo WARNING: Code style fixes completed with warnings (exit code %ERRORLEVEL%)
)

echo Applying analyzer fixes (removing unused usings, etc.)...
dotnet format analyzers "%SOLUTION_FILE%" --verbosity diagnostic
if %ERRORLEVEL% neq 0 (
    echo WARNING: Analyzer fixes completed with warnings (exit code %ERRORLEVEL%)
)

echo.
echo ================================================================
echo                      BUILDING SOLUTION
echo ================================================================
echo.

echo Building solution with code analysis...
dotnet build "%SOLUTION_FILE%" --configuration Release --verbosity normal
if %ERRORLEVEL% neq 0 (
    echo WARNING: Build completed with warnings or errors (exit code %ERRORLEVEL%)
)

echo.
echo ================================================================
echo                         COMPLETED
echo ================================================================
echo.
echo Code cleanup and formatting completed!
echo.
echo Tips:
echo   - Run this script regularly to maintain code quality
echo   - Use 'dotnet format' in your IDE for real-time formatting
echo   - Enable 'Format on Save' in your editor settings
echo   - Consider adding this to your build process
echo.
pause