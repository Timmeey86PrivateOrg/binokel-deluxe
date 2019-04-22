using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BinokelDeluxe.Shared
{
    /// <summary>
    /// This class provides the appropriate image for each card.
    /// </summary>
    public class HungarianCardSprite
    {
        public SpriteBatch SpriteBatch { get; private set; }
        public ContentManager Content { get; private set; }

        private Texture2D _texture = null;

        private readonly IDictionary<Common.CardSuit, Tuple<int, int>> SuitRanges = new Dictionary<Common.CardSuit, Tuple<int, int>>()
        {
            { Common.CardSuit.Hearts, new Tuple<int, int>( 0, 200 ) },
            { Common.CardSuit.Bells, new Tuple<int, int>( 200, 400 ) },
            { Common.CardSuit.Leaves, new Tuple<int, int> ( 402, 602 ) },
            { Common.CardSuit.Acorns, new Tuple<int, int> ( 603, 801 ) }
        };

        private readonly IDictionary<Common.CardType, Tuple<int, int>> TypeRanges = new Dictionary<Common.CardType, Tuple<int, int>>()
        {
            { Common.CardType.Ace, new Tuple<int, int>( 0, 127 ) },
            { Common.CardType.King, new Tuple<int, int>( 129, 256 ) },
            { Common.CardType.Ober, new Tuple<int, int>( 257, 385 ) },
            { Common.CardType.Unter, new Tuple<int, int>( 386, 511 ) },
            { Common.CardType.Ten, new Tuple<int, int>( 514, 639 ) },
            { Common.CardType.Seven, new Tuple<int, int>( 898, 1023 ) }
        };

        public HungarianCardSprite(SpriteBatch spriteBatch, ContentManager content)
        {
            SpriteBatch = spriteBatch;
            Content = content;
        }

        public void Load()
        {
            _texture = Content.Load<Texture2D>("huncards");
        }

        public void Draw(Common.Card card, Rectangle targetRectangle)
        {
            SpriteBatch.Draw(_texture, targetRectangle, GetSourceRectangle(card), Color.White);
        }

        private Rectangle GetSourceRectangle(Common.Card card)
        {
            var yRange = SuitRanges[card.Suit];
            var xRange = TypeRanges[card.Type];
            return new Rectangle(xRange.Item1, yRange.Item1, (xRange.Item2 - xRange.Item1 + 1), (yRange.Item2 - yRange.Item1 + 1));
        }
    }
}
