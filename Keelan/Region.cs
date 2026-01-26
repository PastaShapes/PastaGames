using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastaEngine
{
    public class Region
    {
        public string Name;
        public Rectangle Bounds;

        public Region(string name, Rectangle bounds)
        {
            Name = name;
            Bounds = bounds;
        }
    }
}
