using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinokelDeluxe.DevUI
{
    /// <summary>
    /// This is the central interface for any UI fragment.
    /// </summary>
    internal interface IUIFragment
    {
        void Load(ContentManager content);
        void Update(InputHandler inputHandler);
        void Draw(SpriteBatch spriteBatch);
    }
}
