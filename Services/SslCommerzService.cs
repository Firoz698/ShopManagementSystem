using Newtonsoft.Json;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Services
{
    public interface ISslCommerzService
    {
        Task<string> InitiatePaymentAsync(SslPaymentRequest request);
        Task<bool>   ValidateIpnAsync(SslIpnResponse ipn);
    }

    public class SslCommerzService : ISslCommerzService
    {
        private readonly IConfiguration    _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SslCommerzService> _logger;

        // SSLCommerz Sandbox URL (production: securepay.sslcommerz.com)
        private const string SandboxInitUrl = "https://sandbox.sslcommerz.com/gwprocess/v4/api.php";
        private const string SandboxValidUrl = "https://sandbox.sslcommerz.com/validator/api/validationserverAPI.php";

        public SslCommerzService(
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            ILogger<SslCommerzService> logger)
        {
            _config            = config;
            _httpClientFactory = httpClientFactory;
            _logger            = logger;
        }

        // ── Payment শুরু করুন ─────────────────────────────────────────────────────
        public async Task<string> InitiatePaymentAsync(SslPaymentRequest request)
        {
            var storeId  = _config["SSLCommerz:StoreId"];
            var storePass = _config["SSLCommerz:StorePassword"];
            var baseUrl  = _config["SSLCommerz:BaseUrl"]; // https://yoursite.com

            var postData = new Dictionary<string, string>
            {
                { "store_id",         storeId! },
                { "store_passwd",     storePass! },
                { "total_amount",     request.Amount.ToString("F2") },
                { "currency",         "BDT" },
                { "tran_id",          request.TransactionId },
                { "success_url",      $"{baseUrl}/Cart/PaymentSuccess" },
                { "fail_url",         $"{baseUrl}/Cart/PaymentFail" },
                { "cancel_url",       $"{baseUrl}/Cart/PaymentCancel" },
                { "ipn_url",          $"{baseUrl}/Cart/IPN" },

                // Customer info
                { "cus_name",         request.CustomerName },
                { "cus_email",        request.CustomerEmail },
                { "cus_add1",         request.ShippingAddress },
                { "cus_city",         "Dhaka" },
                { "cus_country",      "Bangladesh" },
                { "cus_phone",        request.CustomerPhone },

                // Shipping info
                { "ship_name",        request.CustomerName },
                { "ship_add1",        request.ShippingAddress },
                { "ship_city",        "Dhaka" },
                { "ship_country",     "Bangladesh" },
                { "shipping_method",  "Courier" },

                // Product info
                { "product_name",     request.ProductName },
                { "product_category", "Mixed" },
                { "product_profile",  "general" },

                { "emi_option",       "0" },
                { "multi_card_name",  "mastercard,visacard,amexcard" },
            };

            try
            {
                var client   = _httpClientFactory.CreateClient();
                var response = await client.PostAsync(SandboxInitUrl, new FormUrlEncodedContent(postData));
                var body     = await response.Content.ReadAsStringAsync();

                dynamic? json = JsonConvert.DeserializeObject(body);
                if (json == null) return string.Empty;

                string status = json.status ?? "";
                if (status == "SUCCESS")
                    return (string)(json.GatewayPageURL ?? string.Empty);

                _logger.LogWarning("SSLCommerz init failed: {body}", body);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSLCommerz InitiatePayment error");
                return string.Empty;
            }
        }

        // ── IPN Validate ──────────────────────────────────────────────────────────
        public async Task<bool> ValidateIpnAsync(SslIpnResponse ipn)
        {
            var storeId   = _config["SSLCommerz:StoreId"];
            var storePass = _config["SSLCommerz:StorePassword"];

            var url = $"{SandboxValidUrl}?val_id={ipn.val_id}" +
                      $"&store_id={storeId}&store_passwd={storePass}&format=json";
            try
            {
                var client   = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(url);
                var body     = await response.Content.ReadAsStringAsync();

                dynamic? json = JsonConvert.DeserializeObject(body);
                if (json == null) return false;

                string status = json.status ?? "";
                return status == "VALID" || status == "VALIDATED";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSLCommerz ValidateIPN error");
                return false;
            }
        }
    }

    // ── Request Model ────────────────────────────────────────────────────────────
    public class SslPaymentRequest
    {
        public string  TransactionId   { get; set; } = string.Empty;
        public decimal Amount          { get; set; }
        public string  CustomerName    { get; set; } = string.Empty;
        public string  CustomerEmail   { get; set; } = string.Empty;
        public string  CustomerPhone   { get; set; } = string.Empty;
        public string  ShippingAddress { get; set; } = string.Empty;
        public string  ProductName     { get; set; } = string.Empty;
    }
}
