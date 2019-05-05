using Microsoft.Xna.Framework;

// EXPERIMENTAL

namespace BinokelDeluxe.Shared
{
    /// <summary>
    /// Defines a card which can be toggled
    /// </summary>
    public class TogglableCard
    {
        private readonly Rectangle _drawingArea;
        private readonly Common.Card _card;
        private bool _visible = true;

        public TogglableCard(Common.Card card, Rectangle drawingArea)
        {
            _card = card;
            _drawingArea = drawingArea;

        }

        public void Update(Point triggeredPos)
        {
            if(_drawingArea.Contains(triggeredPos))
            {
                _visible = !_visible;
            }
        }

        public void Draw(HungarianCardSprite sprite)
        {
            if (_visible)
            {
                sprite.Draw(_card, _drawingArea);
            }
        }
    }
}
