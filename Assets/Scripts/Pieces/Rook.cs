using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ChessPiece[,] board)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        int direction = 1;

        //Move up 
        while (currentY + direction < Board.TILE_COUNT_Y)
        {
            if (board[currentX, currentY + direction] != null)
            {
                if (board[currentX, currentY + direction].team != team)
                {
                    availableMoves.Add(new Vector2Int(currentX, currentY + direction));
                    break;
                }
                else
                {
                    break;
                }
            }
            else
            {
                availableMoves.Add(new Vector2Int(currentX, currentY + direction));
                direction++;
            }
        }

        direction = 1;

        //Move down
        while (currentY - direction >= 0)
        {
            if (board[currentX, currentY - direction] != null)
            {
                if (board[currentX, currentY - direction].team != team)
                {
                    availableMoves.Add(new Vector2Int(currentX, currentY - direction));
                    break;
                }
                else
                {
                    break;
                }
            }
            else
            {
                availableMoves.Add(new Vector2Int(currentX, currentY - direction));
                direction++;
            }
        }

        direction = 1;

        //Move right

        while (currentX + direction < Board.TILE_COUNT_X)
        {
            if (board[currentX + direction, currentY] != null)
            {
                if (board[currentX + direction, currentY].team != team)
                {
                    availableMoves.Add(new Vector2Int(currentX + direction, currentY));
                    break;
                }
                else
                { 
                    break;
                }
            }
            else
            {
                availableMoves.Add(new Vector2Int(currentX + direction, currentY));
                direction++;
            }
        }

        direction = 1;

        //Move left
        while (currentX - direction >= 0)
        {
            if (board[currentX - direction, currentY] != null)
            {
                if (board[currentX - direction, currentY].team != team)
                {
                    availableMoves.Add(new Vector2Int(currentX - direction, currentY));
                    break;
                }
                else
                {
                    break;
                }
            }
            else
            {
                availableMoves.Add(new Vector2Int(currentX - direction, currentY));
                direction++;
            }
        }

        return availableMoves;
    }

}
