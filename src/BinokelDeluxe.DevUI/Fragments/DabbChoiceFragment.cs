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
    /// This fragment is responsible for asking for player choices during the "Exchanging cards with dabb" phase.
    /// This class and BidChoiceFragment can probably be merged into a single configurable class.
    /// </summary>
    internal class DabbChoiceFragment
    {
        public event EventHandler FinishedButtonClicked;
        public event EventHandler GoOutButtonClicked;
        public event EventHandler BettelButtonClicked;
        public event EventHandler DurchButtonClicked;

        private bool _buttonsShallBeShown = false;
        public bool ButtonsShallBeShown
        {
            get { return _buttonsShallBeShown; }
            set
            {
                _buttons.ForEach(button => button.WasPressed = !value);
                _buttonsShallBeShown = value;
            }
        }

        private readonly DevButton _finishButton;
        private readonly DevButton _goOutButton;
        private readonly DevButton _bettelButton;
        private readonly DevButton _durchButton;
        private readonly List<DevButton> _buttons;

        public DabbChoiceFragment()
        {
            _finishButton = new DevButton()
            {
                Position = new Vector2(500, 230),
                Text = "Finish"
            };
            _goOutButton = new DevButton()
            {
                Position = new Vector2(500, 260),
                Text = "Go Out"
            };
            _bettelButton = new DevButton()
            {
                Position = new Vector2(260, 230),
                Text = "Bettel"
            };
            _durchButton = new DevButton()
            {
                Position = new Vector2(260, 260),
                Text = "Durch"
            };

            _buttons = new List<DevButton>()
            {
                _finishButton, _goOutButton, _bettelButton, _durchButton
            };
            _buttons.ForEach(button =>
            {
                button.Width = 40;
                button.Height = 24;
            });
        }

        public void Load(Texture2D texture, Texture2D pressedTexture, SpriteFont font)
        {
            _buttons.ForEach(button => button.Load(texture, pressedTexture, font));

            _finishButton.Activated += (o, e) => FinishedButtonClicked?.Invoke(this, EventArgs.Empty);
            _goOutButton.Activated += (o, e) => GoOutButtonClicked?.Invoke(this, EventArgs.Empty);
            _bettelButton.Activated += (o, e) => BettelButtonClicked?.Invoke(this, EventArgs.Empty);
            _durchButton.Activated += (o, e) => DurchButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        public void Update(GameTime gameTime, InputHandler inputHandler)
        {
            if (!ButtonsShallBeShown) return;

            _buttons.ForEach(button => button.Update(gameTime, inputHandler));
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!ButtonsShallBeShown) return;

            _buttons.ForEach(button => button.Draw(spriteBatch));
        }
    }
}
