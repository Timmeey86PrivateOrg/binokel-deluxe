// DOCUMENTED

namespace BinokelDeluxe.GameLogic
{
    /// <summary>
    /// See https://github.com/Timmeey86/binokel-deluxe/blob/statemachine/doc/modelio/img/08_01_SingleGameStateMachine.png for a graphical representation of the states.
    /// </summary>
    internal enum SingleGameState
    {
        /// <summary>
        /// The initial state of the game.
        /// </summary>
        Initial,

        /// <summary>
        /// The phase where the dealer is distributing cards to the players and the dabb.
        /// </summary>
        Dealing,

        /// <summary>
        /// The phase where players are trying to outbid each other.
        /// </summary>
        Bidding,

        /// <summary>
        /// The substate of the bidding phase where the current player needs to place the first bid (or pass directly).
        /// </summary>
        Bidding_WaitingForFirstBid,

        /// <summary>
        /// The substate of the bidding phase where a player passed before placing the first bid, and the next player is being notified that they may place the bid.
        /// </summary>
        Bidding_SwitchingFirstBidPlayer,

        /// <summary>
        /// The substate of the bidding phase where an action (bid or pass) is being awaited from the current player.
        /// </summary>
        Bidding_WaitingForCurrentPlayer,

        /// <summary>
        /// The substate of the bidding phase where an action (counter or pass) is being awaited from the next player.
        /// </summary>
        Bidding_WaitingForNextPlayer,

        /// <summary>
        /// The substate of the bidding phase where the current player passed (after the initial bid) and the next player is about to become the new current player.
        /// </summary>
        Bidding_SwitchingCurrentPlayer,

        /// <summary>
        /// The substate of the bidding phase where the next player passed and the player after that one is about to become the new "next" player.
        /// </summary>
        Bidding_SwitchingNextPlayer,

        /// <summary>
        /// The phase where the winner of the bidding phase is allowed to exchange cards with the dabb.
        /// </summary>
        ExchangingCardsWithTheDabb,

        /// <summary>
        /// The phase where the winner of the bidding phase decided to do a "Durch" (This is TBD).
        /// </summary>
        Durch,

        /// <summary>
        /// The phase where the winner of the bidding phase decided to do a "Bettel" (This is TBD).
        /// </summary>
        Bettel,

        /// <summary>
        /// The phase where the winner of the bidding phase decided to "go out" and the score for all players is being calculated.
        /// </summary>
        CountingGoingOutScore,

        /// <summary>
        /// The phase where the winner of the bidding phase selected a trump and everyone is displaying their melds to everyone else.
        /// </summary>
        Melding,

        /// <summary>
        /// The phase where tricks are being played for.
        /// </summary>
        TrickTaking,

        /// <summary>
        /// The substate of the trick taking phase where a card is being awaited from the current player.
        /// </summary>
        TrickTaking_WaitingForCurrentPlayer,

        /// <summary>
        /// The substate of the trick taking phase where the card played by the current player is being validated.
        /// </summary>
        TrickTaking_ValidatingCard,

        /// <summary>
        /// The substate of the trick taking phase where an invalid card was placed and the move needs to be reverted.
        /// </summary>
        TrickTaking_RevertingInvalidMove,

        /// <summary>
        /// The substate of the trick taking phase where the winning player is being remembered for the score calculation later on.
        /// </summary>
        TrickTaking_RememberingWinningPlayer,

        /// <summary>
        /// The substate of the trick taking phase where a player placed a valid card and the next player is being notified that they may place a card.
        /// </summary>
        TrickTaking_SwitchingToNextPlayer,

        /// <summary>
        /// The substate of the trick taking phase where a new round is being started since the previous trick is over.
        /// </summary>
        TrickTaking_StartingNewRound,

        /// <summary>
        /// The substate of the trick taking phase where all cards have been played and the final score for the game is being calculated.
        /// </summary>
        CountingGameScore,

        /// <summary>
        /// The state where the state machine ended, and a new one needs to be initialized for the next game.
        /// </summary>
        End,
    }
}
