using BinokelDeluxe.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BinokelDeluxe.DevUI.Screens
{
    /// <summary>
    /// This class is responsible for rendering the screen for the longest phase: The trick-taking phase.
    /// Ideas for actual UI:
    /// - Maybe have an IFragmentFactory
    /// </summary>
    internal class TrickTakingScreen : IUIScreen
    {
        private readonly object _mutex = new object();

        private Fragments.CardFragment _cardFragment;
        private Fragments.StatusFragment _statusFragment;
        private Fragments.IndicatorFragment _indicatorFragment;

        private readonly Func<Fragments.CardFragment> _createCardFragment;
        private readonly Func<Fragments.StatusFragment> _createStatusFragment;
        private readonly Func<Fragments.IndicatorFragment> _createIndicatorFragment;

        public TrickTakingScreen(Func<Fragments.CardFragment> createCardFragment, Func<Fragments.StatusFragment> createStatusFragment, Func<Fragments.IndicatorFragment> createIndicatorFragment)
        {
            _createCardFragment = createCardFragment;
            _createStatusFragment = createStatusFragment;
            _createIndicatorFragment = createIndicatorFragment;
        }

        public void Load(ContentManager content)
        {
            lock (_mutex)
            {
                _cardFragment = _createCardFragment();
                _statusFragment = _createStatusFragment();
                _indicatorFragment = _createIndicatorFragment();
            }
        }

        public void Unload()
        {
            lock (_mutex)
            {
                _cardFragment = null;
                _statusFragment = null;
                _indicatorFragment = null;
            }
        }

        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
            lock (_mutex)
            {
                if (_cardFragment == null) return;

                _cardFragment.Update(gameTime, inputHandler);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            lock (_mutex)
            {
                if (_cardFragment == null) return;

                _cardFragment.Draw(spriteBatch);
                _statusFragment.Draw(spriteBatch);
                _indicatorFragment.Draw(spriteBatch);
            }
        }

        public void Initialize(IEnumerable<IEnumerable<Common.Card>> cards, int dealerPosition)
        {
            _statusFragment.DisplayWaitingStatus(cards.Count());
            _cardFragment.SetCards(cards, dabbCards: new List<Common.Card>());
            _cardFragment.UncoverCards(cards.First());
            _indicatorFragment.SetDealerPosition(dealerPosition);
        }

        public void ActivatePlayer(int playerPosition)
        {
            lock (_mutex)
            {
                _statusFragment.SetPlayerThinking(playerPosition);
                Thread.Sleep(50);
            }
        }

        public void PlaceCardInMiddle(int playerPosition, Common.Card card)
        {
            lock (_mutex)
            {
                Thread.Sleep(42);
                _cardFragment.AddCardToMiddle(playerPosition, card);
                _statusFragment.SetPlayerWaiting(playerPosition);
            }
        }

        private Common.Card _chosenCard = null;
        private Common.Card ChosenCard
        {
            get { lock (_mutex) { return _chosenCard; } }
        }
        public Card LetUserSelectCard()
        {
            lock (_mutex)
            {
                _chosenCard = null;
                _cardFragment.CardClicked += (o, e) =>
                {
                    lock (_mutex)
                    {
                        _chosenCard = e.Card;
                        _cardFragment.SetCardSelected(_chosenCard, true);
                    }
                };
            }
            while (ChosenCard == null)
            {
                Thread.Sleep(30);
            }
            // Let the selection show for a bit
            Thread.Sleep(42);
            lock (_mutex)
            {
                _cardFragment.SetCardSelected(_chosenCard, false);
                return _chosenCard;
            }
        }

        internal void ClearMiddle()
        {
            lock (_mutex)
            {
                _cardFragment.ClearMiddle();
            }
        }
    }
}
