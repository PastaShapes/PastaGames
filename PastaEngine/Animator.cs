using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastaEngine
{
    // A simple container for data about one specific animation (e.g. "WalkDown")
    public class Animation
    {
        public Texture2D Texture;
        public int FrameCount;
        public float FrameSpeed; // Seconds per frame
        public bool IsLooping;

        public int FrameWidth => Texture.Width / FrameCount;
        public int FrameHeight => Texture.Height;
    }

    public class Animator
    {
        private Dictionary<string, Animation> _animations = new Dictionary<string, Animation>();
        private Animation _currentAnimation;
        private string _currentAnimationName;

        // State
        private int _currentFrame;
        private float _timer;
        private bool _isPlaying;

        public void AddAnimation(string name, Texture2D texture, int frameCount, float frameSpeed = 0.1f, bool loop = true)
        {
            _animations[name] = new Animation
            {
                Texture = texture,
                FrameCount = frameCount,
                FrameSpeed = frameSpeed,
                IsLooping = loop
            };
        }

        public void Play(string name)
        {
            // If we are already playing this animation, do nothing (so we don't reset the frame)
            if (_currentAnimationName == name && _isPlaying) return;

            if (_animations.ContainsKey(name))
            {
                _currentAnimation = _animations[name];
                _currentAnimationName = name;
                _currentFrame = 0;
                _timer = 0;
                _isPlaying = true;
            }
        }

        public void Stop()
        {
            // Reset to the first frame (stationary) and stop the timer
            _currentFrame = 0;
            _isPlaying = false;
        }

        public void Update(GameTime gameTime)
        {
            if (!_isPlaying || _currentAnimation == null) return;

            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timer >= _currentAnimation.FrameSpeed)
            {
                _timer -= _currentAnimation.FrameSpeed;
                _currentFrame++;

                if (_currentFrame >= _currentAnimation.FrameCount)
                {
                    if (_currentAnimation.IsLooping)
                        _currentFrame = 0;
                    else
                        _currentFrame = _currentAnimation.FrameCount - 1; // Clamp at end
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
        {
            if (_currentAnimation == null) return;

            // Calculate which part of the sprite sheet to draw
            Rectangle sourceRect = new Rectangle(
                _currentFrame * _currentAnimation.FrameWidth, // Move X based on frame
                0,
                _currentAnimation.FrameWidth,
                _currentAnimation.FrameHeight
            );

            spriteBatch.Draw(_currentAnimation.Texture, position, sourceRect, color);
        }
    }
}
