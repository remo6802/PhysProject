using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using PhysProject;

namespace PhysProject
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _background;
        private SpriteFont _font;

        private Texture2D[] _sonicTextures;
        private SonicPlayer _player;

        private Texture2D _springTextureV;
        private Texture2D _springTextureH;
        private List<Spring> _springs = new();

        private KeyboardState _previousKeyboard;

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

            _background = Content.Load<Texture2D>("green_hill_zone_background");
            _font = Content.Load<SpriteFont>("SystemArialFont");

            _sonicTextures = new Texture2D[5];
            _sonicTextures[0] = Content.Load<Texture2D>("Sonic_Idle");
            _sonicTextures[1] = Content.Load<Texture2D>("Sonic_Walking");
            _sonicTextures[2] = Content.Load<Texture2D>("Sonic_Jog_Run_Dash"); // should be 1127px wide
            _sonicTextures[3] = Content.Load<Texture2D>("Sonic_Spring_Jump");
            _sonicTextures[4] = Content.Load<Texture2D>("Sonic_Horizonal_Spring_Dash");

            _player = new SonicPlayer(_sonicTextures);

            _springTextureV = Content.Load<Texture2D>("Spring-Vertical");
            _springTextureH = Content.Load<Texture2D>("SpringHorizontal");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();
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

            // Update springs
            foreach (var spring in _springs)
                spring.Update(gameTime);

            // Update player with springs
            _player.Update(gameTime, _springs);

            _previousKeyboard = kb;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            // Background
            _spriteBatch.Draw(_background, new Rectangle(0, 0, 800, 480), Color.White);

            // Draw all springs
            foreach (var spring in _springs)
                spring.Draw(_spriteBatch);

            // Draw Sonic
            _player.Draw(_spriteBatch);
            
            // HUD (optional debug info)
            _spriteBatch.DrawString(_font, $"State: {_player.State}", new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(_font, $"Velocity: {_player.Velocity}", new Vector2(10, 30), Color.White);
            _spriteBatch.DrawString(_font, $"Springs: {_springs.Count}", new Vector2(10, 50), Color.White);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
