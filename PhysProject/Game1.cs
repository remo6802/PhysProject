// Game1.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PhysProject
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _background;
        private SpriteFont _font;

        private Texture2D[] _textures = new Texture2D[5];
        private SonicPlayer _sonic;

        private Texture2D _springTextureV;
        private Texture2D _springTextureH;
        private SpriteAnimator _springAnimatorV;
        private SpriteAnimator _springAnimatorH;

        private Vector2 _springVPos;
        private Vector2 _springHPos;

        private float _springCompression = 20f;
        private float _springConstant = 10f;

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

            _textures[0] = Content.Load<Texture2D>("Sonic_Idle");
            _textures[1] = Content.Load<Texture2D>("Sonic_Walking");
            _textures[2] = Content.Load<Texture2D>("Sonic_Jog_Run_Dash");
            _textures[3] = Content.Load<Texture2D>("Sonic_Spring_Jump");
            _textures[4] = Content.Load<Texture2D>("Sonic_Horizonal_Spring_Dash");

            _sonic = new SonicPlayer(_textures);

            _springTextureV = Content.Load<Texture2D>("Spring-Vertical");
            _springTextureH = Content.Load<Texture2D>("SpringHorizontal");

            _springAnimatorV = new SpriteAnimator(_springTextureV, 38, 5, 0.1f);
            _springAnimatorH = new SpriteAnimator(_springTextureH, 25, 5, 0.1f);

            int springVHeight = _springTextureV.Height;
            int springHHeight = _springTextureH.Height;
            _springVPos = new Vector2(400, 350 - springVHeight);
            _springHPos = new Vector2(600, 350 - springHHeight);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            Rectangle springV = new Rectangle((int)_springVPos.X, (int)_springVPos.Y, 38, _springTextureV.Height);
            Rectangle springH = new Rectangle((int)_springHPos.X, (int)_springHPos.Y, 25, _springTextureH.Height);

            _sonic.Update(gameTime, springV, springH, _springAnimatorV, _springAnimatorH);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            _spriteBatch.Draw(_background, new Rectangle(0, 0, 800, 480), Color.White);
            _springAnimatorV.Draw(_spriteBatch, _springVPos);
            _springAnimatorH.Draw(_spriteBatch, _springHPos);
            _sonic.Draw(_spriteBatch);

            float force = _springConstant * (_springCompression / 100f);
            float acceleration = force / _sonic.Mass;

            _spriteBatch.DrawString(_font, $"State: {_sonic.State}", new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(_font, $"Velocity: {_sonic.Velocity}", new Vector2(10, 30), Color.White);
            _spriteBatch.DrawString(_font, $"Spring Force: {force:F2} N", new Vector2(10, 50), Color.White);
            _spriteBatch.DrawString(_font, $"Acceleration: {acceleration:F2} m/s²", new Vector2(10, 70), Color.White);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
