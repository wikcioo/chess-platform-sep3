namespace Application.Tests;

using Domain.DTOs.User;
using Domain.Models;
using Logic;
using LogicInterfaces;
using Mocks;
using System;
using System.Collections.Generic;


public class UserLogicUnitTests
{
    private IUserLogic _userLogic;

    public UserLogicUnitTests()
    {
        _userLogic = new UserLogic(new UserServiceMock());
    }

    
    //Create
    [Fact]
    public void CreatingUserWithTooLongUsernameThrowsArgumentException()
    {
        var user = new User
        {
            Email = "email@email.com",
            Password = "password",
            Role = "admin",
            Username = "UsernameThatIsTooLong"
        };
        Assert.ThrowsAsync<ArgumentException>(() => _userLogic.CreateAsync(user));
    }
    
    [Fact]
    public void CreatingUserInvalidEmailThrowsArgumentException()
    {
        var user = new User
        {
            Email = "email",
            Password = "password",
            Role = "admin",
            Username = "username"
        };
        Assert.ThrowsAsync<ArgumentException>(() => _userLogic.CreateAsync(user));
    }
    [Fact]
    public void CreatingUserWithTooShortPasswordArgumentException()
    {
        var user = new User
        {
            Email = "email@email.com",
            Password = "12",
            Role = "admin",
            Username = "username"
        };
        Assert.ThrowsAsync<ArgumentException>(() => _userLogic.CreateAsync(user));
    }
    
    [Fact]
    public async void CreatingValidUserReturnsSameUser()
    {
        var user = new User
        {
            Email = "email@email.com",
            Password = "password",
            Role = "admin",
            Username = "username"
        };
        var createdUser = await _userLogic.CreateAsync(user);
        Assert.Equal(user, createdUser);
    }
    
    //Get
    [Fact]
    public async void GetInsensitiveReturnsCorrectUserByUsername()
    {
        var users = await _userLogic.GetInsensitiveAsync(new UserSearchParamsDto("username"));
        Assert.IsAssignableFrom<IEnumerable<UserSearchResultDto>>(users);
    }
}