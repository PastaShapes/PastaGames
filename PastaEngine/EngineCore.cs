using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PastaEngine
{
    public static class EngineCore
    {
        // Global access to the GraphicsDevice (so you don't have to pass it everywhere)
        public static GraphicsDevice Graphics { get; private set; }

        // The magic 1x1 white pixel. Use this to draw lines, rectangles, and tint them.
        // No .png file required!
        public static Texture2D Pixel { get; private set; }

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            Graphics = graphicsDevice;

            // Generate the 1x1 white texture programmatically
            Pixel = new Texture2D(Graphics, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }
    }
}