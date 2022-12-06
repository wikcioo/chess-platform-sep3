using Domain.DTOs.GameEvents;

namespace Application.ChessTimers;

public delegate void GameEventHandler(object sender, EventArgs args, GameEventDto dto);
