using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PhysProject;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _background;
    private Texture2D[] _characterTextures;
    private Texture2D _springTexture;
    private SpriteFont _font;

    private SpriteAnimator[] _characterAnimators;
    private SpriteAnimator _springAnimator;

    private Vector2 _characterPosition;
    private Vector2 _velocity;

    private float[] _masses = new float[] { 50f, 30f, 70f }; // Sonic, Tails, Knuckles
    private string[] _names = new string[] { "Sonic", "Tails", "Knuckles" };
    private int _activeCharacter = 0;

    private bool _onSpring = false;
    private float _springCompression = 20f; // in pixels
    private float _springConstant = 10f;    // arbitrary spring stiffness

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _characterPosition = new Vector2(300, 350);
        _velocity = Vector2.Zero;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _background = Content.Load<Texture2D>("green_hill_zone_background");
        _springTexture = Content.Load<Texture2D>("Spring-Vertical");
        _font = Content.Load<SpriteFont>("DefaultFont");

        _characterTextures = new Texture2D[]
        {
                Content.Load<Texture2D>("Sonic_Idle-removebg-preview"),
                Content.Load<Texture2D>("Tails Idle"),
                Content.Load<Texture2D>("Kunckles walk")
        };

        _characterAnimators = new SpriteAnimator[]
        {
                new SpriteAnimator(_characterTextures[0], 49, 11, 0.08f),
                new SpriteAnimator(_characterTextures[1], 32, 3, 0.1f),
                new SpriteAnimator(_characterTextures[2], 40, 10, 0.1f)
        };

        _springAnimator = new SpriteAnimator(_springTexture, 25, 5, 0.1f);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Escape))
            Exit();

        if (keyboard.IsKeyDown(Keys.D1)) _activeCharacter = 0;
        if (keyboard.IsKeyDown(Keys.D2)) _activeCharacter = 1;
        if (keyboard.IsKeyDown(Keys.D3)) _activeCharacter = 2;

        if (_onSpring)
        {
            float compressionMeters = _springCompression / 100f;
            float force = _springConstant * compressionMeters; // F = kx
            float acceleration = force / _masses[_activeCharacter]; // a = F/m
            _velocity.Y = -acceleration * 100f; // scale to pixel world
            _onSpring = false;
        }

        _velocity.Y += 500f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        _characterPosition += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_characterPosition.Y > 350)
        {
            _characterPosition.Y = 350;
            _velocity = Vector2.Zero;
        }

        if (keyboard.IsKeyDown(Keys.Space) && _characterPosition.Y >= 350)
        {
            _onSpring = true;
        }

        _characterAnimators[_activeCharacter].Update(gameTime);
        _springAnimator.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();

        _spriteBatch.Draw(_background, new Rectangle(0, 0, 800, 480), Color.White);
        _springAnimator.Draw(_spriteBatch, new Vector2(310, 400));
        _characterAnimators[_activeCharacter].Draw(_spriteBatch, _characterPosition);

        float compressionMeters = _springCompression / 100f;
        float force = _springConstant * compressionMeters;
        float acceleration = force / _masses[_activeCharacter];

        _spriteBatch.DrawString(_font, $"Character: {_names[_activeCharacter]}", new Vector2(10, 10), Color.White);
        _spriteBatch.DrawString(_font, $"Mass: {_masses[_activeCharacter]} kg", new Vector2(10, 30), Color.White);
        _spriteBatch.DrawString(_font, $"Spring Force: {force:F2} N", new Vector2(10, 50), Color.White);
        _spriteBatch.DrawString(_font, $"Acceleration: {acceleration:F2} m/s²", new Vector2(10, 70), Color.White);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}