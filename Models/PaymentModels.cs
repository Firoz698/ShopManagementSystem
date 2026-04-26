namespace ShopManagementSystem.Models
{
    // ── SSLCommerz Payment Record ────────────────────────────────────────────────
    //public class PaymentTransaction
    //{
    //    public int    Id             { get; set; }
    //    public int    OrderId        { get; set; }
    //    public string TransactionId  { get; set; } = string.Empty;
    //    public string ValidationId   { get; set; } = string.Empty;
    //    public string Status         { get; set; } = "Pending"; // Pending | Success | Failed | Cancelled
    //    public decimal Amount        { get; set; }
    //    public string Currency       { get; set; } = "BDT";
    //    public string PaymentMethod  { get; set; } = string.Empty; // VISA, bKash, Nagad etc.
    //    public DateTime CreatedAt    { get; set; } = DateTime.Now;
    //    public DateTime? UpdatedAt   { get; set; }

    //    public Order? Order { get; set; }
    //}

    // ── SSLCommerz IPN Response ──────────────────────────────────────────────────
    public class SslIpnResponse
    {
        public string? tran_id       { get; set; }
        public string? val_id        { get; set; }
        public string? amount        { get; set; }
        public string? card_type     { get; set; }
        public string? store_amount  { get; set; }
        public string? card_no       { get; set; }
        public string? bank_tran_id  { get; set; }
        public string? status        { get; set; }
        public string? tran_date     { get; set; }
        public string? error         { get; set; }
        public string? currency      { get; set; }
        public string? card_issuer   { get; set; }
        public string? card_brand    { get; set; }
        public string? risk_level    { get; set; }
        public string? risk_title    { get; set; }
    }
}
