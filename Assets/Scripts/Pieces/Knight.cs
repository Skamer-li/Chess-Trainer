using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ChessPiece[,] board)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        int oneModifier = 1;
        int twoModifier = 2;

        //Left and Up moves 
        if (currentX - twoModifier >= 0 && currentY + oneModifier < Board.TILE_COUNT_Y)
        {
            if (board[currentX - twoModifier, currentY + oneModifier] == null || board[currentX - twoModifier, currentY + oneModifier].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX - twoModifier, currentY + oneModifier));
            }
        }

        if (currentX - oneModifier >= 0 && currentY + twoModifier < Board.TILE_COUNT_Y)
        {
            if (board[currentX - oneModifier, currentY + twoModifier] == null || board[currentX - oneModifier, currentY + twoModifier].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX - oneModifier, currentY + twoModifier));
            }
        }

        //Left and Down moves 
        if (currentX - twoModifier >= 0 && currentY - oneModifier >= 0)
        {
            if (board[currentX - twoModifier, currentY - oneModifier] == null || board[currentX - twoModifier, currentY - oneModifier].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX - twoModifier, currentY - oneModifier));
            }
        }

        if (currentX - oneModifier >= 0 && currentY - twoModifier >= 0)
        {
            if (board[currentX - oneModifier, currentY - twoModifier] == null || board[currentX - oneModifier, currentY - twoModifier].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX - oneModifier, currentY - twoModifier));
            }
        }

        //Right and Up moves 
        if (currentX + twoModifier < Board.TILE_COUNT_X && currentY + oneModifier < Board.TILE_COUNT_Y)
        {
            if (board[currentX + twoModifier, currentY + oneModifier] == null || board[currentX + twoModifier, currentY + oneModifier].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX + twoModifier, currentY + oneModifier));
            }
        }

        if (currentX + oneModifier < Board.TILE_COUNT_X && currentY + twoModifier < Board.TILE_COUNT_Y)
        {
            if (board[currentX + oneModifier, currentY + twoModifier] == null || board[currentX + oneModifier, currentY + twoModifier].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX + oneModifier, currentY + twoModifier));
            }
        }

        //Right and Down moves 
        if (currentX + twoModifier < Board.TILE_COUNT_X && currentY - oneModifier >= 0)
        {
            if (board[currentX + twoModifier, currentY - oneModifier] == null || board[currentX + twoModifier, currentY - oneModifier].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX + twoModifier, currentY - oneModifier));
            }
        }

        if (currentX + oneModifier < Board.TILE_COUNT_X && currentY - twoModifier >= 0)
        {
            if (board[currentX + oneModifier, currentY - twoModifier] == null || board[currentX + oneModifier, currentY - twoModifier].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX + oneModifier, currentY - twoModifier));
            }
        }

        return availableMoves;


    }
}
