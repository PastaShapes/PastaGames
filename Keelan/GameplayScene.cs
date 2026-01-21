using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PastaEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keelan
{
    // Inherit from your base 'Scene' class
    public class GameplayScene : Scene
    {
        // 'protected' means Child scenes (like Level1) can access these
        protected Player _player;
        protected DialogueBox _dialogueUI;

        protected Rectangle _roomBounds;

        protected float _gameZoom = 2.0f;

        protected Dictionary<string, List<DialogueLine>> _script;

        public override void Initialize()
        {
            // 1. Setup the Camera Defaults
            SceneCamera.Zoom = 3.0f; // Stardew Style Zoom
            // (You can also set SceneCamera.Limits here if you want)

            // 2. Create the Common Objects
            _player = new Player() { Position = new Vector2(100, 80) };

            _player.SetColliders(Walls);

            _dialogueUI = new DialogueBox();

            // 3. Add them to the Scene Manager AUTOMATICALLY
            // (The Scene class will now handle their Update/Draw loops)
            AddObject(_player);
            AddObject(_dialogueUI);

            _script = DialogueLoader.LoadFromFile("script.txt");

            // 4. Initialize the basics
            base.Initialize();
        }

        public override void LoadContent()
        {
            // Load the UI Font for the dialogue box
            // We do it here so every level automatically has working UI
            SpriteFont font = Content.Load<SpriteFont>("File");
            if (font != null) _dialogueUI.Initialize();

            // Helper to load everything else
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var mouseState = Mouse.GetState();
            Vector2 mouseScreen = new Vector2(mouseState.X, mouseState.Y);
            Vector2 mouseWorld = SceneCamera.ScreenToWorld(mouseScreen, EngineCore.Graphics);

            // 1. Define Max Reach (e.g. 64 pixels = 4 tiles)
            float maxDistance = 32.0f;

            // Calculate Player Center once
            Vector2 playerCenter = _player.Position + new Vector2(_player.Size / 2, _player.Size / 2);

            // 1. Sync Zoom
            // If you want the room to fill the window, multiply by Scale.
            // If you want a fixed retro look, just use _gameZoom.
            SceneCamera.Zoom = DisplayManager.Scale * _gameZoom;

            bool isHoveringInteractable = false;

            if (!_dialogueUI.IsActive)
            {
                foreach (var entity in _entities)
                {
                    if (entity is Interactable item)
                    {
                        if (item.Bounds.Contains(mouseWorld))
                        {
                            float distance = GetDistanceToRect(playerCenter, item.Bounds);

                            if (distance <= maxDistance)
                            {
                                isHoveringInteractable = true;
                                break;
                            }
                        }
                    }
                }
            }

            // Apply the cursor change
            if (isHoveringInteractable)
            {
                Mouse.SetCursor(MouseCursor.Hand);
            }
            else
            {
                Mouse.SetCursor(MouseCursor.Arrow);
            }

            if (_player != null)
            {
                // 2. Just tell the camera "Look at the Player"
                SceneCamera.Position = _player.Position + new Vector2(_player.Size / 2, _player.Size / 2);

                // 3. Always pass the room bounds. 
                // The Camera class now smartly decides whether to Clamp (Big Room) or Center (Small Room).
                SceneCamera.Limits = _roomBounds;
            }

            // --- NEW: HANDLE RIGHT CLICK ---

            // Check if Right Button was just pressed
            // (You might want a helper 'Input' class for 'IsJustPressed', but raw check works for now)
            if ((mouseState.RightButton == ButtonState.Pressed || mouseState.LeftButton == ButtonState.Pressed) && !_dialogueUI.IsActive)
            {
                foreach (var entity in _entities)
                {
                    if (entity is Interactable item)
                    {
                        if (item.Bounds.Contains(mouseWorld))
                        {
                            float distance = GetDistanceToRect(playerCenter, item.Bounds);

                            // Check distance
                            if (distance <= maxDistance)
                            {
                                // Distance is good! Show text.
                                string key = "DESC_" + item.GetMessage().ToUpper();

                                if (_script != null && _script.ContainsKey(key))
                                    _dialogueUI.ShowText(_script[key][0].Text);
                                else
                                    _dialogueUI.ShowText("It's a " + item.GetMessage().ToLower() + ".");
                            }

                            break;
                        }
                    }
                }
            }
        }

        private float GetDistanceToRect(Vector2 point, Rectangle rect)
        {
            // Find the X coordinate on the rect closest to the point
            float closestX = MathHelper.Clamp(point.X, rect.Left, rect.Right);

            // Find the Y coordinate on the rect closest to the point
            float closestY = MathHelper.Clamp(point.Y, rect.Top, rect.Bottom);

            // Calculate distance between the original point and that closest spot
            Vector2 closestPoint = new Vector2(closestX, closestY);
            return Vector2.Distance(point, closestPoint);
        }

        public new void LoadLevelFromFile(string path)
        {
            // Run the base loader to get walls/objects
            base.LoadLevelFromFile(path);

            // Save the room size!
            if (BackgroundTexture != null)
            {
                _roomBounds = new Rectangle(0, 0, BackgroundTexture.Width, BackgroundTexture.Height);
            }
        }

        protected override void HandleLevelObject(TiledObject obj)
        {
            switch (obj.Class)
            {
                case "PlayerStart":
                    // 1. Shift Left by half width (to center horizontally)
                    float centeredX = obj.X - (_player.Size / 2f);

                    // 2. Shift Up by full height (so feet sit ON the point)
                    float bottomY = obj.Y - _player.Size;

                    _player.Position = new Vector2(centeredX, bottomY);
                    break;

                case "Label":
                case "Bed":  // Treat Bed/PC as labels for now
                case "PC":
                    // Create the interaction zone
                    var zone = new Rectangle((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height);

                    // Use the "Name" field from Tiled as the message ID (e.g. "Shower", "Mess")
                    AddObject(new Interactable(zone, obj.Name));
                    break;

                case "Door":
                    // Logic for door transitions (add later)
                    break;
            }
        }
    }
}
