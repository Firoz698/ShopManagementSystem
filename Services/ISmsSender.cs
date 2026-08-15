namespace ShopManagementSystem.Services
{
    public interface ISmsSender
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
    }

    // ✅ BulkSMSBD / Alpha SMS style gateway (HTTP GET API)
    // Provider change korle sudhu URL/params update korle hobe
    public class SmsSender : ISmsSender
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public SmsSender(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            var settings = _config.GetSection("SmsSettings");
            var apiKey = settings["ApiKey"];
            var senderId = settings["SenderId"];
            var baseUrl = settings["ApiUrl"]; // e.g. https://bulksmsbd.net/api/smsapi

            // ফোন নম্বর ফরম্যাট ঠিক করা (01XXXXXXXXX -> 8801XXXXXXXXX)
            var formattedNumber = phoneNumber.StartsWith("0") ? "88" + phoneNumber : phoneNumber;

            var url = $"{baseUrl}?api_key={apiKey}&type=text&number={formattedNumber}" +
                      $"&senderid={senderId}&message={Uri.EscapeDataString(message)}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}