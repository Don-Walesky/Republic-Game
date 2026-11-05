namespace Republic.Core.Tests.Identity;

using System;
using Republic.Core.Identity.Services;
using Xunit;

public sealed class GenderConfirmationTests
{
    [Fact]
    public void RegisterAccount_WithFemaleGender_ConfiguresMadamPresident()
    {
        var svc = new PlayerIdentityService();
        var dob = new DateOnly(1995, 6, 15);
        var account = svc.RegisterAccount("Amara", "Diallo", "amara@republic.test", dob, gender: "Female");

        Assert.Equal("Female", account.PersonalIdentity.Gender);
        Assert.Equal("Madam President", account.PersonalIdentity.HonorificTitle);
        Assert.Equal("👩‍💼", account.PersonalIdentity.AvatarSymbol);
    }

    [Fact]
    public void RegisterAccount_WithMaleGender_ConfiguresMrPresident()
    {
        var svc = new PlayerIdentityService();
        var dob = new DateOnly(1995, 6, 15);
        var account = svc.RegisterAccount("Marcus", "Thorne", "marcus@republic.test", dob, gender: "Male");

        Assert.Equal("Male", account.PersonalIdentity.Gender);
        Assert.Equal("Mr. President", account.PersonalIdentity.HonorificTitle);
        Assert.Equal("👨‍💼", account.PersonalIdentity.AvatarSymbol);
    }
}

