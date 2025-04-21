// SonicPlayer.cs
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PhysProject
{
    public class SonicPlayer
    {
        private enum SonicState { Idle, Walk, Run, SpringJump, SpringDash }

        private Texture2D[] _textures;
        private SpriteAnimator[] _animators;
        private SpriteAnimator _currentAnimator;

        public Vector2 Position;
        private Vector2 _velocity;
        private SpriteEffects _spriteEffect = SpriteEffects.None;
        private SonicState _state = SonicState.Idle;

        private float _frameWidth = 49f;
        private float _frameHeight = 49f;

        private float _mass = 50f;
        private float _springStateTimer = 0f;
        private const float _springStateDuration = 0.4f;

        private bool _touchedSpringV = false;
        private bool _touchedSpringH = false;
        private bool _wasOnGround = true;

        public SonicPlayer(Texture2D[] textures)
        {
            _textures = textures;
            _animators = new SpriteAnimator[5];

            _animators[(int)SonicState.Idle] = new SpriteAnimator(_textures[0], 49, 10, 0.1f);
            _animators[(int)SonicState.Walk] = new SpriteAnimator(_textures[1], 49, 10, 0.08f);
            _animators[(int)SonicState.Run] = new SpriteAnimator(_textures[2], 49, 23, 0.08f);
            _animators[(int)SonicState.SpringJump] = new SpriteAnimator(_textures[3], 50, 10, 0.08f);
            _animators[(int)SonicState.SpringDash] = new SpriteAnimator(_textures[4], 47, 16, 0.06f);

            _currentAnimator = _animators[(int)_state];
            Position = new Vector2(200, 350 - _frameHeight);
        }

        public void Update(GameTime gameTime, Rectangle springV, Rectangle springH, SpriteAnimator springAnimatorV, SpriteAnimator springAnimatorH)
        {
            var kb = Keyboard.GetState();
            float groundY = 350 - _frameHeight; // Use consistent frame height
            bool isOnGround = Position.Y >= groundY - 0.1f; // Tight but stable ground check
            bool justLanded = !_wasOnGround && isOnGround;
            _wasOnGround = isOnGround;

            // Spring interactions (unchanged)
            Rectangle sonicBox = new Rectangle((int)Position.X, (int)Position.Y, (int)_frameWidth, (int)_frameHeight);
            if (sonicBox.Intersects(springV) && isOnGround && !_touchedSpringV)
            {
                _velocity.Y = -300f; // Direct velocity set for consistent bounce
                springAnimatorV.Update(gameTime);
                _springStateTimer = _springStateDuration;
                _state = SonicState.SpringJump;
                _touchedSpringV = true;
            }
            else if (!sonicBox.Intersects(springV)) _touchedSpringV = false;

            if (sonicBox.Intersects(springH) && Math.Abs(_velocity.X) < 1f && !_touchedSpringH)
            {
                _velocity.X = 250f * (kb.IsKeyDown(Keys.Left) ? -1 : 1);
                springAnimatorH.Update(gameTime);
                _springStateTimer = _springStateDuration;
                _state = SonicState.SpringDash;
                _touchedSpringH = true;
            }
            else if (!sonicBox.Intersects(springH)) _touchedSpringH = false;

            // Horizontal movement
            float acceleration = 800f;
            float maxSpeed = 150f;
            float deceleration = 0.2f;

            if (kb.IsKeyDown(Keys.Left))
            {
                _velocity.X = MathHelper.Clamp(_velocity.X - acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds, -maxSpeed, maxSpeed);
                _spriteEffect = SpriteEffects.FlipHorizontally;
            }
            else if (kb.IsKeyDown(Keys.Right))
            {
                _velocity.X = MathHelper.Clamp(_velocity.X + acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds, -maxSpeed, maxSpeed);
                _spriteEffect = SpriteEffects.None;
            }
            else
            {
                _velocity.X = MathHelper.Lerp(_velocity.X, 0, deceleration);
                if (Math.Abs(_velocity.X) < 0.5f) _velocity.X = 0;
            }

            // Vertical movement
            if (!isOnGround)
            {
                _velocity.Y += 500f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                _velocity.Y = 0; // Hard reset when grounded
            }

            Position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Ground clamping with hysteresis
            if (Position.Y > groundY)
            {
                Position.Y = groundY;
                _velocity.Y = 0;
                isOnGround = true;
            }

            // State management
            if (_springStateTimer > 0f)
            {
                _springStateTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                if (!isOnGround)
                {
                    _state = SonicState.SpringJump;
                }
                else if (justLanded)
                {
                    _state = SonicState.Idle;
                }
                else
                {
                    float absVelX = Math.Abs(_velocity.X);
                    if (absVelX >= 120f) _state = SonicState.Run;
                    else if (absVelX > 5f) _state = SonicState.Walk;
                    else _state = SonicState.Idle;
                }
            }

            _currentAnimator = _animators[(int)_state];
            _currentAnimator.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _currentAnimator.Draw(spriteBatch, Position, _spriteEffect);
        }

        public Vector2 Velocity => _velocity;
        public string State => _state.ToString();
        public float Mass => _mass;
    }
}
