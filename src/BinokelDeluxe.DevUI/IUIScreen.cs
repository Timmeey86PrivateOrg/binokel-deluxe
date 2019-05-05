using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BinokelDeluxe.DevUI
{
    /// <summary>
    /// This is the central interface for any UI Screen.
    /// </summary>
    internal interface IUIScreen
    {
        void Load(ContentManager content);
        void Unload();
        void Update(GameTime gameTime, InputHandler inputHandler);
        void Draw(SpriteBatch spriteBatch);
    }
}
