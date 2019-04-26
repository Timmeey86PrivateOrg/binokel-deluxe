using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Linq;

namespace BinokelDeluxe.DevUI
{
    /// <summary>
    /// This class is responsible for drawing an interactable button for development purposes.
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

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if( _font == null )
                {
                    return;
                }

                // Fit the text into 90% of the button's area.
                // Calculation taken from http://bluelinegamestudios.com/posts/drawstring-to-fit-text-to-a-rectangle-in-xna/
                var textMeasure = _font.MeasureString(value);

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

        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(
                    ToInt(Position.X),
                    ToInt(Position.Y),
                    ToInt(Width),
                    ToInt(Height)
                    );
            }
            set
            {
                Position = new Vector2(value.X, value.Y);
                Width = value.Width;
                Height = value.Height;
            }
        }

        private Texture2D _texture = null;
        private SpriteFont _font = null;
        private float _textScale = 1.0f;
        private Vector2 _textPosition = new Vector2(.0f, .0f);

        /// <summary>
        /// Creates a new development button.
        /// </summary>
        public DevButton()
        {
        }

        public void Load(Texture2D texture, SpriteFont font)
        {
            _texture = texture;
            _font = font;
            // Force an initial text recalculation
            Text = _text;
        }

        public void Update(InputHandler inputHandler)
        {
            if(inputHandler.ReleasedPoint != null)
            {
                if(Rectangle.Contains(inputHandler.ReleasedPoint.Value))
                {
                    Activated?.Invoke(this, EventArgs.Empty);
                }
                // Make sure we don't process the event twice.
                inputHandler.Reset();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_texture == null || _font == null)
            {
                return;
            }

            spriteBatch.Draw(_texture, Rectangle, Color.White);
            spriteBatch.DrawString(_font, Text, _textPosition, Color.Black, .0f, Origin, _textScale, SpriteEffects.None, 1.0f);
        }

        private int ToInt(float f)
        {
            return (int)Math.Round(f);
        }
    }
}
