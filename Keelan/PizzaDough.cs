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
    public enum PizzaSize { Small, Medium, Large }

    public class PlacedTopping
    {
        public string Type;        // "Pepperoni", "Cheese"
        public Vector2 LocalPos;   // Offset from the top-left of the pizza
        public Texture2D Texture;
        public Rectangle Bounds;   // For right-clicking later
    }

    public class PizzaDough : Entity
    {
        public PizzaSize SizeType;
        private Texture2D _baseTexture;

        private List<PlacedTopping> _toppings = new List<PlacedTopping>();

        public PizzaDough(PizzaSize size, Texture2D texture, Vector2 startPos)
        {
            SizeType = size;
            _baseTexture = texture;
            Position = startPos;
            //_width = texture.Width;
            //_height = texture.Height;
        }

        public override Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, _baseTexture.Width, _baseTexture.Height);

        // --- NEW: Add Topping Logic ---
        public void AddTopping(string type, Texture2D texture, Vector2 worldMousePos)
        {
            // Calculate position relative to the pizza so it sticks when moved
            Vector2 relativePos = worldMousePos - Position;

            // Center the texture on the mouse (assuming 16x16 toppings)
            relativePos.X -= texture.Width / 2f;
            relativePos.Y -= texture.Height / 2f;

            _toppings.Add(new PlacedTopping
            {
                Type = type,
                Texture = texture,
                LocalPos = relativePos,
                Bounds = new Rectangle(0, 0, texture.Width, texture.Height)
            });
        }

        // --- NEW: Remove Topping Logic (Right Click) ---
        public bool TryRemoveTopping(Vector2 worldMousePos)
        {
            // Iterate backwards so we pick up the "top" one first
            for (int i = _toppings.Count - 1; i >= 0; i--)
            {
                var t = _toppings[i];

                // Calculate where this topping is in the world right now
                Vector2 toppingWorldPos = Position + t.LocalPos;
                Rectangle tRect = new Rectangle((int)toppingWorldPos.X, (int)toppingWorldPos.Y, t.Bounds.Width, t.Bounds.Height);

                if (tRect.Contains(worldMousePos))
                {
                    _toppings.RemoveAt(i);
                    return true; // Successfully removed
                }
            }
            return false;
        }

        // Change return type from 'bool' to 'string' (returns null if nothing found)
        public string TryPickUpTopping(Vector2 worldMousePos)
        {
            for (int i = _toppings.Count - 1; i >= 0; i--)
            {
                var t = _toppings[i];
                Vector2 toppingWorldPos = Position + t.LocalPos;
                Rectangle tRect = new Rectangle((int)toppingWorldPos.X, (int)toppingWorldPos.Y, t.Bounds.Width, t.Bounds.Height);

                if (tRect.Contains(worldMousePos))
                {
                    // Found one! Remove it and return its name.
                    string type = t.Type;
                    _toppings.RemoveAt(i);
                    return type;
                }
            }
            return null; // Nothing found
        }

        public override void Draw(SpriteBatch batch)
        {
            batch.Draw(_baseTexture, Position, Color.White);

            foreach (var t in _toppings)
            {
                batch.Draw(t.Texture, Position + t.LocalPos, Color.White);
            }
        }
    }
}
