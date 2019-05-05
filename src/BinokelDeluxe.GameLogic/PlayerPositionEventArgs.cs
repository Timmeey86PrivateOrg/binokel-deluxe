// DOCUMENTED

namespace BinokelDeluxe.GameLogic
{
    using System;

    /// <summary>
    /// Provides event arguments for a pair of players.
    /// </summary>
    public class PlayerPositionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerPositionEventArgs"/> class.
        /// </summary>
        /// <param name="playerPosition">THe position of the relevant player.</param>
        public PlayerPositionEventArgs(int playerPosition)
        {
            this.PlayerPosition = playerPosition;
        }

        /// <summary>
        /// Gets the position of the relevant player.
        /// </summary>
        public int PlayerPosition { get; private set; }
    }
}
