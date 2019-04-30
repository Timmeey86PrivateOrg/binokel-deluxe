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
    /// The screen which is used during the dealing and bidding phases.
    /// For the actual UI, this code could probably be split into drawing and input handling code
    /// </summary>
    public class BiddingScreen : IUIScreen
    {
        private readonly object _mutex = new object();

        private readonly Fragments.CardFragment _cardFragment;
        private readonly Fragments.StatusFragment _statusFragment;
        private readonly Fragments.BidChoiceFragment _bidChoiceFragment;
        private readonly Fragments.DabbChoiceFragment _dabbChoiceFragment;

        private bool _bidPressed = false;
        private bool _passPressed = false;
        private bool _goOutPressed = false;
        private bool _finishPressed = false;
        private bool _bettelPressed = false;
        private bool _durchPressed = false;

        private IList<IList<Common.Card>> _cardsPerPlayer;
        private IList<Common.Card> _dabbCards;

        private bool BidPressed
        {
            set { lock (_mutex) { _bidPressed = value; } }
            get { lock (_mutex) { return _bidPressed; } }
        }

        private bool PassPressed
        {
            set { lock (_mutex) { _passPressed = value; } }
            get { lock (_mutex) { return _passPressed; } }
        }

        private bool FinishPressed
        {
            set { lock (_mutex) { _finishPressed = value; } }
            get { lock (_mutex) { return _finishPressed; } }
        }


        private bool GoOutPressed
        {
            set { lock (_mutex) { _goOutPressed = value; } }
            get { lock (_mutex) { return _goOutPressed; } }
        }


        private bool BettelPressed
        {
            set { lock (_mutex) { _bettelPressed = value; } }
            get { lock (_mutex) { return _bettelPressed; } }
        }


        private bool DurchPressed
        {
            set { lock (_mutex) { _durchPressed = value; } }
            get { lock (_mutex) { return _durchPressed; } }
        }


        public BiddingScreen(Func<Texture2D> getCardBackTexture, Func<Texture2D> getCardFrontTexture, Func<Texture2D> getCardSelectedTexture, Func<SpriteFont> getFont)
        {
            _cardFragment = new Fragments.CardFragment(getCardBackTexture, getCardFrontTexture, getCardSelectedTexture, getFont);
            _statusFragment = new Fragments.StatusFragment();
            _bidChoiceFragment = new Fragments.BidChoiceFragment();
            _dabbChoiceFragment = new Fragments.DabbChoiceFragment();

            _bidChoiceFragment.BidButtonClicked += (o, e) => BidPressed = true;
            _bidChoiceFragment.PassButtonClicked += (o, e) => PassPressed = true;
            _dabbChoiceFragment.FinishedButtonClicked += (o, e) => FinishPressed = true;
            _dabbChoiceFragment.GoOutButtonClicked += (o, e) => GoOutPressed = true;
            _dabbChoiceFragment.DurchButtonClicked += (o, e) => DurchPressed = true;
            _dabbChoiceFragment.BettelButtonClicked += (o, e) => BettelPressed = true;
        }

        public void SetCards(IEnumerable<IEnumerable<Common.Card>> cardsPerPlayer, IEnumerable<Common.Card> dabbCards)
        {
            lock (_mutex)
            {
                _cardFragment.SetCards(cardsPerPlayer, dabbCards);
                _statusFragment.DisplayWaitingStatus(cardsPerPlayer.Count());
                _cardsPerPlayer = new List<IList<Common.Card>>();
                foreach(var playerCards in cardsPerPlayer)
                {
                    _cardsPerPlayer.Add(playerCards.ToList());
                }
                _dabbCards = dabbCards.ToList();
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
            lock (_mutex)
            {
                // Reset button state
                _bidChoiceFragment.ButtonsShallBeShown = true;
                PassPressed = false;
                BidPressed = false;
            }

            while (!PassPressed && !BidPressed)
            {
                Thread.Sleep(50);
            }
            Common.GameTrigger trigger;
            if (BidPressed)
            {
                trigger = Common.GameTrigger.BidPlaced;
            }
            else
            {
                trigger = Common.GameTrigger.Passed;
            }

            _bidChoiceFragment.ButtonsShallBeShown = false;
            return trigger;
        }

        private Common.Card _selectedPlayerCard = null;
        private Common.Card SelectedPlayerCard
        {
            get { lock (_mutex) { return _selectedPlayerCard; } }
        }
        private Common.Card _selectedDabbCard = null;
        private Common.Card SelectedDabbCard
        {
            get { lock (_mutex) { return _selectedDabbCard; } }
        }
        public Common.GameTrigger LetUserExchangeCardsWithDabb(out IEnumerable<Common.Card> discardedCards, out Common.CardSuit? trumpSuit)
        {
            lock (_mutex)
            {
                _cardFragment.CardClicked += OnPlayerCardSelected;
                _cardFragment.CardClicked += OnDabbCardSelected;
                _dabbChoiceFragment.ButtonsShallBeShown = true;
            }
            // TODO: Offer some kind of "finished" button.
            while (!FinishPressed)
            {
                while ((SelectedPlayerCard == null || SelectedDabbCard == null ) && !FinishPressed)
                {
                    Thread.Sleep(50);
                }
                if (FinishPressed) { break; }
                // Both a player and a dabb card have been selected, exchange them.
                SwapCards();
                Thread.Sleep(30);
            }

            lock (_mutex)
            {
                _cardFragment.CardClicked -= OnPlayerCardSelected;
                _cardFragment.CardClicked -= OnDabbCardSelected;
                _dabbChoiceFragment.ButtonsShallBeShown = false;
            }
            discardedCards = _dabbCards;
            // TODO: Let player select trump
            trumpSuit = Common.CardSuit.Hearts;
            return Common.GameTrigger.TrumpSelected;
        }

        private void SwapCards()
        {
            lock(_mutex)
            {
                var indexOfDabbCard = _dabbCards.IndexOf(_selectedDabbCard);
                _dabbCards[indexOfDabbCard] = _selectedPlayerCard;

                var indexOfPlayerCard = _cardsPerPlayer[0].IndexOf(_selectedPlayerCard);
                _cardsPerPlayer[0][indexOfPlayerCard] = _selectedDabbCard;

                _cardFragment.SwapCards(_selectedDabbCard, _selectedPlayerCard);
                _selectedPlayerCard = null;
                _selectedDabbCard = null;
            }
        }

        private void OnPlayerCardSelected(object sender, Common.CardEventArgs e)
        {
            SelectCardOnClick(_cardsPerPlayer.First(), () => _selectedPlayerCard, (card) => _selectedPlayerCard = card, e.Card);
        }

        private void OnDabbCardSelected(object sender, Common.CardEventArgs e)
        {
            SelectCardOnClick(_dabbCards, () => _selectedDabbCard, (card) => _selectedDabbCard = card, e.Card);
        }

        private void SelectCardOnClick(
            IEnumerable<Common.Card> validCards,
            Func<Common.Card> getSelectedCard,
            Action<Common.Card> setSelectedCard,
            Common.Card card
            )
        {
            if (validCards.Contains(card))
            {
                lock (_mutex)
                {
                    var selectedCard = getSelectedCard();
                    if (selectedCard != null)
                    {
                        _cardFragment.SetCardSelected(selectedCard, false);
                    }
                    if (card != selectedCard)
                    {
                        _cardFragment.SetCardSelected(card, true);
                        setSelectedCard(card);
                    }
                    else
                    {
                        setSelectedCard(null);
                    }
                }
            }
        }

        public void Load(ContentManager content)
        {
            lock (_mutex)
            {
                _statusFragment.Font = content.Load<SpriteFont>("dev/devfont");
                _bidChoiceFragment.Load(
                    content.Load<Texture2D>("dev/devbutton"),
                    content.Load<Texture2D>("dev/devbutton_pressed"),
                    content.Load<SpriteFont>("dev/devfont")
                    );
                _dabbChoiceFragment.Load(
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
                _bidChoiceFragment.Update(gameTime, inputHandler);
                _dabbChoiceFragment.Update(gameTime, inputHandler);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            lock (_mutex)
            {
                _cardFragment.Draw(spriteBatch);
                _statusFragment.Draw(spriteBatch);
                _bidChoiceFragment.Draw(spriteBatch);
                _dabbChoiceFragment.Draw(spriteBatch);
            }
        }
    }
}
