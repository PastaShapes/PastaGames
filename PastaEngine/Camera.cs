using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastaEngine
{
    public class Camera
    {
        public Vector2 Position;
        public float Zoom { get; set; } = 5.0f;
        public float Rotation { get; set; } = 0f;

        // The World Boundaries (e.g., 0, 0, 2000, 2000)
        // If null, the camera has no limits.
        public Rectangle? Limits { get; set; }

        public void LookAt(Vector2 position)
        {
            Position = position;
        }

        // Overload for individual coordinates if needed
        public void LookAt(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        // This matrix is the "lens" we look through
        public Matrix GetTransform(GraphicsDevice graphicsDevice)
        {

            int screenWidth = graphicsDevice.Viewport.Width;
            int screenHeight = graphicsDevice.Viewport.Height;

            // 1.Calculate the target position(centered)
            float x = Position.X;
            float y = Position.Y;


            // 2. CLAMP LOGIC: Stop the camera if it hits the edge
            if (Limits.HasValue)
            {
                float viewWidth = screenWidth / Zoom;
                float viewHeight = screenHeight / Zoom;

                // Calculate the edges
                float minX = Limits.Value.X + viewWidth / 2;
                float maxX = Limits.Value.Width - viewWidth / 2;
                float minY = Limits.Value.Y + viewHeight / 2;
                float maxY = Limits.Value.Height - viewHeight / 2;

                // --- THE FIX ---
                // If Min > Max, it means the Room is smaller than the Screen.
                // In that case, ignore the player and force the camera to the Room Center.
                if (minX > maxX) x = Limits.Value.X + Limits.Value.Width / 2f;
                else x = MathHelper.Clamp(x, minX, maxX);

                if (minY > maxY) y = Limits.Value.Y + Limits.Value.Height / 2f;
                else y = MathHelper.Clamp(y, minY, maxY);
            }

            // 1. Move to position
            // 2. Scale (Zoom)
            // 3. Center on screen (width/2, height/2)
            return 
                Matrix.CreateTranslation(new Vector3(-x, -y, 0)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(screenWidth / 2, screenHeight / 2, 0));
            }

        public Vector2 ScreenToWorld(Vector2 screenPosition, GraphicsDevice graphics)
        {
            // Invert the Matrix: Turn "World -> Screen" math into "Screen -> World" math
            Matrix transform = GetTransform(graphics);
            Matrix inverse = Matrix.Invert(transform);

            // Apply the inverse to the mouse position
            return Vector2.Transform(screenPosition, inverse);
        }
    }
}
