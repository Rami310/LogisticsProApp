using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LogisticsPro.UI.ViewModels
{
    public abstract partial class BaseValidatableViewModel : ObservableObject
    {
        // Single error message property
        private string _errorMessageText = "";
        public string ErrorMessageText
        {
            get => _errorMessageText;
            set 
            {
                if (SetProperty(ref _errorMessageText, value))
                {
                    // Update HasError whenever ErrorMessageText changes
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        // HasError as computed property (most reliable)
        public bool HasError => !string.IsNullOrEmpty(ErrorMessageText);

        // Dictionary to track errors
        protected Dictionary<string, List<string>> ErrorsByPropertyName = new();

        // Add validation error for a property
        protected void AddError(string propertyName, string error)
        {
            if (!ErrorsByPropertyName.ContainsKey(propertyName))
            {
                ErrorsByPropertyName[propertyName] = new List<string>();
            }
            
            if (!ErrorsByPropertyName[propertyName].Contains(error))
            {
                ErrorsByPropertyName[propertyName].Add(error);
                UpdateErrorMessage();
            }
        }

        // Remove validation error for a property
        protected void RemoveError(string propertyName, string error)
        {
            if (ErrorsByPropertyName.ContainsKey(propertyName) && 
                ErrorsByPropertyName[propertyName].Contains(error))
            {
                ErrorsByPropertyName[propertyName].Remove(error);
                UpdateErrorMessage();
            }
        }

        // Just update the message, HasError updates automatically
        private void UpdateErrorMessage()
        {
            var newErrorMessage = ErrorsByPropertyName.Values
                .SelectMany(x => x)
                .FirstOrDefault() ?? "";
    
            ErrorMessageText = newErrorMessage;
    
            // Debug logging
            Console.WriteLine($"Error state - HasError: {HasError}, ErrorMessage: '{ErrorMessageText}'");
        }

        // Clear all errors
        protected void ClearAllErrors()
        {
            ErrorsByPropertyName.Clear();
            ErrorMessageText = "";
            // HasError will automatically become false when ErrorMessageText is empty
        }

        // Validate required field
        protected bool ValidateRequired(string value, string propertyName, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddError(propertyName, $"{fieldName} is required");
                return false;
            }
            
            RemoveError(propertyName, $"{fieldName} is required");
            return true;
        }

        // Additional validation methods
        protected bool ValidateMinLength(string value, int minLength, string propertyName, string fieldName)
        {
            if (!string.IsNullOrEmpty(value) && value.Length < minLength)
            {
                AddError(propertyName, $"{fieldName} must be at least {minLength} characters");
                return false;
            }
            
            RemoveError(propertyName, $"{fieldName} must be at least {minLength} characters");
            return true;
        }
    }
}