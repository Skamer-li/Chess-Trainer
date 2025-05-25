using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public static class Zobrist
{
    public static List<List<List<ulong>>> ZobristTable = new List<List<List<ulong>>>();
    public static Dictionary<ulong, TranspositionPart> transpositionTable = new Dictionary<ulong, TranspositionPart>();
    public static Dictionary<ulong, int> occuredPositions = new Dictionary<ulong, int>();

    private static ulong sideModifier = GenerateRandomUlong();

    public static void InitializeAndFillTable()
    {
        for (int i = 0; i < Board.TILE_COUNT_X; i++)
        {
            ZobristTable.Add(new List<List<ulong>>());
            for (int j = 0; j < Board.TILE_COUNT_Y; j++)
            {
                ZobristTable[i].Add(new List<ulong>());
                for (int k = 0; k < Board.PIECES_COUNT; k++)
                {
                    ZobristTable[i][j].Add((ulong)0);
                }
            }
        }

        for (int i = 0; i < Board.TILE_COUNT_X; i++)
        {
            for (int j = 0; j < Board.TILE_COUNT_Y; j++)
            {
                for (int k = 0; k < Board.PIECES_COUNT; k++)
                {
                    System.Random rnd = new System.Random();
                    ZobristTable[i][j][k] = GenerateRandomUlong();
                }
            }
        }
    }

    public static ulong ComputeHash(ChessPiece[,] board, bool isBlackToMove)
    {
        ulong h = 0;

        for (int i = 0; i < Board.TILE_COUNT_X; i++)
        {
            for (int j = 0; j < Board.TILE_COUNT_Y; j++)
            {
                if (board[i, j] != null)
                {
                    int piece = board[i, j].GetIndex();
                    h ^= ZobristTable[i][j][piece];
                }
            }
        }

        if (isBlackToMove)
            h ^= sideModifier;

        return h;
    }

    public static void Clear()
    {
        transpositionTable.Clear();
        occuredPositions.Clear();
    }

    private static ulong GenerateRandomUlong()
    {
        byte[] bytes = new byte[8];
        RandomNumberGenerator.Fill(bytes);
        return BitConverter.ToUInt64(bytes, 0);
    }
}
