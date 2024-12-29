namespace Core.Models
{
    public class TokenModel
    {
        public int Id { get; set; }
        public string TokenId { get; set; } // jti
        public string UserId { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool Revoked { get; set; }
    }
}
