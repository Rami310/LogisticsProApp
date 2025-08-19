using System;
using System.Text.RegularExpressions;

namespace LogisticsPro.UI.Services
{
    /// <summary>
    /// Utility class for common validation logic
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validate that a string is not null or empty
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <returns>True if valid</returns>
        public static bool IsRequired(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
        
        /// <summary>
        /// Validate minimum string length
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="minLength">Minimum length</param>
        /// <returns>True if valid</returns>
        public static bool HasMinLength(string value, int minLength)
        {
            return !string.IsNullOrEmpty(value) && value.Length >= minLength;
        }
        
        /// <summary>
        /// Validate maximum string length
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="maxLength">Maximum length</param>
        /// <returns>True if valid</returns>
        public static bool HasMaxLength(string value, int maxLength)
        {
            return string.IsNullOrEmpty(value) || value.Length <= maxLength;
        }
        
        /// <summary>
        /// Validate that a string can be parsed as an integer
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <returns>True if valid</returns>
        public static bool IsNumeric(string value)
        {
            return string.IsNullOrEmpty(value) || int.TryParse(value, out _);
        }
        
        /// <summary>
        /// Validate that a string can be parsed as a decimal
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <returns>True if valid</returns>
        public static bool IsDecimal(string value)
        {
            return string.IsNullOrEmpty(value) || decimal.TryParse(value, out _);
        }
        
        /// <summary>
        /// Validate that a string represents a valid email address
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <returns>True if valid</returns>
        public static bool IsEmail(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true; // Empty is valid, use IsRequired to enforce presence
                
            // Simple regex for basic email validation
            var regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(value);
        }
        
        /// <summary>
        /// Validate that a string represents a valid phone number
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <returns>True if valid</returns>
        public static bool IsPhoneNumber(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true; // Empty is valid, use IsRequired to enforce presence
                
            // Simple regex for basic phone validation (accepts different formats)
            var regex = new Regex(@"^\+?[0-9\s\-\(\)]{7,20}$");
            return regex.IsMatch(value);
        }
        
        /// <summary>
        /// Validate that a number is within a specified range
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (inclusive)</param>
        /// <returns>True if valid</returns>
        public static bool IsInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Validate that a decimal is within a specified range
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (inclusive)</param>
        /// <returns>True if valid</returns>
        public static bool IsInRange(decimal value, decimal min, decimal max)
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Validate that a date is within a specified range
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="min">Minimum date (inclusive)</param>
        /// <param name="max">Maximum date (inclusive)</param>
        /// <returns>True if valid</returns>
        public static bool IsInRange(DateTime value, DateTime min, DateTime max)
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Validate a string against a custom regex pattern
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="pattern">Regex pattern</param>
        /// <returns>True if valid</returns>
        public static bool MatchesPattern(string value, string pattern)
        {
            if (string.IsNullOrEmpty(value))
                return true; // Empty is valid, use IsRequired to enforce presence
                
            var regex = new Regex(pattern);
            return regex.IsMatch(value);
        }
    }
}