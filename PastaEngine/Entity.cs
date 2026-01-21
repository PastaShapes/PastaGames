using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastaEngine
{
    public abstract class Entity
    {
        // World Position
        public Vector2 Position;

        // Depth Sorting (0 = Front, 1 = Back) - useful for top-down games
        public float Depth = 0.5f;

        // Flags to control the object state
        public bool IsActive = true;  // If false, Update() is skipped
        public bool IsVisible = true; // If false, Draw() is skipped

        // -- VIRTUAL METHODS (Override these in your specific classes) --

        // Called immediately when added to the scene
        public virtual void Initialize() { }

        // Called to load textures/audio
        public virtual void LoadContent(ContentManager content) { }

        // Called every frame (Game Logic)
        public virtual void Update(GameTime gameTime) { }

        // Called every frame (Rendering)
        public virtual void Draw(SpriteBatch spriteBatch) { }

        // NEW: GUI Draw (Screen Space - always fixed on screen)
        // Like GameMaker's "Draw GUI" event
        public virtual void DrawUI(SpriteBatch spriteBatch) { }
    }
}
