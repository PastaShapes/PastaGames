using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace PastaEngine
{
    public class DialogueBox : Entity
    {
        // --- VARIABLES (Keeping yours exactly as they are) ---
        private string _fullText;
        private StringBuilder _displayedText = new StringBuilder();
        private Speaker _currentSpeaker;
        private SoundEffect _defaultAdvanceSound;

        private bool _isSimpleMode = false;

        // Typing logic
        private float _timer;
        private const float DefaultSpeed = 0.05f;
        private float _charDelay = DefaultSpeed;
        private int _currentIndex;
        private bool _isPaused;
        private float _pauseTimer;

        // UI
        private SpriteFont _font;
        private Texture2D _boxTexture;

        private Microsoft.Xna.Framework.Input.KeyboardState _prevKeyboard;
        private Microsoft.Xna.Framework.Input.MouseState _prevMouse;

        private SoundEffect _defaultTalkSound;    // <--- NEW: The typing sound

        // We still need a fallback font just in case
        private SpriteFont _defaultFont;

        // Queue for conversations
        private Queue<DialogueLine> _conversationQueue = new Queue<DialogueLine>();

        // Speaker lookup
        private Dictionary<string, Speaker> _speakers = new Dictionary<string, Speaker>();

        public bool IsActive { get; private set; }

        // --- SETUP ---
        public void RegisterSpeaker(Speaker speaker)
        {
            if (!_speakers.ContainsKey(speaker.Name))
                _speakers[speaker.Name] = speaker;
        }

        public override void Initialize()
        {
            _boxTexture = EngineCore.Pixel;
        }

        public override void LoadContent(ContentManager content)
        {
            _defaultFont = content.Load<SpriteFont>("Fonts/Alagard");

            foreach (var speaker in _speakers.Values)
            {
                if (!string.IsNullOrEmpty(speaker.FontFile))
                    speaker.Font = content.Load<SpriteFont>(speaker.FontFile);
                else
                    speaker.Font = _defaultFont;
            }
            //_defaultTalkSound = content.Load<SoundEffect>("Sounds/text_blip");
        }

        // --- LOGIC ---

        // 1. Core method to show text
        public void Show(string text, Speaker speaker)
        {
            _charDelay = DefaultSpeed;
            _fullText = text;
            _currentSpeaker = speaker;

            // Reset state
            _displayedText.Clear();
            _currentIndex = 0;
            _timer = 0;
            IsActive = true;
            _isPaused = false;

            // SAFEGUARD: If simple mode (speaker is null), use default font
            if (speaker != null && speaker.Font != null)
                _font = speaker.Font;
            else
                _font = _defaultFont;

            _prevMouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            _prevKeyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            // 5. WRAP THE TEXT
            _font = (speaker != null && speaker.Font != null) ? speaker.Font : _font;
            _prevMouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            _prevKeyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            // --- WRAPPING MATH (Tuned) ---

            // 1. Virtual Box Width (320px screen)
            // Box is 320 wide minus 20px margins on each side = 280px wide box.
            // Inside the box, we have 8px padding on each side.
            // So visual space = 280 - 16 = 264px.
            float visualWidthLimit = 264f;

            // 2. Ratio (HD vs Virtual)
            float fontRatio = 48f / 12f; // Assuming 48 is your HD size, 12 is target

            // 3. The Limit
            // We add a tiny multiplier (1.1f) because MeasureString can sometimes be 
            // too conservative with whitespace, causing early wrapping.
            float maxHdPixels = (visualWidthLimit * fontRatio) * 1.1f;

            // 4. Wrap
            _fullText = WrapText(_font, text, maxHdPixels);
        }

        // 2. Start a Conversation (Normal Mode)
        public void StartConversation(List<DialogueLine> conversation)
        {
            _isSimpleMode = false; // Enforce Normal Mode
            _conversationQueue.Clear();

            foreach (var line in conversation)
                _conversationQueue.Enqueue(line);

            Advance();
        }

        // 3. Show Simple Text (Examine Mode) - FIXED
        public void ShowText(string text)
        {
            _isSimpleMode = true; // Enforce Simple Mode

            // We clear the queue so "Advance" will close the box immediately after this line
            _conversationQueue.Clear();

            // Reuse the existing Show logic, passing null for speaker
            Show(text, null);
        }

        public void Advance()
        {
            if (_conversationQueue.Count > 0)
            {
                DialogueLine nextLine = _conversationQueue.Dequeue();

                Speaker speaker = null;
                if (_speakers.ContainsKey(nextLine.SpeakerName))
                {
                    speaker = _speakers[nextLine.SpeakerName];
                }

                Show(nextLine.Text, speaker);
            }
            else
            {
                IsActive = false; // Close box
            }
        }

        // --- UPDATE LOOP (Your logic was mostly good!) ---
        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var currentKeyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            var currentMouse = Microsoft.Xna.Framework.Input.Mouse.GetState();

            // Check for "Just Pressed" (Pressed now, but wasn't before)
            bool spacePressed = currentKeyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space) &&
                                !_prevKeyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space);

            bool leftClick = currentMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed &&
                             _prevMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released;

            bool rightClick = currentMouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed &&
                              _prevMouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released;

            bool actionPressed = spacePressed || leftClick || rightClick;

            if (actionPressed)
            {
                // CASE A: Still Typing? -> SKIP TO END
                if (_currentIndex < _fullText.Length)
                {
                    _displayedText.Clear();
                    _displayedText.Append(_fullText);
                    _currentIndex = _fullText.Length;

                    // Stop any typing sounds immediately
                    // (Optional: You could play a little 'skip' sound here)
                }
                // CASE B: Done Typing? -> GO TO NEXT LINE
                else
                {
                    Advance();
                }
            }

            if (_isPaused)
            {
                _pauseTimer -= dt;
                if (_pauseTimer <= 0) _isPaused = false;
                return;
            }
            else if (_currentIndex < _fullText.Length)
            {
                // Typing Logic
                _timer += dt;
                if (_timer >= _charDelay && _currentIndex < _fullText.Length)
                {
                    _timer = 0;
                    char nextChar = _fullText[_currentIndex];

                    // [Tag Parsing]
                    if (nextChar == '[')
                    {
                        int closeIndex = _fullText.IndexOf(']', _currentIndex);
                        if (closeIndex != -1)
                        {
                            string tagContent = _fullText.Substring(_currentIndex + 1, closeIndex - _currentIndex - 1);
                            ParseTag(tagContent);
                            _currentIndex = closeIndex + 1;
                            return;
                        }
                    }

                    if (nextChar == '\n' || _font.Characters.Contains(nextChar))
                    {
                        _displayedText.Append(nextChar);
                    }
                    else
                    {
                        _displayedText.Append('?');
                    }

                    _currentIndex++;

                    // Play sound
                    if (_currentIndex % 2 == 0 && _currentSpeaker?.TalkSound != null)
                    {
                        _currentSpeaker.TalkSound.Play(1f, _currentSpeaker.Pitch, 0f);
                    }
                }
            }

            _prevKeyboard = currentKeyboard;
            _prevMouse = currentMouse;
        }

        private void ParseTag(string tag)
        {
            var parts = tag.Split(':');
            string command = parts[0].Trim();

            if (command == "pause" && parts.Length > 1)
            {
                if (float.TryParse(parts[1], out float time))
                {
                    _isPaused = true;
                    _pauseTimer = time;
                }
            }
            else if (command == "speed" && parts.Length > 1)
            {
                if (float.TryParse(parts[1], out float speed)) _charDelay = speed;
            }
        }

        // --- DRAW (Fixed position logic) ---
        public override void Draw(SpriteBatch batch) { } // Empty

        public override void DrawUI(SpriteBatch batch)
        {
            if (!IsActive) return;

            // --- CALC SCALE ---
            float screenW = EngineCore.Graphics.Viewport.Width;
            float screenH = EngineCore.Graphics.Viewport.Height;
            float virtualW = 320;
            float virtualH = 180;

            float scaleX = screenW / virtualW;
            float scaleY = screenH / virtualH;
            float finalScale = System.Math.Min(scaleX, scaleY);

            float offsetX = (screenW - (virtualW * finalScale)) / 2f;
            float offsetY = (screenH - (virtualH * finalScale)) / 2f;

            // --- VIRTUAL POSITIONS ---
            float bottomMargin = 10;
            float height = 50;
            float sideMargin = 20;
            float width = 320 - (sideMargin * 2);
            float y = 180 - height - bottomMargin;

            // --- DRAW BACKGROUND (Using Vector2 for precision) ---
            Vector2 boxPos = new Vector2(
                sideMargin * finalScale + offsetX,
                y * finalScale + offsetY
            );

            // We calculate size separately
            Vector2 boxSize = new Vector2(
                width * finalScale,
                height * finalScale
            );

            // Draw the box using Position + Scale instead of a Rectangle
            // (This requires a white 1x1 pixel texture named EngineCore.Pixel)
            batch.Draw(
                EngineCore.Pixel,
                boxPos,
                null,           // Source Rect
                Color.Black * 0.8f,
                0f,             // Rotation
                Vector2.Zero,   // Origin
                boxSize,        // Scale (Dimensions)
                SpriteEffects.None,
                0f
            );

            // --- FONT SCALING ---
            float targetSize = 10f;
            float hdFontSize = 48f;
            float fontScale = (targetSize * finalScale) / hdFontSize;
            float textPadding = 8f;

            Vector2 textPos = new Vector2(
                boxPos.X + (textPadding * finalScale),
                boxPos.Y + (textPadding * finalScale)
            );

            if (_isSimpleMode)
            {
                batch.DrawString(_font, _displayedText.ToString(), textPos, Color.White,
                    0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
            }
            else
            {
                // Adjust for Portraits if needed
                if (_currentSpeaker != null)
                {
                    Vector2 namePos = new Vector2(
                        boxPos.X + (60 * finalScale),
                        boxPos.Y + (5 * finalScale)
                    );
                    batch.DrawString(_font, _currentSpeaker.Name, namePos, Color.Yellow,
                       0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
                }

                Vector2 mainPos = new Vector2(
                    boxPos.X + (60 * finalScale),
                    boxPos.Y + (20 * finalScale)
                );
                batch.DrawString(_font, _displayedText.ToString(), mainPos, Color.White,
                    0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
            }
        }

        private string WrapText(SpriteFont font, string text, float maxLineWidth)
        {
            if (string.IsNullOrEmpty(text)) return "";

            string[] words = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float spaceWidth = font.MeasureString(" ").X;
            float currentLineWidth = 0f;

            foreach (var word in words)
            {
                Vector2 size = font.MeasureString(word);

                // If the word fits on the current line...
                if (currentLineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    currentLineWidth += size.X + spaceWidth;
                }
                else
                {
                    // If it doesn't fit, start a new line (\n)
                    sb.Append("\n" + word + " ");
                    currentLineWidth = size.X + spaceWidth;
                }
            }
            return sb.ToString();
        }
    }
}
