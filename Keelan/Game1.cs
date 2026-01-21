using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PastaEngine;

namespace Keelan
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Add inside Initialize()
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize; // Hook up the event
            DisplayManager.Update(GraphicsDevice);

            base.Initialize();
        }

        private void OnResize(object sender, System.EventArgs e)
        {
            DisplayManager.Update(GraphicsDevice);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            EngineCore.Initialize(GraphicsDevice);

            SceneManager.Initialize(Content);

            //SceneManager.LoadScene(new TestScene());
            SceneManager.LoadScene(new Bedroom());
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            SceneManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            //GraphicsDevice.SetRenderTarget(ResolutionHelper.GameScreen);
            GraphicsDevice.Clear(SceneManager.CurrentScene?.BackgroundColor ?? Color.Black);

            SceneManager.Draw(_spriteBatch);

            //GraphicsDevice.SetRenderTarget(null); // 'null' means the Backbuffer (Actual Screen)
            //GraphicsDevice.Clear(Color.Black); // Clear borders to black

            //ResolutionHelper.DrawGameToScreen(_spriteBatch);

            base.Draw(gameTime);
        }
    }
}
