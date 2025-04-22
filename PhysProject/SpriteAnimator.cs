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

        private float _timer;
        private int _currentFrame;

        private int? _loopStart = null;
        private int? _loopEnd = null;

        public int FrameWidth => _frameWidth;
        public int FrameHeight => _texture.Height;
        public int FrameCount => _frameCount;

        public SpriteAnimator(Texture2D texture, int frameWidth, int frameCount, float frameTime)
        {
            _texture = texture;
            _frameWidth = frameWidth;
            _frameCount = frameCount;
            _frameTime = frameTime;
            _timer = 0f;
            _currentFrame = 0;
        }

        public void SetLoopRange(int start, int end)
        {
            _loopStart = start;
            _loopEnd = end;
        }

        public void ClearLoopRange()
        {
            _loopStart = null;
            _loopEnd = null;
        }

        public void ResetAnimation()
        {
            _currentFrame = 0;
            _timer = 0f;
        }

        public bool HasCompletedCycle => _currentFrame == _frameCount - 1;

        public void Update(GameTime gameTime)
        {
            if (_frameCount <= 1) return;

            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer >= _frameTime)
            {
                _currentFrame++;
                if (_loopStart.HasValue && _loopEnd.HasValue)
                {
                    if (_currentFrame > _loopEnd.Value)
                        _currentFrame = _loopStart.Value;
                }
                else if (_currentFrame >= _frameCount)
                {
                    _currentFrame = 0;
                }
                _timer = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects effects)
        {
            Rectangle source = new Rectangle(_frameWidth * _currentFrame, 0, _frameWidth, _texture.Height);
            spriteBatch.Draw(_texture, position, source, Color.White, 0f, Vector2.Zero, 1f, effects, 0f);
        }
    }
}
