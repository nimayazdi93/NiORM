# NiORM Error Handling and Logging Guide

## Overview

NiORM now includes comprehensive error handling and logging capabilities to help developers identify and resolve issues quickly. This guide explains how to use these new features.

## Features Added

### 1. Custom Exception Classes
- `NiORMException`: Base exception for all NiORM operations
- `NiORMConnectionException`: Database connection issues
- `NiORMValidationException`: Entity validation errors
- `NiORMMappingException`: Data mapping errors

### 2. Logging System
- Configurable log levels (Info, Warning, Error, Debug)
- Console or file output
- Detailed operation tracking
- SQL query logging

### 3. Enhanced XML Documentation
- Complete IntelliSense support
- Detailed parameter descriptions
- Usage examples
- Exception documentation

## Quick Start

### Enable Logging

```csharp
using NiORM.SQLServer.Core;

// Enable logging to console
NiORMLogger.IsEnabled = true;
NiORMLogger.MinimumLogLevel = LogLevel.Info;

// Or enable logging to file
NiORMLogger.LogFilePath = @"C:\logs\niorm.log";
```

### Basic Usage with Error Handling

```csharp
using NiORM.SQLServer;
using NiORM.SQLServer.Core;

public class MyDataService : DataCore
{
    public MyDataService(string connectionString) : base(connectionString)
    {
        // Configure logging
        NiORMLogger.IsEnabled = true;
        NiORMLogger.MinimumLogLevel = LogLevel.Warning;
    }

    public Person? GetPersonSafely(int id)
    {
        try
        {
            return CreateEntity<Person>().Find(id);
        }
        catch (NiORMException ex)
        {
            // Handle NiORM specific errors
            Console.WriteLine($"Database error: {ex.Message}");
            if (!string.IsNullOrEmpty(ex.SqlQuery))
            {
                Console.WriteLine($"SQL Query: {ex.SqlQuery}");
            }
            return null;
        }
        catch (Exception ex)
        {
            // Handle unexpected errors
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return null;
        }
    }
}
```

## Log Levels

- **Debug**: Detailed information for debugging
- **Info**: General information about operations
- **Warning**: Non-critical issues that don't stop execution
- **Error**: Critical errors that prevent operations

## Log Output Examples

### Console Output
```
[2024-01-15 10:30:15.123] [Info] [DataCore.CreateEntity] Entity collection created successfully for Person
[2024-01-15 10:30:15.456] [Debug] [Entities.Find] Finding Person with id: 123
[2024-01-15 10:30:15.789] [Info] [SqlMaster.Get] Query executed successfully, returned 1 records | SQL: SELECT TOP(1) * FROM People WHERE [Id] = 123
```

### File Output
When `LogFilePath` is set, logs are written to the specified file with the same format.

## Exception Handling Best Practices

### 1. Catch Specific Exceptions

```csharp
try
{
    people.Add(newPerson);
}
catch (NiORMValidationException ex)
{
    // Handle validation errors (e.g., trying to add a view)
    ShowUserError("Validation failed: " + ex.Message);
}
catch (NiORMConnectionException ex)
{
    // Handle connection issues
    ShowUserError("Database connection failed. Please try again later.");
    LogError(ex);
}
catch (NiORMException ex)
{
    // Handle other NiORM errors
    ShowUserError("Database operation failed: " + ex.Message);
}
```

### 2. Use Safe Methods

```csharp
public class SafeDataService : DataCore
{
    public bool TryAddPerson(Person person, out string errorMessage)
    {
        errorMessage = string.Empty;
        try
        {
            if (person == null)
            {
                errorMessage = "Person cannot be null";
                return false;
            }

            CreateEntity<Person>().Add(person);
            return true;
        }
        catch (NiORMException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}
```

## Configuration Options

### Runtime Configuration

```csharp
// Change logging level at runtime
NiORMLogger.MinimumLogLevel = LogLevel.Error;

// Disable logging temporarily
NiORMLogger.IsEnabled = false;

// Switch from console to file logging
NiORMLogger.LogFilePath = @"C:\logs\debug.log";
```

### Logging Methods

```csharp
// Manual logging
NiORMLogger.LogInfo("Custom operation completed");
NiORMLogger.LogWarning("Performance issue detected");
NiORMLogger.LogError("Critical error occurred", operation: "MyOperation", exception: ex);
NiORMLogger.LogDebug("Detailed debug information");
```

## Migration Guide

### From Previous Versions

1. **Replace Generic Exceptions**: 
   - Old: `catch (Exception ex)`
   - New: `catch (NiORMException ex)`

2. **Add Logging Configuration**:
   ```csharp
   // Add to your DataCore constructor or startup
   NiORMLogger.IsEnabled = true;
   NiORMLogger.MinimumLogLevel = LogLevel.Warning;
   ```

3. **Use Enhanced IntelliSense**:
   - All methods now have detailed XML documentation
   - Parameter descriptions and examples available
   - Exception documentation helps with proper error handling

## Performance Impact

- **Logging**: Minimal performance impact when disabled
- **Exception Handling**: No performance impact unless exceptions occur
- **Documentation**: No runtime impact, IntelliSense only

## Troubleshooting

### Common Issues

1. **No Logs Appearing**:
   - Check `NiORMLogger.IsEnabled = true`
   - Verify `MinimumLogLevel` setting
   - Ensure file path is writable (for file logging)

2. **Too Many Logs**:
   - Increase `MinimumLogLevel` (e.g., from Debug to Warning)
   - Disable logging in production if not needed

3. **Missing Exception Details**:
   - Catch `NiORMException` specifically for detailed error information
   - Check `SqlQuery` property for failed queries

## Best Practices Summary

1. ✅ Enable logging during development
2. ✅ Use specific exception types for better error handling
3. ✅ Log to files in production environments
4. ✅ Set appropriate log levels for your environment
5. ✅ Handle validation errors gracefully
6. ✅ Provide meaningful error messages to users
7. ❌ Don't expose internal error details to end users
8. ❌ Don't log sensitive data like passwords or personal information 