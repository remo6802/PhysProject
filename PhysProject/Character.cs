using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace PhysProject
{
    public class Character
    {
        public enum CharacterType { Sonic, Knuckles }

        public Vector2 Position;
        public Vector2 Velocity;
        private readonly SpriteAnimator[] _animations;
        private SpriteAnimator _currentAnimator;
        private int _state; // 0 = idle, 1 = walk, 2 = run, 3 = jump, 4 = spring jump, 5 = spring dash

        private SpriteEffects _spriteEffect = SpriteEffects.None;
        private readonly float _frameWidth = 49;
        private readonly float _frameHeight = 49;

        private readonly float _mass;
        private readonly CharacterType _type;

        private float _springStateTimer = 0f;
        private const float _springStateDuration = 0.4f;
        private float _springTimer = 0f;
        private float _lastSpringForce = 0f;
        private float _lastSpringAccel = 0f;
        private bool _canUseVerticalSpring = true;

        public string StateName => _state switch
        {
            0 => "Idle",
            1 => "Walk",
            2 => "Run",
            3 => "Jump",
            4 => "SpringJump",
            5 => "SpringDash",
            _ => "Unknown"
        };

        public float FrameHeight => _frameHeight;
        public float Mass => _mass;
        public float LastSpringForce => _lastSpringForce;
        public float LastSpringAccel => _lastSpringAccel;
        public float SpringTimer => _springTimer;
        public CharacterType Type => _type;

        public Character(CharacterType type, Texture2D[] textures, float mass)
        {
            _type = type;
            _mass = mass;
            Position = new Vector2(200, 350 - _frameHeight);

            _animations = new SpriteAnimator[6];
            _animations[0] = new SpriteAnimator(textures[0], 49, 11, 0.1f); // idle
            _animations[1] = new SpriteAnimator(textures[1], 49, 13, 0.08f); // walk
            _animations[2] = new SpriteAnimator(textures[2], 49, 23, 0.08f); // run
            _animations[3] = new SpriteAnimator(textures[5], 49, 16, 0.08f); // jump
            _animations[4] = new SpriteAnimator(textures[3], 50, 10, 0.08f); // spring jump
            _animations[5] = new SpriteAnimator(textures[4], 47, 16, 0.06f); // spring dash

            _currentAnimator = _animations[0];
        }

        public void Update(GameTime gameTime, List<Spring> springs)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState kb = Keyboard.GetState();
            bool isGrounded = Position.Y >= 350 - _frameHeight - 0.1f;

            Rectangle bounds = new Rectangle((int)Position.X, (int)Position.Y, (int)_frameWidth, (int)_frameHeight);

            foreach (var spring in springs)
            {
                if (!bounds.Intersects(spring.Bounds)) continue;

                if (spring.Type == Spring.SpringType.Vertical && isGrounded && _canUseVerticalSpring)
                {
                    spring.Activate();
                    _lastSpringForce = 300f;
                    _lastSpringAccel = _lastSpringForce / _mass;
                    Velocity.Y = -_lastSpringAccel * 100f;
                    _springStateTimer = _springStateDuration;
                    _springTimer = 0.3f;
                    _state = 4;
                    _canUseVerticalSpring = false;
                    break;
                }
                else if (spring.Type == Spring.SpringType.Horizontal)
                {
                    spring.Activate();
                    Velocity.X = 250f * (kb.IsKeyDown(Keys.Left) ? -1 : 1);
                    _springStateTimer = _springStateDuration;
                    _state = 5;
                    break;
                }
            }

            if (!springs.Exists(s => s.Type == Spring.SpringType.Vertical && bounds.Intersects(s.Bounds)))
                _canUseVerticalSpring = true;

            if (_springTimer > 0f)
                _springTimer -= dt;

            // Movement logic
            float acceleration = _type == CharacterType.Sonic ? 1000f : 700f;
            float maxSpeed = _type == CharacterType.Sonic ? 200f : 150f;

            if (kb.IsKeyDown(Keys.Left))
            {
                Velocity.X -= acceleration * dt;
                Velocity.X = MathHelper.Clamp(Velocity.X, -maxSpeed, maxSpeed);
                _spriteEffect = SpriteEffects.FlipHorizontally;
            }
            else if (kb.IsKeyDown(Keys.Right))
            {
                Velocity.X += acceleration * dt;
                Velocity.X = MathHelper.Clamp(Velocity.X, -maxSpeed, maxSpeed);
                _spriteEffect = SpriteEffects.None;
            }
            else
            {
                Velocity.X = MathHelper.Lerp(Velocity.X, 0, 0.2f);
                if (Math.Abs(Velocity.X) < 0.5f) Velocity.X = 0;
            }

            if (kb.IsKeyDown(Keys.Space) && isGrounded && _springStateTimer <= 0f)
            {
                Velocity.Y = _type == CharacterType.Sonic ? -400f : -330f;
                _state = 3;
            }

            if (!isGrounded)
                Velocity.Y += 500f * dt;

            Position += Velocity * dt;

            if (Position.Y > 350 - _frameHeight)
            {
                Position.Y = 350 - _frameHeight;
                Velocity.Y = 0;
            }

            // Animation state logic
            if (_springStateTimer > 0)
            {
                _springStateTimer -= dt;
            }
            else
            {
                if (!isGrounded)
                    _state = 3;
                else if (Math.Abs(Velocity.X) >= 150f)
                    _state = 2;
                else if (Math.Abs(Velocity.X) > 5f)
                    _state = 1;
                else
                    _state = 0;
            }

            // Set animator only if state changed
            if (_currentAnimator != _animations[_state])
            {
                _currentAnimator = _animations[_state];
                _currentAnimator.ResetAnimation();
            }

            // Loop only last 5 frames of Run at max speed
            if (_state == 2 && Math.Abs(Velocity.X) >= 195f)
            {
                _currentAnimator.SetLoopRange(18, 22);
            }
            else
            {
                _currentAnimator.ClearLoopRange();
            }

            _currentAnimator.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _currentAnimator.Draw(spriteBatch, Position, _spriteEffect);
        }
    }
}
