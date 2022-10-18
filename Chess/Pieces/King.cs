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
    private readonly List<List<Square>> _threats;
    private bool _hasMoved;
    private readonly Square[] _adjacentSquares;
    public Square[] AdjacentSquares { get => _adjacentSquares; }
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
        _adjacentSquares = CreateAdjacentSquares();
    }
    public King(King other, bool isRaw = false) : base(other, isRaw)
    {
        _threats = new();
        CastlingRights = other.CastlingRights;
        _adjacentSquares = CreateAdjacentSquares();
        _hasMoved = other._hasMoved;
    }
    private Square[] CreateAdjacentSquares()
    {
        return new Square[] {
            new Square(Square, (1, 1)),
            new Square(Square, (1, -1)),
            new Square(Square, (-1, 1)),
            new Square(Square, (-1, -1)),
            new Square(Square, (1, 0)),
            new Square(Square, (-1, 0)),
            new Square(Square, (0, 1)),
            new Square(Square, (0, -1)),
        };
    }
    public bool IsAdjacentSquare(int letter, int digit)
    {
        int minLetter = Square.Letter - 1;
        int maxLetter = Square.Letter + 1;
        int minDigit = Square.Digit - 1;
        int maxDigit = Square.Digit + 1;
        return letter >= minLetter && letter <= maxLetter && digit >= minDigit && digit <= maxDigit;
    }
    public override void Update()
    {
        FindAllThreats();
        base.Update();
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
        _moves = new();
        CheckRegularMoves();
        if(!_hasMoved)
            CheckCastlingMoves();
    }
    private void CheckRegularMoves()
    {
        foreach (var square in _adjacentSquares)
        {
            if (!Square.Validate(square))
                continue;

            Team teamOnTheSquare = Owner.GetTeamOnSquare(square);

            if (IsRaw)
            {
                if (CheckIfSquareIsThreatened(square))
                    continue;

                if(teamOnTheSquare == Team.Empty)
                    _moves.Add(new Move(Square, square, 'm'));
                else if(teamOnTheSquare == _team)
                    _moves.Add(new Move(Square, square, 'd'));
                else
                    _moves.Add(new Move(Square, square, 'x'));
            }
            else
            {
                if (teamOnTheSquare == _team || CheckIfSquareIsThreatened(square))
                    continue;

                if (teamOnTheSquare == Team.Empty)
                    _moves.Add(new Move(Square, square, 'm'));
                else
                    _moves.Add(new Move(Square, square, 'x'));
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
                Piece piece = Owner.GetPiece(s_castlingRookSquares[kingSideLetter]);
                if (piece != null)
                    CheckCastling(GetKingSideCastleSquares(), piece);
            }
            if ((CastlingRights & CastlingRights.QueenSide) == CastlingRights.QueenSide)
            {
                char queenSideLetter = _team == Team.White ? 'Q' : 'q';
                Piece piece = Owner.GetPiece(s_castlingRookSquares[queenSideLetter]);
                if (piece != null)
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

        _moves.Add(new Move(Square, squaresToCheck[1], GetCastlingSideFromRookSquare(r.Square, _team)));
    }
    public Square[] GetKingSideCastleSquares() => s_kingsideCastlingSquares[_team];
    public Square[] GetQueenSideCastleSquares() => s_queensideCastlingSquares[_team];
    public List<Move> FilterMovesThroughThreats(List<Move> movesToFilter)
    {
        if (movesToFilter.Count == 0)
            return movesToFilter;

        if (ArePiecesFacingEachOther(Square, movesToFilter[0].Former))
        {
            if (_threats.Count > 0)
                return FilterMovesByGeneratedAndSolvedThreats(movesToFilter);
            else
                return FilterMovesByGeneratedThreatsOnly(movesToFilter);
        }
        else if (_threats.Count > 0)
        {
            return FilterMovesBySolvedThreatsOnly(movesToFilter);
        }
        else
        {
            return movesToFilter;
        }
    }
    private List<Move> FilterMovesBySolvedThreatsOnly(List<Move> movesToFilter)
    {
        List<Move> filtered = new(movesToFilter.Count);
        foreach(Move move in movesToFilter)
        {
            if (WouldMoveSolveAllThreats(in move))
                filtered.Add(move);
        }
        return filtered;
    }
    private List<Move> FilterMovesByGeneratedThreatsOnly(List<Move> movesToFilter)
    {
        List<Move> filtered = new(movesToFilter.Count);
        foreach (Move move in movesToFilter)
        {
            if (!WouldMoveGenerateThreat(in move))
                filtered.Add(move);
        }
        return filtered;
    }
    private List<Move> FilterMovesByGeneratedAndSolvedThreats(List<Move> movesToFilter)
    {
        List<Move> filtered = new(movesToFilter.Count);
        foreach (Move move in movesToFilter)
        {
            if (!WouldMoveGenerateThreat(in move) && WouldMoveSolveAllThreats(in move))
                filtered.Add(move);
        }
        return filtered;
    }
    public bool WouldMoveGenerateThreat(in Move move)
    {
        int iteratorX = move.Former.Letter - Square.Letter;
        int iteratorY = move.Former.Digit - Square.Digit;

        if (iteratorX == 0 && move.Former.Letter != move.Latter.Letter)
        {
            return CheckDirectionForThreats(Movesets.Rook, (0, Math.Sign(iteratorY)), in move);
        }
        else if (iteratorY == 0 && move.Former.Digit != move.Latter.Digit)
        {
            return CheckDirectionForThreats(Movesets.Rook, (Math.Sign(iteratorX), 0), in move);
        }
        else if (Math.Abs(iteratorX) == Math.Abs(iteratorY))
        {
            if (!IsDiagonalMoveInDirectionOfPotentialThreat(in move, iteratorX, iteratorY))
                return CheckDirectionForThreats(Movesets.Bishop, (Math.Sign(iteratorX), Math.Sign(iteratorY)), in move);
        }
        return false;
    }
    public bool ArePiecesFacingEachOther(Square firstPieceSquare, Square secondPieceSquare)
    {
        (int, int)? iterator = GetStraightIterator(startingPosition: firstPieceSquare, destination: secondPieceSquare);

        if (iterator.HasValue)
        {
            Square currentSquare = firstPieceSquare;
            do
            {
                currentSquare.Transform(iterator.Value);
                if (!Square.Validate(currentSquare))
                    break;
                if (Owner.GetPiece(currentSquare) != null)
                    return currentSquare == secondPieceSquare;
            }
            while (true);
        }
        return false;
    }
    private static (int letter, int digit)? GetStraightIterator(Square startingPosition, Square destination)
    {
        char startLetter = startingPosition.Letter;
        char destLetter = destination.Letter;
        int startDigit = startingPosition.Digit;
        int destDigit = destination.Digit;
        // Squares are on the same horizontal line.
        if (startLetter == destLetter)
        {
            return (0, startDigit > destDigit ? -1 : 1);
        }
        // Squares are on the same vertical line.
        else if (startDigit == destDigit)
        {
            return (startLetter > destLetter ? -1 : 1, 0);
        }
        // Squares are on the same diagonal.
        else if (Math.Abs(startDigit - destDigit) == Math.Abs(startLetter - destLetter))
        {
            int directionLetter = startLetter > destLetter ? -1 : 1;
            int directionDigit = startDigit > destDigit ? -1 : 1;
            return (directionLetter, directionDigit);
        }
        else
        {
            return null;
        }
    }
    private static bool IsDiagonalMoveInDirectionOfPotentialThreat(in Move move, int iteratorX, int iteratorY)
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
    private bool CheckDirectionForThreats(Movesets set, (int, int) iterator, in Move move)
    {      
        List<Square> sequence = GenereateSquareSequence(set, move.Former, iterator, onlyCaptures: true);
        if (sequence != null && sequence[^1] != move.Latter)
            return true;
        else
            return false;
    }
    public bool WouldMoveSolveAllThreats(in Move move)
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
    public void FindAllThreats()
    {
        _threats.Clear();
        Threatened = false;
        foreach (var sequence in GenerateAllThreatSequences())
            _threats.Add(sequence);
        if (_threats.Count > 0)
        {
            Threatened = true;
            OnCheck(EventArgs.Empty);
        }
    }
    private List<List<Square>> GenerateAllThreatSequences()
    {
        int direction = Team == Team.White ? 1 : -1;
        List<List<Square>> sequences = new();
        List<Square> sequenceToAdd;

        sequenceToAdd = GenereateSquareSequence(Movesets.Diagonal, Square, (1, 1));
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Diagonal, Square, (1, -1));
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Diagonal, Square, (-1, 1));
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Diagonal, Square, (-1, -1));
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Vertical, Square, (0, 1));
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Vertical, Square, (0, -1));
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Horizontal, Square, (1, 0));
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Horizontal, Square, (-1, 0));
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Pawn, Square, (1, 1 * direction), isInfinite: false);
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Pawn, Square, (-1, 1 * direction), isInfinite: false);
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, Square, (2, 1), isInfinite: false);
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, Square, (2, -1), isInfinite: false);
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, Square, (-1, -2), isInfinite: false);
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, Square, (1, -2), isInfinite: false);
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, Square, (-2, -1), isInfinite: false);
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, Square, (-2, 1), isInfinite: false);
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, Square, (1, 2), isInfinite: false);
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, Square, (-1, 2), isInfinite: false);
        if (sequenceToAdd != null) sequences.Add(sequenceToAdd);

        return sequences;
    }
    private List<Square> GenereateSquareSequence(Movesets targetedMoveset, Square initialSquare, (int letterIt, int digitIt) iterator, bool isInfinite = true, bool onlyCaptures = false)
    { 
        Square current = new(initialSquare, iterator);
        if (!Square.Validate(current))
            return null;
        if (isInfinite)
        {
            bool foundThreat = false;
            List<Square> sequence = null;
            do
            {
                Piece piece = Owner.GetPiece(current);
                if (piece != null && piece != this)
                {
                    if (piece.Team != Team && (piece.Moveset & targetedMoveset) != 0)
                    {
                        sequence ??= new(1);
                        sequence.Add(current);
                        foundThreat = true;
                    }
                    return foundThreat ? sequence : null;
                }
                else if (!onlyCaptures)
                {
                    sequence ??= new(4);
                    sequence.Add(current);
                }
                current = new(current, iterator);
                if (!Square.Validate(current))
                    return foundThreat ? sequence : null;
            } while (true);        
        }
        else
        {
            Piece piece = Owner.GetPiece(current);
            if (piece != null)
            {
                if (piece.Team != Team && (piece.Moveset & targetedMoveset) != 0)
                {
                    List<Square> sequence = new(1)
                    {
                        current
                    };
                    return sequence;
                }
            }
            return null;
        }
    }
    public bool CheckIfSquareIsThreatened(Square checkedSquare)
    {
        int direction = Team == Team.White ? 1 : -1;
        List<Square> sequenceToAdd;

        sequenceToAdd = GenereateSquareSequence(Movesets.Diagonal, checkedSquare, (1, 1));
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Diagonal, checkedSquare, (1, -1));
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Diagonal, checkedSquare, (-1, 1));
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Diagonal, checkedSquare, (-1, -1));
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Vertical, checkedSquare, (0, 1));
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Vertical, checkedSquare, (0, -1));
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Horizontal, checkedSquare, (1, 0));
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Horizontal, checkedSquare, (-1, 0));
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Pawn, checkedSquare, (1, 1 * direction), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Pawn, checkedSquare, (-1, 1 * direction), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, checkedSquare, (2, 1), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, checkedSquare, (2, -1), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, checkedSquare, (-1, -2), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, checkedSquare, (1, -2), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, checkedSquare, (-2, -1), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, checkedSquare, (-2, 1), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, checkedSquare, (1, 2), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.Knight, checkedSquare, (-1, 2), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.King, checkedSquare, (0, 1), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.King, checkedSquare, (0, -1), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.King, checkedSquare, (1, 0), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.King, checkedSquare, (-1, 0), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.King, checkedSquare, (1, 1), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.King, checkedSquare, (1, -1), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.King, checkedSquare, (-1, 1), isInfinite: false);
        if (sequenceToAdd != null) return true;
        sequenceToAdd = GenereateSquareSequence(Movesets.King, checkedSquare, (-1, -1), isInfinite: false);
        if (sequenceToAdd != null) return true;

        return false;
    }
    public override void MovePiece(in Move move)
    {
        base.MovePiece(in move);
        CastlingRights = CastlingRights.None;
        MoveAdjacentSquares(move.Latter - move.Former);
        _hasMoved = true;
    }
    private void MoveAdjacentSquares((int,int) vector)
    {
        for(int i = 0; i < _adjacentSquares.Length; ++i)
        {
            _adjacentSquares[i].Transform(vector);
        }
    }
    public void OnCheck(EventArgs e)
    {
        if (IsRaw)
            return;

        Check?.Invoke(this, e);
    }
}
