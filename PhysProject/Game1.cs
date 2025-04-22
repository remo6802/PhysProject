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

        // INTERNAL (virtual resolution) & SCREEN (displayed size)
        private const int VirtualWidth = 800;
        private const int VirtualHeight = 480;
        private const int ScreenScale = 2;
        private const int ScreenWidth = VirtualWidth * ScreenScale;
        private const int ScreenHeight = VirtualHeight * ScreenScale;
        private RenderTarget2D _renderTarget;

        private Texture2D _background;
        private SpriteFont _font;
        private Texture2D _pixel;

        private Dictionary<Character.CharacterType, Texture2D[]> _characterTextures;
        private Character _currentCharacter;
        private Character.CharacterType _activeType = Character.CharacterType.Sonic;

        private Texture2D _springTextureV;
        private Texture2D _springTextureH;
        private List<Spring> _springs = new();

        private KeyboardState _previousKeyboard;
        private bool _isPaused = false;
        private bool _showDebug = true;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = ScreenWidth;
            _graphics.PreferredBackBufferHeight = ScreenHeight;
            _graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(GraphicsDevice, VirtualWidth, VirtualHeight);

            _background = Content.Load<Texture2D>("green_hill_zone_background");
            _font = Content.Load<SpriteFont>("SystemArialFont");

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            _springTextureV = Content.Load<Texture2D>("Spring-Vertical");
            _springTextureH = Content.Load<Texture2D>("SpringHorizontal");

            _characterTextures = new()
            {
                [Character.CharacterType.Sonic] = new[]
                {
                    Content.Load<Texture2D>("Sonic_Idle"),
                    Content.Load<Texture2D>("Sonic_Walking"),
                    Content.Load<Texture2D>("Sonic_Jog_Run_Dash"),
                    Content.Load<Texture2D>("Sonic_Spring_Jump"),
                    Content.Load<Texture2D>("Sonic_Horizonal_Spring_Dash"),
                    Content.Load<Texture2D>("Soinc_Jump")
                },
                [Character.CharacterType.Knuckles] = new[]
                {
                    Content.Load<Texture2D>("Knuckles_Idle-Photoroom"),
                    Content.Load<Texture2D>("Knuckles_Walk-Photoroom"),
                    Content.Load<Texture2D>("Knucles_Jog_Run_Dash"),
                    Content.Load<Texture2D>("Knuckles_Spring_Jump"),
                    Content.Load<Texture2D>("Knuckles_Horizonal_Spring_Dash"),
                    Content.Load<Texture2D>("Knuckles_Jump")
                },
                [Character.CharacterType.Tails] = new[]
                {
                    Content.Load<Texture2D>("Tails_Idle"),
                    Content.Load<Texture2D>("Tails_Walk"),
                    Content.Load<Texture2D>("Tails_Run_Jog_Dash"),
                    Content.Load<Texture2D>("Tails_Spring_Jump"),
                    Content.Load<Texture2D>("Tails_Horizonal_Spring_Dash"),
                    Content.Load<Texture2D>("Tails_Jump")
                }
            };

            _currentCharacter = new Character(_activeType, _characterTextures[_activeType], mass: 50f);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();

            if (kb.IsKeyDown(Keys.P) && !_previousKeyboard.IsKeyDown(Keys.P))
                _isPaused = !_isPaused;

            if (kb.IsKeyDown(Keys.F1) && !_previousKeyboard.IsKeyDown(Keys.F1))
                _showDebug = !_showDebug;

            if (kb.IsKeyDown(Keys.Escape))
                Exit();

            if (kb.IsKeyDown(Keys.D1)) SwitchCharacter(Character.CharacterType.Sonic, 50f);
            if (kb.IsKeyDown(Keys.D2)) SwitchCharacter(Character.CharacterType.Knuckles, 80f);
            if (kb.IsKeyDown(Keys.D3)) SwitchCharacter(Character.CharacterType.Tails, 30f);

            if (!_isPaused)
            {
                if (kb.IsKeyDown(Keys.V) && !_previousKeyboard.IsKeyDown(Keys.V))
                {
                    var spring = new Spring(_springTextureV, 38, Spring.SpringType.Vertical, 2f, 0.1f)
                    {
                        Position = new Vector2(_currentCharacter.Position.X + 40, 350 - _springTextureV.Height)
                    };
                    _springs.Add(spring);
                }

                if (kb.IsKeyDown(Keys.H) && !_previousKeyboard.IsKeyDown(Keys.H))
                {
                    var spring = new Spring(_springTextureH, 25, Spring.SpringType.Horizontal, 2f, 0.1f)
                    {
                        Position = new Vector2(_currentCharacter.Position.X + 40, 350 - _springTextureH.Height)
                    };
                    _springs.Add(spring);
                }

                foreach (var spring in _springs)
                    spring.Update(gameTime);

                _currentCharacter.Update(gameTime, _springs);
            }

            _previousKeyboard = kb;
            base.Update(gameTime);
        }

        private void SwitchCharacter(Character.CharacterType newType, float mass)
        {
            if (_activeType != newType)
            {
                _activeType = newType;
                _currentCharacter = new Character(newType, _characterTextures[newType], mass);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _spriteBatch.Draw(_background, new Rectangle(0, 0, VirtualWidth, VirtualHeight), Color.White);

            foreach (var spring in _springs)
                spring.Draw(_spriteBatch);

            _currentCharacter.Draw(_spriteBatch);

            DrawHUD();

            if (_isPaused)
                DrawPauseOverlay();

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_renderTarget, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHUD()
        {
            Vector2 pos = new Vector2(5, 5);
            string h = _currentCharacter.Velocity.X > 1 ? "Right" : _currentCharacter.Velocity.X < -1 ? "Left" : "Idle";
            string v = _currentCharacter.Velocity.Y < -1 ? "Rising" : _currentCharacter.Velocity.Y > 1 ? "Falling" : "Stable";

            string line = $"Character: {_currentCharacter.Type}    " +
                          $"State: {_currentCharacter.StateName}    " +
                          $"Velocity: ({_currentCharacter.Velocity.X:0}, {_currentCharacter.Velocity.Y:0})    " +
                          $"Speed: {_currentCharacter.Velocity.Length():0}    " +
                          $"H: {h}    V: {v}    Springs: {_springs.Count}";

            if (_currentCharacter.SpringTimer > 0)
            {
                line += $"    Spring Force: {_currentCharacter.LastSpringForce:0.00} N    " +
                        $"Acceleration: {_currentCharacter.LastSpringAccel:0.00} m/s²";
            }

            _spriteBatch.DrawString(_font, line, pos, Color.White);
        }

        private void DrawPauseOverlay()
        {
            _spriteBatch.Draw(_pixel, GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            string text = "PAUSED (Press P to resume)";
            Vector2 size = _font.MeasureString(text);
            Vector2 pos = new Vector2(
                (GraphicsDevice.Viewport.Width - size.X) / 2,
                (GraphicsDevice.Viewport.Height - size.Y) / 2);
            _spriteBatch.DrawString(_font, text, pos, Color.White);
        }
    }
}
