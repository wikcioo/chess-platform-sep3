namespace Domain.Enums;

public enum GameStreamEvents
{
    NewFenPosition = 0,
    TimeUpdate = 1,
    Resignation = 2,
    DrawOffer = 3,
    DrawOfferTimeout = 4,
    DrawOfferAcceptation = 5,
    PlayerJoined = 6,
    ReachedEndOfTheGame = 7,
    RematchOffer = 8,
    RematchOfferTimeout = 9,
    RematchOfferAcceptation = 10,
    RematchInvitation = 11,
    GameAborted = 12
}