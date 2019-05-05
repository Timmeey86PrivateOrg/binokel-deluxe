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
        private readonly Vector2 _origin;
        private Rectangle _drawingArea = new Rectangle();
        private readonly Texture2D _backTexture;
        private readonly Texture2D _frontTexture;
        private readonly Texture2D _selectedTexture;
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

        private float _scaleFactor = .3f;
        public float ScaleFactor
        {
            get { return _scaleFactor; }
            set
            {
                _scaleFactor = value;
                RecalculateDimensions();
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

        /// <summary>
        /// True if selected, false otherwise (default).
        /// </summary>
        public bool IsSelected { get; set; } = false;

        public DevCard(Texture2D backTexture, Texture2D frontTexture, Texture2D selectedTexture, SpriteFont font)
        {
            _backTexture = backTexture;
            _frontTexture = frontTexture;
            _selectedTexture = selectedTexture;
            _font = font;
            // Make the card rotate around a point that is 1.5 times its height below it.
            _origin = new Vector2(backTexture.Width / 2.0f, backTexture.Height * 2.5f);
            RecalculateDimensions();
        }

        /// <summary>
        /// Creates a clone of this DevCard instance, with the Card pointing to the same instance.
        /// </summary>
        /// <returns>The cloned card graphics.</returns>
        public DevCard Clone()
        {
            return new DevCard(_backTexture, _frontTexture, _selectedTexture, _font)
            {
                Angle = this.Angle,
                Card = this.Card, // don't clone! Every card should exist only once
                IsCovered = this.IsCovered,
                IsSelected = this.IsSelected,
                Position = this.Position,
                ScaleFactor = this.ScaleFactor
            };
        }

        /// <summary>
        /// Swaps the positions of the two cards. Both DevCard objects will still display the card they displayed before, but somewhere else than before.
        /// </summary>
        /// <param name="first">The graphics of the first card.</param>
        /// <param name="second">The graphics of the second card.</param>
        public static void SwapPositions(DevCard first, DevCard second)
        {
            var firstClone = first.Clone();
            var secondClone = second.Clone();

            first.CopyPositionFrom(secondClone);
            second.CopyPositionFrom(firstClone);
        }

        private void CopyPositionFrom(DevCard other)
        {
            Angle = other.Angle;
            Position = other.Position;
            ScaleFactor = other.ScaleFactor;
            IsSelected = false; // the cards were most likely selected in order to select them for swapping positions.
        }
        

        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Note: This can probably be made a lot less complex by applying additional transformations to the sprite batch (and reverting them at the end)
            //       Currently, each card and text places its origin way out of its own coordinates, since the draw method will rotate around the origin


            var textScaleFactor = 0.5f;
            var totalTextScale = ScaleFactor * textScaleFactor; 
            // When scaling the text down, we need to scale the origin up (unsure why, but it's necessary).
            float textOriginMultiplier = 1.0f / textScaleFactor;

            var angle = Angle * MathHelper.Pi / 180f;

            var frontTexture = IsSelected ? _selectedTexture : _frontTexture;
            var texture = IsCovered ? _backTexture : frontTexture;

            // Draw the card itself
            spriteBatch.Draw(texture, _drawingArea, null, Color.White, angle, _origin, SpriteEffects.None, 1.0f);

            if (!IsCovered)
            {
                // If the card is uncovered, draw strings identifying its suit and type

                var text = String.Format("{0}/{1}", Card.Suit.ToString().Substring(0, 1), Card.Type.ToString().Substring(0, 1));

                var textSize = _font.MeasureString(text) * textScaleFactor;
                // It's highly confusing which scale factor needs to be used when, but the following has been tested at all sizes and angles.
                var firstTextOrigin = (_origin - new Vector2(15f, 15f) * ScaleFactor) * textOriginMultiplier;
                var secondTextOrigin = (_origin - new Vector2(texture.Width - textSize.X, texture.Height - textSize.Y) + new Vector2(15f, 15f) * ScaleFactor) * textOriginMultiplier;

                spriteBatch.DrawString(_font, text, _drawingArea.Location.ToVector2(), Color.MonoGameOrange, angle, firstTextOrigin, totalTextScale, SpriteEffects.None, 1.0f);
                spriteBatch.DrawString(_font, text, _drawingArea.Location.ToVector2(), Color.MonoGameOrange, angle, secondTextOrigin, totalTextScale, SpriteEffects.None, 1.0f);
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

        private void RecalculateDimensions()
        {
            // Make the card be drawn at a fracture of its texture size.
            _drawingArea.Width = (int)Math.Round(_backTexture.Width * ScaleFactor);
            _drawingArea.Height = (int)Math.Round(_backTexture.Height * ScaleFactor);
        }
    }
}
