using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinokelDeluxe.DevUI
{
    /// <summary>
    /// This class allows creating primitive textures used for drawing dots, lines, circles and so on.
    /// </summary>
    public static class TextureFunctions
    {
        /// <summary>
        /// Creates a new texture with the specified width and height, using a custom function which returns a color for each pixel.
        /// Taken from https://stackoverflow.com/a/32429364/808151 and adapted so the function uses X and Y as input rather than a single int.
        /// </summary>
        /// <param name="device">The graphics device used to render the texture.</param>
        /// <param name="width">The width of the texture in pixels.</param>
        /// <param name="height">The height of the texture in pixels.</param>
        /// <param name="paint">A function which returns the Color for each pixel.</param>
        /// <returns>The created texture.</returns>
        public static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, int, Color> paint)
        {
            //initialize a texture
            Texture2D texture = new Texture2D(device, width, height);

            //the array holds the color for each pixel in the texture
            Color[] data = new Color[width * height];
            int pixel = 0;
            for (int x = 0; x < width; x++ )
            {
                for (int y = 0; y < height; y++)
                {
                    //the function applies the color according to the specified pixel
                    data[pixel] = paint(x, y);
                    pixel++;
                }
            }

            //set the color
            texture.SetData(data);

            return texture;
        }

        /// <summary>
        /// Creates a new texture with the specified width and height, using the same color for all pixels.
        /// Taken from https://stackoverflow.com/a/32429364/808151.
        /// </summary>
        /// <param name="device">The graphics device used to render the texture.</param>
        /// <param name="width">The width of the texture in pixels.</param>
        /// <param name="height">The height of the texture in pixels.</param>
        /// <param name="color">The color each pixel will have.</param>
        /// <returns>The created texture.</returns>
        public static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Color color)
        {
            return CreateTexture(device, width, height, (x,y) => color);
        }

        /// <summary>
        /// Convenience function for creating a round dot in a square of size x size pixels. Currently, no anti-aliasing is performed.
        /// </summary>
        /// <param name="device">The graphics device used to render the texture.</param>
        /// <param name="size">The size in pixels.</param>
        /// <param name="color">The color of the dot.</param>
        /// <returns>The created texture.</returns>
        public static Texture2D CreateDot(GraphicsDevice device, int size, Color color)
        {
            var center = (size-1) / 2.0f;
            return CreateTexture(device, size, size, (x,y) =>
            {
                var xDistance = Math.Abs(x - center);
                var yDistance = Math.Abs(y - center);
                var distance = Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
                if (distance <= (size / 2.0d))
                {
                    return color;
                }
                else
                {
                    return Color.Transparent;
                }
            });
        }

        /// <summary>
        /// Creates a line along the X axis with the given length and thickness. The line can be rotated in the draw method as necessary.
        /// Note: You could also use CreateDot(device, 1, color) instead, and draw that into a larger rectangle in the draw method.
        /// </summary>
        /// <param name="device">The graphics device used to render the texture.</param>
        /// <param name="length">The length of the line.</param>
        /// <param name="thickness">The thickness of the line. Currently, the line will be a rectangle, but rounded corners could be added in future.</param>
        /// <param name="color">The color of the dot.</param>
        /// <returns>The created texture.</returns>
        public static Texture2D CreateLine(GraphicsDevice device, int length, int thickness, Color color)
        {
            return CreateTexture(device, length, thickness, color);
        }
    }
}
