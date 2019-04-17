using Stateless;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinokelDeluxe.GameLogic
{
    /// <summary>
    /// See https://github.com/Timmeey86/binokel-deluxe/blob/statemachine/doc/modelio/img/08_01_SingleGameStateMachine.png for a graphical representation of the states.
    /// </summary>
    public enum SingleGameState
    {
        Initial,
        Dealing,
        Bidding,
        Bidding_WaitingForFirstBid,
        Bidding_SwitchingFirstBidPlayer,
        Bidding_WaitingForCurrentPlayer,
        Bidding_WaitingForNextPlayer,
        Bidding_SwitchingCurrentPlayer,
        Bidding_SwitchingNextPlayer,
        ExchangingCardsWithTheDabb,
        Durch,
        Bettel,
        CountingGoingOutScore,
        Melding,
        TrickTaking,
        TrickTaking_WaitingForCurrentPlayer,
        TrickTaking_ValidatingCard,
        TrickTaking_RevertingInvalidMove,
        TrickTaking_RememberingWinningPlayer,
        TrickTaking_SwitchingToNextPlayer,
        TrickTaking_StartingNewRound,
        CountingGameScore,
        End
    }
}
