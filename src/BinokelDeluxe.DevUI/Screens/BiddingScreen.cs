using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

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
        private readonly Fragments.PlayerChoiceFragment _playerChoiceFragment;

        public BiddingScreen(Func<Texture2D> getCardBackTexture, Func<Texture2D> getCardFrontTexture, Func<SpriteFont> getFont)
        {
            _cardFragment = new Fragments.CardFragment(getCardBackTexture, getCardFrontTexture, getFont);
            _statusFragment = new Fragments.StatusFragment();
            _playerChoiceFragment = new Fragments.PlayerChoiceFragment();
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

        public Common.GameTrigger WaitForBidOrPass(int nextBidAmount)
        {
            // TODO: Display potential bid amount
            lock(_mutex)
            {
                _playerChoiceFragment.ButtonsShallBeShown = true;
            }

            bool choiceWasMade = false;
            while (!choiceWasMade)
            {
                Thread.Sleep(500);
            }
            return Common.GameTrigger.None;
        }

        public Common.GameTrigger LetUserExchangeCardsWithDabb()
        {
            return Common.GameTrigger.None;
        }

        public void Load(ContentManager content)
        {
            lock (_mutex)
            {
                _statusFragment.Font = content.Load<SpriteFont>("dev/devfont");
                _playerChoiceFragment.Load(
                    content.Load<Texture2D>("dev/devbutton"),
                    content.Load<Texture2D>("dev/devbutton_pressed"),
                    content.Load<SpriteFont>("dev/devfont")
                    );
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
                _playerChoiceFragment.Update(gameTime, inputHandler);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            lock (_mutex)
            {
                _cardFragment.Draw(spriteBatch);
                _statusFragment.Draw(spriteBatch);
                _playerChoiceFragment.Draw(spriteBatch);
            }
        }
    }
}
