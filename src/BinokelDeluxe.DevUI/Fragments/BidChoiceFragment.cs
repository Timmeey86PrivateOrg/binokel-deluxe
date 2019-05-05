using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinokelDeluxe.DevUI.Fragments
{
    /// <summary>
    /// This fragment is responsible for asking for player choices.
    /// </summary>
    internal class BidChoiceFragment
    {
        public event EventHandler BidButtonClicked;
        public event EventHandler PassButtonClicked;

        private bool _buttonsShallBeShown = false;
        public bool ButtonsShallBeShown
        {
            get { return _buttonsShallBeShown; }
            set
            {
                _bidButton.WasPressed = !value;
                _passButton.WasPressed = !value;
                _buttonsShallBeShown = value;
            }
        }

        private readonly DevButton _bidButton = new DevButton();
        private readonly DevButton _passButton = new DevButton();

        public BidChoiceFragment()
        {
            _bidButton.Position = new Vector2(400 + 200 - 40, 250);
            _bidButton.Text = "Bid";
            _passButton.Position = new Vector2(400 - 200 - 40, 250);
            _passButton.Text = "Pass";
        }

        public void Load(Texture2D texture, Texture2D pressedTexture, SpriteFont font)
        {
            _bidButton.Load(texture, pressedTexture, font);
            _passButton.Load(texture, pressedTexture, font);

            _bidButton.Activated += (o, e) => BidButtonClicked?.Invoke(this, EventArgs.Empty);
            _passButton.Activated += (o, e) => PassButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
            if (!ButtonsShallBeShown) return;

            _bidButton.Update(gameTime, inputHandler);
            _passButton.Update(gameTime, inputHandler);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!ButtonsShallBeShown) return;

            _bidButton.Draw(spriteBatch);
            _passButton.Draw(spriteBatch);
        }
    }
}
