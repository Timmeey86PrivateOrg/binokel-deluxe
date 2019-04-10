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
    }
}
