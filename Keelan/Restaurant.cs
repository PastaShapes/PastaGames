using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PastaEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keelan
{
    public class Restaurant : GameplayScene
    {
        // Textures
        private Texture2D _diningLayer;  // Walls/Booths (Player stands in front)
        private Texture2D _kitchenLayer; // Kitchen counters (Player stands in front

        // Region Logic
        private Region _kitchenRegion;
        private float _kitchenAlpha = 0f;
        private const float FadeSpeed = 3.0f;

        public override void Initialize()
        {
            // 1. Setup Player, UI, Camera (from Parent)
            base.Initialize();

            // 2. Load your specific Tiled map
            // Make sure the .tmj file is in your Content folder and set to "Copy if Newer"
            LoadLevelFromFile("restaurant.tmj");

            _diningLayer = Content.Load<Texture2D>("Backgrounds/restaurant layer 3");
            _kitchenLayer = Content.Load<Texture2D>("Backgrounds/restaurant layer 4");

            foreach (var r in Regions)
            {
                if (r.Name == "Kitchen")
                {
                    _kitchenRegion = r;
                    break;
                }
            }

            MidgroundTexture = null;
            ForegroundTexture = null;

            // 3. Set Room Specifics (Zoom, Limits)
            SceneCamera.Zoom = 3.0f; // Zoom in for indoors
            SceneCamera.Limits = new Microsoft.Xna.Framework.Rectangle(0, 0, 320, 180); // Match image size
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. Calculate Target Alpha (0 = Dining, 1 = Kitchen)
            float targetAlpha = 0.0f;
            if (_player != null && _kitchenRegion != null)
            {
                if (_kitchenRegion.Bounds.Contains(_player.Bounds.Center))
                    targetAlpha = 1.0f;
            }

            // 2. Smooth Fade
            if (_kitchenAlpha < targetAlpha)
            {
                _kitchenAlpha += FadeSpeed * dt;
                if (_kitchenAlpha > targetAlpha) _kitchenAlpha = targetAlpha;
            }
            else if (_kitchenAlpha > targetAlpha)
            {
                _kitchenAlpha -= FadeSpeed * dt;
                if (_kitchenAlpha < targetAlpha) _kitchenAlpha = targetAlpha;
            }

            // 1. Get the position the camera WANTS to be (Player Center)
            // (We assume base.Update set this to the player's position)
            Vector2 standardPos = SceneCamera.Position;

            // 2. Define the "Kitchen View" (Top-Left of the room)
            // Since the camera centers on its target:
            // To see (0,0) at the top-left, we must center on (HalfWidth, HalfHeight).
            // Assuming 320x180 resolution:
            Vector2 kitchenFocus = new Vector2(160, 90);

            // 3. Blend them based on how "inside" the kitchen we are
            // If _kitchenAlpha is 0 (Dining), we use standardPos.
            // If _kitchenAlpha is 1 (Kitchen), we use kitchenFocus.
            // Vector2.Lerp does the math for us!
            Vector2 finalPos = Vector2.Lerp(standardPos, kitchenFocus, _kitchenAlpha);

            // 4. Apply the override
            SceneCamera.LookAt(finalPos);
        }

        // --- DRAWING OVERRIDES ---

        protected override void DrawMidground(SpriteBatch spriteBatch)
        {
            float diningOpacity = 1.0f - _kitchenAlpha;
            float kitchenOpacity = _kitchenAlpha;

            // Draw Dining Layer (Behind Player)
            if (diningOpacity > 0 && _diningLayer != null)
                spriteBatch.Draw(_diningLayer, Vector2.Zero, Color.White * diningOpacity);

            // Draw Kitchen Layer (Behind Player)
            if (kitchenOpacity > 0 && _kitchenLayer != null)
                spriteBatch.Draw(_kitchenLayer, Vector2.Zero, Color.White * kitchenOpacity);
        }

        // If you still have "Above Player" stuff (like lights), put them here:
        protected override void DrawForeground(SpriteBatch spriteBatch)
        {
            // Example: If you had a separate image for lights
            // spriteBatch.Draw(_lightsTexture, Vector2.Zero, Color.White);
        }
    }
}
