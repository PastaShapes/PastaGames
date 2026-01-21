using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PastaEngine
{
    public static class SceneManager
    {
        private static Scene _currentScene;
        public static Scene CurrentScene => _currentScene;

        private static ContentManager _content;

        public static void Initialize(ContentManager content)
        {
            _content = content;
        }

        public static void LoadScene(Scene newScene)
        {
            // If a scene is already running, clean it up
            _currentScene?.Unload();

            // Switch and load the new one
            _currentScene = newScene;
            _currentScene.Load(_content);
            _currentScene.Initialize();
            _currentScene.LoadContent();
        }

        public static void Update(GameTime gameTime)
        {
            _currentScene?.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            _currentScene?.Draw(spriteBatch);
        }
    }
}