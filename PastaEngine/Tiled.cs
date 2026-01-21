using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastaEngine
{
    // These match the Tiled JSON structure exactly
    public class TiledMapData
    {
        public int Width;      // Map width in tiles
        public int Height;     // Map height in tiles
        public int TileWidth;  // e.g. 32
        public int TileHeight; // e.g. 32
        public List<TiledLayer> Layers;
    }

    public class TiledLayer
    {
        public string Name;
        public string Type;    // "objectgroup", "tilelayer", etc.
        public List<TiledObject> Objects;

        public string Image;   // e.g. "keelan room.png"
        public float Opacity;
        public float OffsetX;
        public float OffsetY;
    }

    public class TiledObject
    {
        public string Name;

        [JsonProperty("type")]
        public string Class; // This is the "Type" in Tiled
        public float X;
        public float Y;
        public float Width;
        public float Height;

        // Tiled stores custom properties (like ScriptID) in a list
        public List<TiledProperty> Properties;
    }

    public class TiledProperty
    {
        public string Name;
        public object Value; // Can be string, int, bool, etc.
    }
}
