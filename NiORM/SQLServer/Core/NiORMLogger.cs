using System;
using System.IO;

namespace NiORM.SQLServer.Core
{
    /// <summary>
    /// Log level enumeration for NiORM operations
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Information level logging
        /// </summary>
        Info,
        /// <summary>
        /// Warning level logging
        /// </summary>
        Warning,
        /// <summary>
        /// Error level logging
        /// </summary>
        Error,
        /// <summary>
        /// Debug level logging
        /// </summary>
        Debug
    }

    /// <summary>
    /// Simple logging system for NiORM operations
    /// </summary>
    public static class NiORMLogger
    {
        /// <summary>
        /// Gets or sets whether logging is enabled
        /// </summary>
        public static bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the minimum log level to be recorded
        /// </summary>
        public static LogLevel MinimumLogLevel { get; set; } = LogLevel.Warning;

        /// <summary>
        /// Gets or sets the log file path. If null, logs to console
        /// </summary>
        public static string? LogFilePath { get; set; }

        /// <summary>
        /// Logs an information message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="operation">The operation being performed</param>
        /// <param name="sqlQuery">The SQL query if applicable</param>
        public static void LogInfo(string message, string? operation = null, string? sqlQuery = null)
        {
            Log(LogLevel.Info, message, operation, sqlQuery);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="operation">The operation being performed</param>
        /// <param name="sqlQuery">The SQL query if applicable</param>
        public static void LogWarning(string message, string? operation = null, string? sqlQuery = null)
        {
            Log(LogLevel.Warning, message, operation, sqlQuery);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="operation">The operation being performed</param>
        /// <param name="sqlQuery">The SQL query if applicable</param>
        /// <param name="exception">The exception if applicable</param>
        public static void LogError(string message, string? operation = null, string? sqlQuery = null, Exception? exception = null)
        {
            var logMessage = message;
            if (exception != null)
            {
                logMessage += $" | Exception: {exception.Message}";
                if (exception.InnerException != null)
                {
                    logMessage += $" | Inner Exception: {exception.InnerException.Message}";
                }
            }
            Log(LogLevel.Error, logMessage, operation, sqlQuery);
        }

        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="operation">The operation being performed</param>
        /// <param name="sqlQuery">The SQL query if applicable</param>
        public static void LogDebug(string message, string? operation = null, string? sqlQuery = null)
        {
            Log(LogLevel.Debug, message, operation, sqlQuery);
        }

        /// <summary>
        /// Core logging method
        /// </summary>
        /// <param name="level">The log level</param>
        /// <param name="message">The message to log</param>
        /// <param name="operation">The operation being performed</param>
        /// <param name="sqlQuery">The SQL query if applicable</param>
        private static void Log(LogLevel level, string message, string? operation, string? sqlQuery)
        {
            if (!IsEnabled || level < MinimumLogLevel)
                return;

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] [{level}]";
            
            if (!string.IsNullOrEmpty(operation))
                logEntry += $" [{operation}]";
                
            logEntry += $" {message}";
            
            if (!string.IsNullOrEmpty(sqlQuery))
                logEntry += $" | SQL: {sqlQuery}";

            try
            {
                if (string.IsNullOrEmpty(LogFilePath))
                {
                    Console.WriteLine(logEntry);
                }
                else
                {
                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                }
            }
            catch
            {
                // Fail silently for logging errors to avoid cascading issues
            }
        }
    }
} 