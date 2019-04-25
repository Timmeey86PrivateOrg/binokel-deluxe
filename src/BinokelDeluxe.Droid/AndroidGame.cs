using BinokelDeluxe.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BinokelDeluxe.Droid
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class AndroidGame : Shared.GameBase
    {
        public AndroidGame()
        {
            Graphics.IsFullScreen = true;
            Graphics.PreferredBackBufferWidth = 800;
            Graphics.PreferredBackBufferHeight = 480;
            Graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
        }

        /// <summary>
        /// Make the game exit when the back button is pressed.
        /// </summary>
        /// <returns>True if the game shall exit.</returns>
        protected override bool ExitButtonsArePressed()
        {
            return GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed;
        }

        /// <summary>
        /// Quits the game in an Android specific way.
        /// </summary>
        protected override void QuitGame()
        {
            // Exit currently does not work for Android.
            // See https://github.com/MonoGame/MonoGame/issues/5702
            Game.Activity.MoveTaskToBack(true);
        }

        protected override ScaleFactor GetDisplayScaleFactor()
        {
            return new ScaleFactor()
            {
                XScale = (float)GraphicsDevice.Viewport.Width / 800f,
                YScale = (float)GraphicsDevice.Viewport.Height / 480f
            };
        }
    }
}
