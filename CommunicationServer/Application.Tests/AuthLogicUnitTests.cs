namespace Application.Tests;

using Logic;
using LogicInterfaces;
using Mocks;
using System;


public class AuthLogicUnitTests
{
    private IAuthLogic _authLogic;

    public AuthLogicUnitTests()
    {
        _authLogic = new AuthLogic(new UserServiceMock());
    }

    [Fact]
    public void ValidateUserThrowsExceptionWhenUserNotFound()
    {
        Assert.ThrowsAsync<Exception>(() => _authLogic.ValidateUser("wrongEmail", "wrongPassword"));
    }

    [Fact]
    public void ValidateUserThrowsExceptionWhenWrongPassword()
    {
        Assert.ThrowsAsync<Exception>(() => _authLogic.ValidateUser("email", "wrongPassword"));
    }

    [Fact]
    public async void ValidateUserReturnsCorrectUserWhenValidCredentials()
    {
        var foundUser = await _authLogic.ValidateUser("email", "password");
        Assert.Equal("email", foundUser.Email);
        Assert.Equal("password", foundUser.Password);
    }

}