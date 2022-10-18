using Chess.Board;
using Chess.Movement;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;

namespace Chess.Pieces;

internal sealed class King : Piece
{
    public CastlingRights CastlingRights { get; set; }
    private readonly static Dictionary<char, Square> s_castlingRookSquares;
    private readonly static Dictionary<Team, Square[]> s_kingsideCastlingSquares;
    private readonly static Dictionary<Team, Square[]> s_queensideCastlingSquares;
    private readonly List<Square[]> _threats;
    public static event EventHandler Check;
    public bool Threatened { get; private set; }
    static King()
    {
        s_castlingRookSquares = new()
        {
            ['K'] = new Square("H1"),
            ['Q'] = new Square("A1"),
            ['k'] = new Square("H8"),
            ['q'] = new Square("A8")
        };
        s_kingsideCastlingSquares = new()
        {
            [Team.White] = new[] { new Square("F1"), new Square("G1") },
            [Team.Black] = new[] { new Square("F8"), new Square("G8") }
        };
        s_queensideCastlingSquares = new()
        {
            [Team.White] = new[] { new Square("B1"), new Square("C1"), new Square("D1") },
            [Team.Black] = new[] { new Square("B8"), new Square("C8"), new Square("D8") }
        };
    }
    public King(Team team, Square square, bool isRaw = false) : base(team, square, PieceType.King, isRaw)
    {
        _threats = new List<List<Square>>();
        _moveset = Movesets.King;
        CastlingRights = CastlingRights.None;
        Value = 0;
    }
    public King(King other, bool isRaw = false) : base(other, isRaw)
    {
        _threats = other.CreateCopyOfThreats();
        CastlingRights = other.CastlingRights;
    }
    public override int Update()
    {
        FindAllThreats();
        return base.Update();
    }
    private List<Square[]> CreateCopyOfThreats()
    {
        List<Square[]> copy = new();
        foreach (var threat in _threats)
        {
            copy.Add(threat);
        }
        return copy;
    }
    public static Square GetCastlingRookSquare(char sideOfCastling, Team team)
    {
        return (team, sideOfCastling) switch
        {
            (Team.White, 'k') => s_castlingRookSquares['K'],
            (Team.White, 'q') => s_castlingRookSquares['Q'],
            (Team.Black, 'k') => s_castlingRookSquares['k'],
            (Team.Black, 'q') => s_castlingRookSquares['q'],
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    public static char GetCastlingSideFromRookSquare(Square square, Team team)
    {
        string squareAsString = square.ToString();
        return (team, squareAsString) switch
        {
            (Team.White, "h1") => 'k',
            (Team.White, "a1") => 'q',
            (Team.Black, "h8") => 'k',
            (Team.Black, "a8") => 'q',
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    public override void CheckPossibleMoves()
    {
        CheckRegularMoves();
        CheckCastlingMoves();
    }
    private void CheckRegularMoves()
    {
        foreach (var square in GetSquaresAroundTheKing())
        {
            if (!square.IsValid)
                continue;

            Team teamOnTheSquare = Owner.GetTeamOnSquare(square);

            if (teamOnTheSquare == _team || CheckIfSquareIsThreatened(square))
                continue;

            if (teamOnTheSquare == Team.Empty)
            {
                moves.Add(new Move(Square, square, 'm'));
            }
            else
            {
                moves.Add(new Move(Square, square, 'x'));
            }
        }
    }
    public void CheckCastlingMoves()
    {
        if (_threats.Count == 0)
        {
            if ((CastlingRights & CastlingRights.KingSide) == CastlingRights.KingSide)
            {
                char kingSideLetter = _team == Team.White ? 'K' : 'k';
                if (Owner.TryGetPiece(s_castlingRookSquares[kingSideLetter], out Piece piece))
                    CheckCastling(GetKingSideCastleSquares(), piece);
            }
            if ((CastlingRights & CastlingRights.QueenSide) == CastlingRights.QueenSide)
            {
                char queenSideLetter = _team == Team.White ? 'Q' : 'q';
                if (Owner.TryGetPiece(s_castlingRookSquares[queenSideLetter], out Piece piece))
                    CheckCastling(GetQueenSideCastleSquares(), piece);
            }
        }
    }
    private void CheckCastling(Square[] squaresToCheck, Piece potentialRook)
    {
        if (potentialRook is not Rook { HasMoved: false} r)
        {
            if (squaresToCheck[^1] == GetKingSideCastleSquares()[^1])
            {
                if ((CastlingRights & CastlingRights.QueenSide) != 0)
                    CastlingRights = CastlingRights.QueenSide;
                else
                    CastlingRights = CastlingRights.None;
            }
            else
            {
                if ((CastlingRights & CastlingRights.KingSide) != 0)
                    CastlingRights = CastlingRights.KingSide;
                else
                    CastlingRights = CastlingRights.None;
            }
            return;
        }

        foreach (var square in squaresToCheck)
        {
            Team teamOnTheSquare = Owner.GetTeamOnSquare(square);
            if (teamOnTheSquare != Team.Empty || CheckIfSquareIsThreatened(square))
                return;
        }

        moves.Add(new Move(Square, squaresToCheck[1], GetCastlingSideFromRookSquare(r.Square, _team)));
    }
    public Square[] GetSquaresAroundTheKing()
    {
        return new Square[] {
            new Square((char)(Square.Letter + 1), Square.Digit + 1),
            new Square((char)(Square.Letter + 1), Square.Digit - 1),
            new Square((char)(Square.Letter - 1), Square.Digit + 1),
            new Square((char)(Square.Letter - 1), Square.Digit - 1),
            new Square((char)(Square.Letter + 1), Square.Digit),
            new Square((char)(Square.Letter - 1), Square.Digit),
            new Square(Square.Letter, Square.Digit + 1),
            new Square(Square.Letter, Square.Digit - 1),
        };
    }
    public Square[] GetKingSideCastleSquares() => s_kingsideCastlingSquares[_team];
    public Square[] GetQueenSideCastleSquares() => s_queensideCastlingSquares[_team];
    public bool CheckMoveAgainstThreats(Piece piece, Move move) => !WouldMoveGenerateThreat(piece, move) && WouldMoveSolveAllThreats(move);
    public bool WouldMoveGenerateThreat(Piece piece, Move move)
    {
        if (!Owner.ArePiecesFacingEachOther(this, piece))
            return false;

        int iteratorX = piece.Square.Letter - Square.Letter;
        int iteratorY = piece.Square.Digit - Square.Digit;

        if (iteratorX == 0 && move.Former.Letter != move.Latter.Letter)
        {
            return CheckDirectionForThreats(MoveSets.Rook, (0, Math.Sign(iteratorY)), piece, move);
        }
        else if (iteratorY == 0 && move.Former.Digit != move.Latter.Digit)
        {
            return CheckDirectionForThreats(MoveSets.Rook, (Math.Sign(iteratorX), 0), piece, move);
        }
        else if (Math.Abs(iteratorX) == Math.Abs(iteratorY))
        {
            if (!IsDiagonalMoveInDirectionOfPotentialThreat(move, iteratorX, iteratorY))
                return CheckDirectionForThreats(MoveSets.Bishop, (Math.Sign(iteratorX), Math.Sign(iteratorY)), piece, move);
        }
        return false;
    }
    private static bool IsDiagonalMoveInDirectionOfPotentialThreat(Move move, int iteratorX, int iteratorY)
    {
        int directionX = move.Latter.Letter - move.Former.Letter;
        int directionY = move.Latter.Digit - move.Former.Digit;
        if (Math.Abs(directionY) == Math.Abs(directionX))
        {
            if (Math.Sign(iteratorX) == Math.Sign(directionX) && Math.Sign(iteratorY) == Math.Sign(directionY))
                return true;
        }
        return false;
    }
    private bool CheckDirectionForThreats(MoveSets set, (int, int) iterator, Piece piece, Move move)
    {
        Move[] moves = MovementManager.GenerateMovesInADirection(piece.Square, iterator, Owner, excludeMove: "moves");
        if (moves.Length == 0)
            return false;

        if (Owner.TryGetPiece(moves[^1].Latter, out Piece attackingPiece))
        {
            if (((attackingPiece.MoveSet & set) != 0) && attackingPiece.Square != move.Latter)
                return true;
        }
        return false;
    }
    public bool WouldMoveSolveAllThreats(Move move)
    {
        if (_threats.Count == 0)
            return true;

        Square destination = move.Description == 'p' ? new Square(move.Latter.Letter, move.Former.Digit) : move.Latter;

        foreach (var threat in _threats)
        {
            if (!threat.Contains(destination))
                return false;
        }

        return true;
    }
    public void ClearThreats() => _threats.Clear();
    public void FindAllThreats()
    {
        ClearThreats();
        List<(MoveSets set, Move[] moves)> groupedMoves = new();
        groupedMoves.AddRange(MovementManager.GenerateEveryMove(Square, MoveSets.Diagonal, Owner));
        groupedMoves.AddRange(MovementManager.GenerateEveryMove(Square, MoveSets.Rook, Owner));
        groupedMoves.AddRange(MovementManager.GenerateEveryMove(Square, MoveSets.Pawn, Owner));
        groupedMoves.AddRange(MovementManager.GenerateEveryMove(Square, MoveSets.Knight, Owner));
        foreach (var grouping in groupedMoves)
        {
            if (grouping.moves.Length == 0)
                continue;
            if (Owner.TryGetPiece(grouping.moves[^1].Latter, out Piece piece))
            {
                if ((grouping.set & piece.MoveSet) != 0)
                {
                    var squares = (from m in grouping.moves select m.Latter).ToArray();
                    _threats.Add(squares);
                }
            }
        }
        if (_threats.Count > 0)
            OnCheck(EventArgs.Empty);
    }
    public bool CheckIfSquareIsThreatened(Square checkedSquare)
    {
        MoveSets[] setsToCheck = { MoveSets.Diagonal, MoveSets.Rook, MoveSets.Pawn, MoveSets.Knight, MoveSets.King };
        foreach (MoveSets set in setsToCheck)
        {
            foreach ((MoveSets set, Move[] moves) groupedMoves in MovementManager.GenerateEveryMove(checkedSquare, set, Owner, pieceToIgnore: this))
            {
                if (groupedMoves.moves.Length == 0)
                    continue;

                Move lastMove = groupedMoves.moves[^1];
                if (Owner.TryGetPiece(lastMove.Latter, out Piece piece))
                {
                    if (lastMove.Description == 'x' && (groupedMoves.set & piece.MoveSet) != 0)
                        return true;
                }
            }
        }
        return false;
    }
    public override void MovePiece(Move move)
    {
        base.MovePiece(move);
        CastlingRights = CastlingRights.None;
    }
    public void OnCheck(EventArgs e)
    {
        if (IsRaw)
            return;

        Check?.Invoke(this, e);
    }
}
