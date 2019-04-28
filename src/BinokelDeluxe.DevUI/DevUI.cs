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
                () => { return _cardFrontTexture; }
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
#if DEBUG
            Resolution.SetResolution(800, 480, false);
#else
            Resolution.SetResolution(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, _deviceManager.IsFullScreen);
#endif

            _devButtonTexture = contentManager.Load<Texture2D>("dev/devbutton");
            _devButtonPressedTexture = contentManager.Load<Texture2D>("dev/devbutton_pressed");
            _font = contentManager.Load<SpriteFont>("dev/devfont");
            _cardFrontTexture = contentManager.Load<Texture2D>("dev/frame");
            _cardBackTexture = contentManager.Load<Texture2D>("dev/back");

            _contentManager = contentManager;
        }

        public void Update(GameTime gameTime)
        {
            _inputHandler.Update();

            _currentScreen.Update(gameTime, _inputHandler);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Resolution.BeginDraw();

            spriteBatch.Begin(transformMatrix: Resolution.getTransformationMatrix());

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

        public void PlayDealingAnimation(
            int dealerPosition,
            IEnumerable<IEnumerable<Common.Card>> playerCards,
            IEnumerable<Common.Card> dabbCards
            )
        {
            _biddingScreen.Load(_contentManager);
            _biddingScreen.SetCards(playerCards);

            while (true)
            {
                Thread.Sleep(50);
            }
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
            throw new NotImplementedException();
        }

        public void UncoverDabb(IEnumerable<Card> cardsInDabb)
        {
            throw new NotImplementedException();
        }
    }
}
