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

        private readonly Dictionary<Common.Card, DevCard> _cards = new Dictionary<Common.Card, DevCard>();

        private readonly Fragments.CardFragment _cardFragment;

        public BiddingScreen(Func<Texture2D> getCardBackTexture, Func<Texture2D> getCardFrontTexture)
        {
            _cardFragment = new Fragments.CardFragment(getCardBackTexture, getCardFrontTexture);
        }

        public void SetCards(IEnumerable<IEnumerable<Common.Card>> cardsPerPlayer)
        {
            lock (_mutex)
            {
                _cardFragment.SetCards(cardsPerPlayer);
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
            }
        }
    }
}
