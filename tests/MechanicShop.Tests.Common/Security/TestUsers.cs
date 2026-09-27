using MechanicShop.Infrastructure.Identity;

namespace MechanicShop.Tests.Common.Security;

public static class TestUsers
{
    public static AppUser Manager => new()
    {
        Id = "19a59129-6c20-417a-834d-11a208d32d96",
        Email = "pm@localhost",
        UserName = "pm@localhost",
        EmailConfirmed = true
    };

    public static AppUser Labor01 => new()
    {
        Id = "b6327240-0aea-46fc-863a-777fc4e42560",
        Email = "john.labor@localhost",
        UserName = "john.labor@localhost",
        EmailConfirmed = true
    };

    public static AppUser Labor02 => new()
    {
        Id = "8104ab20-26c2-4651-b1de-c0baf04dbbd9",
        Email = "peter.labor@localhost",
        UserName = "peter.labor@localhost",
        EmailConfirmed = true
    };

    public static AppUser Labor03 => new()
    {
        Id = "e17c83de-1089-4f19-bf79-5f789133d37f",
        Email = "kevin.labor@localhost",
        UserName = "kevin.labor@localhost",
        EmailConfirmed = true
    };

    public static AppUser Labor04 => new()
    {
        Id = "54cd01ba-b9ae-4c14-bab6-f3df0219ba4c",
        Email = "suzan.labor@localhost",
        UserName = "suzan.labor@localhost",
        EmailConfirmed = true
    };
}