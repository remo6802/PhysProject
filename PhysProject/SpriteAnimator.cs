using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysProject
{
    public class SpriteAnimator
    {
        private readonly Texture2D _texture;
        private readonly int _frameWidth;
        private readonly int _frameCount;
        private readonly float _frameTime;
        private float _timer = 0f;
        private int _currentFrame = 0;
        private bool _hasCompletedCycle = false;

        private readonly int _loopStart;
        private readonly int _loopEnd;

        public bool HasCompletedCycle => _hasCompletedCycle;
        public int FrameWidth => _frameWidth;
        public int FrameHeight => _texture.Height;

        public SpriteAnimator(Texture2D texture, int frameWidth, int frameCount, float frameTime, int loopStart = 0, int loopEnd = -1)
        {
            _texture = texture;
            _frameWidth = frameWidth;
            _frameCount = frameCount;
            _frameTime = frameTime;
            _loopStart = loopStart;
            _loopEnd = (loopEnd == -1) ? frameCount - 1 : loopEnd;
        }

        public void ResetAnimation()
        {
            _currentFrame = 0;
            _timer = 0;
            _hasCompletedCycle = false;
        }

        public void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer >= _frameTime)
            {
                _currentFrame++;
                if (_currentFrame > _loopEnd)
                {
                    _currentFrame = _loopStart;
                    _hasCompletedCycle = true;
                }
                else
                {
                    _hasCompletedCycle = false;
                }
                _timer = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects effect)
        {
            Rectangle source = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, FrameHeight);
            spriteBatch.Draw(_texture, position, source, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);
        }
    }
}
