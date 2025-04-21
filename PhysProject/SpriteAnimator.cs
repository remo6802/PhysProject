using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysProject
{
    public class SpriteAnimator
    {
        private Texture2D _texture;
        private int _frameWidth, _frameHeight;
        private int _frameCount;
        private float _frameTime;
        private float _timer;
        private int _currentFrame;
        
        public int FrameWidth => _frameWidth;
        public int FrameHeight => _frameHeight;
        public bool HasCompletedCycle { get; private set; }

        public SpriteAnimator(Texture2D texture, int frameWidth, int frameCount, float frameTime)
        {
            _texture = texture;
            _frameWidth = frameWidth;
            _frameHeight = texture.Height;
            _frameCount = frameCount;
            _frameTime = frameTime;
            ResetAnimation();
        }

        public void ResetAnimation()
        {
            _currentFrame = 0;
            _timer = 0f;
            HasCompletedCycle = false;
        }

        public void Update(GameTime gameTime)
        {
            if (_frameCount <= 1) return;

            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer >= _frameTime)
            {
                _currentFrame++;
                if (_currentFrame >= _frameCount)
                {
                    _currentFrame = 0;
                    HasCompletedCycle = true;
                }
                else
                {
                    HasCompletedCycle = false;
                }
                _timer = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            Rectangle source = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            spriteBatch.Draw(_texture, position, source, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects effects)
        {
            Rectangle source = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            spriteBatch.Draw(_texture, position, source, Color.White, 0f, Vector2.Zero, 1f, effects, 0f);
        }
    }
}