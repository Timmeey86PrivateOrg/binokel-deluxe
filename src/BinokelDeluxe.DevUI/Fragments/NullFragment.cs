using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BinokelDeluxe.DevUI.Fragments
{
    /// <summary>
    /// This class implements the null object pattern for the IUIFragment interface.
    /// Using this class allows skipping a null check in every update and paint call.
    /// </summary>
    internal class NullFragment : IUIFragment
    {
        public void Load(ContentManager content)
        {
            // Null object. Do nothing
        }

        public void Update(InputHandler inputHandler)
        {
            // Null object. Do nothing.
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            // Null object. Do nothing.
        }
    }
}
