using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Chess.AI;
using Chess.Board;
using Chess.Clock;
using Chess.Data;
using Chess.Graphics;
using Chess.Movement;
using Chess.Pieces;

namespace Chess
{
    internal sealed class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public static Game1 Instance;
        private Chessboard _chessboard;
        public Texture2D Overlays { get; private set; }
        private readonly List<Controller> _controllers;
        private FENObject _fenObject;
        private SpriteFont _clockTimeFont;
        private bool _isRunning;
        private bool _updateController;
        private string _endgameMessage;
        private bool _tabPressed = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Instance = this;
            _controllers = new List<Controller>();
            _isRunning = true;
            _updateController = true;
        }

        private void SubscribeToEvents()
        {
            ChessClock.TimerExpired += OnChessClockExpired;
            Arbiter.GameConcluded += OnGameConcluded;
            Controller.MoveMade += OnMoveChosen;
        }
        private void OnMoveChosen(object sender, MoveMadeEventArgs args) => _updateController = true;
        private void OnChessClockExpired(object sender, TimerExpiredEventArgs args)
        {
            _isRunning = false;
            _endgameMessage = $"{args.VictoriousController.Team} is victorious!";
        }
        private void OnGameConcluded(object sender, GameResultEventArgs args)
        {
            _isRunning = false;
            if(args.GameResult != GameResult.White && args.GameResult != GameResult.Black)
                _endgameMessage = _endgameMessage = "Game ended in a draw!";
            else
            {
                Team victoriousTeam = args.GameResult == GameResult.White ? Team.White : Team.Black;
                _endgameMessage = $"{victoriousTeam} is victorious!";
            }
        }
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            _graphics.PreferredBackBufferWidth = 576;  // set this value to the desired width of your window
            _graphics.PreferredBackBufferHeight = 576;   // set this value to the desired height of your window
            _graphics.ApplyChanges();
            PieceFactory.Initilize(this.Content.Load<Texture2D>("Tpieces"));

            //"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            //"6k1/6p1/5p1p/1p2p3/p2p2q1/Pr1P2Nn/Q7/6RK b - - 1 38"
            //"r1bk1q2/pp4pB/4pn2/3p4/3P3Q/2P3P1/PP1N2PP/R5K1 b - - 6 18"
            //5k2/1q6/8/8/4n3/4K3/8/8 w - - 0 1
            //2r2k2/8/8/8/Q7/6b1/6PP/7K w - - 0 1  mate in one test
            //5r2/p2b2k1/4p3/2p1R3/5B1b/2P5/PP1N3P/R5K1 w - - 2 23 real game
            //6k1/3b3r/1p1p4/p1n2p2/1PPNpP1q/P3Q1p1/1R1RB1P1/5K2 b - - 0 1 mate in 5
            //4r1k1/3n1ppp/4r3/3n3q/Q2P4/5P2/PP2BP1P/R1B1R1K1 b - - 0 1 mate in 3
            //rn3rk1/pbppq1pp/1p2pb2/4N2Q/3PN3/3B4/PPP2PPP/R3K2R w KQ - 7 11 mate in 7
            //3k4/6R1/4K3/8/8/8/8/8 b - - 9 5 mate in 4 king+rook endgame
            //8/8/4k3/8/3K4/5R2/8/8 w - - 0 1 mate in 12 king+rook endgame
            //5r1k/5p2/1p4rN/2nQ4/P6R/6P1/5P2/5K2 w - - 0 1
            //5r1k/5p2/6rN/3Q4/7R/6P1/8/5K2 w - - 0 1
            //2k2r2/3R4/4Qp2/8/8/6P1/8/5K2 w - - 0 1
            //1k6/8/8/4bn2/4K3/8/8/8 w - - 0 1 material draw knight/bishop
            //1k6/8/8/4b3/4K3/8/8/8 w - - 0 1 material draw king vs king
            //1k6/8/5b2/8/4K3/8/4R3/8 w - - 0 1 3fold, 5fold
            //1k6/8/5b2/8/4K3/8/5B2/8 w - - 0 1 both bishops same colors
            //k7/2Q5/8/8/4K3/8/8/8 w - - 0 1 stalemate
            try
            {
                string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
                _fenObject = FENParser.Parse(fen.Trim());
                Arbiter.Initilize(fen, _fenObject.HalfMoves);            
            }
            catch (ArgumentException)
            {
                throw;
            }
            //Controller white = new HumanController(Team.White, fenObject.WhitePieces, fenObject.WhiteCastling, fenObject.EnPassantSquare);
            Controller white = new AIController(Team.White, _fenObject.WhitePieces, _fenObject.WhiteCastling, _fenObject.EnPassantSquare);
            Controller black = new AIController(Team.Black, _fenObject.BlackPieces, _fenObject.BlackCastling, _fenObject.EnPassantSquare);
            //Controller black = new HumanController(Team.Black, fenObject.BlackPieces, fenObject.BlackCastling, fenObject.EnPassantSquare);
            _controllers.Add(white);
            _controllers.Add(black);
            ChessClock.Initialize(white, black, new TimeSpan(0, 0, 10), new TimeSpan(0,0,0), _fenObject.TeamToMove);
            _chessboard.InitilizeBoard(_fenObject.AllPieces);
            ChessClock.ActiveController.Update();
            SubscribeToEvents();
            ChessClock.Start();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _chessboard = new Chessboard(this.Content.Load<Texture2D>("chessboard"));
            Overlays = this.Content.Load<Texture2D>("Tsquares");
            _clockTimeFont = this.Content.Load<SpriteFont>("ChessClock");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Tab) && !_tabPressed)
            {
                _tabPressed = true;
                Chessboard.Instance.ToggleInversion();
            }
            else if(Keyboard.GetState().IsKeyUp(Keys.Tab))
            {
                _tabPressed = false;
            }
            MovementManager.Update(gameTime);

            if (_isRunning && (_updateController || ChessClock.ActiveController is HumanController))
            {
                _updateController = false;
                ChessClock.ActiveController.MakeMove();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            _spriteBatch.Begin();
            _spriteBatch.Draw(_chessboard.Model.RawTexture, _chessboard.Position, _chessboard.Model.TextureRect, Color.White);
            List<DrawableObject> overlays = OverlayManager.GetOverlays();
            foreach (var obj in overlays)
            {
                _spriteBatch.Draw(obj.Model.RawTexture, obj.Position, obj.Model.TextureRect, obj.Color);
            }
            foreach (var controller in _controllers)
            {
                foreach (var obj in controller.Pieces)
                {
                    _spriteBatch.Draw(obj.Model.RawTexture, obj.Position, obj.Model.TextureRect, obj.Color);
                    if (controller is HumanController hc)
                    {
                        if (hc.DragedPiece.PieceTexture != null)
                        {
                            _spriteBatch.Draw(hc.DragedPiece.PieceTexture.RawTexture, hc.DragedPiece.Position, hc.DragedPiece.PieceTexture.TextureRect, Color.White);
                        }
                    }
                }
            }
            (TimeSpan white, TimeSpan black) = ChessClock.GetTimers();
            Vector2 topClock = new Vector2(15, 216);
            Vector2 bottomClock = new Vector2(15, 288);
            Vector2 whiteClock, blackClock;
            if (Chessboard.Instance.Inverted)
            {
                whiteClock = topClock;
                blackClock = bottomClock;
            }
            else
            {
                whiteClock = bottomClock;
                blackClock = topClock;
            }
            _spriteBatch.DrawString(_clockTimeFont, $"{black.Minutes:d2}:{black.Seconds:d2}:{black.Milliseconds:d2}", blackClock, Color.Black);
            _spriteBatch.DrawString(_clockTimeFont, $"{white.Minutes:d2}:{white.Seconds:d2}:{white.Milliseconds:d2}", whiteClock, Color.Black);

            if(!_isRunning)
            {
                _spriteBatch.DrawString(_clockTimeFont, $"{_endgameMessage}", new Vector2(100, 400), Color.Red);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    } 
}
