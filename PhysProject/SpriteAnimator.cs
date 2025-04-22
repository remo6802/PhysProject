using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysProject
{
    public class SpriteAnimator
    {
        private Texture2D _texture;
        private int _frameWidth;
        private int _frameHeight;
        private int _frameCount;
        private float _frameTime;
        private float _timer;
        private int _currentFrame;

        private int _loopStart = 0;
        private int _loopEnd = -1;

        public bool HasCompletedCycle { get; private set; }

        public int FrameWidth => _frameWidth;
        public int FrameHeight => _frameHeight;

        public SpriteAnimator(Texture2D texture, int frameWidth, int frameCount, float frameTime)
        {
            _texture = texture;
            _frameWidth = frameWidth;
            _frameCount = frameCount;
            _frameHeight = texture.Height;
            _frameTime = frameTime;
            _currentFrame = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (_frameCount <= 1) return;

            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer >= _frameTime)
            {
                _currentFrame++;
                int effectiveEnd = (_loopEnd >= 0) ? _loopEnd + 1 : _frameCount;

                if (_currentFrame >= effectiveEnd)
                {
                    _currentFrame = (_loopStart >= 0) ? _loopStart : 0;
                    HasCompletedCycle = true;
                }
                else
                {
                    HasCompletedCycle = false;
                }

                _timer = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects effects = SpriteEffects.None)
        {
            Rectangle sourceRect = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            spriteBatch.Draw(_texture, position, sourceRect, Color.White, 0f, Vector2.Zero, 1f, effects, 0f);
        }

        public void ResetAnimation()
        {
            _currentFrame = _loopStart;
            _timer = 0f;
            HasCompletedCycle = false;
        }

        public void SetLoopRange(int startFrame, int endFrame)
        {
            _loopStart = startFrame;
            _loopEnd = endFrame;
        }

        public void ClearLoopRange()
        {
            _loopStart = 0;
            _loopEnd = -1;
        }
    }
}
