using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Chess.Board;
using Chess.Graphics;
using Chess.Pieces;
using System;
using Chess.AI;
using Chess.Data;
using Chess.Clock;
using System.Runtime;
using Chess.Movement;

namespace Chess
{
    internal class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public SpriteBatch SpriteBatch { get => _spriteBatch; }
        public static Game1 Instance;

        private Chessboard chessboard;
        public Texture2D Overlays { get; private set; }
        private readonly List<Controller> controllers;
        private FENObject fenObject;
        private SpriteFont clockTimeFont;
        private bool isRunning;
        private bool updateController;
        private string endgameMessage;
        bool tabPressed = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Instance = this;
            controllers = new List<Controller>();
            isRunning = true;
            updateController = true;
        }

        private void SubscribeToEvents()
        {
            ChessClock.TimerExpired += OnChessClockExpired;
            Arbiter.GameConcluded += OnGameConcluded;
            Controller.MoveMade += OnMoveChosen;
        }
        private void OnMoveChosen(object sender, MoveMadeEventArgs args) => updateController = true;
        private void OnChessClockExpired(object sender, TimerExpiredEventArgs args)
        {
            isRunning = false;
            endgameMessage = $"{args.VictoriousController.Team} is victorious!";
        }
        private void OnGameConcluded(object sender, GameResultEventArgs args)
        {
            isRunning = false;
            if(args.GameResult != GameResult.White && args.GameResult != GameResult.Black)
                endgameMessage = endgameMessage = "Game ended in a draw!";
            else
            {
                Team victoriousTeam = args.GameResult == GameResult.White ? Team.White : Team.Black;
                endgameMessage = $"{victoriousTeam} is victorious!";
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
                fenObject = FENParser.Parse(fen.Trim());
                Arbiter.Initilize(fen, fenObject.HalfMoves);            
            }
            catch (ArgumentException)
            {
                throw;
            }
            //Controller white = new HumanController(Team.White, fenObject.WhitePieces, fenObject.WhiteCastling, fenObject.EnPassantSquare);
            Controller white = new AIController(Team.White, fenObject.WhitePieces, fenObject.WhiteCastling, fenObject.EnPassantSquare);
            Controller black = new AIController(Team.Black, fenObject.BlackPieces, fenObject.BlackCastling, fenObject.EnPassantSquare);
            //Controller black = new HumanController(Team.Black, fenObject.BlackPieces, fenObject.BlackCastling, fenObject.EnPassantSquare);
            controllers.Add(white);
            controllers.Add(black);
            ChessClock.Initialize(white, black, new TimeSpan(0, 0, 10), new TimeSpan(0,0,0), fenObject.TeamToMove);
            chessboard.InitilizeBoard(fenObject.AllPieces);
            ChessClock.ActiveController.Update();
            SubscribeToEvents();
            ChessClock.Start();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            chessboard = new Chessboard(this.Content.Load<Texture2D>("chessboard"));
            Overlays = this.Content.Load<Texture2D>("Tsquares");
            clockTimeFont = this.Content.Load<SpriteFont>("ChessClock");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Tab) && !tabPressed)
            {
                tabPressed = true;
                Chessboard.Instance.ToggleInversion();
            }
            else if(Keyboard.GetState().IsKeyUp(Keys.Tab))
            {
                tabPressed = false;
            }
            MovementManager.Update(gameTime);

            if (isRunning && (updateController || ChessClock.ActiveController is HumanController))
            {
                updateController = false;
                ChessClock.ActiveController.MakeMove();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            _spriteBatch.Begin();
            _spriteBatch.Draw(chessboard.Model.RawTexture, chessboard.Position, chessboard.Model.TextureRect, Color.White);
            List<DrawableObject> overlays = OverlayManager.GetOverlays();
            foreach (var obj in overlays)
            {
                _spriteBatch.Draw(obj.Model.RawTexture, obj.Position, obj.Model.TextureRect, obj.Color);
            }
            foreach (var controller in controllers)
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
            _spriteBatch.DrawString(clockTimeFont, $"{black.Minutes:d2}:{black.Seconds:d2}:{black.Milliseconds:d2}", blackClock, Color.Black);
            _spriteBatch.DrawString(clockTimeFont, $"{white.Minutes:d2}:{white.Seconds:d2}:{white.Milliseconds:d2}", whiteClock, Color.Black);

            if(!isRunning)
            {
                _spriteBatch.DrawString(clockTimeFont, $"{endgameMessage}", new Vector2(100, 400), Color.Red);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    } 
}
