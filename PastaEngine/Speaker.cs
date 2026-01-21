using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastaEngine
{
    public class Speaker
    {
        public string Name;
        public Texture2D Portrait; // The face
        public SoundEffect TalkSound; // The "beep"
        public float Pitch = 0.0f; // High or low voice
                                   // NEW: The font file name (default to your standard font)
        public string FontFile = "File";

        // NEW: The actual loaded font
        public SpriteFont Font;
    }
}
