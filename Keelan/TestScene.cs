using Keelan;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PastaEngine; // Using your engine!

public class TestScene : Scene
{
    private Vector2 _position = new Vector2(100, 100);
    Player _player;
    DialogueBox _dialogue = new DialogueBox();
    Speaker _playerSpeaker;
    Dictionary<string, List<DialogueLine>> _script;
    private float _gameZoom = 2.0f;

    public override void Initialize()
    {
        base.Initialize();
        _player = new Player() { Position = new Vector2(100, 100) };
        _dialogue = new DialogueBox();

        // Define our speaker (you would load the texture in LoadContent)
        _playerSpeaker = new Speaker
        {
            Name = "Hero",
            Pitch = 0.5f,
            Portrait = EngineCore.Pixel
        };

        var hero = new Speaker { Name = "Hero", Pitch = 0.5f }; // Add Portrait if you have one
        var wizard = new Speaker { Name = "Wizard", Pitch = -0.2f };

        _dialogue.RegisterSpeaker(hero);
        _dialogue.RegisterSpeaker(wizard);

        // 3. Load the script file
        _script = DialogueLoader.LoadFromFile("script.txt");

        // Set the background color (GameMaker style)
        BackgroundColor = Color.DarkGray;

        // 1. Set the Map Borders (Example: The world is 1000x1000 pixels big)
        // The camera will now STOP when you reach the edge of this rectangle.
        SceneCamera.Limits = new Rectangle(0, 0, 1000, 1000);

        // 2. Set the Zoom (Make pixels look big!)
        //SceneCamera.Zoom = 3.0f;

        // Add some "Walls" to the scene list
        // Eventually, we will load these from a file instead of typing them!
        Walls.Add(new Rectangle(300, 100, 50, 200)); // A vertical wall
        Walls.Add(new Rectangle(100, 300, 400, 50)); // A horizontal wall

        _player.SetColliders(Walls);

        AddObject(_player);
        AddObject(_dialogue);
    }

    protected override void HandleLevelObject(TiledObject obj)
    {
        switch (obj.Class)
        {
            case "PlayerStart":
                // TestScene knows about '_player', so this works perfectly here!
                _player.Position = new Vector2(obj.X, obj.Y);
                break;

            case "DialogueTrigger":
                // Handle your events...
                break;

            case "Skeleton":
                // Create enemy...
                break;
        }
    }

    public override void LoadContent()
    {
        base.LoadContent();
        //SpriteFont font = Content.Load<SpriteFont>("File");
        //_dialogue.Initialize(font);
        //_player.LoadContent(Content);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        // 1. Update Player (Pass in the walls so it knows what to hit)
        //_player.Update(gameTime, Walls);

        SceneCamera.Zoom = DisplayManager.Scale * _gameZoom;

        var kState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        if (kState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.OemPlus)) _gameZoom += 0.01f;
        if (kState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.OemMinus)) _gameZoom -= 0.01f;

        // Clamp it so you don't flip the universe inside out
        if (_gameZoom < 0.1f) _gameZoom = 0.1f;

        if (SceneCamera.Limits.HasValue)
        {
            Rectangle map = SceneCamera.Limits.Value;

            // MathHelper.Clamp(Value, Min, Max)
            // Note: We subtract _player.Size (e.g. 40) from the Max so they don't stick halfway out
            float clampedX = MathHelper.Clamp(_player.Position.X, map.X, map.Right - _player.Size);
            float clampedY = MathHelper.Clamp(_player.Position.Y, map.Y, map.Bottom - _player.Size);

            _player.Position = new Vector2(clampedX, clampedY);
        }

        SceneCamera.Position = _player.Position + new Vector2(_player.Size / 2, _player.Size / 2);

        //_dialogue.Update(gameTime);

        // Test Trigger: Press T to talk
        if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.T) && !_dialogue.IsActive)
        {
            // Note the syntax: [pause:1.0] inside the string!
            //_dialogue.Show("Hello there! [pause:0.5] I am speaking slowly... [speed:0.2] AND NOW SLOWLY.", _playerSpeaker);
            if (_script.ContainsKey("INTRO_MEETING"))
            {
                _dialogue.StartConversation(_script["INTRO_MEETING"]);
            }
        }

        // 2. Make Camera follow the player
        // We use Linear Interpolation (Lerp) for that smooth "delayed" follow effect
        //SceneCamera.Position = Vector2.Lerp(SceneCamera.Position, _player.Position, 0.1f);
        SceneCamera.Position = _player.Position + new Vector2(_player.Size / 2, _player.Size / 2);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        
        spriteBatch.Begin(transformMatrix: SceneCamera.GetTransform(EngineCore.Graphics), samplerState: SamplerState.PointClamp);

        // Draw Walls
        foreach (var wall in Walls)
        {
            spriteBatch.Draw(EngineCore.Pixel, wall, Color.Black);
        }

        // DEBUG: Draw the Map Limits so we can see them!
        if (SceneCamera.Limits.HasValue)
        {
            Rectangle r = SceneCamera.Limits.Value;

            // Draw Top Line
            spriteBatch.Draw(EngineCore.Pixel, new Rectangle(r.X, r.Y, r.Width, 2), Color.Red);
            // Draw Bottom Line
            spriteBatch.Draw(EngineCore.Pixel, new Rectangle(r.X, r.Bottom, r.Width, 2), Color.Red);
            // Draw Left Line
            spriteBatch.Draw(EngineCore.Pixel, new Rectangle(r.X, r.Y, 2, r.Height), Color.Red);
            // Draw Right Line
            spriteBatch.Draw(EngineCore.Pixel, new Rectangle(r.Right, r.Y, 2, r.Height), Color.Red);
        }

        // Draw Player
        //_player.Draw(spriteBatch);

        spriteBatch.End();

        base.Draw(spriteBatch);
    }
}