using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastaEngine
{
    public static class DisplayManager
    {
        // The resolution you "Designed" the game for (e.g. 640x360)
        public static readonly float TargetHeight = 360f;

        public static float Scale { get; private set; } = 1f;

        public static void Update(GraphicsDevice graphics)
        {
            // Calculate how much bigger the window is compared to 360p
            // We only care about Height to keep the aspect ratio correct (Fit Height)
            Scale = graphics.Viewport.Height / TargetHeight;

            // Optional: Clamp scale so it doesn't get too small (e.g. never below 1x)
            if (Scale < 1f) Scale = 1f;
        }

        // Returns a Matrix to scale the UI
        public static Matrix GetUIMatrix()
        {
            return Matrix.CreateScale(new Vector3(Scale, Scale, 1));
        }

        public static float GetVirtualWidth(GraphicsDevice graphics)
        {
            return graphics.Viewport.Width / Scale;
        }

        public static float GetVirtualHeight(GraphicsDevice graphics)
        {
            return graphics.Viewport.Height / Scale;
        }
    }
}
