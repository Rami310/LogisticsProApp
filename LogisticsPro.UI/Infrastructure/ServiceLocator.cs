using System;
using System.Collections.Generic;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.Services;

namespace LogisticsPro.UI.Infrastructure
{
    /// <summary>
    /// Simple service locator for dependency management
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        
        /// <summary>
        /// Register all application services
        /// </summary>
        public static void RegisterServices()
        {
            // Register services
            Register<NavigationService>(null);
            Register<IChartService>(new ChartService());
        }
        
        /// <summary>
        /// Register a service implementation
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="implementation">Service implementation</param>
        public static void Register<T>(T implementation) where T : class
        {
            _services[typeof(T)] = implementation;
        }
        
        /// <summary>
        /// Get a registered service
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service implementation</returns>
        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            
            ErrorHandler.LogError("ServiceLocator", new InvalidOperationException($"Service {typeof(T).Name} not registered"));
            return null;
        }
        
    }
}