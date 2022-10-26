using Chess.AI;
using Chess.Input;
using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chess.Graphics.UI;

internal class MoveHistory : ITextProvider
{
    private readonly InputManager _inputManager;
    private readonly List<TextObject> _history;
    private readonly SpriteFont _font;
    private int _fontHeight;
    private readonly int _firstNumber;
    private int _currentNumber;
    private bool _isFirstEntryWritten;
    private readonly int _maxItemsOnScreen;
    private double? _itemToHighlight;
    private static readonly int s_maxHalfmoveEntryLength = 8;
    private const int s_maxNumberOfDigits = 3;
    private const float _propotionalDistanceFromEdges = 0.05f;
    private int _watchingOffset;
    private bool _isInOffsetedState;
    private (Team team, string description) _moveToAdd;
    private string _promotionInfo;
    private bool _shouldInsertCheckInfo;
    private bool _checkmateOnBoard;
    public int Height { get; private set; }
    public int Width { get; private set; }
    
    public MoveHistory(int moveNo, SpriteFont font, int maxItemsOnScreen, InputManager inputManager)
    {
        _inputManager = inputManager;
        _history = new List<TextObject>();
        _font = font;
        _firstNumber = moveNo;
        _currentNumber = moveNo;
        _maxItemsOnScreen = maxItemsOnScreen;
        _watchingOffset = _history.Count - _maxItemsOnScreen;
        _promotionInfo = string.Empty;
        SetSize();
        SubscribeToEvents();
    }
    private void SetSize()
    {
        int maxStringLength = s_maxHalfmoveEntryLength * 2 + s_maxNumberOfDigits + 1;
        StringBuilder sb = new(maxStringLength);
        for (int i = 0; i < maxStringLength; ++i)
            sb.Append('x');
        Vector2 measurments = _font.MeasureString(sb.ToString());

        _fontHeight = (int)measurments.Y;
        Width = (int)(measurments.X * (1 + _propotionalDistanceFromEdges * 2));
        Height = (int)(measurments.Y * _maxItemsOnScreen * (1 + _propotionalDistanceFromEdges * 2));
    }
    private void SubscribeToEvents()
    {
        Piece.PieceMoved += OnPieceMoved;
        King.Check += OnCheck;
        Chess.TurnEnded += OnTurnEnded;
        Chess.PromotionConcluded += OnPromotionConcluded;
        Arbiter.GameConcluded += OnGameConcluded;
        _inputManager.SubscribeToEvent(Keys.Left, OnLeftPressed);
        _inputManager.SubscribeToEvent(Keys.Right, OnRightPressed);
        _inputManager.SubscribeToEvent(Keys.Space, OnSpacePressed);
        _inputManager.SubscribeToEvent(Keys.Up, OnUpPressed);
        _inputManager.SubscribeToEvent(Keys.Down, OnDownPressed);
    }
    private void OnUpPressed()
    {
        if (!_isInOffsetedState)
        {
            int listOffset = _history.Count - _maxItemsOnScreen;
            if (listOffset > 0)
            {
                _isInOffsetedState = true;
                _watchingOffset = listOffset - 1;
            }
        }
        else if (_watchingOffset > 0)
        {              
                _watchingOffset--;
        }
    }
    private void OnDownPressed()
    {
        if (_isInOffsetedState)
        {
            _watchingOffset++;
            int listOffset = _history.Count - _maxItemsOnScreen;
            if (_watchingOffset == listOffset)
                _isInOffsetedState = false;
        }
    }
    private void OnLeftPressed()
    {
        if (_history.Count > 0)
        {
            if (_itemToHighlight.HasValue is false)
            {
                if (_history[^1].Text.Length > s_maxHalfmoveEntryLength * 2)
                    _itemToHighlight = _history.Count - 1;
                else
                    _itemToHighlight = _history.Count - 1.5;

                int index = (int)Math.Floor(_itemToHighlight.Value);
                if (index >= 0)
                    _history[index].Color = Color.Green;
            }
            else if (_itemToHighlight >= 0)
            {
                int oldIndex = (int)Math.Floor(_itemToHighlight.Value);
                _itemToHighlight -= 0.5;
                int newIndex = (int)Math.Floor(_itemToHighlight.Value);
                if (oldIndex != newIndex)
                {
                    if (oldIndex >= 0)
                        _history[oldIndex].Color = Color.Black;
                    if (newIndex >= 0)
                        _history[newIndex].Color = Color.Green;
                }
            }
        }
    }
    private void OnRightPressed()
    {
        if(_itemToHighlight.HasValue)
        {
            int oldIndex = (int)Math.Floor(_itemToHighlight.Value);
            _itemToHighlight += 0.5;
            int newIndex = (int)Math.Floor(_itemToHighlight.Value);
            if (oldIndex != newIndex)
            {
                if (oldIndex >= 0)
                    _history[oldIndex].Color = Color.Black;
                if (newIndex >= 0 && newIndex < _history.Count)
                    _history[newIndex].Color = Color.Green;
            }
            if (_history[^1].Text.Length < s_maxHalfmoveEntryLength * 2 && _itemToHighlight == _history.Count - 1 || _itemToHighlight == _history.Count - 0.5)
            {
                _itemToHighlight = null;
                _history[newIndex].Color = Color.Black;
            }
        }
    }
    private void OnSpacePressed()
    {
        if (_itemToHighlight.HasValue)
        {
            int index = (int)Math.Floor(_itemToHighlight.Value);
            if(index >= 0)
                _history[index].Color = Color.Black;
        }
        _itemToHighlight = null;
    }
    private void OnGameConcluded(object sender, GameResultEventArgs e)
    {
        if (e.GameResult is GameResult.White or GameResult.Black)
            _checkmateOnBoard = true;
    }
    private void OnCheck(object sender, EventArgs e) => _shouldInsertCheckInfo = true;
    private void InsertCheckInfo()
    {
        TextObject lastEntry = _history.Last();
        StringBuilder sb = new(lastEntry.Text);
        int index = lastEntry.Text.IndexOf(' ', lastEntry.Text.Length - s_maxHalfmoveEntryLength);
        sb.Replace(' ', '+', index, 1);
        lastEntry.Text = sb.ToString();
    }
    private void InsertPromotionInfo()
    {
        TextObject lastEntry = _history.Last();
        StringBuilder sb = new(lastEntry.Text);
        int index = lastEntry.Text.IndexOf(' ', lastEntry.Text.Length - s_maxHalfmoveEntryLength);
        for (int j = 0, i = index; j < _promotionInfo.Length; ++i, ++j)
        {
            sb[i] = _promotionInfo[j];
        }
        lastEntry.Text = sb.ToString();
    }
    private void OnPromotionConcluded(object sender, PromotionEventArgs e)
    {
        _promotionInfo = $"={char.ToUpper(PieceFactory.GetPieceCharacter(e.TypeToBePromotedInto, e.PromotedPawn.Team))}";
    }
    private void OnPieceMoved(object sender, PieceMovedEventArgs e)
    {
        // If the moved piece was a rook participating in castling we don't want to add it to move history.
        if (e.Move.Description == MoveType.ParticipatesInCastling)
            return;

        string moveDescription = CheckForCastling(e.Move.Description);
        moveDescription ??= BuildMoveDescription(e);
        _moveToAdd = (e.Piece.Team, moveDescription);
    }
    private void OnTurnEnded(object sender, EventArgs e) => AddMoveToHistory(_moveToAdd.team, _moveToAdd.description);
    private void AddEntry(string moveDescription)
    {;
        Vector2 position = CalculateEntryPosition(_currentNumber - _firstNumber);
        TextObject newEntry = new TextObject(_font, $"{_currentNumber, s_maxNumberOfDigits}.{moveDescription}", position, Color.Black, 1.0f);
        _history.Add(newEntry);
    }
    private void UpdateEntry(string moveDescription)
    {
        TextObject lastEntry = _history.Last();
        lastEntry.Text += moveDescription;
    }
    private static string CheckForCastling(MoveType moveDescription)
    {
        return moveDescription switch
        {
            MoveType.CastlesKingside => "O-O ",
            MoveType.CastlesQueenside => "O-O-O ",
            _ => null
        };
    }
    private void AddMoveToHistory(Team team, string moveDescription)
    {
        moveDescription = FillToMaxHalfmoveLength(moveDescription);
        if (!_isFirstEntryWritten)
        {
            AddFirstMove(team, moveDescription);
            _isFirstEntryWritten = true;
        }
        else if (team == Team.White)
        {
            AddEntry(moveDescription);
        }
        else
        {
            UpdateEntry(moveDescription);
            _currentNumber++;
        }

        if (_promotionInfo != string.Empty)
        {
            InsertPromotionInfo();
            _promotionInfo = string.Empty;
        }
        if (_shouldInsertCheckInfo)
        {
            InsertCheckInfo();
            _shouldInsertCheckInfo = false;
        }
        if(_checkmateOnBoard)
            _history[^1].Text = _history[^1].Text.Replace('+', '#');
    }
    private void AddFirstMove(Team team, string moveDescription)
    {
        string whitesPart = team == Team.White ? string.Empty : FillToMaxHalfmoveLength(string.Empty);
        moveDescription = moveDescription.Insert(0, whitesPart);
        AddEntry(moveDescription);
        if (team == Team.Black)
            _currentNumber++;
    }
    private static string FillToMaxHalfmoveLength(string moveDescription)
    {
        StringBuilder sb = new(moveDescription);
        while (sb.Length < s_maxHalfmoveEntryLength)
            sb.Append(' ');
        return sb.ToString();
    }
    private static string BuildMoveDescription(PieceMovedEventArgs e)
    {
        StringBuilder description = new();
        char pieceType = PieceFactory.GetPieceCharacter(e.Piece);

        if (e.Piece is not Pawn)
            description.Append(char.ToUpper(pieceType));

        string prefix = GetPrefix(e, pieceType);
        description.Append(prefix);

        if (e.Move.Description == MoveType.Takes || e.Move.Description == MoveType.EnPassant)
        {
            if (prefix == string.Empty && e.Piece is Pawn)
                description.Append(e.Move.Former.Letter);
            description.Append('x');
        }

        description.Append($"{e.Move.Latter}");
        return description.ToString();
    }
    private static string GetPrefix(PieceMovedEventArgs e, char pieceType)
    {
        string prefix = string.Empty;
        if (e.Piece.Owner is Controller c)
        {
            Piece[] piecesOfTheSameType = (from piece in c.Pieces where piece != e.Piece && PieceFactory.GetPieceCharacter(piece) == pieceType select piece).ToArray();
            var conflictInfo = CheckForConflicts(e.Move, piecesOfTheSameType);
            if (conflictInfo.inConflict)
                return ResolveConflicts(conflictInfo.letterConflict, conflictInfo.digitConflict, e.Move);
        }
        return prefix;
    }
    private static string ResolveConflicts(bool letterConflict, bool digitConflict, Move move)
    {
        string prefix = string.Empty;

        if (!letterConflict)
            prefix += move.Former.Letter.ToString();
        else if (!digitConflict)
            prefix += move.Former.Digit.ToString();
        else
            prefix = move.Former.ToString();

        return prefix;
    }
    private static (bool inConflict, bool letterConflict, bool digitConflict) CheckForConflicts(Move move, Piece[] piecesOfTheSameType)
    {
        (bool inConflict, bool letterConflict, bool digitConflict) conflictInfo = (false, false, false);
        foreach (var otherPiece in piecesOfTheSameType)
        {
            if ((from m in otherPiece.Moves where m.Latter == move.Latter select m).Any())
            {
                conflictInfo.inConflict = true;
                if (otherPiece.Square.Letter == move.Former.Letter)
                    conflictInfo.letterConflict = true;
                else if (otherPiece.Square.Digit == move.Former.Digit)
                    conflictInfo.digitConflict = true;
            }
        }
        return conflictInfo;
    }
    public IEnumerable<TextObject> GetTextObjects()
    {
        int listOffset = _isInOffsetedState ? _watchingOffset : _history.Count - _maxItemsOnScreen;
        if (listOffset > 0 || _isInOffsetedState)
        {
            UpdateEntriesPosition(listOffset);
            return _history.ToArray()[listOffset..];
        }
        else
            return _history.ToArray();
    }
    private void UpdateEntriesPosition(int offset)
    {
        for(int i = 0; i < _history.Count; ++i)
        {
            _history[i].Position = CalculateEntryPosition(i - offset);
        }
        }
    private Vector2 CalculateEntryPosition(int rowNumber)
    {
        float positionX = Width * _propotionalDistanceFromEdges;
        float positionY = Height * _propotionalDistanceFromEdges + rowNumber * _fontHeight;
        return new Vector2(positionX, positionY);
    }
}