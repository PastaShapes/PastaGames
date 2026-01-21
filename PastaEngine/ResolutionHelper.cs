using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastaEngine
{
    public static class ResolutionHelper
    {
        // The size you WANT the game to look like (e.g., 640x360)
        public static readonly int VirtualWidth = 640;
        public static readonly int VirtualHeight = 360;

        public static RenderTarget2D GameScreen { get; private set; }
        private static GraphicsDevice _graphicsDevice;
        private static Rectangle _destinationRectangle;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            // Create the "Virtual Canvas"
            GameScreen = new RenderTarget2D(graphicsDevice, VirtualWidth, VirtualHeight);

            // Calculate initial size
            UpdateDestinationRectangle();
        }

        public static void UpdateDestinationRectangle()
        {
            var screenSize = _graphicsDevice.PresentationParameters.Bounds;

            // Calculate the scaling factor (how much bigger is the window than 640x360?)
            float scaleX = (float)screenSize.Width / VirtualWidth;
            float scaleY = (float)screenSize.Height / VirtualHeight;
            float finalScale = MathHelper.Min(scaleX, scaleY); // Pick the smaller one to fit inside

            int newWidth = (int)(VirtualWidth * finalScale);
            int newHeight = (int)(VirtualHeight * finalScale);

            // Center it
            int posX = (screenSize.Width - newWidth) / 2;
            int posY = (screenSize.Height - newHeight) / 2;

            _destinationRectangle = new Rectangle(posX, posY, newWidth, newHeight);
        }

        public static void DrawGameToScreen(SpriteBatch spriteBatch)
        {
            // Draw the tiny GameScreen onto the full window, scaled up
            spriteBatch.Begin(samplerState: SamplerState.PointClamp); // PointClamp keeps pixels "crisp"
            spriteBatch.Draw(GameScreen, _destinationRectangle, Color.White);
            spriteBatch.End();
        }
    }
}
