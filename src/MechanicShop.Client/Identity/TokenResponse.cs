namespace MechanicShop.Client.Identity
{
    public sealed class TokenResponse
    {
        public string? AccessToken { get; set; }
        public DateTime ExpiresOnUtc { get; set; }
        public string? RefreshToken { get; set; }
    }
}