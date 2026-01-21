using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastaEngine
{
    public class Interactable : Entity
    {
        private string _message;
        private Rectangle _triggerZone;

        public Rectangle Bounds => _triggerZone;

        public Interactable(Rectangle zone, string messageName)
        {
            _triggerZone = zone;
            _message = messageName; // e.g. "PC", "Bed"

            // It's invisible, but it still exists!
            IsVisible = false;
        }

        // We can check this from the Player or GameplayScene
        public bool CanInteract(Rectangle interactionSensor)
        {
            return _triggerZone.Intersects(interactionSensor);
        }

        public string GetMessage() => _message;
    }
}
