using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace LogisticsPro.UI.Services
{
    /// <summary>
    /// Centralized API configuration for all services
    /// </summary>
    public static class ApiConfiguration
    {
        // API Settings
        public static readonly string BaseUrl = "https://localhost:7001/api/";
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan QuickTimeout = TimeSpan.FromSeconds(3);
        
        // Single HttpClient instance for better performance
        private static readonly Lazy<HttpClient> _httpClientLazy = new Lazy<HttpClient>(() =>
        {
            // Create HttpClientHandler that bypasses SSL verification for development
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = DefaultTimeout
            };
            
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "LogisticsPro-UI/1.0");
            
            return client;
        });

        public static HttpClient HttpClient => _httpClientLazy.Value;

        // JSON Serialization options
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // API Status
        private static bool? _isApiAvailable;
        private static DateTime _lastApiCheck = DateTime.MinValue;
        private static readonly TimeSpan ApiCheckInterval = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Check if API is available with caching
        /// </summary>
        public static async Task<bool> IsApiAvailableAsync()
        {
            // Return cached result if recent
            if (_isApiAvailable.HasValue && DateTime.Now - _lastApiCheck < ApiCheckInterval)
            {
                return _isApiAvailable.Value;
            }

            try
            {
                // Create a separate client for health check with SSL bypass
                using var handler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };
                
                // Test health endpoint
                var response = await client.GetAsync("https://localhost:7001/health");
                _isApiAvailable = response.IsSuccessStatusCode;
                
                if (_isApiAvailable == true)
                {
                    Console.WriteLine("✅ Health check successful");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Health check failed: {ex.Message}");
                
                // Try HTTP fallback
                try
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                    var response = await client.GetAsync("http://localhost:5014/health");
                    _isApiAvailable = response.IsSuccessStatusCode;
                    
                    if (_isApiAvailable == true)
                    {
                        Console.WriteLine("✅ HTTP fallback successful");
                        // Update base URL to use HTTP
                        Console.WriteLine("💡 Using HTTP fallback - consider fixing SSL certificates");
                    }
                }
                catch
                {
                    _isApiAvailable = false;
                }
            }

            _lastApiCheck = DateTime.Now;
            
            if (_isApiAvailable == true)
                Console.WriteLine("🟢 API is available");
            else
                Console.WriteLine("🔴 API is not available - using mock data");

            return _isApiAvailable.Value;
        }

        /// <summary>
        /// Reset API availability check (force recheck on next call)
        /// </summary>
        public static void ResetApiAvailabilityCheck()
        {
            _isApiAvailable = null;
            _lastApiCheck = DateTime.MinValue;
        }

        /// <summary>
        /// Get appropriate timeout based on operation type
        /// </summary>
        public static TimeSpan GetTimeout(bool isQuickOperation = false)
        {
            return isQuickOperation ? QuickTimeout : DefaultTimeout;
        }

        /// <summary>
        /// Create HttpClient with SSL bypass for development
        /// </summary>
        public static HttpClient CreateDevHttpClient()
        {
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            
            return new HttpClient(handler)
            {
                Timeout = DefaultTimeout
            };
        }
    }
}