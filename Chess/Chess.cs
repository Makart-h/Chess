using Chess.AI;
using Chess.Board;
using Chess.Clock;
using Chess.Data;
using Chess.Graphics;
using Chess.Graphics.UI;
using Chess.Input;
using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using IDrawable = Chess.Graphics.IDrawable;

namespace Chess;

internal sealed class Chess : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public static Chess Instance;
    private Chessboard _chessboard;
    private readonly List<Controller> _controllers;
    private SpriteFont _clockTimeFont;
    private SpriteFont _messagesFont;
    private bool _isRunning;
    private bool _updateController;
    private string _endgameMessage;
    private readonly List<UIModule> _uiModules;  
    private UIModule _chessboardUIModule;
    private readonly InputManager _chessboardInputManager;
    private readonly InputManager _menuInputManager;
    private BoardSaver _boardSaver;
    private Texture2D _lightTile;
    private Texture2D _darkTile;
    private bool _buttonInputOnly;
    public static EventHandler TurnEnded;
    public static EventHandler<PromotionEventArgs> PromotionConcluded;
    private int _unresolvedUserInputs;
    private readonly List<Button> _promotionButtons;
    private ChessGameSettings _chessGameSettings;
    private UIModule _menuUI;
    private bool _inMenu;
    private DrawableObject _piecesExplanation;
    private DrawableObject _controlsExplanation;
    private bool _explanationVisible;
    private bool _controlsVisible;

    public Chess() : base()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Instance = this;
        _controllers = new();
        _isRunning = true;
        _updateController = true;
        _uiModules = new();       
        _promotionButtons = new();
        _chessGameSettings = new ChessGameSettings();
        _chessboardInputManager = new();
        _menuInputManager = new();
        _inMenu = true;
    }

    private void SubscribeToEvents()
    {
        ChessClock.TimerExpired += OnChessClockExpired;
        Arbiter.GameConcluded += OnGameConcluded;
        MovementManager.MovementConcluded += OnMovementConcluded;
        Pawn.Promotion += OnPromotion;
        _chessboardInputManager.SubscribeToEvent(Keys.Tab, OnTabPressed);
        _chessboardInputManager.SubscribeToEvent(Keys.Escape, OnEscapePressed);
        _chessboardInputManager.SubscribeToEvent(Keys.F11, OnF11Pressed);
    }
    private void OnTabPressed() => Chessboard.Instance.ToggleInversion();
    private void OnEscapePressed() => Exit();
    private void OnF11Pressed()
    {
        _graphics.IsFullScreen = !_graphics.IsFullScreen;
        _graphics.ApplyChanges();
    }
    private void TryEndTurn()
    {
        if (_unresolvedUserInputs == 0)
        {
            _updateController = true;           
            TurnEnded?.Invoke(this, EventArgs.Empty);
        }
    }
    private void OnMovementConcluded(object sender, EventArgs e) => TryEndTurn();
    private void OnChessClockExpired(object sender, TimerExpiredEventArgs e)
    {
        _isRunning = false;
        _endgameMessage = $"{e.VictoriousController.Team} won on time!";
        AddEndgameMessage();
    }
    private void OnGameConcluded(object sender, GameResultEventArgs e)
    {
        _isRunning = false;
        string gameResultExplanation = Arbiter.ExplainGameResult(e.GameResult);
        if (e.GameResult != GameResult.White && e.GameResult != GameResult.Black)
            _endgameMessage = $"Game ended in a draw by \n{gameResultExplanation}.";
        else
        {
            Team victoriousTeam = e.GameResult == GameResult.White ? Team.White : Team.Black;
            _endgameMessage = $"{victoriousTeam} won by \n{gameResultExplanation}.";
        }
        AddEndgameMessage();
    }
    private void AddEndgameMessage()
    {
        float messageWidth = _messagesFont.MeasureString($" {_endgameMessage} ").X;       
        UIModule controlsUI = _uiModules[^1];
        float scale = controlsUI.RenderTarget.Width / messageWidth;
        TextObject text = new(_messagesFont, _endgameMessage, new Vector2(controlsUI.RenderTarget.Width * 0.05f, controlsUI.RenderTarget.Height * 0.05f), Color.Black, scale);
        controlsUI.SubmitElementToAdd(text);
    }
    private void HandlePromotionDecision(Pawn promotedPawn, PieceType type, bool fromUserInput = true)
    {
        PromotionConcluded?.Invoke(this, new PromotionEventArgs(promotedPawn, type));
        if (fromUserInput)
        {
            _unresolvedUserInputs--;
            foreach (Button button in _promotionButtons)
            {
                _chessboardUIModule.SubmitElementToRemove(button);
            }
            TryEndTurn();
            _buttonInputOnly = false;
        }
    }
    private void OnPromotion(object sender, EventArgs e)
    {
        Pawn pawn = (Pawn)sender;
        if (pawn.Owner is AIController)
            HandlePromotionDecision(pawn, PieceType.Queen, false);
        else
        {
            _buttonInputOnly = true;
            _unresolvedUserInputs++;
            AddPromotionButtons(pawn);
        }
    }
    private void AddPromotionButtons(Pawn pawn)
    {
        PieceType[] types = { PieceType.Queen, PieceType.Knight, PieceType.Rook, PieceType.Bishop };
        int direction = pawn.Team == Team.White ? -1 : 1;
        for(int i = 0; i < types.Length; ++i)
        {
            Square square = new(pawn.Square, (0, direction * i));
            var button = CreatePromotionButton(square, pawn, types[i]);
            _promotionButtons.Add(button);
            _chessboardUIModule.SubmitElementToAdd(button);
        }
    }
    private Button CreatePromotionButton(Square square, Pawn pawn, PieceType type)
    {
        if (pawn.Owner is Controller controller)
        {
            Square overlaySquare = Chessboard.Instance.Inverted ? new Square("h1") : new Square("a8");
            IDrawable overlay = new SquareOverlay(Content.Load<Texture2D>("Tsquares"), SquareOverlayType.CanTake, overlaySquare);
            Vector2 position = Chessboard.Instance.ToCordsFromSquare(square);
            Rectangle buttonRectangle = new((int)position.X, (int)position.Y, Chessboard.Instance.SquareSideLength, Chessboard.Instance.SquareSideLength);
            Rectangle drawableRectangle = controller.GetPieceModel(pawn).DestinationRect with { X = 0, Y = 0 };
            IDrawable appearance = CreatePieceAppearance(type, pawn.Team, drawableRectangle);
            Texture2D tile = Square.IsLightSquare(square) ? _lightTile : _darkTile;
            ButtonActionInfo actions = new();
            actions.OnRelease = () => HandlePromotionDecision(pawn, type);
            return new Button(GraphicsDevice, tile, new[] { appearance, overlay }, null, buttonRectangle, actions);
        }
        throw new ArgumentNullException();
    }
    private DrawableObject CreatePiecesExplanation()
    {
        Texture2D texture = Content.Load<Texture2D>("piecesExplanation");      
        Rectangle textureRect = new(0, 0, texture.Width, texture.Height);
        int width = (int)(_graphics.PreferredBackBufferWidth * 0.25);
        int height = _graphics.PreferredBackBufferHeight;
        DrawableObject piecesExplanation = new(texture, textureRect, new Rectangle(0,0, width, height));
        return piecesExplanation;
    }
    private DrawableObject CreateControlsExplanation()
    {
        Texture2D texture = Content.Load<Texture2D>("controlsExplanation");
        Rectangle textureRect = new(0, 0, texture.Width, texture.Height);
        int width = (int)(_graphics.PreferredBackBufferWidth * 0.25);
        int height = _graphics.PreferredBackBufferHeight;
        DrawableObject controlsExplanation = new(texture, textureRect, new Rectangle(0, 0, width, height));
        return controlsExplanation;
    }
    private Button[] CreateControlsButtons(UIModule controlsUI)
    {
        List<Button> buttons = new();

        Texture2D help = Content.Load<Texture2D>("help");
        int width = (int)(controlsUI.RenderTarget.Width * 0.25f);
        int posX = (int)(controlsUI.RenderTarget.Width * 0.1f);
        int posY = controlsUI.RenderTarget.Height - width;
        Rectangle dest = new(posX, posY, width, width);
        ButtonActionInfo actions = new();
        actions.OnHoverStarted = () => _explanationVisible = true;
        actions.OnHoverEnded = () => _explanationVisible = false;
        Button button = new(GraphicsDevice, help, null, null, dest, actions);
        buttons.Add(button);

        Texture2D controls = Content.Load<Texture2D>("controls");
        width = (int)(controlsUI.RenderTarget.Width * 0.5f);
        posX = (int)(controlsUI.RenderTarget.Width * 0.4f);
        posY = (int)(controlsUI.RenderTarget.Height - width / 2.0f);
        dest = new Rectangle(posX, posY, width, (int)(width / 2.0f));
        actions.OnHoverStarted = () => _controlsVisible = true;
        actions.OnHoverEnded = () => _controlsVisible = false;
        button = new Button(GraphicsDevice, controls, null, null, dest, actions);
        buttons.Add(button);

        return buttons.ToArray();
    }
    private static IDrawable CreatePieceAppearance(PieceType type, Team team, Rectangle destinationRectangle)
    {
        int texturePosX = PieceFactory.PieceTextureWidth * (int)type + (PieceFactory.PiecesRawTexture.Width / 2 * ((byte)team & 1));
        Rectangle textureRect = new(texturePosX, 0, PieceFactory.PieceTextureWidth, PieceFactory.PieceTextureWidth);
        DrawableObject appearance = new(PieceFactory.PiecesRawTexture, textureRect, destinationRectangle);
        return appearance;
    }
    private void InitializeGraphics()
    {
        /*_graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphics.IsFullScreen = true;*/
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();
    }
    private FENObject InitializeFENObject()
    {
        try
        {
            //string fen = "r2qkb1r/1pp2ppp/p1n1pnb1/3p4/3P1BP1/P1N1PN2/1PP2P1P/R2QKB1R w KQkq - 0 1";
            string fen = _chessGameSettings.FEN;
            return FENParser.Parse(fen);
        }
        catch (ArgumentException)
        {
            throw;
        }
    }
    private void InitializeControllers(FENObject fenObject)
    {
        Controller white, black;
        if (_chessGameSettings.IsWhiteHuman)
            white = new HumanController(Team.White, fenObject.WhitePieces, fenObject.WhiteCastling, fenObject.EnPassantSquare);
        else
            white = new AIController(Team.White, fenObject.WhitePieces, fenObject.WhiteCastling, fenObject.EnPassantSquare);

        if (_chessGameSettings.IsBlackHuman)
            black = new HumanController(Team.Black, fenObject.BlackPieces, fenObject.BlackCastling, fenObject.EnPassantSquare);
        else
            black = new AIController(Team.Black, fenObject.BlackPieces, fenObject.BlackCastling, fenObject.EnPassantSquare);

        _controllers.Add(white);
        _controllers.Add(black);
    }
    private void InitializeUI(FENObject fenObject)
    {
        IDrawableProvider[] drawableProviders = { OverlayManager.Instance, _controllers[0], _controllers[1] };
        var chessboardUI = new UIModule(GraphicsDevice, _chessboard, new Vector2(0.25f, 0.01f), new Vector2(0.50f, 0.98f), drawableProviders, Array.Empty<ITextProvider>());
        chessboardUI.Layer = 2;
        _chessboardUIModule = chessboardUI;
        _boardSaver = new(GraphicsDevice, _chessboardInputManager);

        Texture2D backgroundTexture = Content.Load<Texture2D>("uiBackground");
        DrawableObject uiBackground = new DrawableObject(backgroundTexture, new(0, 0, backgroundTexture.Width, backgroundTexture.Height), Rectangle.Empty);

        MoveHistory moveHistory = new MoveHistory(fenObject.MoveNo, _clockTimeFont, 10, _chessboardInputManager);        
        var moveHistoryUI = new UIModule(GraphicsDevice, uiBackground, (moveHistory.Width, moveHistory.Height), new Vector2(0.75f, 0f), new Vector2(0.25f, 0.75f), Array.Empty<IDrawableProvider>(), new[] { moveHistory });

        float graveyardUIWidthScale = 0.25f;
        PieceGraveyard pieceGraveyard = new((int)(_graphics.PreferredBackBufferWidth*graveyardUIWidthScale), 6, 6);
        var graveyardUI = new UIModule(GraphicsDevice, uiBackground, (pieceGraveyard.Width, pieceGraveyard.Height), new Vector2(0f, 0.25f), new Vector2(graveyardUIWidthScale, 0.5f), new[] { pieceGraveyard }, Array.Empty<ITextProvider>());
        graveyardUI.Layer = 1;

        ClockReader clockReader = new(_clockTimeFont, 1.0f, 0.25f, Color.Black);
        var clockUI = new UIModule(GraphicsDevice, uiBackground, (clockReader.Width, clockReader.Height), new Vector2(0f, 0f), new Vector2(0.25f, 1f), Array.Empty<IDrawableProvider>(), new[] { clockReader });

        float w = _graphics.PreferredBackBufferWidth * 0.25f;
        float h = _graphics.PreferredBackBufferHeight * 0.25f;
        var controlsUI = new UIModule(GraphicsDevice, uiBackground, ((int)w, (int)h), new Vector2(0.75f, 0.75f), new Vector2(0.25f, 0.25f), null, null);

        foreach (Button button in CreateControlsButtons(controlsUI))
        {
            controlsUI.SubmitElementToAdd(button);
        }
        controlsUI.Layer = int.MaxValue;

        _uiModules.Add(controlsUI);
        _uiModules.Add(graveyardUI);
        _uiModules.Add(chessboardUI);
        _uiModules.Add(moveHistoryUI);
        _uiModules.Add(clockUI);
        _uiModules.Sort();
    }
    private void InitializeClock(FENObject fenObject)
    {
        ChessClock.Initialize(_controllers[0], _controllers[1], (_chessGameSettings.WhiteClockTime, _chessGameSettings.BlackClockTime), (_chessGameSettings.WhiteIncrement, _chessGameSettings.BlackIncrement), fenObject.TeamToMove);
        ChessClock.ActiveController.Update();       
    }
    private void CreateChessGame(ChessGameSettings chessGameSettings)
    {
        FENObject fenObject;
        _chessGameSettings = chessGameSettings;
        SubscribeToEvents();
        PieceFactory.Initilize(Content.Load<Texture2D>("pieces"));
        try
        {
            fenObject = InitializeFENObject();
        }
        catch (ArgumentException)
        { throw; }
        Arbiter.Initilize(fenObject);
        _chessboard.InitilizeBoard(fenObject.AllPieces);
        InitializeControllers(fenObject);
        IOverlayFactory overlayFactory = new SquareOverlayFactory(Content.Load<Texture2D>("Tsquares"));
        OverlayManager.Create(overlayFactory);
        InitializeClock(fenObject);
        InitializeUI(fenObject);
        _inMenu = false;
        _piecesExplanation = CreatePiecesExplanation();
        _controlsExplanation = CreateControlsExplanation();
        ChessClock.Start();
    }
    protected override void Initialize()
    {
        base.Initialize();
        InitializeGraphics();
        InitializeMenu();
    }
    private void InitializeMenu()
    {
        MainMenu menu = new(GraphicsDevice, Content, CreateChessGame);
        Button[] buttons = menu.GetButtons();
        Rectangle textureRect = new(0, 0, _lightTile.Width, _lightTile.Height);
        DrawableObject background = new DrawableObject(_lightTile, textureRect, new Rectangle(0, 0, 0, 0));
        _menuUI = new UIModule(GraphicsDevice, background, (menu.Width, menu.Height), new Vector2(0,0), new Vector2(1f,1f), new[] {menu}, new[] {menu}, buttons);
        _menuInputManager.SubscribeToEvent(Keys.Escape, () => Exit());
        _menuInputManager.SubscribeToEvent(Keys.F11, () => { _graphics.IsFullScreen = !_graphics.IsFullScreen; _graphics.ApplyChanges(); });
    }
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _chessboard = Chessboard.Create(Content.Load<Texture2D>("chessboard"));
        _clockTimeFont = Content.Load<SpriteFont>("ChessClock");
        _messagesFont = Content.Load<SpriteFont>("messages");
        _lightTile = Content.Load<Texture2D>("lightTile");
        _darkTile = Content.Load<Texture2D>("darkTile");
    }
    protected override void Update(GameTime gameTime)
    {
        if (!_inMenu)
        {
            if (!_buttonInputOnly)
                _chessboardInputManager.CheckInput(gameTime);
            MovementManager.Instance.Update(gameTime);

            var mouseState = Mouse.GetState();
            foreach (UIModule module in _uiModules)
                module.Interact(mouseState.Position);

            if (_isRunning && (_updateController || ChessClock.ActiveController is HumanController) && !_buttonInputOnly)
            {
                _updateController = false;
                ChessClock.ActiveController.MakeMove();
            }
        }
        else
        {
            _menuInputManager.CheckInput(gameTime);
            _menuUI.Interact(Mouse.GetState().Position);
        }
        base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();
        GraphicsDevice.Clear(Color.CornflowerBlue);
        if (!_inMenu)
        {
            PrepareUIModulesForDrawing();
            DrawUIModules();
            if (_explanationVisible)
                _spriteBatch.Draw(_piecesExplanation.RawTexture, _piecesExplanation.DestinationRect, _piecesExplanation.TextureRect, Color.White);
            if (_controlsVisible)
                _spriteBatch.Draw(_controlsExplanation.RawTexture, _controlsExplanation.DestinationRect, _controlsExplanation.TextureRect, Color.White);
        }
        else
        {           
            _menuUI.PrepareRenderTarget();
            _spriteBatch.Draw(_menuUI.RenderTarget, new Rectangle((int)_menuUI.Position.X, (int)_menuUI.Position.Y, _menuUI.Width, _menuUI.Height), Color.White);
        }
        _spriteBatch.End();
        base.Draw(gameTime);
    }
    private void PrepareUIModulesForDrawing()
    {
        foreach (UIModule module in _uiModules)
        {
            module.PrepareRenderTarget();
            if (module == _chessboardUIModule && _boardSaver.UpdateRequired)
                _boardSaver.SaveBoardInBothInversions(module);
        }
    }
    private void DrawUIModules()
    {
        foreach (UIModule module in _uiModules)
        {
            if (module == _chessboardUIModule && _boardSaver.ShouldBeDrawn)
                _spriteBatch.Draw(_boardSaver.CurrentFrame, new Rectangle((int)module.Position.X, (int)module.Position.Y, module.Width, module.Height), Color.Wheat);
            else
                _spriteBatch.Draw(module.RenderTarget, new Rectangle((int)module.Position.X, (int)module.Position.Y, module.Width, module.Height), Color.White);
        }
    }    
    public Point TranslateToBoard(Point point) => _chessboardUIModule.ToLocalCoordinates(point);
}
