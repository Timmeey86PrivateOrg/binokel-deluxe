using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BinokelDeluxe.Shared
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public abstract class GameBase : Game
    {
        protected GraphicsDeviceManager Graphics { private set; get; }
        protected SpriteBatch SpriteBatch { private set; get; }
        protected HungarianCardSprite CardSprite { private set; get; }
        protected List<TogglableCard> Cards = new List<TogglableCard>();

        protected Core.GameController GameController { private set; get; }
        private DevUI.DevUI DevUI { set; get; }
        
        protected GameBase()
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            Graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            DevUI = new DevUI.DevUI(Graphics);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();


            TouchPanel.EnabledGestures = GestureType.Tap;

            var scaleFactor = 1.0f;

            var yOffset = 5 * scaleFactor;
            foreach (Common.CardSuit suit in Enum.GetValues(typeof(Common.CardSuit)))
            {
                var xOffset = 5 * scaleFactor;
                foreach (Common.CardType type in Enum.GetValues(typeof(Common.CardType)))
                {
                    Cards.Add(new TogglableCard(
                        new Common.Card() { Suit = suit, Type = type },
                        new Rectangle((int)xOffset, (int)yOffset, (int)(65 * scaleFactor), (int)(100 * scaleFactor))
                        ));
                    xOffset += 70 * scaleFactor;
                }
                yOffset += 105 * scaleFactor;
            }


            // TODO: Add user interface
            GameController = new Core.GameController(DevUI);
            GameController.StartNewGame(
                new GameLogic.RuleSettings()
                {
                    GameType = GameLogic.GameType.FourPlayerCrossBinokelGame,
                    SevensAreIncluded = false
                },
                new List<string>() { null, "TEMPAI", "TEMPAI", "TEMPAI" }
                );            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            CardSprite = new HungarianCardSprite(SpriteBatch, Content);
            CardSprite.Load();
            DevUI.LoadContent(Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        bool _leftButtonWasPressed = false;
        bool _gameIsRunning = false;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (ExitButtonsArePressed())
            {
                DevUI.Exit();
                QuitGame();
                return;
            }
            if (!_gameIsRunning)
            {
                _gameIsRunning = true;

            }
            
            DevUI.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// Check whether or not the exit buttons are currently pressed. This is usually the back or escape key.
        /// </summary>
        /// <returns></returns>
        protected abstract bool ExitButtonsArePressed();

        /// <summary>
        /// Quits the game. This is a separate method since Exit() is not supported on all platforms.
        /// </summary>
        protected virtual void QuitGame()
        {
            Exit();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            DevUI.Draw(SpriteBatch);

            base.Draw(gameTime);
        }
    }
}

