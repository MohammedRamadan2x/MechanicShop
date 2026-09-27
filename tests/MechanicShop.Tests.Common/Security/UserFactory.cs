using MechanicShop.Infrastructure.Identity;

namespace MechanicShop.Tests.Common.Security;

internal class UserFactory
{
    public static AppUser CreateUser()
    {
        return new AppUser
        {
            Id = "19a59129-6c20-417a-834d-11a208d32d96",
            Email = "user@localhost",
            UserName = "user@localhost",
            EmailConfirmed = true
        };
    }
}