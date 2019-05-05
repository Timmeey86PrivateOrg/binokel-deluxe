namespace BinokelDeluxe.Shared
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input.Touch;

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public abstract class GameBase : Game
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameBase"/> class.
        /// </summary>
        protected GameBase()
        {
            this.Graphics = new GraphicsDeviceManager(this)
            {
                SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight,
            };
            this.Graphics.ApplyChanges();

            this.Content.RootDirectory = "Content";
            this.DevUI = new DevUI.DevUI(this.Graphics);
        }

        /// <summary>
        /// Gets the graphics device manager.
        /// </summary>
        protected GraphicsDeviceManager Graphics { get; private set; }

        /// <summary>
        /// Gets the sprite batch used for drawing.
        /// </summary>
        protected SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// Gets the game controller used by the game.
        /// </summary>
        protected Core.GameController GameController { get; private set; }

        private DevUI.DevUI DevUI { get; set; }

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

            this.GameController = new Core.GameController(this.DevUI);
            this.GameController.StartNewGame(
                new GameLogic.RuleSettings()
                {
                    GameType = GameLogic.GameType.FourPlayerCrossBinokelGame,
                    SevensAreIncluded = false,
                },
                new List<string>() { null, "TEMPAI", "TEMPAI", "TEMPAI" });
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.DevUI.LoadContent(this.Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (this.ExitButtonsArePressed())
            {
                this.DevUI.Exit();
                this.QuitGame();
                return;
            }

            this.DevUI.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// Check whether or not the exit buttons are currently pressed. This is usually the back or escape key.
        /// </summary>
        /// <returns>True in case the exit buttons are currently being pressed.</returns>
        protected abstract bool ExitButtonsArePressed();

        /// <summary>
        /// Quits the game. This is a separate method since Exit() is not supported on all platforms.
        /// </summary>
        protected virtual void QuitGame()
        {
            this.Exit();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            this.DevUI.Draw(this.SpriteBatch);

            base.Draw(gameTime);
        }
    }
}
