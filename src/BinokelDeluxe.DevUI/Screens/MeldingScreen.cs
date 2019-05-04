using BinokelDeluxe.DevUI.Fragments;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BinokelDeluxe.DevUI.Screens
{
    /// <summary>
    /// The screen which is used for the melding phase.
    /// </summary>
    internal class MeldingScreen : IUIScreen
    {
        private readonly Func<CardFragment> _cardFragmentFunc;
        private readonly Func<StatusFragment> _statusFragmentFunc;

        private CardFragment _cardFragment = null;
        private StatusFragment _statusFragment = null; 

        public MeldingScreen(Func<CardFragment> cardFragmentFunc, Func<StatusFragment> statusFragmentFunc)
        {
            _cardFragmentFunc = cardFragmentFunc;
            _statusFragmentFunc = statusFragmentFunc;
        }

        public void DisplayMelds(IEnumerable<Common.MeldData> melds)
        {
            // Abuse the status fragment to display melds (as bids)
            _statusFragment.DisplayWaitingStatus(melds.Count());

            var cards = new List<IEnumerable<Common.Card>>();
            var allCards = new List<Common.Card>();
            var playerPosition = 0;
            foreach (var meldData in melds)
            {
                cards.Add(meldData.Cards);
                allCards.AddRange(meldData.Cards);

                _statusFragment.SetPlayerBid(playerPosition, meldData.Points);
                playerPosition++;
            }

            // Abuse the card fragment to display melded cards
            _cardFragment.SetCards(cards, dabbCards: new List<Common.Card>());
            _cardFragment.UncoverCards(allCards);
        }
        public void Load(ContentManager content)
        {
            _cardFragment = _cardFragmentFunc();
            _statusFragment = _statusFragmentFunc();
        }

        public void Unload()
        {
            _cardFragment = null;
            _statusFragment = null;
        }

        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
            if (_cardFragment == null)
            {
                return;
            }

            _cardFragment.Update(gameTime, inputHandler);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_cardFragment == null)
            {
                return;
            }

            _cardFragment.Draw(spriteBatch);
            _statusFragment.Draw(spriteBatch);
        }
    }
}
