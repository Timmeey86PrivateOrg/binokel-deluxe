// DOCUMENTED

namespace BinokelDeluxe.GameLogic
{
    using System;

    /// <summary>
    /// Provides event arguments for a pair of players.
    /// </summary>
    public class PlayerPairEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerPairEventArgs"/> class.
        /// </summary>
        /// <param name="currentPlayerPosition">The position of the current player.</param>
        /// <param name="nextPlayerPosition">The position of the next player, e.g. the player who is allowed to counter the bid.</param>
        public PlayerPairEventArgs(int currentPlayerPosition, int nextPlayerPosition)
        {
            this.CurrentPlayerPosition = currentPlayerPosition;
            this.NextPlayerPosition = nextPlayerPosition;
        }

        /// <summary>
        /// Gets the position of the current player.
        /// </summary>
        public int CurrentPlayerPosition { get; private set; }

        /// <summary>
        /// Gets the position of the next player.
        /// </summary>
        public int NextPlayerPosition { get; private set; }
    }
}
