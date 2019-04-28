using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinokelDeluxe.DevUI
{
    /// <summary>
    /// This class allows drawing a card for the development UI.
    /// </summary>
    public class DevCard
    {
        private const float ScaleFactor = .06f;
        private readonly Vector2 _origin;
        private Rectangle _drawingArea;
        private readonly Texture2D _backTexture;
        private readonly Texture2D _frontTexture;
        private readonly SpriteFont _font;
        
        /// <summary>
        /// Gets or sets the position of the card.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return _drawingArea.Location.ToVector2();
            }
            set
            {
                _drawingArea.Location = value.ToPoint();
            }
        }

        /// <summary>
        /// Gets or sets the rotation angle in degrees.
        /// </summary>
        public float Angle { get; set; } = .0f;
        
        /// <summary>
        /// Gets or sets the playing card which is represented by this UI object.
        /// </summary>
        public Common.Card Card { get; set; }

        /// <summary>
        /// True if covered, false if uncovered.
        /// </summary>
        public bool IsCovered { get; set; } = true;

        public DevCard(Texture2D backTexture, Texture2D frontTexture, SpriteFont font)
        {
            _backTexture = backTexture;
            _frontTexture = frontTexture;
            _font = font;
            // Make the card rotate around a point that is 1.5 times its height below it.
            _origin = new Vector2(.0f, backTexture.Height * 2.5f);
            // Make the card be drawn at a fracture of its texture size.
            _drawingArea = new Rectangle(0, 0, (int)Math.Round(backTexture.Width * ScaleFactor), (int)Math.Round(backTexture.Height * ScaleFactor));
        }

        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var texture = IsCovered ? _backTexture : _frontTexture;
            spriteBatch.Draw(texture, _drawingArea, null, Color.White, Angle * MathHelper.Pi / 180f, _origin, SpriteEffects.None, 1.0f);
            if (!IsCovered)
            {
                var text = String.Format("{0}{1}", Card.Suit.ToString().Substring(0, 1), Card.Type.ToString().Substring(0, 1));
                var textOrigin = _origin * ScaleFactor * 5f + new Vector2(-20f, -20f);
                spriteBatch.DrawString(_font, text, _drawingArea.Location.ToVector2(), Color.MonoGameOrange, Angle * MathHelper.Pi / 180f, textOrigin, .2f, SpriteEffects.None, 1.0f);
            }
        }

        /// <summary>
        /// Tests if the given position is in the drawing area of the card.
        /// </summary>
        /// <param name="position">The position vector.</param>
        /// <returns>True if the position is within the drawing bounds.</returns>
        public bool IsInDrawingArea(Vector2 position)
        {
            // TODO - PERFORMANCE - Precaculate matrix whenever angle, drawing area, origin or scale factor changes.

            // In order to detect if a card was selected, we can use the Rectangle.Contains function. However, this will not work directly for rotated and translated card graphics.
            // The most efficient way to do this is the following:
            // We have to rotate the point along the same point as the card, but in the opposite direction.
            // In order to do this, we first need to translate the rotation point to (0,0), then rotate the vector to the point and translate back to the rotation point

            var rotationMatrix =
                Matrix.CreateTranslation(-1f * _drawingArea.X, -1f * _drawingArea.Y, .0f) *
                Matrix.CreateRotationZ(-1f * Angle * MathHelper.Pi / 180f) *
                Matrix.CreateTranslation(_drawingArea.X, _drawingArea.Y, 0f);

            var rotatedPosition = Vector2.Transform(position, rotationMatrix);

            // Now translate the rotated position by the negative origin vector to get the vector in card coordinates.
            var translatedPosition = Vector2.Transform(
                rotatedPosition,
                Matrix.CreateTranslation( _origin.X * ScaleFactor, _origin.Y * ScaleFactor, .0f)
                );

            return _drawingArea.Contains(translatedPosition);
        }
    }
}
