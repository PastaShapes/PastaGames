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
    public class DraggableItem : Entity
    {
        public string Name;
        public Vector2 OriginalPosition;
        public Texture2D Texture;
        public Rectangle Collider;

        public DraggableItem(string name, Texture2D tex, Vector2 pos, int width, int height)
        {
            Name = name;
            Texture = tex;
            Position = pos;
            OriginalPosition = pos;
            Collider = new Rectangle(0, 0, width, height); // Local size
        }

        public override Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Collider.Width, Collider.Height);

        public override void Draw(SpriteBatch batch)
        {
            batch.Draw(Texture, Bounds, Color.White);
        }
    }
}
