namespace AuthServiceAPI.Models
{
    public class Session
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }

        public string IpAddress { get; set; }
        public string DeviceHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public User User { get; set; }
    }

}
