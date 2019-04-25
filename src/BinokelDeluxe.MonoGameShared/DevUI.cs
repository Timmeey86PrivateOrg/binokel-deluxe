using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BinokelDeluxe.Common;
using BinokelDeluxe.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BinokelDeluxe.Shared
{
    internal struct TextureData
    {
        public Rectangle Rectangle;
        public Texture2D Texture;
    }
    /// <summary>
    /// This is a development user interface for Binokel Deluxe. Do not use it as an example of how to design or implement a proper UI
    /// </summary>
    internal class DevUI : UI.IUserInterface
    {
        private readonly SynchronizationContext _uiContext;
        private Texture2D _devButton;

        private readonly List<TextureData> _drawables = new List<TextureData>();

        private bool _buttonPressed = false;
        private bool _mainMenuActive = false;

        public DevUI()
        {
            // Remember the UI context used for rendering the UI
            _uiContext = SynchronizationContext.Current;
        }

        public void LoadContent(ContentManager contentManager)
        {
            _devButton = contentManager.Load<Texture2D>("devbutton");
        }

        public void Update()
        {
            _drawables.Clear();
            if(_mainMenuActive)
            {
                _drawables.Add(new TextureData
                {
                    Rectangle = new Rectangle(200, 100, 80, 48),
                    Texture = _devButton
                });
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach(var drawable in _drawables)
            {
                spriteBatch.Draw(drawable.Texture, drawable.Rectangle, Color.White);
            }
        }

        public MainMenuActions DisplayMainMenu()
        {
            _mainMenuActive = true;
            while( !_buttonPressed )
            {
                Thread.Sleep(50);
            }
            return MainMenuActions.StartGame;
        }

        public void ActivatePlayer(int playerPosition)
        {
            throw new NotImplementedException();
        }

        public void DisplayAIBid(int playerPosition, int bidAmount)
        {
            throw new NotImplementedException();
        }

        public void DisplayGameScore(IEnumerable<ScoreData> playerOrTeamScores)
        {
            throw new NotImplementedException();
        }

        public void DisplayGoingOutScore(IEnumerable<ScoreData> playerOrTeamScores)
        {
            throw new NotImplementedException();
        }

        public void DisplayMelds(IEnumerable<MeldData> meldsByPlayers)
        {
            throw new NotImplementedException();
        }

        public void DisplayPlayerAsPassed(int playerPosition)
        {
            throw new NotImplementedException();
        }

        public void HandleInvalidMove(IEnumerable<Card> validCards)
        {
            throw new NotImplementedException();
        }

        public GameTrigger LetUserDoCounterBidOrPass(int nextBidAmount)
        {
            throw new NotImplementedException();
        }

        public GameTrigger LetUserExchangeCardsWithDabb(out IEnumerable<Card> discardedCards, out CardSuit? trumpSuit)
        {
            throw new NotImplementedException();
        }

        public GameTrigger LetUserPlaceFirstBidOrPass(int initialBidAmount)
        {
            throw new NotImplementedException();
        }

        public Card LetUserSelectCard()
        {
            throw new NotImplementedException();
        }

        public void MoveCardsToTrickWinner(int playerPosition)
        {
            throw new NotImplementedException();
        }

        public void PlaceCardInMiddle(int playerPosition, Card card)
        {
            throw new NotImplementedException();
        }

        public void PlayDealingAnimation(int dealerPosition, int numberOfCardsPerPlayer, int numberOfCardsInDabb)
        {
            throw new NotImplementedException();
        }

        public void PrepareTable(int dealerPosition)
        {
            throw new NotImplementedException();
        }

        public void RearrangeCardsForUser(IEnumerable<Card> rearrangedCards)
        {
            throw new NotImplementedException();
        }

        public void RemoveValidityState()
        {
            throw new NotImplementedException();
        }

        public void UncoverCardsForUser(IEnumerable<Card> userCards)
        {
            throw new NotImplementedException();
        }

        public void UncoverDabb(IEnumerable<Card> cardsInDabb)
        {
            throw new NotImplementedException();
        }
    }
}
