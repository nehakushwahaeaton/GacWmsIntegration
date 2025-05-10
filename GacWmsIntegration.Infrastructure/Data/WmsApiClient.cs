using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GacWmsIntegration.Infrastructure.Data
{
    public class WmsApiClient : IWmsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WmsApiClient> _logger;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public WmsApiClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<WmsApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Get configuration values
            _baseUrl = configuration["WmsApi:BaseUrl"] ?? throw new ArgumentNullException("WMS API Base URL is not configured");
            _apiKey = configuration["WmsApi:ApiKey"] ?? throw new ArgumentNullException("WMS API Key is not configured");

            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // Customer operations
        public async Task<bool> SendCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            try
            {
                _logger.LogInformation("Sending customer data to WMS: {CustomerId}", customer.CustomerID);

                var content = new StringContent(
                    JsonConvert.SerializeObject(customer),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/customers", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent customer data to WMS: {CustomerId}", customer.CustomerID);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to send customer data to WMS. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending customer data to WMS: {CustomerId}", customer.CustomerID);
                return false;
            }
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            try
            {
                _logger.LogInformation("Updating customer data in WMS: {CustomerId}", customer.CustomerID);

                var content = new StringContent(
                    JsonConvert.SerializeObject(customer),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/customers/{customer.CustomerID}", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated customer data in WMS: {CustomerId}", customer.CustomerID);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to update customer data in WMS. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer data in WMS: {CustomerId}", customer.CustomerID);
                return false;
            }
        }

        // Product operations
        public async Task<bool> SendProductAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            try
            {
                _logger.LogInformation("Sending product data to WMS: {ProductCode}", product.ProductCode);

                var content = new StringContent(
                    JsonConvert.SerializeObject(product),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/products", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent product data to WMS: {ProductCode}", product.ProductCode);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to send product data to WMS. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending product data to WMS: {ProductCode}", product.ProductCode);
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            try
            {
                _logger.LogInformation("Updating product data in WMS: {ProductCode}", product.ProductCode);

                var content = new StringContent(
                    JsonConvert.SerializeObject(product),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/products/{product.ProductCode}", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated product data in WMS: {ProductCode}", product.ProductCode);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to update product data in WMS. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product data in WMS: {ProductCode}", product.ProductCode);
                return false;
            }
        }

        // Purchase Order operations
        public async Task<bool> SendPurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
            {
                throw new ArgumentNullException(nameof(purchaseOrder));
            }

            try
            {
                _logger.LogInformation("Sending purchase order data to WMS: {OrderId}", purchaseOrder.OrderID);

                var content = new StringContent(
                    JsonConvert.SerializeObject(purchaseOrder),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/purchaseorders", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent purchase order data to WMS: {OrderId}", purchaseOrder.OrderID);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to send purchase order data to WMS. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending purchase order data to WMS: {OrderId}", purchaseOrder.OrderID);
                return false;
            }
        }

        public async Task<bool> UpdatePurchaseOrderStatusAsync(int orderId, string status)
        {
            try
            {
                _logger.LogInformation("Updating purchase order status in WMS: {OrderId}, Status: {Status}", orderId, status);

                var statusUpdate = new
                {
                    Status = status
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(statusUpdate),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/purchaseorders/{orderId}/status", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated purchase order status in WMS: {OrderId}, Status: {Status}", orderId, status);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to update purchase order status in WMS. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase order status in WMS: {OrderId}, Status: {Status}", orderId, status);
                return false;
            }
        }

        // Sales Order operations
        public async Task<bool> SendSalesOrderAsync(SalesOrder salesOrder)
        {
            if (salesOrder == null)
            {
                throw new ArgumentNullException(nameof(salesOrder));
            }

            try
            {
                _logger.LogInformation("Sending sales order data to WMS: {OrderId}", salesOrder.OrderID);

                var content = new StringContent(
                    JsonConvert.SerializeObject(salesOrder),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/salesorders", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent sales order data to WMS: {OrderId}", salesOrder.OrderID);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to send sales order data to WMS. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending sales order data to WMS: {OrderId}", salesOrder.OrderID);
                return false;
            }
        }

        public async Task<bool> UpdateSalesOrderStatusAsync(int orderId, string status)
        {
            try
            {
                _logger.LogInformation("Updating sales order status in WMS: {OrderId}, Status: {Status}", orderId, status);

                var statusUpdate = new
                {
                    Status = status
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(statusUpdate),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/salesorders/{orderId}/status", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated sales order status in WMS: {OrderId}, Status: {Status}", orderId, status);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to update sales order status in WMS. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sales order status in WMS: {OrderId}, Status: {Status}", orderId, status);
                return false;
            }
        }

        // System operations
        public async Task<bool> PingWmsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/system/ping");

                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Ping to WMS successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pinging WMS");
                return false;
            }
        }

        public async Task<string> GetWmsVersionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/system/version");

                response.EnsureSuccessStatusCode();
                var version = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Retrieved WMS version: {Version}", version);
                return version;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving WMS version");
                throw;
            }
        }
    }
}
