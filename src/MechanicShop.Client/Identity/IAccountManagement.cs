namespace MechanicShop.Client.Identity
{
    public interface IAccountManagement
    {
        public Task<FormResult> LoginAsync(string email, string password);

        public Task<TokenResponse?> RefreshTokenAsync();

        public Task LogoutAsync();

        public Task<bool> CheckAuthenticatedAsync();

        public Task<TokenResponse?> LoadAccessTokenFromStorage();
    }
}