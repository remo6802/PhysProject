using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhysProject
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

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

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
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

            // Switch characters
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
                    var spring = new Spring(_springTextureH, 25, Spring.SpringType.Horizontal, 2f, 0.4f)
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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix scaleMatrix = Matrix.CreateScale(1280f / 800f, 720f / 480f, 1f);

            _spriteBatch.Begin(
                transformMatrix: scaleMatrix,
                samplerState: SamplerState.PointClamp); // Ensures pixel-perfect visuals

            _spriteBatch.Draw(_background, new Rectangle(0, 0, 800, 480), Color.White);

            foreach (var spring in _springs)
                spring.Draw(_spriteBatch);

            _currentCharacter.Draw(_spriteBatch);
            DrawHUD();

            if (_isPaused)
                DrawPauseOverlay();

            _spriteBatch.End();

            base.Draw(gameTime);
        }


        private void DrawHUD()
        {
            Vector2 pos = new Vector2(5, 5);
            float unitsPerMeter = 100f;
            float speedMetersPerSec = _currentCharacter.Velocity.Length() / unitsPerMeter;
            float speedKPH = speedMetersPerSec * 3.6f;
            // HUD Background
            var hudBackground = new Rectangle(0, 0, 400, 160);
            _spriteBatch.Draw(_pixel, hudBackground, Color.Black * 0.5f);

            // Lines
            _spriteBatch.DrawString(_font, $"Character: {_currentCharacter.Type}", pos, Color.White);
            pos.Y += 20;
            float mass = _activeType switch
            {
                Character.CharacterType.Sonic => 50f,
                Character.CharacterType.Knuckles => 80f,
                Character.CharacterType.Tails => 30f,
                _ => 0f
            };
            _spriteBatch.DrawString(_font, $"Mass: {mass} kg", pos, Color.White);
            pos.Y += 20;
            Vector2 velocityMPS = _currentCharacter.Velocity / unitsPerMeter;
            _spriteBatch.DrawString(_font, $"Velocity: ({velocityMPS.X:0.##}, {velocityMPS.Y:0.##}) m/s", pos, Color.White);
            pos.Y += 20;
            _spriteBatch.DrawString(_font, $"Speed: {speedKPH:0.#} km/h", pos, Color.White);
            pos.Y += 20;
            _spriteBatch.DrawString(_font, $"Springs: {_springs.Count}", pos, Color.White);
            pos.Y += 20;

            if (_currentCharacter.IsInSpringJump)
            {
                _spriteBatch.DrawString(_font, $"Spring Force: {_currentCharacter.LastSpringForce:0} N", pos, Color.White);
                pos.Y += 20;
                _spriteBatch.DrawString(_font, $"Acceleration: {_currentCharacter.LastSpringAccel:0} m/s²", pos, Color.White);
            }
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
