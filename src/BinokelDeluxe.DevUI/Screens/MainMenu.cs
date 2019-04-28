using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BinokelDeluxe.DevUI.Screens
{
    /// <summary>
    /// </summary>
    internal class MainMenu : IUIScreen
    {
        private const int xOffset = 20;
        private const int yOffset = 20;
        private const int buttonWidth = 200;
        private const int buttonHeight = 100;
        private const int yMargin = 10;

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
            Rectangle = new Rectangle(xOffset, yOffset + 2 * (buttonHeight + yMargin), buttonWidth, buttonHeight)
        };
        private readonly Func<Texture2D> _getTexture;
        private readonly Func<Texture2D> _getPressedTexture;
        private readonly Func<SpriteFont> _getFont;

        private readonly object _mutex = new object();
        private UI.MainMenuActions? _uiState = null;
        public UI.MainMenuActions? CurrentState
        {
            get
            {
                lock (_mutex)
                {
                    return _uiState;
                }
            }
            set
            {
                lock (_mutex)
                {
                    _uiState = value;
                }
            }
        }

        public MainMenu(Func<Texture2D> getTexture, Func<Texture2D> getPressedTexture, Func<SpriteFont> getFont)
        {
            _getTexture = getTexture;
            _getPressedTexture = getPressedTexture;
            _getFont = getFont;

            MapButtonAndState(_startButton, UI.MainMenuActions.StartGame);
            MapButtonAndState(_scoreboardButton, UI.MainMenuActions.DisplayScoreboard);
            MapButtonAndState(_quitButton, UI.MainMenuActions.Quit);
        }

        /// <summary>
        /// Sets the internal state to state when button is pressed.
        /// This method is thread safe.
        /// </summary>
        /// <param name="button">The button to watch.</param>
        /// <param name="state">The state to be set.</param>
        private void MapButtonAndState(DevButton button, UI.MainMenuActions state)
        {
            button.Activated += (o, e) =>
            {
                lock (_mutex)
                {
                    if (!_uiState.HasValue)
                    {
                        _uiState = state;
                    }
                }
            };
        }

        public void Load(ContentManager content)
        {
            var texture = _getTexture();
            var pressedTexture = _getPressedTexture();
            var font = _getFont();
            _startButton.Load(texture, pressedTexture, font);
            _scoreboardButton.Load(texture, pressedTexture, font);
            _quitButton.Load(texture, pressedTexture, font);
        }

        public void Unload()
        {
            // TODO: Implement this. This probably requires an own content manager which loads stuff itself.
        }
        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
            _startButton.Update(gameTime, inputHandler);
            _scoreboardButton.Update(gameTime, inputHandler);
            _quitButton.Update(gameTime, inputHandler);

            if(inputHandler.ReleasedPoint != null)
            {
                // Make sure we don't process the event twice.
                inputHandler.Reset();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _startButton.Draw(spriteBatch);
            _scoreboardButton.Draw(spriteBatch);
            _quitButton.Draw(spriteBatch);
        }
    }
}
