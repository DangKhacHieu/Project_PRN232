namespace DAL.Entities
{
    public class ChangePasswordOtp
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        // storing code (short-lived) — consider hashing in production
        public string Code { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}