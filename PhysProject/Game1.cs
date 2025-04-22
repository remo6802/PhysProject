using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace PhysProject
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _background;
        private SpriteFont _font;
        private Texture2D _pixel;

        private Texture2D _springTextureV, _springTextureH;
        private List<Spring> _springs = new();

        private Dictionary<int, Character> _characters = new();
        private int _currentCharacterIndex = 1;
        private Character _player;

        private KeyboardState _previousKeyboard;
        private Keys _lastPauseKeyState;
        private bool _isPaused = false;
        private bool _showDebug = true;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _background = Content.Load<Texture2D>("green_hill_zone_background");
            _font = Content.Load<SpriteFont>("SystemArialFont");

            // Sonic textures
            var sonicTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("Sonic_Idle"),
                Content.Load<Texture2D>("Sonic_Walking"),
                Content.Load<Texture2D>("Sonic_Jog_Run_Dash"),
                Content.Load<Texture2D>("Sonic_Spring_Jump"),
                Content.Load<Texture2D>("Sonic_Horizonal_Spring_Dash"),
                Content.Load<Texture2D>("Soinc_Jump")
            };

            // Knuckles textures
            var knucklesTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("Knuckles_Idle-Photoroom"),
                Content.Load<Texture2D>("Knuckles_Walk-Photoroom"),
                Content.Load<Texture2D>("Knucles_Jog_Run_Dash"),
                Content.Load<Texture2D>("Knuckles_Spring_Jump"),
                Content.Load<Texture2D>("Knuckles_Horizonal_Spring_Dash"),
                Content.Load<Texture2D>("Knuckles_Jump")
            };

            // Add characters
            _characters[1] = new Character(Character.CharacterType.Sonic, sonicTextures, 50f);
            _characters[2] = new Character(Character.CharacterType.Knuckles, knucklesTextures, 80f);
            _player = _characters[_currentCharacterIndex];

            _springTextureV = Content.Load<Texture2D>("Spring-Vertical");
            _springTextureH = Content.Load<Texture2D>("SpringHorizontal");

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();

            if (kb.IsKeyDown(Keys.P) && _lastPauseKeyState != Keys.P)
                _isPaused = !_isPaused;

            _lastPauseKeyState = kb.IsKeyDown(Keys.P) ? Keys.P : Keys.None;

            if (!_isPaused)
            {
                if (kb.IsKeyDown(Keys.F1) && !_previousKeyboard.IsKeyDown(Keys.F1))
                    _showDebug = !_showDebug;

                if (kb.IsKeyDown(Keys.Escape))
                    Exit();

                if (kb.IsKeyDown(Keys.D1)) _player = _characters[1];
                if (kb.IsKeyDown(Keys.D2)) _player = _characters[2];

                // Spawn vertical spring
                if (kb.IsKeyDown(Keys.V) && !_previousKeyboard.IsKeyDown(Keys.V))
                {
                    var spring = new Spring(_springTextureV, 38, Spring.SpringType.Vertical, 2f, 0.1f)
                    {
                        Position = new Vector2(_player.Position.X + 40, 350 - _springTextureV.Height)
                    };
                    _springs.Add(spring);
                }

                // Spawn horizontal spring
                if (kb.IsKeyDown(Keys.H) && !_previousKeyboard.IsKeyDown(Keys.H))
                {
                    var spring = new Spring(_springTextureH, 25, Spring.SpringType.Horizontal, 2f, 0.1f)
                    {
                        Position = new Vector2(_player.Position.X + 40, 350 - _springTextureH.Height)
                    };
                    _springs.Add(spring);
                }

                foreach (var spring in _springs)
                    spring.Update(gameTime);

                _player.Update(gameTime, _springs);
            }

            _previousKeyboard = kb;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            // Draw background
            _spriteBatch.Draw(_background, new Rectangle(0, 0, 800, 480), Color.White);

            // Draw all springs
            foreach (var spring in _springs)
                spring.Draw(_spriteBatch);

            // Draw player
            _player.Draw(_spriteBatch);

            // ──────────────── HUD ────────────────
            Vector2 hudPos = new Vector2(10, 10);

            string character = $"Character: {_player.Type}";
            string state = $"State: {_player.StateName}";
            string velocity = $"Velocity: ({_player.Velocity.X:0.#}, {_player.Velocity.Y:0.#})";
            string speed = $"Speed: {_player.Velocity.Length():0.#}";
            string horizontal = _player.Velocity.X > 1 ? "Right" : _player.Velocity.X < -1 ? "Left" : "Idle";
            string vertical = _player.Velocity.Y < -1 ? "Rising" : _player.Velocity.Y > 1 ? "Falling" : "Stable";
            string springCount = $"Springs: {_springs.Count}";

            string springStats = _player.SpringTimer > 0
                ? $"  Spring Force: {_player.LastSpringForce:0.0}  Accel: {_player.LastSpringAccel:0.0}"
                : "";

            string hudLine = $"{character}     {state}     {velocity}     {speed}     H: {horizontal}     V: {vertical}     {springCount}{springStats}";

            _spriteBatch.DrawString(_font, hudLine, hudPos, Color.White);

            _spriteBatch.End();
            base.Draw(gameTime);
        }

    }
}
