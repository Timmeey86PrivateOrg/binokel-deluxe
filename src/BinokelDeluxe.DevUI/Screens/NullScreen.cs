using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BinokelDeluxe.DevUI.Screens
{
    /// <summary>
    /// This class implements the null object pattern for the IUIScreen interface.
    /// Using this class allows skipping a null check in every update and paint call.
    /// </summary>
    internal class NullScreen : IUIScreen
    {
        public void Load(ContentManager content)
        {
            // Null object. Do nothing
        }

        public void Unload()
        {
            // Null object. Do nothing
        }

        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
            // Null object. Do nothing.
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            // Null object. Do nothing.
        }
    }
}
