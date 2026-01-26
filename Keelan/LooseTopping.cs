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
    public class LooseTopping : Entity
    {
        public string Name; // "Pepperoni", "Cheese"
        private Texture2D _texture;

        public LooseTopping(string name, Texture2D texture, Vector2 position)
        {
            Name = name;
            _texture = texture;
            // Center the sprite on the position
            Position = new Vector2(position.X - texture.Width / 2, position.Y - texture.Height / 2);
        }

        public override Rectangle Bounds =>
            new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);

        public override void Draw(SpriteBatch batch)
        {
            batch.Draw(_texture, Position, Color.White);
        }
    }
}
