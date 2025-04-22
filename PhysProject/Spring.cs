using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysProject
{
    public class Spring
    {
        public enum SpringType { Vertical, Horizontal }

        public Vector2 Position;
        public SpringType Type;
        public float Force;
        public SpriteAnimator Animator;
        public bool Flip;
        public bool IsActivated { get; private set; }
        public float SpringConstant { get; private set; }
        public float Compression { get; private set; } = 20f;

        public Spring(Texture2D texture, int frameWidth, SpringType type, float force, float animationSpeed, bool flip = false)
        {
            Type = type;
            Force = force;
            Flip = flip;

            int frameCount = texture.Width / frameWidth;
            Animator = new SpriteAnimator(texture, frameWidth, frameCount, animationSpeed);
        }

        public void Activate()
        {
            IsActivated = true;
            Animator.ResetAnimation();
        }

        public void Update(GameTime gameTime)
        {
            if (IsActivated)
            {
                Animator.Update(gameTime);

                if (Animator.HasCompletedCycle)
                {
                    IsActivated = false;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            SpriteEffects fx = Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Animator.Draw(spriteBatch, Position, fx);
        }

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Animator.FrameWidth, Animator.FrameHeight);
    }
}
