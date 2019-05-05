using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BinokelDeluxe.Droid
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class AndroidGame : Shared.GameBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidGame"/> class.
        /// </summary>
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
    }
}
