using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ChessPiece[,] board)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        //Move by one and by two
        if (currentY + direction < Board.TILE_COUNT_Y && currentY + direction >= 0 && board[currentX, currentY + direction] == null)
        {
            availableMoves.Add(new Vector2Int(currentX, currentY + direction));

            if (currentY + 2 * direction < Board.TILE_COUNT_Y && currentY + 2 * direction >= 0 && board[currentX, currentY + 2 * direction] == null)
            {
                if ((team == 0 && currentY == 1) || (team == 1 && currentY == 6))
                {
                    availableMoves.Add(new Vector2Int(currentX, currentY + 2 * direction));
                }
            }
        }

        //Kill move
        if (currentY + direction < Board.TILE_COUNT_Y && currentY + direction >= 0)
        {
            if (currentX != 0 && board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
            }

            if (currentX != Board.TILE_COUNT_X - 1 && board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
            }
        }

        //En passant
        if (team == 0 && currentY == 4 || team == 1 && currentY == 3)
        {
            if (currentX != 0 && board[currentX - 1, currentY] != null && board[currentX - 1, currentY].team != team && board[currentX - 1, currentY].type == PieceType.Pawn && board[currentX - 1, currentY].movesMade == 1)
            {
                if (team == 0 && Board.lastMoveB == new Vector2(currentX - 1, currentY) || team == 1 && Board.lastMoveW == new Vector2(currentX - 1, currentY))
                    availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
            }

            if (currentX != Board.TILE_COUNT_X - 1 && board[currentX + 1, currentY] != null && board[currentX + 1, currentY].team != team && board[currentX + 1, currentY].type == PieceType.Pawn && board[currentX + 1, currentY].movesMade == 1)
            {
                if (team == 0 && Board.lastMoveB == new Vector2(currentX + 1, currentY) || team == 1 && Board.lastMoveW == new Vector2(currentX + 1, currentY))
                    availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
            }
        }
        return availableMoves;
    }
}
