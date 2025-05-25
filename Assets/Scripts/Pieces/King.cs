using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ChessPiece[,] board)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        for (int x = currentX - 1; x <= currentX + 1; x++)
        {
            for (int y = currentY - 1; y <= currentY + 1; y++)
            {
                if (x == currentX && y == currentY)
                {
                    continue;
                }

                if (x >= 0 && x < Board.TILE_COUNT_X && y >= 0 && y < Board.TILE_COUNT_Y)
                {
                    if (board[x, y] == null || board[x, y].team != team)
                    {
                        availableMoves.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        bool isCheck = false;

        if (movesMade == 0 && ((team == 0 && currentX == 4 && currentY == 0) || (team == 1 && currentX == 4 && currentY == 7))) 
        {
            foreach (ChessPiece piece in board)
            {
                if (piece != null && piece.type != PieceType.King && piece.team != team && piece.GetAvailableMoves(board).Contains(new Vector2Int(currentX, currentY)))
                {
                    isCheck = true;
                    break;
                }
            }

            if (!isCheck)
            {
                if (team == 0)
                {
                    //Left
                    if (board[0, 0] != null && board[0, 0].team == team && board[0, 0].type == PieceType.Rook && board[0, 0].movesMade == 0)
                    {
                        bool pathBlocked = false;

                        for (int x = 2; x < currentX; x++)
                        {
                            if (TileBlocked(board, x, currentY))
                            {
                                pathBlocked = true;
                                break;
                            }
                        }

                        if (board[1, currentY] != null)
                            pathBlocked = true;

                        if (!pathBlocked)
                            availableMoves.Add(new Vector2Int(currentX - 2, currentY));
                    }

                    //Right
                    if (board[7, 0] != null && board[7, 0].team == team && board[7, 0].type == PieceType.Rook && board[7, 0].movesMade == 0)
                    {
                        bool pathBlocked = false;

                        for (int x = currentX + 1; x < Board.TILE_COUNT_X - 1; x++)
                        {
                            if (TileBlocked(board, x, currentY))
                            {
                                pathBlocked = true;
                                break;
                            }
                        }

                        if (!pathBlocked)
                            availableMoves.Add(new Vector2Int(currentX + 2, currentY));
                    }
                }
                else
                {
                    //Left
                    if (board[0, 7] != null && board[0, 7].team == team && board[0, 7].type == PieceType.Rook && board[0, 7].movesMade == 0)
                    {
                        bool pathBlocked = false;

                        for (int x = 2; x < currentX; x++)
                        {
                            if (TileBlocked(board, x, currentY))
                            {
                                pathBlocked = true;
                                break;
                            }
                        }

                        if (board[1, currentY] != null)
                            pathBlocked = true;

                        if (!pathBlocked)
                            availableMoves.Add(new Vector2Int(currentX - 2, currentY));
                    }

                    //Right
                    if (board[7, 7] != null && board[7, 7].team == team && board[7, 7].type == PieceType.Rook && board[7, 7].movesMade == 0)
                    {
                        bool pathBlocked = false;

                        for (int x = currentX + 1; x < Board.TILE_COUNT_X - 1; x++)
                        {
                            if (TileBlocked(board, x, currentY))
                            {
                                pathBlocked = true;
                                break;
                            }
                        }

                        if (!pathBlocked)
                            availableMoves.Add(new Vector2Int(currentX + 2, currentY));
                    }
                }
            }
        }

        return availableMoves;
    }

    private bool TileBlocked(ChessPiece[,] board, int posX, int posY)
    {
        if (board[posX, posY] != null)
            return true;

        foreach (ChessPiece chessPiece in board)
        {
            if (chessPiece != null && chessPiece.team != team)
            {
                if (chessPiece.type == PieceType.King && chessPiece.movesMade == 0)
                    continue;

                List<Vector2Int> currentMoves = chessPiece.GetAvailableMoves(board);

                if (currentMoves.Contains(new Vector2Int(posX, posY)))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
