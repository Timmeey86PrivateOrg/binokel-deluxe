using System;

// DOCUMENTED

namespace BinokelDeluxe.Core
{
    /// <summary>
    /// Stores delta information about a single sate change in case of human interaction. In case of AI interaction, no state change is stored since this can be reproduced automatically.
    /// </summary>
    public sealed class GameStateChangeInfo : IEquatable<GameStateChangeInfo>
    {
        /// <summary>
        /// The trigger which was used by the human player to initiate the state change (optional).
        /// </summary>
        public Common.GameTrigger HumanTrigger { get; set; } = Common.GameTrigger.None;
        /// <summary>
        /// The card which was played by the human to trigger the state change (optional).
        /// </summary>
        public Common.Card CardPlayedByHuman { get; set; } = null;

        public override bool Equals(object obj)
        {
            return Equals(obj as GameStateChangeInfo);
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 29 + HumanTrigger.GetHashCode();
                hash = hash * 29 + Common.ValueComparer<Common.Card>.GetHashCode(CardPlayedByHuman);
                return hash;
            }
        }
        public bool Equals(GameStateChangeInfo other)
        {
            if (other == null)
            {
                return false;
            }
            return Enum.Equals(HumanTrigger, other.HumanTrigger) && Common.ValueComparer<Common.Card>.Equals(CardPlayedByHuman, other.CardPlayedByHuman);

        }
    }
}
