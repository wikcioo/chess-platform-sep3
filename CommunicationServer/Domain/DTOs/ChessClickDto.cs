using Rudzoft.ChessLib.Types;

namespace Domain.DTOs;

public class ChessClickDto
{
    public Pieces? PieceType { get; set; }
    public Squares SquareType { get; set; }

    public bool HasMoveTo { get; set; }

    public ChessClickDto(Pieces? pieceType, Squares squareType, bool hasMoveTo)
    {
        PieceType = pieceType;
        SquareType = squareType;
        HasMoveTo = hasMoveTo;
    }

    public override string ToString()
    {
        return
            $"{nameof(PieceType)}: {PieceType}, {nameof(SquareType)}: {SquareType}, {nameof(HasMoveTo)}: {HasMoveTo}";
    }
}