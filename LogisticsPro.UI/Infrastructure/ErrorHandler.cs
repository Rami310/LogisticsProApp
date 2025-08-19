using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace LogisticsPro.UI.Infrastructure
{
    /// <summary>
    /// Centralized error handling utility for the application.
    /// </summary>
    public static class ErrorHandler
    {
        private static readonly string _logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LogisticsPro", 
            "logs",
            $"error-log-{DateTime.Now:yyyy-MM-dd}.txt");
            
        /// <summary>
        /// Logs an exception to console and optionally to file.
        /// </summary>
        /// <param name="context">The context where the exception occurred</param>
        /// <param name="ex">The exception that was thrown</param>
        /// <param name="logToFile">Whether to log to file in addition to console</param>
        public static void LogError(string context, Exception ex, bool logToFile = true)
        {
            string errorMessage = $"[{DateTime.Now}] ERROR in {context}: {ex.Message}";
            
            string details = string.Empty;
            if (ex.InnerException != null)
            {
                details += $"\n  Inner Exception: {ex.InnerException.Message}";
            }
            
            details += $"\n  Stack Trace: {ex.StackTrace}";
            
            Console.WriteLine(errorMessage);
            Console.WriteLine(details);
            
            Debug.WriteLine(errorMessage);
            Debug.WriteLine(details);
            
            if (logToFile)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
                    File.AppendAllText(_logFilePath, $"{errorMessage}{details}\n\n");
                }
                catch
                {
                    Console.WriteLine("Could not write to error log file.");
                }
            }
        }
        
        /// <summary>
        /// Safely executes an action catching and logging any exceptions that occur.
        /// </summary>
        /// <param name="context">The context identifier for error logging</param>
        /// <param name="action">The action to execute</param>
        /// <returns>True if action completed without exceptions, false otherwise</returns>
        public static bool TrySafe(string context, Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                LogError(context, ex);
                return false;
            }
        }
        
        /// <summary>
        /// Safely executes an async action catching and logging any exceptions that occur.
        /// </summary>
        /// <param name="context">The context identifier for error logging</param>
        /// <param name="asyncAction">The async action to execute</param>
        /// <returns>True if action completed without exceptions, false otherwise</returns>
        public static async Task<bool> TrySafeAsync(string context, Func<Task> asyncAction)
        {
            try
            {
                await asyncAction();
                return true;
            }
            catch (Exception ex)
            {
                LogError(context, ex);
                return false;
            }
        }
        
        /// <summary>
        /// Safely executes a function catching and logging any exceptions that occur.
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="context">The context identifier for error logging</param>
        /// <param name="func">The function to execute</param>
        /// <param name="defaultValue">Default value to return in case of exception</param>
        /// <returns>The function result or defaultValue if an exception occurred</returns>
        public static T TrySafe<T>(string context, Func<T> func, T? defaultValue = default)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                LogError(context, ex);
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Safely executes an async function catching and logging any exceptions that occur.
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="context">The context identifier for error logging</param>
        /// <param name="asyncFunc">The async function to execute</param>
        /// <param name="defaultValue">Default value to return in case of exception</param>
        /// <returns>The function result or defaultValue if an exception occurred</returns>
        public static async Task<T> TrySafeAsync<T>(string context, Func<Task<T>> asyncFunc, T? defaultValue = default)
        {
            try
            {
                return await asyncFunc();
            }
            catch (Exception ex)
            {
                LogError(context, ex);
                return defaultValue;
            }
        }
    }
}