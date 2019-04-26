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
        private Texture2D _devButtonTexture;
        private SpriteFont _font;
        private InputHandler _inputHandler = new InputHandler();
        private bool _exited = false;

        // Fragments
        private readonly Fragments.MainMenu _mainMenu;

        private static IUIFragment _nullFragment = new Fragments.NullFragment();
        private IUIFragment _currentFragment = _nullFragment;

        public DevUI(GraphicsDeviceManager deviceManager)
        {
            // Remember the UI context used for rendering the UI
            _uiContext = SynchronizationContext.Current;
            _deviceManager = deviceManager;
            Resolution.Init(ref deviceManager);

            // Fragment initialization
            _mainMenu = new Fragments.MainMenu(() => { return _devButtonTexture; }, () => { return _font; });
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
            _font = contentManager.Load<SpriteFont>("dev/devfont");

            _mainMenu.Load(contentManager);
        }

        public void Update()
        {
            _inputHandler.Update();

            _currentFragment.Update(_inputHandler);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Resolution.BeginDraw();

            spriteBatch.Begin(transformMatrix: Resolution.getTransformationMatrix());

            _currentFragment.Draw(spriteBatch);

            spriteBatch.End();
        }

        public MainMenuActions DisplayMainMenu()
        {
            while( !_exited )
            {
                _currentFragment = _mainMenu;
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
