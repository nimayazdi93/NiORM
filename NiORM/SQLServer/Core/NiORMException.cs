using System;

namespace NiORM.SQLServer.Core
{
    /// <summary>
    /// Custom exception class for NiORM SQL Server operations
    /// </summary>
    public class NiORMException : Exception
    {
        /// <summary>
        /// The SQL query that caused the exception, if applicable
        /// </summary>
        public string? SqlQuery { get; }

        /// <summary>
        /// The operation type that was being performed when the exception occurred
        /// </summary>
        public string? OperationType { get; }

        /// <summary>
        /// Initializes a new instance of the NiORMException class
        /// </summary>
        public NiORMException() : base() { }

        /// <summary>
        /// Initializes a new instance of the NiORMException class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public NiORMException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the NiORMException class with a specified error message and a reference to the inner exception
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public NiORMException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the NiORMException class with detailed error information
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        /// <param name="sqlQuery">The SQL query that caused the exception</param>
        /// <param name="operationType">The operation type being performed</param>
        public NiORMException(string message, string? sqlQuery, string? operationType) : base(message)
        {
            SqlQuery = sqlQuery;
            OperationType = operationType;
        }

        /// <summary>
        /// Initializes a new instance of the NiORMException class with detailed error information and inner exception
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        /// <param name="sqlQuery">The SQL query that caused the exception</param>
        /// <param name="operationType">The operation type being performed</param>
        public NiORMException(string message, Exception innerException, string? sqlQuery, string? operationType) : base(message, innerException)
        {
            SqlQuery = sqlQuery;
            OperationType = operationType;
        }
    }

    /// <summary>
    /// Exception thrown when there's a connection issue with the database
    /// </summary>
    public class NiORMConnectionException : NiORMException
    {
        /// <summary>
        /// Initializes a new instance of the NiORMConnectionException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public NiORMConnectionException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when there's a validation error with entity data
    /// </summary>
    public class NiORMValidationException : NiORMException
    {
        /// <summary>
        /// The entity that failed validation
        /// </summary>
        public object? Entity { get; }

        /// <summary>
        /// Initializes a new instance of the NiORMValidationException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="entity">The entity that failed validation</param>
        public NiORMValidationException(string message, object? entity = null) : base(message)
        {
            Entity = entity;
        }
    }

    /// <summary>
    /// Exception thrown when there's a mapping error between entity and database
    /// </summary>
    public class NiORMMappingException : NiORMException
    {
        /// <summary>
        /// Initializes a new instance of the NiORMMappingException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public NiORMMappingException(string message, Exception? innerException = null) : base(message, innerException) { }
    }
} 