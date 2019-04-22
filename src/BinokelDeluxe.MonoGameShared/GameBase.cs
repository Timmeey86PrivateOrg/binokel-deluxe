using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BinokelDeluxe.Shared
{
    public struct ScaleFactor
    {
        public float XScale;
        public float YScale;
    }
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public abstract class GameBase : Game
    {
        protected GraphicsDeviceManager Graphics { private set; get; }
        protected SpriteBatch SpriteBatch { private set; get; }
        protected HungarianCardSprite CardSprite { private set; get; }

        protected GameBase()
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            Graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();

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
            if (ExitButtonsArePressed())
            {
                Exit();
            }

            // TODO: Add your update logic here            

            base.Update(gameTime);
        }

        /// <summary>
        /// Check whether or not the exit buttons are currently pressed. This is usually the back or escape key.
        /// </summary>
        /// <returns></returns>
        protected abstract bool ExitButtonsArePressed();

        /// <summary>
        /// Retrieves the scale factors for X and Y scaling.
        /// </summary>
        /// <returns>The factor to be used.</returns>
        protected virtual ScaleFactor GetDisplayScaleFactor()
        {
            return new ScaleFactor { XScale = 1.0f, YScale = 1.0f };
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            SpriteBatch.Begin();

            var scaleFactor = GetDisplayScaleFactor();

            var yOffset = 5 * scaleFactor.XScale;
            foreach (Common.CardSuit suit in Enum.GetValues(typeof(Common.CardSuit)))
            {
                var xOffset = 5 * scaleFactor.YScale;
                foreach (Common.CardType type in Enum.GetValues(typeof(Common.CardType)))
                {
                    CardSprite.Draw(
                        new Common.Card() { Suit = suit, Type = type },
                        new Rectangle((int)xOffset, (int)yOffset, (int)( 65 * scaleFactor.XScale ), (int)( 100 * scaleFactor.YScale )));
                    xOffset += 70 * scaleFactor.XScale;
                }
                yOffset += 105 * scaleFactor.YScale;
            }
            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

