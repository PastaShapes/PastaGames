using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PastaEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keelan
{
    public class Button : Entity
    {
        public string ActionName;
        public Rectangle Rect;

        public bool IsHovered = false;

        public Button(string name, Rectangle rect)
        {
            ActionName = name;
            Rect = rect;
        }

        public override Rectangle Bounds => Rect;

        public void OnClick()
        {
            System.Console.WriteLine("Clicked: " + ActionName);
            if (ActionName == "4") // Exit
            {
                // EngineCore.ChangeScene(new Restaurant()); 
            }
        }

        // Optional: Draw debug box if invisible
        public override void Draw(SpriteBatch batch) 
        {
            if (IsHovered)
            {
                // Draws a black layer at 50% opacity (dimming the area)
                batch.Draw(EngineCore.Pixel, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width + 1, Bounds.Height + 1), Color.Black * 0.5f);
            }
        }
    }
}
