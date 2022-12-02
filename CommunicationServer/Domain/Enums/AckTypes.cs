namespace Domain.Enums;

public enum AckTypes
{
    Success = 0,
    GameNotFound = 1,
    InvalidMove = 2,
    ConnectionFailure = 3,
    GameHasFinished = 4,
    NotUserTurn = 5,
    DrawNotOffered = 6,
    DrawOfferDeclined = 7,
    DrawOfferExpired = 8,
    RematchNotOffered = 9,
    RematchOfferDeclined = 10,
    RematchOfferExpired = 11
}