using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BinokelDeluxe.DevUI.Screens
{
    /// <summary>
    /// The screen which is used during the dealing and bidding phases.
    /// </summary>
    public class BiddingScreen : IUIScreen
    {
        public int DealerPosition { get; set; } = -1;
        public int NumberOfCardsPerPlayer { get; set; } = -1;
        public int NumberOfCardsInDabb { get; set; } = -1;

        private readonly Dictionary<Common.Card, DevCard> _cards = new Dictionary<Common.Card, DevCard>();

        private readonly Func<Texture2D> _getCardBackTexture;
        private readonly Func<Texture2D> _getCardFrontTexture;

        public BiddingScreen(Func<Texture2D> getCardBackTexture, Func<Texture2D> getCardFrontTexture)
        {
            _getCardBackTexture = getCardBackTexture;
            _getCardFrontTexture = getCardFrontTexture;
        }

        public void SetCards(IEnumerable<IEnumerable<Common.Card>> cardsPerPlayer)
        {
            var backTexture = _getCardBackTexture();
            var frontTexture = _getCardFrontTexture();

            var playerPosition = 0;
            var cardNumber = 0;
            var amountOfCardsPerPlayer = cardsPerPlayer.First().Count();
            var playerXBases = new List<int>() { 400, 600, 400, 200 };
            var playerYBases = new List<int>() { 440, 280, 120, 280 };

            foreach (var playerCards in cardsPerPlayer)
            {
                cardNumber = 0;
                foreach (var card in playerCards)
                {
                    _cards.Add(
                        card,
                        new DevCard(backTexture, frontTexture)
                        {
                            Card = card,
                            Position = new Vector2(playerXBases[playerPosition], playerYBases[playerPosition]),
                            Angle = (cardNumber - amountOfCardsPerPlayer / 2 - 1) * 10f
                        });
                    cardNumber++;
                }
                playerPosition++;
            }
        }

        public void Load(ContentManager content)
        {

        }
        public void Unload()
        {

        }
        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
            _cards.Values.ToList().ForEach(card => card.Update(gameTime, inputHandler));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _cards.Values.ToList().ForEach(card => card.Draw(spriteBatch));
        }
    }
}
