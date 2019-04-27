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
        private const int CardWidth = 26;
        private const int CardHeight = 40;
        private static readonly Vector2 Origin = new Vector2(.0f, .0f);
        private Rectangle _drawingArea = new Rectangle(0, 0, CardWidth, CardHeight);
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
        /// Gets or sets the rotation angle, where 0 is default, and 1 is a full clockwise 360° rotation.
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
        }

        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var texture = IsCovered ? _backTexture : _frontTexture;
            spriteBatch.Draw(texture, _drawingArea, null, Color.White, Angle, Origin, SpriteEffects.None, 1.0f); 
        }
    }
}
