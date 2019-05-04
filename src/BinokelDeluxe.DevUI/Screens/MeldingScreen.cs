using BinokelDeluxe.DevUI.Fragments;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BinokelDeluxe.DevUI.Screens
{
    /// <summary>
    /// The screen which is used for the melding phase.
    /// </summary>
    internal class MeldingScreen : IUIScreen
    {
        private readonly Func<CardFragment> _cardFragmentFunc;

        private CardFragment _cardFragment = null;

        public MeldingScreen(Func<CardFragment> cardFragmentFunc)
        {
            _cardFragmentFunc = cardFragmentFunc;
        }

        public void DisplayMelds(IEnumerable<Common.MeldData> melds)
        {
            var cards = new List<IEnumerable<Common.Card>>();
            var allCards = new List<Common.Card>();
            foreach (var meldData in melds)
            {
                cards.Add(meldData.Cards);
                allCards.AddRange(meldData.Cards);
            }
            _cardFragment.SetCards(cards, dabbCards: new List<Common.Card>());
            _cardFragment.UncoverCards(allCards);
        }
        public void Load(ContentManager content)
        {
            _cardFragment = _cardFragmentFunc();
        }

        public void Unload()
        {
            _cardFragment = null;
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
        }
    }
}
