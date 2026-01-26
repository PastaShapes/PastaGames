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
    public class PizzaScene : GameplayScene
    {
        // Track the mouse for dragging
        private MouseState _currentMouse;
        private MouseState _prevMouse;
        private Vector2 _mouseWorldPos;

        // Draggable Logic
        private Entity _draggedItem = null;
        private Vector2 _dragOffset;

        // --- NEW: HAND STATE ---
        private string _heldToppingName = null; // If null, hand is empty
        private int _heldToppingCount = 0;
        private Texture2D _heldToppingTexture;  // The image following the mouse

        private Rectangle _toppingArea;

        // Game State
        private List<string> _currentToppings = new List<string>(); // "Cheese", "Pep", etc.

        public override void Initialize()
        {
            // Do NOT call base.Initialize() if it spawns a Player automatically.
            // If base.Initialize() just sets up the camera, call it.
            // Assuming we want a fresh start:

            _entities = new List<Entity>();
            SceneCamera = new Camera(); // Default camera
            SceneCamera.LookAt(160, 90); // Center on the 320x180 screen

            _dialogueUI = new DialogueBox();
            AddObject(_dialogueUI);

            // Load your specific minigame map
            LoadLevelFromFile("topping menu.tmj");

            // MANUALLY ADD PIZZA FOR TESTING
            Texture2D medPizzaTex = Content.Load<Texture2D>("Pizza/medium_pizza");

            // Spawn it in the middle of the "Topping space" roughly
            Vector2 spawnPos = new Vector2(_toppingArea.X + 20, _toppingArea.Y + 10);

            PizzaDough myPizza = new PizzaDough(PizzaSize.Medium, medPizzaTex, spawnPos);
            AddObject(myPizza);
        }

        public override void Update(GameTime gameTime)
        {
            if (_pendingEntities.Count > 0)
            {
                _entities.AddRange(_pendingEntities);
                _pendingEntities.Clear();
            }

            // --- 1. MOUSE CONVERSION ---
            _currentMouse = Mouse.GetState();

            // Calculate the Transform and then INVERT it
            Matrix transform = GetLetterboxMatrix();
            Matrix inverseTransform = Matrix.Invert(transform);

            // Convert raw mouse (Screen) to world mouse (320x180)
            Vector2 rawMouse = new Vector2(_currentMouse.X, _currentMouse.Y);
            _mouseWorldPos = Vector2.Transform(rawMouse, inverseTransform);

            // --- 2. LOGIC (Same as before) ---
            HandleInput(); // Now uses the corrected _mouseWorldPos

            foreach (var e in _entities) e.Update(gameTime);

            foreach (var e in _entities)
            {
                if (e is Button btn)
                {
                    // Check if the world-space mouse is inside the button rect
                    if (btn.Bounds.Contains(_mouseWorldPos))
                    {
                        btn.IsHovered = true;
                    }
                    else
                    {
                        btn.IsHovered = false;
                    }
                }
            }

            _prevMouse = _currentMouse;
        }

        // We don't want the standard Draw because we might have specific UI layering
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Use GetLetterboxMatrix() here!
            // SamplerState.PointClamp ensures the pixel art stays crisp even when scaled weirdly.
            spriteBatch.Begin(transformMatrix: GetLetterboxMatrix(), samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);

            // 1. Draw Background
            if (BackgroundTexture != null)
                spriteBatch.Draw(BackgroundTexture, Vector2.Zero, Color.White);

            // 2. Draw Tubs, Buttons, etc.
            foreach (var e in _entities) e.Draw(spriteBatch);

            // 3. Draw Dragged Item
            //if (_draggedItem != null) _draggedItem.Draw(spriteBatch);

            // DRAW CURSOR ITEM (If holding toppings)
            if (_heldToppingTexture != null)
            {
                // Draw texture centered on mouse
                Vector2 drawPos = _mouseWorldPos - new Vector2(_heldToppingTexture.Width / 2, _heldToppingTexture.Height / 2);
                spriteBatch.Draw(_heldToppingTexture, drawPos, Color.White * 0.8f); // Slightly transparent

                // Optional: Draw Count number
                // batch.DrawString(_font, _heldToppingCount.ToString(), drawPos, Color.White);
            }

            spriteBatch.End();
        }

        private Matrix GetLetterboxMatrix()
        {
            // 1. The Design Resolution (Your Tiled Map Size)
            float targetW = 320f;
            float targetH = 180f;

            // 2. The Real Window Size
            float screenW = EngineCore.Graphics.Viewport.Width;
            float screenH = EngineCore.Graphics.Viewport.Height;

            // 3. Calculate Scale (Fit within screen)
            float scaleX = screenW / targetW;
            float scaleY = screenH / targetH;
            float finalScale = System.Math.Min(scaleX, scaleY); // Uses the smaller scale to avoid cropping

            // 4. Calculate Centering Offsets (Black Bars)
            float offsetX = (screenW - (targetW * finalScale)) / 2f;
            float offsetY = (screenH - (targetH * finalScale)) / 2f;

            // 5. Build Matrix: Scale -> Then Move to Center
            return Matrix.CreateScale(finalScale) * Matrix.CreateTranslation(offsetX, offsetY, 0);
        }

        private void HandleInput()
        {
            bool isLeftClick = _currentMouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
            bool isRightClick = _currentMouse.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Released;
            bool isLeftHold = _currentMouse.LeftButton == ButtonState.Pressed;

            // --- CASE 1: HAND IS EMPTY (Pick up stuff or Drag Pizza) ---
            if (_heldToppingName == null)
            {
                // CLICK: Check what we clicked on
                if (isLeftClick || isRightClick)
                {
                    foreach (var e in _entities)
                    {
                        if (e.Bounds.Contains(_mouseWorldPos))
                        {
                            // A. Clicked a TUB? -> Pick up toppings
                            if (e is ToppingTub tub)
                            {
                                _heldToppingName = tub.ToppingName;
                                _heldToppingCount = isRightClick ? 20 : 1; // Right click = Handful

                                // Load the single topping texture (e.g., "Toppings/Pepperoni")
                                _heldToppingTexture = Content.Load<Texture2D>("Pizza/" + tub.ToppingName);
                                return;
                            }

                            // B. Clicked PIZZA? -> Move it or Remove Topping
                            else if (e is PizzaDough pizza)
                            {
                                if (isRightClick)
                                {
                                    // NEW: Pick up the topping from the pizza
                                    string pickedUp = pizza.TryPickUpTopping(_mouseWorldPos);
                                    if (pickedUp != null)
                                    {
                                        SetHeldTopping(pickedUp, 1); // Helper method below
                                    }
                                }
                                else if (isLeftClick)
                                {
                                    // Start dragging the dough
                                    _draggedItem = pizza;
                                    _dragOffset = pizza.Position - _mouseWorldPos;
                                }
                                return;
                            }
                            // C. Clicked LOOSE TOPPING (On the table)
                            else if (e is LooseTopping loose)
                            {
                                // Right click or Left click both pick it up
                                SetHeldTopping(loose.Name, 1);

                                // Remove it from the world
                                _entities.Remove(loose);
                                return;
                            }
                        }
                    }
                }

                // DRAG: If we are already dragging the dough (Left Hold)
                if (isLeftHold && _draggedItem != null)
                {
                    // ... (Keep your existing Clamp Logic here) ...
                    Vector2 newPos = _mouseWorldPos + _dragOffset;

                    // Reuse your clamp code:
                    if (_toppingArea.Width > 0 && _draggedItem is PizzaDough p)
                    {
                        float minX = _toppingArea.X;
                        float maxX = _toppingArea.X + _toppingArea.Width - p.Bounds.Width;
                        float minY = _toppingArea.Y;
                        float maxY = _toppingArea.Y + _toppingArea.Height - p.Bounds.Height;
                        newPos.X = MathHelper.Clamp(newPos.X, minX, maxX);
                        newPos.Y = MathHelper.Clamp(newPos.Y, minY, maxY);
                    }
                    _draggedItem.Position = newPos;
                }
                else
                {
                    _draggedItem = null; // Stop dragging if let go
                }
            }

            // --- CASE 2: HAND HAS TOPPINGS (Place them) ---
            else
            {
                // LEFT CLICK: Place ONE topping
                if (isLeftClick)
                {
                    bool placed = false;

                    // 1. Try to place on PIZZA
                    foreach (var e in _entities)
                    {
                        if (e is PizzaDough pizza && pizza.Bounds.Contains(_mouseWorldPos))
                        {
                            pizza.AddTopping(_heldToppingName, _heldToppingTexture, _mouseWorldPos);
                            placed = true;
                            break;
                        }
                    }

                    // 2. If missed pizza, check TABLE (Topping Space)
                    if (!placed && _toppingArea.Contains(_mouseWorldPos))
                    {
                        // Create a LooseTopping entity at mouse position
                        LooseTopping loose = new LooseTopping(_heldToppingName, _heldToppingTexture, _mouseWorldPos);

                        // Add to a temp list or handle the addition safely (avoid modifying list while iterating)
                        AddObject(loose);
                        placed = true;
                    }

                    // If we successfully placed it somewhere, decrease count
                    if (placed)
                    {
                        _heldToppingCount--;
                        if (_heldToppingCount <= 0) ClearHand();
                    }
                }

                // RIGHT CLICK: Cancel / Put back to tub
                if (isRightClick)
                {
                    ClearHand();
                }
            }
        }

        private void SetHeldTopping(string name, int count)
        {
            _heldToppingName = name;
            _heldToppingCount = count;
            // Load texture freshly (or cache it if you prefer)
            _heldToppingTexture = Content.Load<Texture2D>("Pizza/" + name);
        }

        private void ClearHand()
        {
            _heldToppingName = null;
            _heldToppingTexture = null;
            _heldToppingCount = 0;
        }

        protected override void HandleLevelObject(TiledObject obj)
        {
            // Adjust Position (Tiled Top-Left vs Bottom-Left check)
            // Tiled Objects are usually Top-Left, but if they are Tile Objects, they might differ.
            // Based on your JSON, "x" and "y" look like Top-Left.
            Vector2 pos = new Vector2(obj.X, obj.Y);

            switch (obj.Class) // In your JSON, it's "type": "Tub"
            {
                case "Tub":
                    // 1. DYNAMIC LOAD: Tries to find "Content/Pizza/Tub_Pepperoni.png"
                    string texName = "Pizza/Tub_" + obj.Name;

                    // (Optional Safety: Check if file exists or catch error if missing)
                    Texture2D tubTex = Content.Load<Texture2D>(texName);

                    Rectangle tubRect = new Rectangle((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height);

                    // 2. Pass the texture into the tub
                    ToppingTub t = new ToppingTub(obj.Name, tubRect, tubTex);
                    AddObject(t);
                    break;

                case "Screen":
                    // Create a Screen Entity (draws text orders)
                    // AddObject(new PizzaScreen(pos));
                    break;

                case "Button":
                    Rectangle btnRect = new Rectangle((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height);
                    Button btn = new Button(obj.Name, btnRect);
                    AddObject(btn);
                    break;

                case "Topping space":
                    // Store this rectangle for drop detection logic later!
                    _toppingArea = new Rectangle((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height);
                    break;
            }
        }
    }
}
