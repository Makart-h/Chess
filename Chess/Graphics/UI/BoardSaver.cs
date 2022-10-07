using Chess.Board;
using Chess.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Chess.Graphics.UI;

internal sealed class BoardSaver
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly InputManager _inputManager;
    private readonly Dictionary<bool, List<RenderTarget2D>> _framesAfterEachMove;
    private int _framesOffset;
    private bool _saveFrame;
    private bool _firstFrame;
    private bool _isWatchingPreviousFrames;
    public bool UpdateRequired { get => _saveFrame || _firstFrame; }
    public bool ShouldBeDrawn { get => _isWatchingPreviousFrames; }
    public RenderTarget2D CurrentFrame { get => _framesAfterEachMove[Chessboard.Instance.Inverted][_framesOffset]; }
    public BoardSaver(GraphicsDevice graphicsDevice, InputManager inputManager)
    {
        _graphicsDevice = graphicsDevice;
        _inputManager = inputManager;
        _framesAfterEachMove = new()
        {
            [false] = new(),
            [true] = new()
        };
        _firstFrame = true;
        SubscribeToEvents();
    }
    private void SubscribeToEvents()
    {
        Chess.TurnEnded += OnTurnEnded;
        _inputManager.SubscribeToEvent(Keys.Left, OnLeftPressed);
        _inputManager.SubscribeToEvent(Keys.Right, OnRightPressed);
        _inputManager.SubscribeToEvent(Keys.Space, OnSpacePressed);
    }
    public void SaveBoardInBothInversions(UIModule module)
    {
        SaveFrame(Chessboard.Instance.Inverted, module.RenderTarget);
        Chessboard.Instance.ToggleInversion();
        module.PrepareRenderTarget();
        SaveFrame(Chessboard.Instance.Inverted, module.RenderTarget);
        Chessboard.Instance.ToggleInversion();
        module.PrepareRenderTarget();
        if (!_isWatchingPreviousFrames)
            _framesOffset++;
        _saveFrame = false;
        _firstFrame = false;
    }
    private void SaveFrame(bool inversion, RenderTarget2D frameToSave)
    {
        SpriteBatch spriteBatch = new SpriteBatch(_graphicsDevice);
        RenderTarget2D frame = new RenderTarget2D(_graphicsDevice, frameToSave.Width, frameToSave.Height);
        _graphicsDevice.SetRenderTarget(frame);
        spriteBatch.Begin();
        spriteBatch.Draw(frameToSave, Vector2.Zero, Color.White);
        spriteBatch.End();
        _graphicsDevice.SetRenderTarget(null);
        _framesAfterEachMove[inversion].Add(frame);
    } 
    private void OnTurnEnded(object sender, EventArgs e) => _saveFrame = true;
    private void OnLeftPressed()
    {
        if (_framesOffset > 0)
        {
            if (_isWatchingPreviousFrames)
                _framesOffset--;
            // We need at least one frame beyond the starting position.
            else if (_framesAfterEachMove[Chessboard.Instance.Inverted].Count > 1)
            {
                _framesOffset = _framesAfterEachMove[Chessboard.Instance.Inverted].Count - 2;
                _isWatchingPreviousFrames = true;
            }
        }
    }
    private void OnRightPressed()
    {
        if (_framesOffset < _framesAfterEachMove[Chessboard.Instance.Inverted].Count - 1)
            _framesOffset++;

        if (_framesOffset == _framesAfterEachMove[Chessboard.Instance.Inverted].Count - 1)
            _isWatchingPreviousFrames = false;
    }
    private void OnSpacePressed()
    {
        _framesOffset = _framesAfterEachMove[false].Count - 1;
        _isWatchingPreviousFrames = false;
    }
}
