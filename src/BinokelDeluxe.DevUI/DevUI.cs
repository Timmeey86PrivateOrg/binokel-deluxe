using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BinokelDeluxe.Common;
using BinokelDeluxe.UI;
using IndependentResolutionRendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BinokelDeluxe.DevUI
{
    /// <summary>
    /// This is a development user interface for Binokel Deluxe. Do not use it as an example of how to design or implement a proper UI
    /// Ideas for the actual UI:
    /// - Buttons could be created by a button factory. The factory could then be supplied with textures and fonts, and the user of the button factory
    ///   would not have to care about those properties any more.
    /// - There should be something like button groups, where all buttons share the same font size (which is determined by the button with the most text).
    /// - There should probably be several spritefonts for maximum readability.
    /// - Actual UI should probably make use of layer depths
    /// </summary>
    public class DevUI : UI.IUserInterface
    {
        private readonly SynchronizationContext _uiContext;
        private readonly GraphicsDeviceManager _deviceManager;
        private ContentManager _contentManager = null;
        private Texture2D _devButtonTexture;
        private Texture2D _devButtonPressedTexture;
        private SpriteFont _font;
        private Texture2D _cardFrontTexture;
        private Texture2D _cardBackTexture;
        private Texture2D _backgroundTexture;
        private InputHandler _inputHandler = new InputHandler();
        private bool _exited = false;

        // Screens
        private readonly Screens.MainMenu _mainMenu;
        private readonly Screens.BiddingScreen _biddingScreen;

        private static IUIScreen _nullScreen = new Screens.NullScreen();
        private IUIScreen _currentScreen = _nullScreen;

        public DevUI(GraphicsDeviceManager deviceManager)
        {
            // Remember the UI context used for rendering the UI
            _uiContext = SynchronizationContext.Current;
            _deviceManager = deviceManager;
            Resolution.Init(ref deviceManager);

            // Screen initialization
            _mainMenu = new Screens.MainMenu(
                () => { return _devButtonTexture; },
                () => { return _devButtonPressedTexture; },
                () => { return _font; }
                );
            _biddingScreen = new Screens.BiddingScreen(
                () => { return _cardBackTexture; },
                () => { return _cardFrontTexture; },
                () => { return _font; }
                );
        }
        
        public void Exit()
        {
            _exited = true;
        }

        public void LoadContent(ContentManager contentManager)
        {
            // Allow drawing on a virtual 800x480 screen (default resolution of many android devices) and stretch that to the actual device size.
            Resolution.SetVirtualResolution(800, 480);
            Resolution.SetResolution(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, _deviceManager.IsFullScreen);

            _devButtonTexture = contentManager.Load<Texture2D>("dev/devbutton");
            _devButtonPressedTexture = contentManager.Load<Texture2D>("dev/devbutton_pressed");
            _font = contentManager.Load<SpriteFont>("dev/devfont");
            _cardFrontTexture = contentManager.Load<Texture2D>("dev/frame");
            _cardBackTexture = contentManager.Load<Texture2D>("dev/back");
            _backgroundTexture = contentManager.Load<Texture2D>("dev/background");

            _contentManager = contentManager;
        }

        public void Update(GameTime gameTime)
        {
            _inputHandler.Update();

            _currentScreen.Update(gameTime, _inputHandler);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Resolution.BeginDraw(spriteBatch, _backgroundTexture);

            _currentScreen.Draw(spriteBatch);

            spriteBatch.End();
        }

        public MainMenuActions DisplayMainMenu()
        {
            _mainMenu.Load(_contentManager);
            _currentScreen = _mainMenu;
            while ( !_mainMenu.CurrentState.HasValue )
            {
                Thread.Sleep(50);
            }
            return _mainMenu.CurrentState.Value;
        }

        public void ActivatePlayer(int playerPosition)
        {
            throw new NotImplementedException();
        }

        public void DisplayAIBid(int playerPosition, int bidAmount)
        {
            _biddingScreen.SetPlayerBid(playerPosition, bidAmount);
            Thread.Sleep(500);
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
            _biddingScreen.SetPlayerPassed(playerPosition);
            Thread.Sleep(500);
        }

        public void HandleInvalidMove(IEnumerable<Card> validCards)
        {
            throw new NotImplementedException();
        }

        public GameTrigger LetUserDoCounterBidOrPass(int nextBidAmount)
        {
            return _biddingScreen.WaitForBidOrPass(nextBidAmount);
        }

        public GameTrigger LetUserExchangeCardsWithDabb(out IEnumerable<Card> discardedCards, out CardSuit? trumpSuit)
        {
            discardedCards = null;
            trumpSuit = null;
            return _biddingScreen.LetUserExchangeCardsWithDabb();
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

        public void PlayDealingAnimation(
            int dealerPosition,
            IEnumerable<IEnumerable<Common.Card>> playerCards,
            IEnumerable<Common.Card> dabbCards
            )
        {
            _biddingScreen.Load(_contentManager);
            _biddingScreen.SetCards(playerCards, dabbCards);
            _biddingScreen.StartDealing(dealerPosition);
            // No animation in Dev UI, switch to next action after two seconds
            Thread.Sleep(500);
            _biddingScreen.FinishDealing();
        }

        public void PrepareTable(int dealerPosition)
        {
            _currentScreen = _biddingScreen;
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
            _biddingScreen.UncoverCards(userCards);
            Thread.Sleep(500);
        }

        public void UncoverDabb(IEnumerable<Card> cardsInDabb)
        {
            _biddingScreen.UncoverCards(cardsInDabb);
            Thread.Sleep(500);
        }
    }
}
