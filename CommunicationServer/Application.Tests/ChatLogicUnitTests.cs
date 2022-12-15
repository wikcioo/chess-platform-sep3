namespace Application.Tests;

using Domain.DTOs.Chat;
using Logic;
using LogicInterfaces;
using System.Collections.Generic;


public class ChatLogicUnitTests
{
    private IChatLogic _chatLogic;

    public ChatLogicUnitTests()
    {
        _chatLogic = new ChatLogic();
    }

    //Add
    [Fact]
    public void AddingMessageToNonExistentRoomDoesNotFail()
    {
        _chatLogic.Add(new MessageDto
        {
            GameRoom = 0,
            Body = "",
            Username = "a"
        });
    }

    [Fact]
    public void AddingMessageToAvailableRoomDoesNotFail()
    {
        var dto = new MessageDto
        {
            GameRoom = 0,
            Body = "",
            Username = "a"
        };
        _chatLogic.Add(dto);
        _chatLogic.Add(dto);
    }

    //Get log
    [Fact]
    public void GetLogReturnsIEnumerableOfMessageDtos()
    {
        var dto = new MessageDto
        {
            GameRoom = 0,
            Body = "",
            Username = "a"
        };
        _chatLogic.Add(dto);
        _chatLogic.Add(dto);
        _chatLogic.GetLog(0);
        Assert.IsAssignableFrom<IEnumerable<MessageDto>>(_chatLogic.GetLog(0));
    }

}