namespace BLL.DTOs
{
    public class MomoPaymentConfig
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string MomoApiUrl { get; set; } = string.Empty;
    }

    public class MomoWebhookRequestDTO
    {
        public string? PartnerCode { get; set; }
        public string? OrderId { get; set; }
        public string? RequestId { get; set; }
        public long Amount { get; set; }
        public string? OrderInfo { get; set; }
        public string? OrderType { get; set; }
        public long TransId { get; set; }
        public int ResultCode { get; set; }
        public string? Message { get; set; }
        public string? PayType { get; set; }
        public long ResponseTime { get; set; }
        public string? ExtraData { get; set; }
        public string? Signature { get; set; }
    }
}
