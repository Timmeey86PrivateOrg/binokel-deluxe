using System;
using System.Collections.Generic;

// DOCUMENTED

namespace BinokelDeluxe.Core
{
    /// <summary>
    /// Stores information required for creating an identical game.
    /// </summary>
    public class GameCreationInfo
    {
        /// <summary>
        /// The version of the game state store/restore mechanism, in case it gets changed in future. This allows for upgrading older saves.
        /// </summary>
        public Version Version { get; set; }
        /// <summary>
        /// The random seed which was used to iniitalize the random number generator.
        /// </summary>
        public int RandomSeed { get; set; }
        /// <summary>
        /// Contains a type for each player position which is null in case of a human player or an AI Strategy type otherwise.
        /// </summary>
        public IList<Type> AIStrategyTypes { get; set; }
        /// <summary>
        /// The rule settings the game was started with.
        /// </summary>
        public GameLogic.RuleSettings RuleSettings { get; set; }

        /// <summary>
        /// Checks whether or not a human player is at the given position.
        /// </summary>
        /// <param name="playerPosition">The position of the player, where 0 = human player (single player) or host (multiplayer).</param>
        /// <returns>True if the player is a human player, false if it is an AI player.</returns>
        public bool PlayerIsHuman(int playerPosition)
        {
            if (AIStrategyTypes == null)
            {
                throw new InvalidOperationException("The list of AI strategy types was unexpectedly null.");
            }
            if (AIStrategyTypes.Count <= playerPosition)
            {
                throw new ArgumentOutOfRangeException(
                    "playerPosition",
                    String.Format(
                        "A caller wanted to know if player #{0} is human, but only {1} players were used to create the game.",
                        playerPosition,
                        AIStrategyTypes.Count
                        )
                    );
            }
            return AIStrategyTypes[playerPosition] == null;
        }
    }
}
