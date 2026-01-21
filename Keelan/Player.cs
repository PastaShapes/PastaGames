using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using PastaEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keelan
{
    public class Player : Entity
    {
        public Vector2 Position;
        public int Speed = 200;
        public int Size = 36;

        // Helper to get the rectangle for collision
        //public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Size, Size);

        private Animator _animator = new Animator();

        // To track direction when stopped (so we don't default to "Down" instantly)
        private string _lastDirection = "down";

        private List<Rectangle> _wallsToCheck;

        public void SetColliders(List<Rectangle> walls)
        {
            _wallsToCheck = walls;
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            // Load the 4 separate sheets
            Texture2D texDown = content.Load<Texture2D>("Player/keelan_down");
            Texture2D texUp = content.Load<Texture2D>("Player/keelan_up");
            Texture2D texLeft = content.Load<Texture2D>("Player/keelan_left");
            Texture2D texRight = content.Load<Texture2D>("Player/keelan_right");

            // Register them to the Animator
            // (Name, Texture, FrameCount, Speed)
            _animator.AddAnimation("down", texDown, 4, 0.15f);
            _animator.AddAnimation("up", texUp, 4, 0.15f);
            _animator.AddAnimation("left", texLeft, 4, 0.15f);
            _animator.AddAnimation("right", texRight, 4, 0.15f);

            // Start facing down
            _animator.Play("down");
            _animator.Stop();
        }

        public override void Update(GameTime gameTime)
        {
            if (_wallsToCheck == null) return;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 velocity = Vector2.Zero;
            KeyboardState kState = Keyboard.GetState();

            if (kState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W)) velocity.Y = -1;
            if (kState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S)) velocity.Y = 1;
            if (kState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A)) velocity.X = -1;
            if (kState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D)) velocity.X = 1;

            if (velocity != Vector2.Zero)
            {
                // Normalize so diagonal isn't faster
                velocity.Normalize();

                // Pick animation based on direction
                if (velocity.Y > 0) _lastDirection = "down";
                else if (velocity.Y < 0) _lastDirection = "up";
                else if (velocity.X > 0) _lastDirection = "right";
                else if (velocity.X < 0) _lastDirection = "left";

                // Play the animation for the direction we are facing
                _animator.Play(_lastDirection);
            }
            else
            {
                // If stopped, keep the last animation set, but force it to Frame 0
                _animator.Play(_lastDirection);
                _animator.Stop();
            }

            float speed = 1f;
            Vector2 moveAmount = velocity * speed;

            // --- HANDLE X AXIS ---
            Position.X += moveAmount.X;

            // We moved X. Now did we hit anything?
            foreach (var wall in _wallsToCheck)
            {
                if (Bounds.Intersects(wall))
                {
                    // Crash! Undo the X movement immediately.
                    Position.X -= moveAmount.X;
                    break;
                }
            }

            // --- HANDLE Y AXIS ---
            Position.Y += moveAmount.Y;

            // We moved Y. Now did we hit anything?
            foreach (var wall in _wallsToCheck)
            {
                if (Bounds.Intersects(wall))
                {
                    // Crash! Undo the Y movement immediately.
                    Position.Y -= moveAmount.Y;
                    break;
                }
            }

            // 4. Update the animator timer
            _animator.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // The Animator handles the source rectangles now!
            _animator.Draw(spriteBatch, Position, Color.White);
        }

        // 1. PHYSICS BOX (The "Feet")
        // Used for: Walls, Floor Triggers (stepping on a trap)
        public Rectangle Bounds
        {
            get
            {
                // Only the bottom 12 pixels are for collision
                int collisionHeight = 10;
                int yOffset = Size - collisionHeight;

                // Shrink width slightly so you don't get stuck on corners
                int widthPadding = 18;

                return new Rectangle(
                    (int)Position.X + widthPadding / 2,
                    (int)Position.Y + yOffset,
                    Size - widthPadding,
                    collisionHeight
                );
            }
        }

        // 2. INTERACTION BOX (The "Sensor")
        // Used for: Pressing 'Space' to talk/interact
        public Rectangle GetInteractionBounds()
        {
            // Project a box 20 pixels in front of the player
            int reach = 20;
            Rectangle sensor = Bounds; // Start at feet

            // Move the sensor based on direction
            switch (_lastDirection)
            {
                case "up": sensor.Y -= reach; break;
                case "down": sensor.Y += reach; break;
                case "left": sensor.X -= reach; break;
                case "right": sensor.X += reach; break;
            }

            return sensor;
        }

        // 3. HURTBOX (Full Body)
        // Used for: Touching enemies, getting shot
        public Rectangle Hurtbox => new Rectangle((int)Position.X, (int)Position.Y, Size, Size);
    }
}
