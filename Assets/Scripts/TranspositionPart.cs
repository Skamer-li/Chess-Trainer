using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class TranspositionPart
    {
        public int evaluation;
        public int depth;
        public ChessPiece bestPiece;
        public Vector2Int bestMove;

        public TranspositionPart(int eval, int depth, ChessPiece piece, Vector2Int move)
        {
            evaluation = eval;
            this.depth = depth;
            bestPiece = piece;
            bestMove = move;
        }
    }
}
