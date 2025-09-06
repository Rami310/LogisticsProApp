using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LogisticsPro.UI.Services;

namespace LogisticsPro.UI.ViewModels
{
    public abstract partial class LocalizedViewModelBase : ObservableObject
    {
        protected readonly LocalizationService _localization;

        protected LocalizedViewModelBase()
        {
            _localization = LocalizationService.Instance;
            _localization.LanguageChanged += OnLanguageChanged;
        }

        protected virtual void OnLanguageChanged(object sender, EventArgs e)
        {
            // Override in derived classes to update localized properties
            OnPropertyChanged(string.Empty); // Refresh all bindings
        }

        protected string Localize(string key) => _localization.GetString(key);
        protected string Localize(string key, params object[] args) => _localization.GetString(key, args);
    }
}