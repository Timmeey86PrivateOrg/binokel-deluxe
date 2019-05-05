using IndependentResolutionRendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinokelDeluxe.DevUI
{
    /// <summary>
    /// This class is responsible for providing input information in a cross-platform way.
    /// For now, this class will only handle single touch.
    /// </summary>
    public class InputHandler
    {
        /// <summary>
        /// The location in virtual coordinates where the mouse was pressed down, or the touch panel was first touched.
        /// Null while the mouse is not pressed and the touch panel is not touched.
        /// </summary>
        public Vector2? PressedPoint { get; private set; } = null;
        /// <summary>
        /// The current in virtual coordinates location of the mouse or finger.
        /// Null while the mouse is not pressed and the touch panel is not touched.
        /// </summary>
        public Vector2? CurrentPoint { get; private set; } = null;
        /// <summary>
        /// The location in virtual coordinates where the mouse or finger was released.
        /// Null while the mouse is being pressed or the touch panel is being touched.
        /// </summary>
        public Vector2? ReleasedPoint { get; private set; } = null;
        /// <summary>
        /// True while the user is dragging the mouse, i.e. moving it with the mouse button pressed down.
        /// </summary>
        public bool IsDragging { get; private set; } = false;
       
        /// <summary>
        /// Updates the internal state dependent on user interaction.
        /// </summary>
        public void Update()
        {
            if (TouchPanel.GetCapabilities().IsConnected)
            {
                UpdateFromTouchPanel();
            }
            else
            {
                UpdateFromMouse();
            }
        }

        /// <summary>
        /// Resets the handler so a release event is not processed twice.
        /// </summary>
        public void Reset()
        {
            PressedPoint = null;
            ReleasedPoint = null;
            CurrentPoint = null;
            IsDragging = false;
        }

        private void UpdateFromMouse()
        {
            var mouseState = Mouse.GetState();
            if( PressedPoint == null && mouseState.LeftButton == ButtonState.Pressed )
            {
                RegisterPress(mouseState.Position.ToVector2());
            }
            else if( PressedPoint != null && mouseState.LeftButton == ButtonState.Pressed)
            {
                RegisterDrag(mouseState.Position.ToVector2());
            }
            else if( PressedPoint != null && mouseState.LeftButton == ButtonState.Released)
            {
                RegisterRelease(mouseState.Position.ToVector2());
            }
        }

        private void UpdateFromTouchPanel()
        {
            var touchCollection = TouchPanel.GetState();
            if (touchCollection.Count > 0)
            {
                var touchInfo = touchCollection.First();
                switch(touchInfo.State)
                {
                    case TouchLocationState.Pressed:
                        RegisterPress(touchInfo.Position);
                        break;
                    case TouchLocationState.Moved:
                        RegisterDrag(touchInfo.Position);
                        break;
                    case TouchLocationState.Released:
                        if (ReleasedPoint == null)
                        {
                            RegisterRelease(touchInfo.Position);
                        }
                        break;
                    default:
                        // Do nothing
                        break;
                }
            }
        }

        private void RegisterRelease(Vector2 position)
        {
            ReleasedPoint = FromScreen(position);
            PressedPoint = null;
            CurrentPoint = null;
            IsDragging = false;
        }

        private void RegisterDrag(Vector2 position)
        {
            CurrentPoint = FromScreen(position);
            if (!IsDragging && Vector2.Distance(PressedPoint.Value, CurrentPoint.Value) > 5)
            {
                IsDragging = true;
            }
        }

        private void RegisterPress(Vector2 position)
        {
            ReleasedPoint = null;
            PressedPoint = FromScreen(position);
            CurrentPoint = PressedPoint;
            IsDragging = false;
        }

        /// <summary>
        /// Transforms the mouse position from screen coordinates to virtual coordinates.
        /// </summary>
        /// <param name="screenPos">The screen position.</param>
        /// <returns>The screen position in virtual coordinates.</returns>
        private Vector2 FromScreen(Vector2 screenPos)
        {
            // Correct the screen position by any viewport differences
            var correctedPos = new Vector2(screenPos.X - Resolution.GetViewport().X, screenPos.Y - Resolution.GetViewport().Y);
            // Now transform that position by the inverted matrix which is used for scaling the display. This way, the position will be scaled to the virtual screen size.
            return Vector2.Transform(correctedPos, Matrix.Invert(Resolution.getTransformationMatrix()));
        }
    }
}
