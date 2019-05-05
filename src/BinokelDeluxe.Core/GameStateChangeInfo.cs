// DOCUMENTED

namespace BinokelDeluxe.Core
{
    using System;

    /// <summary>
    /// Stores delta information about a single sate change in case of human interaction. In case of AI interaction, no state change is stored since this can be reproduced automatically.
    /// </summary>
    public sealed class GameStateChangeInfo : IEquatable<GameStateChangeInfo>
    {
        /// <summary>
        /// Gets or sets the trigger which was used by the human player to initiate the state change (optional).
        /// </summary>
        public Common.GameTrigger HumanTrigger { get; set; } = Common.GameTrigger.None;

        /// <summary>
        /// Gets or sets the card which was played by the human to trigger the state change (optional).
        /// </summary>
        public Common.Card CardPlayedByHuman { get; set; } = null;

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as GameStateChangeInfo);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // Overflow is fine, just wrap
            unchecked
            {
                int hash = 17;
                hash = (hash * 29) + this.HumanTrigger.GetHashCode();
                hash = (hash * 29) + Common.ValueComparer<Common.Card>.GetHashCode(this.CardPlayedByHuman);
                return hash;
            }
        }

        /// <inheritdoc/>
        public bool Equals(GameStateChangeInfo other)
        {
            if (other == null)
            {
                return false;
            }

            return Enum.Equals(this.HumanTrigger, other.HumanTrigger) && Common.ValueComparer<Common.Card>.Equals(this.CardPlayedByHuman, other.CardPlayedByHuman);
        }
    }
}
