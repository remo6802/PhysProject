using System;
using System.Collections.Generic;
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

        public void Update(GameTime gameTime, List<Spring> springs)
        {
            var kb = Keyboard.GetState();
            float groundY = 350 - _frameHeight;
            bool isOnGround = Position.Y >= groundY - 0.1f;
            bool justLanded = !_wasOnGround && isOnGround;
            _wasOnGround = isOnGround;

            Rectangle sonicBox = new Rectangle((int)Position.X, (int)Position.Y, (int)_frameWidth, (int)_frameHeight);

            // Spring collisions
            foreach (var spring in springs)
            {
                if (sonicBox.Intersects(spring.Bounds))
                {
                    spring.Animator.Update(gameTime);

                    if (spring.Type == Spring.SpringType.Vertical && isOnGround)
                    {
                        _velocity.Y = -300f;
                        _springStateTimer = _springStateDuration;
                        _state = SonicState.SpringJump;
                    }
                    else if (spring.Type == Spring.SpringType.Horizontal && Math.Abs(_velocity.X) < 1f)
                    {
                        _velocity.X = 250f * (kb.IsKeyDown(Keys.Left) ? -1 : 1);
                        _springStateTimer = _springStateDuration;
                        _state = SonicState.SpringDash;
                    }
                }
            }

            // Movement logic
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

            // Apply gravity
            if (!isOnGround)
            {
                _velocity.Y += 500f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                _velocity.Y = 0;
            }

            Position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Position.Y > groundY)
            {
                Position.Y = groundY;
                _velocity.Y = 0;
                isOnGround = true;
            }

            // Animation state switching
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
