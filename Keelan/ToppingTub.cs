using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PastaEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keelan
{
    public class ToppingTub : Entity
    {
        public string ToppingName;
        private Texture2D _texture; // The current look of the tub
        ContentManager _content;

        // We use this to track how full it is later
        public int AmountRemaining = 100;

        public ToppingTub(string name, Rectangle bounds, Texture2D texture)
        {
            ToppingName = name;
            // Use the position from Tiled
            Position = new Vector2(bounds.X, bounds.Y);
            _texture = texture;
        }

        // Tubs usually don't move, so Bounds is just Position + Texture Size
        public override Rectangle Bounds =>
            new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);

        public override void LoadContent(ContentManager content)
        {
            _content = content;
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_texture != null)
            {
                batch.Draw(_texture, Position, Color.White);
            }
        }

        // Call this later to change the graphic!
        public void UpdateTexture(Texture2D newTexture)
        {
            _texture = newTexture;
        }

        public void TakeScoop()
        {
            AmountRemaining -= 10;
            if (AmountRemaining < 50)
            {
                // Swap to the half-empty sprite
                _texture = _content.Load<Texture2D>("Pizza/Tub_" + ToppingName + "_Half");
            }
        }
    }
}
