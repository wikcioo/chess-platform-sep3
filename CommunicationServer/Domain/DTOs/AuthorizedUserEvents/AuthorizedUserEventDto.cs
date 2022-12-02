using Domain.Enums;

namespace Domain.DTOs.AuthorizedUserEvents;

public class AuthorizedUserEventDto
{
    public AuthUserEvents Event { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public string ReceiverUsername { get; set; } = string.Empty;
    public ulong GameRoomId { get; set; }
}