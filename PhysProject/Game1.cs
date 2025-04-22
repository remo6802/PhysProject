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

        // Game assets
        private Texture2D _background;
        private Texture2D _pixel; // For debug drawing
        private SpriteFont _font;

        // Game objects
        private Texture2D[] _textures;
        private SonicPlayer _player;
        private Texture2D _springTextureV;
        private Texture2D _springTextureH;
        private List<Spring> _springs = new();

        // Input states
        private KeyboardState _previousKeyboard;
        private Keys _lastPauseKeyState;

        // Game states
        private bool _isPaused = false;
        private bool _showDebug = true;
        

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load textures
            _background = Content.Load<Texture2D>("green_hill_zone_background");
            _font = Content.Load<SpriteFont>("SystemArialFont");

            // Load Sonic textures
            _textures = new Texture2D[6];
            _textures[0] = Content.Load<Texture2D>("Sonic_Idle");
            _textures[1] = Content.Load<Texture2D>("Sonic_Walking");
            _textures[2] = Content.Load<Texture2D>("Sonic_Jog_Run_Dash");
            _textures[3] = Content.Load<Texture2D>("Sonic_Spring_Jump");
            _textures[4] = Content.Load<Texture2D>("Sonic_Horizonal_Spring_Dash");
            _textures[5] = Content.Load<Texture2D>("Soinc_Jump");


            _player = new SonicPlayer(_textures);

            // Load spring textures
            _springTextureV = Content.Load<Texture2D>("Spring-Vertical");
            _springTextureH = Content.Load<Texture2D>("SpringHorizontal");

            // Create debug pixel
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();

            // Toggle pause with P key (with key debounce)
            if (kb.IsKeyDown(Keys.P) && _lastPauseKeyState != Keys.P)
            {
                _isPaused = !_isPaused;
            }
            _lastPauseKeyState = kb.IsKeyDown(Keys.P) ? Keys.P : Keys.None;

            if (!_isPaused)
            {
                // Toggle debug with F1
                if (kb.IsKeyDown(Keys.F1) && _previousKeyboard.IsKeyUp(Keys.F1))
                    _showDebug = !_showDebug;

                if (kb.IsKeyDown(Keys.Escape))
                    Exit();

                // Spawn vertical spring with V key
                if (kb.IsKeyDown(Keys.V) && !_previousKeyboard.IsKeyDown(Keys.V))
                {
                    var spring = new Spring(_springTextureV, 38, Spring.SpringType.Vertical, 2f, 0.1f)
                    {
                        Position = new Vector2(_player.Position.X + 40, 350 - _springTextureV.Height)
                    };
                    _springs.Add(spring);
                }

                // Spawn horizontal spring with H key
                if (kb.IsKeyDown(Keys.H) && !_previousKeyboard.IsKeyDown(Keys.H))
                {
                    var spring = new Spring(_springTextureH, 25, Spring.SpringType.Horizontal, 2f, 0.1f)
                    {
                        Position = new Vector2(_player.Position.X + 40, 350 - _springTextureH.Height)
                    };
                    _springs.Add(spring);
                }

                // Update game objects
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

            // Draw all active springs
            foreach (var spring in _springs)
            {
                spring.Draw(_spriteBatch);
            }

            // Draw Sonic
            _player.Draw(_spriteBatch);

            // ──────────────── HUD ────────────────

            Vector2 hudPos = new Vector2(10, 10);

            string state = $"State: {_player.State}";
            string velocity = $"Velocity: ({_player.Velocity.X:0.#}, {_player.Velocity.Y:0.#})";
            float speed = _player.Velocity.Length();
            string horizontal = _player.Velocity.X > 1 ? "Right" : _player.Velocity.X < -1 ? "Left" : "Idle";
            string vertical = _player.Velocity.Y < -1 ? "Rising" : _player.Velocity.Y > 1 ? "Falling" : "Stable";
            string springCount = $"Springs: {_springs.Count}";

            string hudLine = $"{state}     {velocity}     Speed: {speed:0.#}     H: {horizontal}     V: {vertical}     {springCount}";

            _spriteBatch.DrawString(_font, hudLine, hudPos, Color.White);

            _spriteBatch.End();
            base.Draw(gameTime);
        }


        private void DrawPhysicsDebugInfo()
        {
            // Removed unused 'scale' variable
            Vector2 debugPos = new Vector2(10, 90);

            // Basic movement info
            _spriteBatch.DrawString(_font, "Movement Info:", debugPos, Color.Yellow);
            debugPos.Y += 20;

            // Speed (magnitude of velocity)
            float speed = _player.Velocity.Length();
            _spriteBatch.DrawString(_font, $"Speed: {speed:F1} units/sec", debugPos, Color.White);
            debugPos.Y += 20;

            // Horizontal movement
            string direction = _player.Velocity.X > 0 ? "Right" : "Left";
            _spriteBatch.DrawString(_font, $"Moving: {direction} ({Math.Abs(_player.Velocity.X):F1} u/s)", debugPos, Color.White);
            debugPos.Y += 20;

            // Vertical movement
            string verticalState = _player.Velocity.Y > 0 ? "Falling" : "Rising";
            _spriteBatch.DrawString(_font, $"Vertical: {verticalState} ({Math.Abs(_player.Velocity.Y):F1} u/s)", debugPos, Color.White);
            debugPos.Y += 30;

            // State info
            _spriteBatch.DrawString(_font, "Character State:", debugPos, Color.Yellow);
            debugPos.Y += 20;
            _spriteBatch.DrawString(_font, $"{_player.State}", debugPos, Color.White);
            debugPos.Y += 20;

            // Grounded status - now using public property
            string groundedStatus = PhysicsEngine.CheckGroundCollision(_player.Position, 350, _player.FrameHeight)
                ? "On Ground" : "In Air";
            _spriteBatch.DrawString(_font, groundedStatus, debugPos, Color.White);
        }

        private void DrawPauseOverlay()
        {
            // Semi-transparent overlay
            _spriteBatch.Draw(_pixel, GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);

            // Pause text
            string pauseText = "PAUSED (Press P to resume)";
            Vector2 textSize = _font.MeasureString(pauseText);
            Vector2 textPos = new Vector2(
                (GraphicsDevice.Viewport.Width - textSize.X) / 2,
                (GraphicsDevice.Viewport.Height - textSize.Y) / 2);

            _spriteBatch.DrawString(_font, pauseText, textPos, Color.White);
        }

        private void DrawArrow(Vector2 start, Vector2 direction, Color color, float thickness)
        {
            // Draw line
            float angle = (float)Math.Atan2(direction.Y, direction.X);
            Vector2 end = start + direction;

            _spriteBatch.Draw(
                _pixel,
                start,
                null,
                color,
                angle,
                Vector2.Zero,
                new Vector2(direction.Length(), thickness),
                SpriteEffects.None,
                0);

            // Draw arrowhead
            Vector2 arrowHead1 = end + new Vector2(
                (float)Math.Cos(angle + Math.PI * 0.75f) * 10f,
                (float)Math.Sin(angle + Math.PI * 0.75f) * 10f);

            Vector2 arrowHead2 = end + new Vector2(
                (float)Math.Cos(angle - Math.PI * 0.75f) * 10f,
                (float)Math.Sin(angle - Math.PI * 0.75f) * 10f);

            _spriteBatch.Draw(_pixel, end, null, color, 0, Vector2.Zero, new Vector2((arrowHead1 - end).Length(), thickness), SpriteEffects.None, 0);
            _spriteBatch.Draw(_pixel, end, null, color, 0, Vector2.Zero, new Vector2((arrowHead2 - end).Length(), thickness), SpriteEffects.None, 0);
        }
    }
}