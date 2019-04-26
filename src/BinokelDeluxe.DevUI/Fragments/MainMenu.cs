using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BinokelDeluxe.DevUI.Fragments
{
    internal class MainMenu : IUIFragment
    {
        private static int xOffset = 20;
        private static int yOffset = 20;
        private static int buttonWidth = 200;
        private static int buttonHeight = 100;
        private static int xMargin = 10;
        private static int yMargin = 10;

        private readonly DevButton _startButton = new DevButton()
        {
            Text = "Start Directly",
            Rectangle = new Rectangle(xOffset, yOffset, buttonWidth, buttonHeight)
        };
        private readonly DevButton _scoreboardButton = new DevButton()
        {
            Text = "Scoreboard",
            Rectangle = new Rectangle(xOffset, yOffset + buttonHeight + yMargin, buttonWidth, buttonHeight)
        };
        private readonly DevButton _quitButton = new DevButton()
        {
            Text = "Quit",
            Rectangle = new Rectangle(xOffset, yOffset + 2 * ( buttonHeight + yMargin ), buttonWidth, buttonHeight)
        };
        private readonly Func<Texture2D> _getTexture;
        private readonly Func<SpriteFont> _getFont;

        public MainMenu(Func<Texture2D> getTexture, Func<SpriteFont> getFont)
        {
            _getTexture = getTexture;
            _getFont = getFont;
        }

        public void Load(ContentManager content)
        {
            var texture = _getTexture();
            var font = _getFont();
            _startButton.Load(texture, font);
            _scoreboardButton.Load(texture, font);
            _quitButton.Load(texture, font);
        }
        public void Update(InputHandler inputHandler)
        {
            _startButton.Update(inputHandler);
            _scoreboardButton.Update(inputHandler);
            _quitButton.Update(inputHandler);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _startButton.Draw(spriteBatch);
            _scoreboardButton.Draw(spriteBatch);
            _quitButton.Draw(spriteBatch);
        }
    }
}
