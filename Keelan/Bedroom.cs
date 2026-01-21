using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keelan
{
    public class Bedroom : GameplayScene
    {
        public override void Initialize()
        {
            // 1. Setup Player, UI, Camera (from Parent)
            base.Initialize();

            // 2. Load your specific Tiled map
            // Make sure the .tmj file is in your Content folder and set to "Copy if Newer"
            LoadLevelFromFile("keelan room.tmj");

            // 3. Set Room Specifics (Zoom, Limits)
            SceneCamera.Zoom = 3.0f; // Zoom in for indoors
            SceneCamera.Limits = new Microsoft.Xna.Framework.Rectangle(0, 0, 320, 180); // Match image size
        }
    }
}
