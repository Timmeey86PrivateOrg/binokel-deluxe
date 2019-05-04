using BinokelDeluxe.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace BinokelDeluxe.DevUI.Fragments
{
    /// <summary>
    /// This UI fragment is responsible for drawing and detecting clicks on cards.
    /// </summary>
    internal class CardFragment
    {
        public event EventHandler<Common.CardEventArgs> CardClicked;

        private readonly Dictionary<Common.Card, DevCard> _cardGraphics = new Dictionary<Common.Card, DevCard>();
        private List<Common.Card> _drawingOrder;

        private readonly Func<Texture2D> _getBackTexture;
        private readonly Func<Texture2D> _getFrontTexture;
        private readonly Func<Texture2D> _getSelectedTexture;
        private readonly Func<SpriteFont> _getFont;

        private int _dealerNumber = -1;
        private HashSet<Common.Card> _clickableCards;

        // For less calculation
        private IList<Vector2> _playerPositions = null;
        private int _amountOfPlayers = 0;

        public CardFragment(Func<Texture2D> getCardBackTexture, Func<Texture2D> getCardFrontTexture, Func<Texture2D> getSelectedTexture, Func<SpriteFont> getFont)
        {
            _getBackTexture = getCardBackTexture;
            _getFrontTexture = getCardFrontTexture;
            _getSelectedTexture = getSelectedTexture;
            _getFont = getFont;
        }

        /// <summary>
        /// Displays the given list of cards on the fragment.
        /// </summary>
        /// <param name="cardsPerPlayer">The cards for each player.</param>
        /// <param name="dabbCards">The cards in the dabb.</param>
        public void SetCards(IEnumerable<IEnumerable<Common.Card>> cardsPerPlayer, IEnumerable<Common.Card> dabbCards)
        {
            _amountOfPlayers = cardsPerPlayer.Count();

            _clickableCards = new HashSet<Common.Card>(cardsPerPlayer.First());
            _clickableCards.UnionWith(dabbCards);

            _drawingOrder = cardsPerPlayer.SelectMany(innerList => innerList).ToList();
            _drawingOrder.AddRange(dabbCards);

            CalculatePlayerPositions();
            CalculateCardGraphics(cardsPerPlayer, dabbCards);
        }

        /// <summary>
        /// Uncovers the given list of cards.
        /// </summary>
        /// <param name="cards">The cards to be uncovered.</param>
        public void UncoverCards(IEnumerable<Common.Card> cards)
        {
            foreach (var card in cards)
            {
                _cardGraphics[card].IsCovered = false;
            }
        }

        /// <summary>
        /// Sets the number of the dealer.
        /// </summary>
        /// <param name="dealerNumber">The position of the dealer, where 0 is the user.</param>
        public void SetDealer(int dealerNumber)
        {
            _dealerNumber = dealerNumber;
            // TODO: Add dealer button
        }

        public void SetCardSelected(Common.Card card, bool selected)
        {
            if (_cardGraphics.ContainsKey(card))
            {
                _cardGraphics[card].IsSelected = selected;
            }
        }

        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
            _cardGraphics.Values.ToList().ForEach(card => card.Update(gameTime, inputHandler));

            if (inputHandler.ReleasedPoint.HasValue)
            {
                // Try detecting clicks on cards
                foreach (var card in _drawingOrder.Reverse<Common.Card>())
                {
                    // Skip cards of enemy players
                    if (!_clickableCards.Contains(card)) continue;

                    var cardGraphics = _cardGraphics[card];
                    if (cardGraphics.IsInDrawingArea(inputHandler.ReleasedPoint.Value))
                    {
                        CardClicked?.Invoke(this, new Common.CardEventArgs(card));
                        inputHandler.Reset(); // Prevent the click from being processed any further
                        return; // Detecting one card is sufficient
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_drawingOrder == null) return;

            _drawingOrder.ForEach(card => _cardGraphics[card].Draw(spriteBatch));
        }

        public void SwapCards(Common.Card first, Common.Card second)
        {
            Contract.Requires(first != null);
            Contract.Requires(second != null);
            Contract.Requires(Contract.Exists(_cardGraphics.Keys, x => x == first));
            Contract.Requires(Contract.Exists(_cardGraphics.Keys, x => x == second));

            DevCard.SwapPositions(_cardGraphics[first], _cardGraphics[second]);

            // Swap the drawing order as well so the new player card is not drawn above the other player cards.
            var indexOfFirstCard = _drawingOrder.IndexOf(first);
            var indexOfSecondCard = _drawingOrder.IndexOf(second);
            _drawingOrder[indexOfFirstCard] = second;
            _drawingOrder[indexOfSecondCard] = first;
        }

        private void CalculateCardGraphics(IEnumerable<IEnumerable<Common.Card>> cardsPerPlayer, IEnumerable<Common.Card> dabbCards)
        {
            var backTexture = _getBackTexture();
            var frontTexture = _getFrontTexture();
            var selectedTexture = _getSelectedTexture();
            var font = _getFont();
            var playerPosition = 0;
            var cardNumber = 0;

            _cardGraphics.Clear();
            foreach (var playerCards in cardsPerPlayer)
            {
                cardNumber = 0;
                var cardCount = playerCards.Count();
                foreach (var card in playerCards)
                {
                    _cardGraphics.Add(
                        card,
                        new DevCard(backTexture, frontTexture, selectedTexture, font)
                        {
                            Card = card,
                            Position = _playerPositions[playerPosition],
                            // 10 degrees rotation per card, with the middle card having a rotation of zero.
                            Angle = CalculateAngle(cardNumber, cardCount)
                        });
                    Debug.Assert(_cardGraphics.ContainsKey(card));
                    cardNumber++;
                }
                if (playerPosition == 0)
                {
                    // Make the cards of the human player larger
                    playerCards.ToList().ForEach(card => _cardGraphics[card].ScaleFactor = 0.1f);
                }
                playerPosition++;
            }

            cardNumber = 0;
            var centerPosition = new Vector2(400, 240);
            var numberOfCardsInDabb = dabbCards.Count();
            var spacing = 10;
            var dabbScaleFactor = .1f;
            var height = backTexture.Height * dabbScaleFactor;
            var cardWidth = backTexture.Width * dabbScaleFactor;

            // X Position of the first card: 
            // Center - two times the width of a card plus its spacing,
            // then corrected by half the width of the card since it's X origin is in the X center of the card,
            // then corrected by half a spacing so (0,0) is directly between two cards rather than on the edge of the right one (in case of two cards).
            var xBase = centerPosition.X - (numberOfCardsInDabb / 2.0f) * (cardWidth + spacing) + (cardWidth / 2.0f) + (spacing / 2.0f);
            var yBase = centerPosition.Y - (height / 2.0f);
            // Correct y base by the origin offset which is used for drawing cards
            yBase += (height * 2.5f);
            // Shift the cards upwards a bit since player 0's cards are larger than the other ones
            yBase -= 25;

            foreach (var card in dabbCards)
            {
                _cardGraphics.Add(
                    card,
                    new DevCard(backTexture, frontTexture, selectedTexture, font)
                    {
                        Card = card,
                        Position = new Vector2(xBase + cardNumber * (cardWidth + spacing), yBase),
                        ScaleFactor = dabbScaleFactor
                    });
                cardNumber++;
            }
        }

        private float CalculateAngle(int cardNumber, int cardCount)
        {
            return ((float)cardNumber - (cardCount - 1) / 2f) * 10f;
        }

        private void CalculatePlayerPositions()
        {
            if (_amountOfPlayers == 3)
            {
                _playerPositions = new List<Vector2>()
                {
                    new Vector2(400, 550),
                    new Vector2(680, 250),
                    new Vector2(120, 250)
                };
            }
            else
            {
                _playerPositions = new List<Vector2>()
                {
                    new Vector2(400, 550),
                    new Vector2(680, 250),
                    new Vector2(400, 180),
                    new Vector2(120, 250)
                };
            }
        }
    }
}
