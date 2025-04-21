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

        public SpriteAnimator(Texture2D texture, int frameWidth, int frameCount, float frameTime)
        {
            _texture = texture;
            _frameWidth = frameWidth;
            _frameHeight = texture.Height;
            _frameCount = frameCount;
            _frameTime = frameTime;
            _timer = 0f;
            _currentFrame = 0;
        }

        public void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer >= _frameTime)
            {
                _currentFrame = (_currentFrame + 1) % _frameCount;
                _timer = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            Rectangle source = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            spriteBatch.Draw(_texture, position, source, Color.White);
        }
    }
}
