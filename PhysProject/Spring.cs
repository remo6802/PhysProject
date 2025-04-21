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
            Animator.ResetAnimation(); // We'll need to add this method to SpriteAnimator
        }

        public void Update(GameTime gameTime)
        {
            if (IsActivated)
            {
                Animator.Update(gameTime);
                
                // Deactivate after one full animation cycle
                if (Animator.HasCompletedCycle) // We'll need to add this property to SpriteAnimator
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