using System;
using System.Collections.Generic;
using System.Text;
using Chess.Board;
using Chess.Pieces;

namespace Chess
{
    class GameState
    {
        private (Square, Piece)[,] squares;
        private Team toMove;
        private Piece king;
        private GameState previous;
        private GameState next;

        public GameState((Square, Piece)[,] squares, Team toMove, GameState previous = null, GameState nex = null)
        {
            this.squares = new (Square, Piece)[squares.GetLength(0), squares.GetLength(1)];
            this.toMove = toMove;
            for(int i = 0; i < squares.GetLength(0); ++i)
            {
                for(int j = 0; j < squares.GetLength(1); ++j)
                {
                    this.squares[i, j].Item1 = squares[i, j].Item1;
                    switch(squares[i,j].Item2)
                    {
                        case Pawn p:
                            this.squares[i, j].Item2 = new Pawn(p);
                            break;
                        case King k:
                            this.squares[i, j].Item2 = new King(k);
                            this.king = this.squares[i, j].Item2;
                            break;
                        case Queen q:
                            this.squares[i, j].Item2 = new Queen(q);
                            break;
                        case Bishop b:
                            this.squares[i, j].Item2 = new Bishop(b);
                            break;
                        case Knight n:
                            this.squares[i, j].Item2 = new Knight(n);
                            break;
                        case Rook r:
                            this.squares[i, j].Item2 = new Rook(r);
                            break;
                        default:
                            this.squares[i, j].Item2 = null;
                            break;
                    }
                }
            }
        }
        public bool IsStateValid()
        {
            foreach (var item in squares)
            {
                if (item.Item2 != null && item.Item2.Team == toMove)
                {
                    item.Item2.Update();
                    if (item.Item2.CanMoveToSquare(king.Square))
                        return false;
                }
            }
            return true;
        }

    }
}
