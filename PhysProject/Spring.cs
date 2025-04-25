using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysProject
{
    public class Spring
    {
        public enum SpringType { Vertical, Horizontal }

        private readonly Texture2D _texture;
        private readonly int _frameWidth;
        public Vector2 Position;
        public SpringType Type;
        public bool Flip;
        public SpriteAnimator Animator;

        private bool _isActivated = false;
        private float _activationTimer = 0f;
        private readonly float _activationDuration;

        public float Force { get; private set; }

        public Spring(Texture2D texture, int frameWidth, SpringType type, float force, float duration, bool flip = false)
        {
            _texture = texture;
            _frameWidth = frameWidth;
            Type = type;
            Force = force;
            Flip = flip;
            _activationDuration = duration;

            // Set proper frame count for each spring type
            int frameCount = (Type == SpringType.Horizontal) ? 5 : 6;

            Animator = new SpriteAnimator(texture, frameWidth, frameCount, 0.05f);
        }

        public void Activate()
        {
            _isActivated = true;
            _activationTimer = 0f;
            Animator.ResetAnimation();
        }

        public void Update(GameTime gameTime)
        {
            if (_isActivated)
            {
                Animator.Update(gameTime);
                _activationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_activationTimer >= _activationDuration)
                {
                    _isActivated = false;
                    Animator.ResetAnimation(); 
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            SpriteEffects flipEffect = (Flip && Type == SpringType.Horizontal)
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            Animator.Draw(spriteBatch, Position, flipEffect);
        }

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Animator.FrameWidth, Animator.FrameHeight);
    }
}
