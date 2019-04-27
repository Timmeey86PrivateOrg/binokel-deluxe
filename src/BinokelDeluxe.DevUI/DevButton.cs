using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BinokelDeluxe.DevUI
{
    /// <summary>
    /// This class is responsible for drawing an interactable button for development purposes.
    /// The button can only be pressed once until it is explicitly reset. This prevents timing issues where two triggers are fired before the first one is processed.
    /// </summary>
    internal class DevButton
    {
        /// <summary>
        /// This event will be fired whenever the button gets activated by either clicking it or releasing the finger on it.
        /// </summary>
        public event EventHandler Activated;

        private static Vector2 Origin = new Vector2(.0f, .0f);

        public bool Enabled { get; set; } = true;
        public Vector2 Position { get; set; } = new Vector2(.0f, .0f);
        public float Width { get; set; } = 80;
        public float Height { get; set; } = 48;
        private string _text = "BUTTON";

        public bool WasPressed { get; set; } = false;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                UpdateTextPosition();
            }
        }
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(ToInt(Position.X), ToInt(Position.Y), ToInt(Width), ToInt(Height));
            }
            set
            {
                Position = new Vector2(value.X, value.Y);
                Width = value.Width;
                Height = value.Height;
            }
        }

        private Texture2D _texture = null;
        private Texture2D _pressedTexture = null;
        private SpriteFont _font = null;
        private float _textScale = 1.0f;
        private Vector2 _textPosition = new Vector2(.0f, .0f);
        private GameTime _pressedTime = null;

        /// <summary>
        /// Creates a new development button.
        /// </summary>
        public DevButton()
        {
        }

        public void Load(Texture2D texture, Texture2D pressedTexture, SpriteFont font)
        {
            _texture = texture;
            _pressedTexture = pressedTexture;
            _font = font;
            // Force an initial text recalculation
            UpdateTextPosition();
        }

        public void Unload()
        {
            _texture = null;
            _pressedTexture = null;
            _font = null;
        }

        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
            // Handle mouse releases on buttons, but skip them if the pressed animation is still being played.
            if (inputHandler.ReleasedPoint != null)
            {
                if (Rectangle.Contains(inputHandler.ReleasedPoint.Value) && !WasPressed)
                {
                    // Display a different texture for the button for 100ms to make sure the user knows it's pressed.
                    WasPressed = true;
                    _pressedTime = new GameTime(gameTime.TotalGameTime, gameTime.ElapsedGameTime);
                }
            }
            else if (WasPressed && _pressedTime != null && gameTime.TotalGameTime.TotalMilliseconds - _pressedTime.TotalGameTime.TotalMilliseconds > 100)
            {
                // Stop displaying a different graphics. Since the button is a one-time button, WasPressed will not be reset here.
                _pressedTime = null;
                // Let listeners know the button was pressed.
                Activated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                return;
            }

            var texture = _pressedTime != null ? _pressedTexture : _texture;
            spriteBatch.Draw(texture, Rectangle, Color.White);
            spriteBatch.DrawString(_font, Text, _textPosition, Color.Black, .0f, Origin, _textScale, SpriteEffects.None, 1.0f);
        }

        private int ToInt(float f)
        {
            return (int)Math.Round(f);
        }

        private void UpdateTextPosition()
        {
            if (_font == null)
            {
                return;
            }

            // Fit the text into 90% of the button's area.
            // Calculation taken from http://bluelinegamestudios.com/posts/drawstring-to-fit-text-to-a-rectangle-in-xna/
            var textMeasure = _font.MeasureString(Text);

            // Taking the smaller scaling value will result in the text always fitting in the boundaires.
            _textScale = Math.Min((Width * 0.9f / textMeasure.X), (Height * 0.9f / textMeasure.Y));

            // Figure out the location to absolutely-center it in the boundaries rectangle.
            var scaledTextWidth = (float)Math.Round(textMeasure.X * _textScale);
            var scaledTextHeight = (float)Math.Round(textMeasure.Y * _textScale);

            _textPosition = new Vector2(
                Position.X + (Width * 0.9f - scaledTextWidth) / 2 + Width * 0.05f,
                Position.Y + (Height * 0.9f - scaledTextHeight) / 2 + Height * 0.05f
                );
        }
    }
}
