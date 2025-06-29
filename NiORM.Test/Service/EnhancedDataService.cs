using NiORM.SQLServer;
using NiORM.SQLServer.Core;
using NiORM.Test.Models;

namespace NiORM.Test.Service
{
    /// <summary>
    /// Enhanced data service demonstrating logging and error handling features
    /// </summary>
    public class EnhancedDataService : DataCore
    {
        /// <summary>
        /// Initializes the enhanced data service with logging enabled
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="enableLogging">Whether to enable logging (default: true)</param>
        /// <param name="logLevel">Minimum log level (default: Info)</param>
        /// <param name="logFilePath">Optional log file path (if null, logs to console)</param>
        public EnhancedDataService(string connectionString, 
                                   bool enableLogging = true, 
                                   LogLevel logLevel = LogLevel.Info,
                                   string? logFilePath = null) : base(connectionString)
        {
            // Configure logging
            NiORMLogger.IsEnabled = enableLogging;
            NiORMLogger.MinimumLogLevel = logLevel;
            NiORMLogger.LogFilePath = logFilePath;

            NiORMLogger.LogInfo("Enhanced Data Service initialized", "EnhancedDataService.Constructor");
        }

        /// <summary>
        /// Gets the People entity collection
        /// </summary>
        public IEntities<Person> People => CreateEntity<Person>();

        /// <summary>
        /// Gets the Cats entity collection  
        /// </summary>
        public IEntities<Cat> Cats => CreateEntity<Cat>();

        /// <summary>
        /// Safe method to get a person by ID with proper error handling
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <returns>Person if found, null if not found or error occurred</returns>
        public Person? GetPersonSafely(int id)
        {
            try
            {
                NiORMLogger.LogInfo($"Attempting to get person with ID: {id}", "EnhancedDataService.GetPersonSafely");
                return People.Find(id);
            }
            catch (NiORMException ex)
            {
                NiORMLogger.LogError($"NiORM error getting person with ID {id}: {ex.Message}", 
                                   "EnhancedDataService.GetPersonSafely", ex.SqlQuery, ex);
                return null;
            }
            catch (Exception ex)
            {
                NiORMLogger.LogError($"Unexpected error getting person with ID {id}: {ex.Message}", 
                                   "EnhancedDataService.GetPersonSafely", null, ex);
                return null;
            }
        }

        /// <summary>
        /// Safe method to add a person with validation and error handling
        /// </summary>
        /// <param name="person">Person to add</param>
        /// <returns>True if successfully added, false otherwise</returns>
        public bool AddPersonSafely(Person person)
        {
            try
            {
                if (person == null)
                {
                    NiORMLogger.LogWarning("Attempted to add null person", "EnhancedDataService.AddPersonSafely");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(person.Name))
                {
                    NiORMLogger.LogWarning("Attempted to add person with empty name", "EnhancedDataService.AddPersonSafely");
                    return false;
                }

                NiORMLogger.LogInfo($"Attempting to add person: {person.Name}", "EnhancedDataService.AddPersonSafely");
                People.Add(person);
                NiORMLogger.LogInfo($"Successfully added person: {person.Name}", "EnhancedDataService.AddPersonSafely");
                return true;
            }
            catch (NiORMValidationException ex)
            {
                NiORMLogger.LogError($"Validation error adding person: {ex.Message}", 
                                   "EnhancedDataService.AddPersonSafely", null, ex);
                return false;
            }
            catch (NiORMException ex)
            {
                NiORMLogger.LogError($"NiORM error adding person: {ex.Message}", 
                                   "EnhancedDataService.AddPersonSafely", ex.SqlQuery, ex);
                return false;
            }
            catch (Exception ex)
            {
                NiORMLogger.LogError($"Unexpected error adding person: {ex.Message}", 
                                   "EnhancedDataService.AddPersonSafely", null, ex);
                return false;
            }
        }

        /// <summary>
        /// Demonstrates custom query execution with error handling
        /// </summary>
        /// <param name="minAge">Minimum age filter</param>
        /// <returns>List of people or empty list if error occurred</returns>
        public List<Person> GetPeopleByMinAge(int minAge)
        {
            try
            {
                NiORMLogger.LogInfo($"Getting people with minimum age: {minAge}", "EnhancedDataService.GetPeopleByMinAge");
                var result = People.Where(p => p.Age >= minAge);
                NiORMLogger.LogInfo($"Found {result.Count} people with age >= {minAge}", "EnhancedDataService.GetPeopleByMinAge");
                return result;
            }
            catch (Exception ex)
            {
                NiORMLogger.LogError($"Error getting people by minimum age {minAge}: {ex.Message}", 
                                   "EnhancedDataService.GetPeopleByMinAge", null, ex);
                return new List<Person>();
            }
        }

        /// <summary>
        /// Demonstrates raw SQL execution with error handling
        /// </summary>
        /// <returns>List of person names or empty list if error occurred</returns>
        public List<string> GetAllPersonNames()
        {
            try
            {
                NiORMLogger.LogInfo("Getting all person names using raw SQL", "EnhancedDataService.GetAllPersonNames");
                var names = SqlRaw<string>("SELECT [Name] FROM People ORDER BY [Name]");
                NiORMLogger.LogInfo($"Retrieved {names.Count} person names", "EnhancedDataService.GetAllPersonNames");
                return names;
            }
            catch (Exception ex)
            {
                NiORMLogger.LogError($"Error getting person names: {ex.Message}", 
                                   "EnhancedDataService.GetAllPersonNames", null, ex);
                return new List<string>();
            }
        }

        /// <summary>
        /// Configures logging settings at runtime
        /// </summary>
        /// <param name="enabled">Enable or disable logging</param>
        /// <param name="logLevel">Minimum log level</param>
        /// <param name="logFilePath">Log file path (null for console)</param>
        public void ConfigureLogging(bool enabled, LogLevel logLevel, string? logFilePath = null)
        {
            NiORMLogger.IsEnabled = enabled;
            NiORMLogger.MinimumLogLevel = logLevel;
            NiORMLogger.LogFilePath = logFilePath;
            
            if (enabled)
            {
                NiORMLogger.LogInfo($"Logging reconfigured - Level: {logLevel}, File: {logFilePath ?? "Console"}", 
                                  "EnhancedDataService.ConfigureLogging");
            }
        }
    }
} 