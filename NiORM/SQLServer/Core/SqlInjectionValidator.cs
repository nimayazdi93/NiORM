using System.Text.RegularExpressions;

namespace NiORM.SQLServer.Core
{
    /// <summary>
    /// Utility class for detecting potential SQL injection patterns in SQL strings
    /// </summary>
    public static class SqlInjectionValidator
    {
        private static readonly string[] DangerousKeywords = {
            "DROP", "DELETE", "TRUNCATE", "ALTER", "CREATE", "INSERT", "UPDATE",
            "EXEC", "EXECUTE", "SP_", "XP_", "BULK", "OPENROWSET", "OPENDATASOURCE",
            "SHUTDOWN", "DBCC", "BACKUP", "RESTORE"
        };

        private static readonly string[] SuspiciousPatterns = {
            @"'[^']*;[^']*'", // String with semicolon
            @"--", // SQL comments
            @"/\*.*?\*/", // Block comments
            @"\bunion\b.*\bselect\b", // UNION SELECT
            @"'\s*or\s*'", // OR condition in quotes
            @"'\s*and\s*'", // AND condition in quotes
            @"'.*=.*'", // Equals in quotes
            @"'.*<.*'", // Less than in quotes
            @"'.*>.*'", // Greater than in quotes
            @"\bselect\b.*\bfrom\b", // Nested SELECT
            @"'.*\+.*'", // String concatenation
            @"char\s*\(", // CHAR function
            @"ascii\s*\(", // ASCII function
            @"cast\s*\(", // CAST function
            @"convert\s*\(", // CONVERT function
            @"substring\s*\(", // SUBSTRING function
            @"waitfor\s+delay", // Time delay
            @"benchmark\s*\(", // MySQL benchmark
            @"sleep\s*\(", // MySQL sleep
            @"pg_sleep\s*\(", // PostgreSQL sleep
            @"0x[0-9a-fA-F]+", // Hexadecimal values
            @"'.*\|\|.*'", // String concatenation (Oracle)
            @"'.*\+\+.*'", // String concatenation attempts
        };

        /// <summary>
        /// Validates a SQL string for potential injection patterns
        /// </summary>
        /// <param name="sql">The SQL string to validate</param>
        /// <returns>Validation result containing warnings and risk level</returns>
        public static SqlValidationResult ValidateSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return new SqlValidationResult { IsValid = true, RiskLevel = RiskLevel.None };

            var result = new SqlValidationResult { IsValid = true };
            var warnings = new List<string>();
            var normalizedSql = sql.ToUpperInvariant();

            // Check for dangerous keywords
            foreach (var keyword in DangerousKeywords)
            {
                if (normalizedSql.Contains(keyword))
                {
                    warnings.Add($"Dangerous keyword detected: {keyword}");
                    result.RiskLevel = RiskLevel.High;
                }
            }

            // Check for suspicious patterns
            foreach (var pattern in SuspiciousPatterns)
            {
                if (Regex.IsMatch(sql, pattern, RegexOptions.IgnoreCase))
                {
                    warnings.Add($"Suspicious pattern detected: {pattern}");
                    if (result.RiskLevel < RiskLevel.Medium)
                        result.RiskLevel = RiskLevel.Medium;
                }
            }

            // Check for multiple statements
            if (sql.Contains(";") && !sql.Trim().EndsWith(";"))
            {
                warnings.Add("Multiple SQL statements detected (separated by semicolons)");
                result.RiskLevel = RiskLevel.High;
            }

            // Check for unbalanced quotes
            var singleQuoteCount = sql.Count(c => c == '\'');
            if (singleQuoteCount % 2 != 0)
            {
                warnings.Add("Unbalanced single quotes detected");
                result.RiskLevel = RiskLevel.Medium;
            }

            // Check for common injection techniques
            if (Regex.IsMatch(sql, @"'\s*(or|and)\s+.*=.*", RegexOptions.IgnoreCase))
            {
                warnings.Add("Potential tautology injection detected (OR/AND with equals)");
                result.RiskLevel = RiskLevel.High;
            }

            // Check for comment-based injections
            if (Regex.IsMatch(sql, @"'.*--", RegexOptions.IgnoreCase))
            {
                warnings.Add("Potential comment-based injection detected");
                result.RiskLevel = RiskLevel.High;
            }

            result.Warnings = warnings;
            result.IsValid = result.RiskLevel != RiskLevel.High;

            return result;
        }

        /// <summary>
        /// Sanitizes a SQL string by removing potentially dangerous characters
        /// WARNING: This is not foolproof. Use parameterized queries instead.
        /// </summary>
        /// <param name="input">The input string to sanitize</param>
        /// <returns>Sanitized string</returns>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Replace dangerous characters
            return input
                .Replace("'", "''") // Escape single quotes
                .Replace(";", "") // Remove semicolons
                .Replace("--", "") // Remove comment indicators
                .Replace("/*", "") // Remove block comment start
                .Replace("*/", "") // Remove block comment end
                .Replace("xp_", "") // Remove extended procedure prefix
                .Replace("sp_", ""); // Remove stored procedure prefix
        }

        /// <summary>
        /// Checks if a SQL string appears to be a safe SELECT statement
        /// </summary>
        /// <param name="sql">The SQL string to check</param>
        /// <returns>True if it appears to be a safe SELECT, false otherwise</returns>
        public static bool IsSafeSelectStatement(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return false;

            var trimmed = sql.Trim();
            var normalized = trimmed.ToUpperInvariant();

            // Must start with SELECT
            if (!normalized.StartsWith("SELECT"))
                return false;

            // Must not contain dangerous keywords
            foreach (var keyword in DangerousKeywords)
            {
                if (keyword != "SELECT" && normalized.Contains(keyword))
                    return false;
            }

            // Must not contain multiple statements
            if (sql.Contains(";") && !sql.Trim().EndsWith(";"))
                return false;

            // Must not contain suspicious patterns (simplified check)
            if (normalized.Contains("--") || 
                normalized.Contains("/*") || 
                Regex.IsMatch(sql, @"'\s*(or|and)\s+.*=.*", RegexOptions.IgnoreCase))
                return false;

            return true;
        }
    }

    /// <summary>
    /// Result of SQL validation
    /// </summary>
    public class SqlValidationResult
    {
        /// <summary>
        /// Whether the SQL is considered valid (not high risk)
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Risk level of the SQL
        /// </summary>
        public RiskLevel RiskLevel { get; set; } = RiskLevel.None;

        /// <summary>
        /// List of warnings found
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Summary of the validation result
        /// </summary>
        public string Summary => $"Risk Level: {RiskLevel}, Warnings: {Warnings.Count}";
    }

    /// <summary>
    /// Risk levels for SQL injection
    /// </summary>
    public enum RiskLevel
    {
        /// <summary>
        /// No risk detected
        /// </summary>
        None = 0,

        /// <summary>
        /// Low risk - minor concerns
        /// </summary>
        Low = 1,

        /// <summary>
        /// Medium risk - suspicious patterns
        /// </summary>
        Medium = 2,

        /// <summary>
        /// High risk - dangerous patterns detected
        /// </summary>
        High = 3
    }
} 