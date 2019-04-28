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
        private const float ScaleFactor = .04f;
        private readonly Vector2 _origin;
        private Rectangle _drawingArea;
        private readonly Texture2D _backTexture;
        private readonly Texture2D _frontTexture;

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

        public DevCard(Texture2D backTexture, Texture2D frontTexture)
        {
            _backTexture = backTexture;
            _frontTexture = frontTexture;
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
        }
    }
}
