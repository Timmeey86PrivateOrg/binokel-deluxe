// DOCUMENTED

namespace BinokelDeluxe.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Stores information required for creating an identical game.
    /// </summary>
    public sealed class GameCreationInfo : IEquatable<GameCreationInfo>
    {
        /// <summary>
        /// Gets or sets the version of the game state store/restore mechanism, in case it gets changed in future. This allows for upgrading older saves.
        /// </summary>
        public Common.SerializableVersion Version { get; set; }

        /// <summary>
        /// Gets or sets the random seed which was used to iniitalize the random number generator.
        /// </summary>
        public int RandomSeed { get; set; }

        /// <summary>
        /// Gets or sets a type for each player position which is null in case of a human player or an AI Strategy type otherwise.
        /// </summary>
        public List<string> AIStrategyTypes { get; set; }

        /// <summary>
        /// Gets or sets the rule settings the game was started with.
        /// </summary>
        public GameLogic.RuleSettings RuleSettings { get; set; }

        /// <summary>
        /// Checks whether or not a human player is at the given position.
        /// </summary>
        /// <param name="playerPosition">The position of the player, where 0 = human player (single player) or host (multiplayer).</param>
        /// <returns>True if the player is a human player, false if it is an AI player.</returns>
        public bool PlayerIsHuman(int playerPosition)
        {
            if (this.AIStrategyTypes == null)
            {
                throw new InvalidOperationException("The list of AI strategy types was unexpectedly null.");
            }

            if (this.AIStrategyTypes.Count <= playerPosition)
            {
                throw new ArgumentOutOfRangeException(
                    "playerPosition",
                    string.Format(
                        "A caller wanted to know if player #{0} is human, but only {1} players were used to create the game.",
                        playerPosition,
                        this.AIStrategyTypes.Count));
            }

            return this.AIStrategyTypes[playerPosition] == null;
        }

        /// <summary>
        /// Checks whether or not other is equal to this. Two GameCreationInfo objects are equal if:
        /// - They have the same version and
        /// - They have the same random seed and
        /// - They have the same list of strategy types in the same order and
        /// - They have equal rule settings.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns>True if the two objects are equal.</returns>
        public bool Equals(GameCreationInfo other)
        {
            if (other == null)
            {
                return false;
            }

            return
                Common.ValueComparer<Common.SerializableVersion>.Equals(this.Version, other.Version) &&
                this.RandomSeed == other.RandomSeed &&
                Common.ListComparer<string>.Equals(this.AIStrategyTypes, other.AIStrategyTypes) &&
                Common.ValueComparer<GameLogic.RuleSettings>.Equals(this.RuleSettings, other.RuleSettings);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as GameCreationInfo);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // overflow is fine, just wrap
            unchecked
            {
                int hash = 17;
                hash = (hash * 29) + Common.ValueComparer<Common.SerializableVersion>.GetHashCode(this.Version);
                hash = (hash * 29) + this.RandomSeed;
                hash = (hash * 29) + Common.ListComparer<string>.GetHashCode(this.AIStrategyTypes);
                hash = (hash * 29) + Common.ValueComparer<GameLogic.RuleSettings>.GetHashCode(this.RuleSettings);
                return hash;
            }
        }
    }
}
