using System.Security.Claims;

namespace MechanicShop.Client.Identity
{
    public record UserInfo(string UserId, string Email, IList<string> Roles, IList<Claim> Claims);
}