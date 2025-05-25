using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public enum PieceType
{
    Pawn = 1, 
    Rook = 2, 
    Bishop = 3, 
    Knight = 4, 
    Queen = 5, 
    King = 6
}

public class ChessPiece : MonoBehaviour
{
    public int team;

    public int currentX;
    public int currentY;
    public int pieceValue;
    public int movesMade = 0;

    public PieceType type;

    public virtual List<Vector2Int> GetAvailableMoves(ChessPiece[,] board)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        return availableMoves;
    }

    public int GetIndex()
    {
        if (team == 0)
        {
            switch (type)
            {
                case PieceType.Pawn:
                    return 0;
                case PieceType.Knight:
                    return 1;
                case PieceType.Bishop:
                    return 2;
                case PieceType.Rook:
                    return 3;
                case PieceType.Queen:
                    return 4;
                case PieceType.King:
                    return 5;
                default:
                    return -1;
            }
        }
        else
        {
            switch (type)
            {
                case PieceType.Pawn:
                    return 6;
                case PieceType.Knight:
                    return 7;
                case PieceType.Bishop:
                    return 8;
                case PieceType.Rook:
                    return 9;
                case PieceType.Queen:
                    return 10;
                case PieceType.King:
                    return 11;
                default:
                    return -1;
            }
        }
    }
}
