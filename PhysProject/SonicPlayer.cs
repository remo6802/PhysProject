using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PhysProject
{
    public class SonicPlayer
    {
        private enum SonicState { Idle, Walk, Run, SpringJump, SpringDash, Jump }

        // Character properties
        public Vector2 Position;
        public Vector2 Velocity;
        public float Mass = 50f;
        public string State => _state.ToString();

        // Movement constants (all rounded for clarity)
        private const float MoveAcceleration = 500f;   // units/sec²
        private const float MaxMoveSpeed = 200f;       // units/sec
        private const float GroundDrag = 0.8f;         // 80% reduction/sec
        private const float AirDrag = 0.2f;            // 20% reduction/sec
        private const float GroundY = 350f;            // Ground level
        private const float SpringBounce = -500f;      // Vertical spring force
        private const float SpringBoost = 300f;        // Horizontal spring force

        // Animation system
        private readonly Texture2D[] _textures;
        private readonly SpriteAnimator[] _animators = new SpriteAnimator[6];
        private SpriteAnimator _currentAnimator;
        private SonicState _state = SonicState.Idle;
        private float _springStateTimer;
        private const float SpringStateDuration = 0.4f;
        private bool _wasOnGround = true;
        private SpriteEffects _spriteEffect = SpriteEffects.None;
        private float _frameWidth = 49f;
        private float _frameHeight = 49f;
        public float FrameWidth => _frameWidth;
        public float FrameHeight => _frameHeight;
        public SonicPlayer(Texture2D[] textures)
        {
            _textures = textures;
            InitializeAnimators();
            Position = new Vector2(200, GroundY - _frameHeight);
        }

        private void InitializeAnimators()
        {
            // _animators is already initialized in its declaration
            _animators[(int)SonicState.Idle] = new SpriteAnimator(_textures[0], 49, 10, 0.1f);
            _animators[(int)SonicState.Walk] = new SpriteAnimator(_textures[1], 49, 10, 0.08f);
            _animators[(int)SonicState.Run] = new SpriteAnimator(_textures[2], 49, 23, 0.08f);
            _animators[(int)SonicState.SpringJump] = new SpriteAnimator(_textures[3], 50, 10, 0.08f);
            _animators[(int)SonicState.SpringDash] = new SpriteAnimator(_textures[4], 47, 16, 0.06f);
            _animators[(int)SonicState.Jump] = new SpriteAnimator(_textures[5], 48, 16, 0.06f);

            _currentAnimator = _animators[(int)_state];
        }

        public void Update(GameTime gameTime, List<Spring> springs)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var kb = Keyboard.GetState();

            bool isGrounded = PhysicsEngine.CheckGroundCollision(Position, GroundY, _frameHeight);

            HandleSpringCollisions(springs, kb, isGrounded, deltaTime);
            HandleMovement(kb, isGrounded, deltaTime);
            PhysicsEngine.HandleGroundClamping(ref Position, ref Velocity, GroundY, _frameHeight);

            UpdateAnimationState(isGrounded, gameTime);
            _currentAnimator.Update(gameTime);

            _wasOnGround = isGrounded;
        }

        private void HandleSpringCollisions(List<Spring> springs, KeyboardState kb, bool isGrounded, float deltaTime)
        {
            var bounds = new Rectangle((int)Position.X, (int)Position.Y, (int)_frameWidth, (int)_frameHeight);

            foreach (var spring in springs)
            {
                if (!bounds.Intersects(spring.Bounds)) continue;

                if (spring.Type == Spring.SpringType.Vertical && isGrounded)
                {
                    spring.Activate();
                    Velocity.Y = SpringBounce;
                    _springStateTimer = SpringStateDuration;
                    _state = SonicState.SpringJump;
                    _wasOnGround = false;
                    break;
                }
                else if (spring.Type == Spring.SpringType.Horizontal)
                {
                    spring.Activate();
                    Velocity.X = SpringBoost * (kb.IsKeyDown(Keys.Left) ? -1 : 1);
                    _springStateTimer = SpringStateDuration;
                    _state = SonicState.SpringDash;
                    break;
                }
            }
        }

        private void HandleMovement(KeyboardState kb, bool isGrounded, float deltaTime)
        {
            // Horizontal movement
            if (kb.IsKeyDown(Keys.Left))
            {
                Velocity.X = MathHelper.Clamp(Velocity.X - MoveAcceleration * deltaTime, -MaxMoveSpeed, MaxMoveSpeed);
                _spriteEffect = SpriteEffects.FlipHorizontally;
            }
            else if (kb.IsKeyDown(Keys.Right))
            {
                Velocity.X = MathHelper.Clamp(Velocity.X + MoveAcceleration * deltaTime, -MaxMoveSpeed, MaxMoveSpeed);
                _spriteEffect = SpriteEffects.None;
            }
            else // Natural deceleration
            {
                float drag = isGrounded ? GroundDrag : AirDrag;
                Velocity.X *= (1 - drag * deltaTime * 60); // Normalized for 60 FPS
                if (Math.Abs(Velocity.X) < 1f) Velocity.X = 0;
            }
            if (kb.IsKeyDown(Keys.Space) && isGrounded)
            {
                Velocity.Y = -300f;
                _springStateTimer = 0.2f;
                _state = SonicState.Jump;
            }

            // Apply gravity if not grounded
            if (!isGrounded)
            {
                Velocity.Y += PhysicsEngine.Gravity * deltaTime;
            }

            // Apply velocity to position
            Position += Velocity * deltaTime;
        }


        private void UpdateAnimationState(bool isGrounded, GameTime gameTime)
        {
            if (_springStateTimer > 0)
            {
                _springStateTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                return;
            }

            if (!isGrounded)
            {
                if (_state != SonicState.Jump && _state != SonicState.SpringJump)
                {
                    _state = SonicState.Jump; // use normal jump sprite
                }
            }
            else
            {
                float absVelX = Math.Abs(Velocity.X);

                if (absVelX >= MaxMoveSpeed - 1f)
                {
                    _state = SonicState.Run;

                    var runAnimator = _animators[(int)SonicState.Run];
                    runAnimator.StartFrameOverride = 18;
                    runAnimator.EndFrameOverride = 23;
                }
                else if (absVelX > 10f)
                {
                    _state = SonicState.Walk;

                    var runAnimator = _animators[(int)SonicState.Run];
                    runAnimator.StartFrameOverride = -1;
                    runAnimator.EndFrameOverride = -1;
                }
                else
                {
                    _state = SonicState.Idle;

                    var runAnimator = _animators[(int)SonicState.Run];
                    runAnimator.StartFrameOverride = -1;
                    runAnimator.EndFrameOverride = -1;
                }
            }

            _currentAnimator = _animators[(int)_state];
        }



        public void Draw(SpriteBatch spriteBatch)
        {
            _currentAnimator.Draw(spriteBatch, Position, _spriteEffect);
        }
    }
}