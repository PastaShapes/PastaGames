using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace PastaEngine
{
    public abstract class Scene
    {
        // The Master List of everything in this level
        protected List<Entity> _entities = new List<Entity>();
        private List<Entity> _pendingEntities = new List<Entity>();

        public ContentManager Content;
        public Camera SceneCamera = new Camera(); // <--- New Camera
        public Color BackgroundColor = Color.Black;//Color.CornflowerBlue; // <--- GameMaker style background

        // A simple list of rectangles that are "Solid"
        public List<Rectangle> Walls = new List<Rectangle>();

        public Texture2D BackgroundTexture;
        public Texture2D ForegroundTexture;

        public void AddObject(Entity obj)
        {
            _pendingEntities.Add(obj);

            // If the scene is already running, initialize the object immediately
            obj.Initialize();
            if (Content != null) obj.LoadContent(Content);
        }

        public virtual void Initialize() { }

        public virtual void LoadContent()
        {
            // 1. Load the active objects
            foreach (var e in _entities) e.LoadContent(Content);

            // 2. THE FIX: Load the pending objects too!
            foreach (var e in _pendingEntities) e.LoadContent(Content);
        }

        public void Load(ContentManager parentContent)
        {
            Content = new ContentManager(parentContent.ServiceProvider, parentContent.RootDirectory);
        }

        public virtual void Unload()
        {
            Content?.Unload();
        }

        public virtual void Update(GameTime gameTime)
        {
            // Add new objects safely
            if (_pendingEntities.Count > 0)
            {
                _entities.AddRange(_pendingEntities);
                _pendingEntities.Clear();
            }

            // Update everyone
            foreach (var e in _entities)
            {
                if (e.IsActive) e.Update(gameTime);
            }
        }

        // IMPORTANT: virtual, not abstract.
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // PASS 1: World
            spriteBatch.Begin(
                transformMatrix: SceneCamera.GetTransform(EngineCore.Graphics),
                samplerState: SamplerState.PointClamp
            );
            if (BackgroundTexture != null)
            {
                spriteBatch.Draw(BackgroundTexture, Vector2.Zero, Color.White);
            }
            foreach (var e in _entities) if (e.IsVisible) e.Draw(spriteBatch);
            if (ForegroundTexture != null)
                spriteBatch.Draw(ForegroundTexture, Vector2.Zero, Color.White);
            spriteBatch.End();

            // PASS 2: UI (We added this earlier!)
            spriteBatch.Begin(
                blendState:BlendState.AlphaBlend,
                samplerState: SamplerState.LinearClamp
            );
            foreach (var e in _entities) if (e.IsVisible) e.DrawUI(spriteBatch);
            spriteBatch.End();
        }

        public void LoadLevelFromFile(string filePath)
        {
            // 1. Read the text from the file
            string json = File.ReadAllText(filePath);

            // 2. Convert JSON text into our C# Data classes
            TiledMapData mapData = JsonConvert.DeserializeObject<TiledMapData>(json);

            // 3. Loop through layers
            foreach (var layer in mapData.Layers)
            {
                if (layer.Type == "imagelayer")
                {
                    string cleanName = System.IO.Path.GetFileNameWithoutExtension(layer.Image);
                    Texture2D tex = Content.Load<Texture2D>("Backgrounds/" + cleanName);

                    // CHECK: Is this the specific "Foreground" layer?
                    if (layer.Name == "Foreground")
                    {
                        ForegroundTexture = tex;
                    }
                    else
                    {
                        // If it's not named Foreground, assume it's the background
                        BackgroundTexture = tex;
                    }
                }

                // We only care about Object Layers for now
                else if (layer.Type == "objectgroup")
                {
                    if (layer.Objects == null) continue;

                    foreach (var obj in layer.Objects)
                    {
                        // 1. Handle "Universal" things the Engine knows about (like Walls)
                        if (obj.Class == "Wall")
                        {
                            Walls.Add(new Rectangle((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height));
                        }
                        else
                        {
                            // 2. If the Engine doesn't know what it is, pass it to the Game!
                            HandleLevelObject(obj);
                        }
                    }
                }
            }
        }

        protected virtual void HandleLevelObject(TiledObject obj) { }
    }
}