using System;
using System.Collections.Generic;
using System.Text;

// DOCUMENTED

namespace BinokelDeluxe.Core
{
    /// <summary>
    /// Stores delta information about a single sate change in case of human interaction. In case of AI interaction, no state change is stored since this can be reproduced automatically.
    /// </summary>
    public class GameStateChangeInfo
    {
        /// <summary>
        /// The trigger which was used by the human player to initiate the state change (optional).
        /// </summary>
        public GameLogic.SingleGameTrigger? HumanTrigger { get; set; } = null;
        /// <summary>
        /// The card which was played by the human to trigger the state change (optional).
        /// </summary>
        public Common.Card CardPlayedByHuman { get; set; } = null;
    }
}
