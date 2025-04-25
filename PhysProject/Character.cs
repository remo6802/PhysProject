using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace PhysProject
{
    public class Character
    {
        public enum CharacterType { Sonic, Knuckles, Tails }

        public Vector2 Position;
        private Vector2 _velocity;

        private readonly SpriteAnimator[] _animations;
        private SpriteAnimator _currentAnimator;
        private SpriteEffects _spriteEffect = SpriteEffects.None;

        private readonly float _frameWidth = 49;
        private readonly float _frameHeight = 49;
        private readonly float _mass;
        private readonly CharacterType _type;

        private int _state = 0; // 0 = idle, 1 = walk, 2 = run, 3 = jump, 4 = spring jump, 5 = spring dash

        private float _springStateTimer = 0f;
        private const float _springStateDuration = 0.4f;

        private float _springTimer = 0f;
        private const float _springCooldown = 0.2f;

        private float _lastSpringForce = 0f;
        private float _lastSpringAccel = 0f;

        public CharacterType Type => _type;
        public float FrameHeight => _frameHeight;
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

        public Vector2 Velocity => _velocity;
        public float SpringTimer => _springTimer;
        public float LastSpringForce => _lastSpringForce;
        public float LastSpringAccel => _lastSpringAccel;
        private bool _isInSpringJump = false;
        public bool IsInSpringJump => _isInSpringJump;

        public Character(CharacterType type, Texture2D[] textures, float mass)
        {
            _type = type;
            _mass = mass;

            Position = new Vector2(200, 350 - _frameHeight);

            _animations = new SpriteAnimator[6];
            _animations[0] = new SpriteAnimator(textures[0], 49, GetFrameCount(type, 0), 0.1f); // idle
            _animations[1] = new SpriteAnimator(textures[1], 49, GetFrameCount(type, 1), 0.08f); // walk
            _animations[2] = new SpriteAnimator(textures[2], 49, GetFrameCount(type, 2), 0.08f); // run
            _animations[3] = new SpriteAnimator(textures[5], 49, GetFrameCount(type, 5), 0.08f); // jump
            _animations[4] = new SpriteAnimator(textures[3], 50, GetFrameCount(type, 3), 0.08f); // spring jump
            _animations[5] = new SpriteAnimator(textures[4], 47, GetFrameCount(type, 4), 0.06f); // spring dash

            _currentAnimator = _animations[0];
        }

        public void Update(GameTime gameTime, List<Spring> springs)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState kb = Keyboard.GetState();
            bool isGrounded = Position.Y >= 350 - _frameHeight - 0.1f;

            if (_springTimer > 0f)
                _springTimer -= dt;

            Rectangle bounds = new Rectangle((int)Position.X, (int)Position.Y, (int)_frameWidth, (int)_frameHeight);

            foreach (var spring in springs)
            {
                if (!bounds.Intersects(spring.Bounds)) continue;

                if (spring.Type == Spring.SpringType.Vertical && isGrounded && _springTimer <= 0f)
                {
                    spring.Activate();
                    _lastSpringForce = 300f;
                    _lastSpringAccel = _lastSpringForce / _mass;
                    _velocity.Y = -_lastSpringAccel * 100f;
                    _springStateTimer = _springStateDuration;
                    _isInSpringJump = true;
                    _springTimer = _springCooldown;
                    _state = 4;
                    break;
                }
                else if (spring.Type == Spring.SpringType.Horizontal && _springTimer <= 0f)
                {
                    spring.Activate();

                    float force = spring.Force * 100f; 
                    float acceleration = force / _mass;

                    _velocity.X += (spring.Flip ? -1 : 1) * acceleration * 100f;

                    _springStateTimer = _springStateDuration;
                    _springTimer = _springCooldown;
                    _state = 5;
                    break;
                }


            }

            // Movement input
            float moveAccel = 500f;

            if (kb.IsKeyDown(Keys.A))
            {
                _velocity.X -= moveAccel * dt;
                _spriteEffect = SpriteEffects.FlipHorizontally;
            }
            else if (kb.IsKeyDown(Keys.D))
            {
                _velocity.X += moveAccel * dt;
                _spriteEffect = SpriteEffects.None;
            }
            else
            {
                _velocity.X = MathHelper.Lerp(_velocity.X, 0, 0.2f);
            }

            if (kb.IsKeyDown(Keys.Space) && isGrounded)
            {
                _velocity.Y = -300f;
                _state = 3;
            }

            if (!isGrounded)
                _velocity.Y += 500f * dt;

            Position += _velocity * dt;

            if (Position.Y > 350 - _frameHeight)
            {
                Position.Y = 350 - _frameHeight;
                _velocity.Y = 0;
                isGrounded = true;
                _isInSpringJump = false;
            }

            // Horizontal wraparound
            if (Position.X < -_frameWidth)
                Position.X = 1280;
            else if (Position.X > 1280)
                Position.X = -_frameWidth;

            // Set animation state
            if (_springStateTimer > 0f)
            {
                _springStateTimer -= dt;
            }
            else
            {
                if (!isGrounded)
                    _state = 3;
                else if (Math.Abs(_velocity.X) >= 150f)
                    _state = 2;
                else if (Math.Abs(_velocity.X) > 5f)
                    _state = 1;
                else
                    _state = 0;
            }

            _currentAnimator = _animations[_state];
            _currentAnimator.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _currentAnimator.Draw(spriteBatch, Position, _spriteEffect);
        }

        private int GetFrameCount(CharacterType type, int animationIndex)
        {
            return (type, animationIndex) switch
            {
                (CharacterType.Sonic, 0) => 11,
                (CharacterType.Sonic, 1) => 13,
                (CharacterType.Sonic, 2) => 23,
                (CharacterType.Sonic, 3) => 10,
                (CharacterType.Sonic, 4) => 16,
                (CharacterType.Sonic, 5) => 16,

                (CharacterType.Knuckles, 0) => 6,
                (CharacterType.Knuckles, 1) => 12,
                (CharacterType.Knuckles, 2) => 23,
                (CharacterType.Knuckles, 3) => 10,
                (CharacterType.Knuckles, 4) => 16,
                (CharacterType.Knuckles, 5) => 16,

                (CharacterType.Tails, 0) => 24,
                (CharacterType.Tails, 1) => 11,
                (CharacterType.Tails, 2) => 23,
                (CharacterType.Tails, 3) => 10,
                (CharacterType.Tails, 4) => 16,
                (CharacterType.Tails, 5) => 6,

                _ => 1
            };
        }
    }
}
