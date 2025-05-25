using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Evaluation
{
    const int ENDGAME_TRESHHOLD = 1200;

    //Piece-Square Tables

    private static readonly int[,] kingMidGameTable = new int[8, 8]
    {
        {-30, -40, -40, -50, -50, -40, -40, -30},
        {-30, -40, -40, -50, -50, -40, -40, -30},
        {-30, -40, -40, -50, -50, -40, -40, -30},
        {-30, -40, -40, -50, -50, -40, -40, -30},
        {-20, -30, -30, -40, -40, -30, -30, -20},
        {-10, -20, -20, -20, -20, -20, -20, -10},
        { 20,  20,   0,   0,   0,   0,  20,  20},
        { 20,  30,  10,   0,   0,  10,  30,  20}
    };

    private static readonly int[,] kingEndGameTable = new int[8, 8]
    {
        {-50, -40, -30, -20, -20, -30, -40, -50},
        {-30, -20, -10,   0,   0, -10, -20, -30},
        {-30, -10,  20,  30,  30,  20, -10, -30},
        {-30, -10,  30,  40,  40,  30, -10, -30},
        {-30, -10,  30,  40,  40,  30, -10, -30},
        {-30, -10,  20,  30,  30,  20, -10, -30},
        {-30, -30,   0,   0,   0,   0, -30, -30},
        {-50, -30, -30, -30, -30, -30, -30, -50}
    };

    private static readonly int[,] queenMidGameTable = new int[8, 8]
    {
        {-20, -10, -10, -5,  -5, -10, -10, -20},
        {-10,   0,   0,  0,   0,   0,   0, -10},
        {-10,   0,   5,  5,   5,   5,   0, -10},
        { -5,   0,   5,  5,   5,   5,   0,  -5},
        {  0,   0,   5,  5,   5,   5,   0,  -5},
        {-10,   5,   5,  5,   5,   5,   0, -10},
        {-10,   0,   5,  0,   0,   0,   0, -10},
        {-20, -10, -10, -5,  -5, -10, -10, -20}
    };

    private static readonly int[,] queenEndGameTable = new int[8, 8]
    {
        {-20, -10, -10, -5,  -5, -10, -10, -20},
        {-10,   0,   0,  0,   0,   0,   0, -10},
        {-10,   0,   5,  5,   5,   5,   0, -10},
        { -5,   0,   5,  5,   5,   5,   0,  -5},
        {  0,   0,   5,  5,   5,   5,   0,  -5},
        {-10,   5,   5,  5,   5,   5,   0, -10},
        {-10,   0,   5,  0,   0,   0,   0, -10},
        {-20, -10, -10, -5,  -5, -10, -10, -20}
    };

    private static readonly int[,] rookMidGameTable = new int[8, 8]
    {
        {  0,   0,   0,   0,   0,   0,   0,   0},
        {  5,  10,  10,  10,  10,  10,  10,   5},
        { -5,   0,   0,   0,   0,   0,   0,  -5},
        { -5,   0,   0,   0,   0,   0,   0,  -5},
        { -5,   0,   0,   0,   0,   0,   0,  -5},
        { -5,   0,   0,   0,   0,   0,   0,  -5},
        { -5,   0,   0,   0,   0,   0,   0,  -5},
        {  0,   0,   0,   5,   5,   0,   0,   0}
    };

    private static readonly int[,] rookEndGameTable = new int[8, 8]
    {
        {  0,   0,   0,   0,   0,   0,   0,   0},
        {  5,  10,  10,  10,  10,  10,  10,   5},
        { -5,   0,   0,   0,   0,   0,   0,  -5},
        { -5,   0,   0,   0,   0,   0,   0,  -5},
        { -5,   0,   0,   0,   0,   0,   0,  -5},
        { -5,   0,   0,   0,   0,   0,   0,  -5},
        { -5,   0,   0,   0,   0,   0,   0,  -5},
        {  0,   0,   0,   5,   5,   0,   0,   0}
    };

    private static readonly int[,] knightMidGameTable = new int[8, 8]
    {
        { -50, -40, -30, -30, -30, -30, -40, -50 },
        { -40, -20,   0,   5,   5,   0, -20, -40 },
        { -30,   5,  10,  15,  15,  10,   5, -30 },
        { -30,   0,  15,  20,  20,  15,   0, -30 },
        { -30,   5,  15,  20,  20,  15,   5, -30 },
        { -30,   0,  10,  15,  15,  10,   0, -30 },
        { -40, -20,   0,   0,   0,   0, -20, -40 },
        { -50, -40, -30, -30, -30, -30, -40, -50 }
    };

    private static readonly int[,] knightEndGameTable = new int[8, 8]
    {
        { -50, -40, -30, -30, -30, -30, -40, -50 },
        { -40, -20,   0,   0,   0,   0, -20, -40 },
        { -30,   0,  10,  15,  15,  10,   0, -30 },
        { -30,   5,  15,  20,  20,  15,   5, -30 },
        { -30,   5,  15,  20,  20,  15,   5, -30 },
        { -30,   0,  10,  15,  15,  10,   0, -30 },
        { -40, -20,   0,   0,   0,   0, -20, -40 },
        { -50, -40, -30, -30, -30, -30, -40, -50 }
    };

    private static readonly int[,] bishopMidGameTable = new int[8, 8]
    {
        { -20, -10, -10, -10, -10, -10, -10, -20 },
        { -10,   5,   0,   0,   0,   0,   5, -10 },
        { -10,  10,  10,  10,  10,  10,  10, -10 },
        { -10,   0,  10,  10,  10,  10,   0, -10 },
        { -10,   5,   5,  10,  10,   5,   5, -10 },
        { -10,   0,   5,  10,  10,   5,   0, -10 },
        { -10,   0,   0,   0,   0,   0,   0, -10 },
        { -20, -10, -10, -10, -10, -10, -10, -20 }
    };

    private static readonly int[,] bishopEndGameTable = new int[8, 8]
    {
        { -20, -10, -10, -10, -10, -10, -10, -20 },
        { -10,   5,   0,   0,   0,   0,   5, -10 },
        { -10,  10,  10,  10,  10,  10,  10, -10 },
        { -10,   0,  10,  15,  15,  10,   0, -10 },
        { -10,   5,   5,  15,  15,   5,   5, -10 },
        { -10,   0,  10,  10,  10,  10,   0, -10 },
        { -10,   0,   0,   0,   0,   0,   0, -10 },
        { -20, -10, -10, -10, -10, -10, -10, -20 }
    };

    private static readonly int[,] pawnMidGameTable = new int[8, 8]
    {
        {  0,   0,   0,   0,   0,   0,   0,   0 },
        { 50,  50,  50,  50,  50,  50,  50,  50 },
        { 10,  10,  20,  30,  30,  20,  10,  10 },
        {  5,   5,  10,  25,  25,  10,   5,   5 },
        {  0,   0,   0,  20,  20,   0,   0,   0 },
        {  5,  -5, -10,   0,   0, -10,  -5,   5 },
        {  5,  10,  10, -20, -20,  10,  10,   5 },
        {  0,   0,   0,   0,   0,   0,   0,   0 }
    };

    private static readonly int[,] pawnEndGameTable = new int[8, 8]
    {
        {  0,   0,   0,   0,   0,   0,   0,   0 },
        { 60,  60,  60,  60,  60,  60,  60,  60 },
        { 20,  20,  30,  40,  40,  30,  20,  20 },
        { 10,  10,  20,  35,  35,  20,  10,  10 },
        {  0,   0,  10,  30,  30,  10,   0,   0 },
        { 10,  -5, -10,   0,   0, -10,  -5,  10 },
        { 10,  20,  20, -30, -30,  20,  20,  10 },
        {  0,   0,   0,   0,   0,   0,   0,   0 }
    };

    public static int EvaluatePiecePos(PieceType type, bool isEndgame, int team, int posX, int posY)
    {
        if (team == 0)
        {
            posY = 7 - posY;
        }

        switch (type)
        {
            case PieceType.King:
                if (isEndgame)
                    return kingEndGameTable[posY, posX];
                else
                    return kingMidGameTable[posY, posX];
            case PieceType.Queen:
                if (isEndgame)
                    return queenEndGameTable[posY, posX];
                else
                    return queenMidGameTable[posY, posX];
            case PieceType.Rook:
                if (isEndgame)
                    return rookEndGameTable[posY, posX];
                else
                    return rookMidGameTable[posY, posX];
            case PieceType.Knight:
                if (isEndgame)
                    return knightEndGameTable[posY, posX];
                else
                    return knightMidGameTable[posY, posX];
            case PieceType.Bishop:
                if (isEndgame)
                    return bishopEndGameTable[posY, posX];
                else
                    return bishopMidGameTable[posY, posX];
            case PieceType.Pawn:
                if (isEndgame)
                    return pawnEndGameTable[posY, posX];
                else
                    return pawnMidGameTable[posY, posX];
            default:
                Debug.Log("Invalid piece type");
                return 0;
        }
    }

    public static bool IsEndgame(ChessPiece[,] board)
    {
        int totalSum = 0;

        foreach (ChessPiece piece in board)
            if (piece != null && piece.type != PieceType.King)
            {
                totalSum += piece.pieceValue;
            }

        if (totalSum <= 2 * ENDGAME_TRESHHOLD)
            return true;

        return false;
    }
}
