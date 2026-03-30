using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SnakeEvolution.Core.AI;
using SnakeEvolution.Core.Game;
using SnakeEvolution.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace SnakeEvolution.MonoGame
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly GameLaunchOptions _launchOptions;
        private SpriteBatch _spriteBatch = null!;
        private Texture2D _pixel = null!;
        private SpriteFont _uiFont = null!;

        private const int CellSize = 32;
        private const int GridWidth = 15;
        private const int GridHeight = 10;
        private const int HudHeight = 110;

        private Snake _snake = null!;
        private Position _food;
        private readonly Random _random = new();

        private float _moveTimer;
        private const float MoveInterval = 0.10f;

        private bool _gameOver;
        private int _score;
        private float _gameOverTimer;
        private const float AutoExitDelaySeconds = 2.0f;

        private readonly QTablePolicy _aiPolicy = new();
        private readonly bool _aiMode = true;
        private bool _aiPolicyLoaded;
        private string _loadedPolicyName = "none";

        private KeyboardState _previousKeyboardState;

        public Game1(GameLaunchOptions launchOptions = null)
        {
            _launchOptions = launchOptions ?? new GameLaunchOptions();

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = GridWidth * CellSize;
            _graphics.PreferredBackBufferHeight = HudHeight + (GridHeight * CellSize);

            Window.Title = $"Snake Evolution AI - Episode {_launchOptions.EpisodeLabel}";
        }

        protected override void Initialize()
        {
            StartNewGame();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            _uiFont = Content.Load<SpriteFont>("UIFont");

            InitializeAiPolicy();
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                if (_launchOptions.AutoExitOnGameOver)
                {
                    Environment.ExitCode = 130;
                }

                Exit();
            }

            if (_gameOver)
            {
                if (_launchOptions.AutoExitOnGameOver)
                {
                    _gameOverTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (_gameOverTimer >= AutoExitDelaySeconds)
                    {
                        Exit();
                    }
                }
                else if (IsNewKeyPress(keyboardState, Keys.Enter))
                {
                    StartNewGame();
                }

                _previousKeyboardState = keyboardState;
                base.Update(gameTime);
                return;
            }

            _moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_moveTimer >= MoveInterval)
            {
                _moveTimer = 0f;

                if (_aiMode && _aiPolicyLoaded)
                {
                    ApplyAiDecision();
                }
                else
                {
                    HandleInput(keyboardState);
                }

                UpdateGame();
            }

            _previousKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(18, 18, 18));

            _spriteBatch.Begin();

            DrawGrid();
            DrawFood();
            DrawSnake();
            DrawOverlay();

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void StartNewGame()
        {
            _snake = new Snake(new Position(GridWidth / 2, GridHeight / 2), Direction.Right, 3);
            _food = SpawnFood();
            _score = 0;
            _gameOver = false;
            _moveTimer = 0f;
            _gameOverTimer = 0f;
        }

        private void HandleInput(KeyboardState keyboardState)
        {
            if (IsNewKeyPress(keyboardState, Keys.Up) || IsNewKeyPress(keyboardState, Keys.W))
            {
                _snake.SetDirection(Direction.Up);
            }
            else if (IsNewKeyPress(keyboardState, Keys.Down) || IsNewKeyPress(keyboardState, Keys.S))
            {
                _snake.SetDirection(Direction.Down);
            }
            else if (IsNewKeyPress(keyboardState, Keys.Left) || IsNewKeyPress(keyboardState, Keys.A))
            {
                _snake.SetDirection(Direction.Left);
            }
            else if (IsNewKeyPress(keyboardState, Keys.Right) || IsNewKeyPress(keyboardState, Keys.D))
            {
                _snake.SetDirection(Direction.Right);
            }
        }

        private bool IsNewKeyPress(KeyboardState keyboardState, Keys key)
        {
            return keyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }

        private void UpdateGame()
        {
            Position nextHead = _snake.GetNextHeadPosition();
            bool willGrow = nextHead == _food;

            bool hitsWall =
                nextHead.x < 0 ||
                nextHead.x >= GridWidth ||
                nextHead.y < 0 ||
                nextHead.y >= GridHeight;

            bool hitsSelf = _snake.WouldCollide(nextHead, willGrow);

            if (hitsWall || hitsSelf)
            {
                _gameOver = true;
                return;
            }

            _snake.Move(willGrow);

            if (willGrow)
            {
                _score++;
                _food = SpawnFood();
            }
        }

        private Position SpawnFood()
        {
            Position candidate;

            do
            {
                candidate = new Position(
                    _random.Next(0, GridWidth),
                    _random.Next(0, GridHeight)
                );
            }
            while (_snake.Occupies(candidate));

            return candidate;
        }

        private void DrawGrid()
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Rectangle cellRectangle = new Rectangle(
                        x * CellSize,
                        HudHeight + (y * CellSize),
                        CellSize - 1,
                        CellSize - 1
                    );

                    _spriteBatch.Draw(_pixel, cellRectangle, new Color(35, 35, 35));
                }
            }
        }

        private void DrawSnake()
        {
            foreach (Position segment in _snake.Segments)
            {
                Rectangle rectangle = new Rectangle(
                    segment.x * CellSize,
                    HudHeight + (segment.y * CellSize),
                    CellSize - 1,
                    CellSize - 1
                );

                Color color = segment == _snake.Head
                    ? new Color(80, 255, 120)
                    : new Color(30, 180, 70);

                _spriteBatch.Draw(_pixel, rectangle, color);
            }
        }

        private void DrawFood()
        {
            Rectangle rectangle = new Rectangle(
                _food.x * CellSize,
                HudHeight + (_food.y * CellSize),
                CellSize - 1,
                CellSize - 1
            );

            _spriteBatch.Draw(_pixel, rectangle, new Color(220, 50, 50));
        }

        private void DrawOverlay()
        {
            _spriteBatch.Draw(
                _pixel,
                new Rectangle(0, 0, GridWidth * CellSize, HudHeight),
                Color.Black);

            _spriteBatch.Draw(
                _pixel,
                new Rectangle(0, HudHeight - 2, GridWidth * CellSize, 2),
                new Color(45, 45, 45));

            Vector2 origin = new Vector2(12f, 10f);
            _spriteBatch.DrawString(_uiFont, $"Episode: {_launchOptions.EpisodeLabel}", origin, Color.White);
            _spriteBatch.DrawString(_uiFont, $"Score: {_score}", origin + new Vector2(0f, 24f), Color.White);

            string modeText = _aiPolicyLoaded ? "Mode: AI Policy" : "Mode: Manual Fallback";
            _spriteBatch.DrawString(_uiFont, modeText, origin + new Vector2(0f, 48f), Color.LightGray);
            _spriteBatch.DrawString(_uiFont, $"Policy: {_loadedPolicyName}", origin + new Vector2(0f, 72f), Color.LightGray);

            if (_launchOptions.AutoExitOnGameOver)
            {
                _spriteBatch.DrawString(_uiFont, "ESC: Stop demo", origin + new Vector2(520f, 10f), Color.LightGray);
            }

            if (_gameOver)
            {
                _spriteBatch.DrawString(_uiFont, "GAME OVER", origin + new Vector2(360f, 72f), Color.OrangeRed);
            }
        }

        private void InitializeAiPolicy()
        {
            if (!_aiMode)
            {
                return;
            }

            string policyPath = ResolvePolicyPath();

            if (string.IsNullOrWhiteSpace(policyPath))
            {
                Console.WriteLine("AI disattivata: q_table.json non trovato.");
                return;
            }

            try
            {
                _aiPolicy.Load(policyPath);
                _aiPolicyLoaded = true;
                _loadedPolicyName = Path.GetFileName(policyPath);
                Console.WriteLine($"AI policy caricata da: {policyPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI disattivata: errore caricamento policy - {ex.Message}");
            }
        }

        private void ApplyAiDecision()
        {
            string stateKey = SnakeAiStateBuilder.BuildStateKey(_snake, _food, GridWidth, GridHeight);
            string bestAction = _aiPolicy.GetBestAction(stateKey);
            Direction? aiDirection = AiDirectionMapper.ParseAction(bestAction);

            if (aiDirection.HasValue)
            {
                _snake.SetDirection(aiDirection.Value);
            }
        }

        private string ResolvePolicyPath()
        {
            if (!string.IsNullOrWhiteSpace(_launchOptions.PolicyJsonPath) && File.Exists(_launchOptions.PolicyJsonPath))
            {
                return _launchOptions.PolicyJsonPath;
            }

            return FindQTablePath();
        }

        private static string FindQTablePath()
        {
            foreach (string basePath in GetSearchRoots())
            {
                string[] candidates =
                [
                    Path.Combine(basePath, "q_table.json"),
                    Path.Combine(basePath, "SnakeEvolutionAI", "q_table.json")
                ];

                foreach (string candidate in candidates)
                {
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }

            return string.Empty;
        }

        private static IEnumerable<string> GetSearchRoots()
        {
            var roots = new List<string>
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory
            };

            foreach (string root in roots.ToArray())
            {
                DirectoryInfo current = new DirectoryInfo(root);

                while (current != null)
                {
                    roots.Add(current.FullName);
                    current = current.Parent;
                }
            }

            return roots;
        }
    }
}
