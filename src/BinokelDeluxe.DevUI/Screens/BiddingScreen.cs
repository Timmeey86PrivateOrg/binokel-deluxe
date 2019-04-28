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
        private readonly object _mutex = new object();

        private readonly Fragments.CardFragment _cardFragment;
        private readonly Fragments.StatusFragment _statusFragment;

        public BiddingScreen(Func<Texture2D> getCardBackTexture, Func<Texture2D> getCardFrontTexture, Func<SpriteFont> getFont)
        {
            _cardFragment = new Fragments.CardFragment(getCardBackTexture, getCardFrontTexture, getFont);
            _statusFragment = new Fragments.StatusFragment();
        }

        public void SetCards(IEnumerable<IEnumerable<Common.Card>> cardsPerPlayer, IEnumerable<Common.Card> dabbCards)
        {
            lock (_mutex)
            {
                _cardFragment.SetCards(cardsPerPlayer, dabbCards);
                _statusFragment.DisplayWaitingStatus(cardsPerPlayer.Count());
            }
        }

        public void UncoverCards(IEnumerable<Common.Card> cards)
        {
            lock (_mutex)
            {
                _cardFragment.UncoverCards(cards);
            }
        }

        public void StartDealing(int dealerPosition)
        {
            lock (_mutex)
            {
                _statusFragment.StartDealing(dealerPosition);
            }
        }

        public void FinishDealing()
        {
            lock (_mutex)
            {
                _statusFragment.FinishDealing();
            }
        }

        public void SetPlayerBidding(int playerPosition)
        {
            lock (_mutex)
            {
                _statusFragment.SetPlayerBidding(playerPosition);
            }
        }

        public void SetPlayerPassed(int playerPosition)
        {
            lock (_mutex)
            {
                _statusFragment.SetPlayerPassed(playerPosition);
            }
        }

        public void SetPlayerBid(int playerPosition, int amount)
        {
            lock (_mutex)
            {
                _statusFragment.SetPlayerBid(playerPosition, amount);
            }
        }

        public void Load(ContentManager content)
        {
            lock (_mutex)
            {
                _statusFragment.Font = content.Load<SpriteFont>("dev/devfont");
            }
        }
        public void Unload()
        {

        }
        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
            lock (_mutex)
            {
                _cardFragment.Update(gameTime, inputHandler);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            lock (_mutex)
            {
                _cardFragment.Draw(spriteBatch);
                _statusFragment.Draw(spriteBatch);
            }
        }
    }
}
