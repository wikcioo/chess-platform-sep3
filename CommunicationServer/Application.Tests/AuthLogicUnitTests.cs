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
        Assert.ThrowsAsync<Exception>(() => _authLogic.LoginAsync("wrongEmail", "wrongPassword"));
    }

    [Fact]
    public void ValidateUserThrowsExceptionWhenWrongPassword()
    {
        Assert.ThrowsAsync<Exception>(() => _authLogic.LoginAsync("email", "wrongPassword"));
    }

    [Fact]
    public async void ValidateUserReturnsCorrectUserWhenValidCredentials()
    {
        var foundUser = await _authLogic.LoginAsync("email", "password");
        Assert.Equal("email", foundUser.Email);
        Assert.Equal("password", foundUser.Password);
    }

}