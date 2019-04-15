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
        ExchangingCardsWithTheDabb,
        Durch,
        Bettel,
        CountingGoingOutScore,
        Melding,
        TrickTaking,
        CountingGameScore
    }

    /// <summary>
    /// Defines the sub states of the SingleGameState.Bidding state.
    /// </summary>
    public enum BiddingState
    {
        WaitingForCurrentPlayer,
        WaitingForNextPlayer,
        SwitchingCurrentPlayer,
        SwitchingNextPlayer
    }

    /// <summary>
    /// Defines the sub states of the SingleGameState.TrickTaking state.
    /// </summary>
    public enum TrickTackingState
    {
        WaitingForCurrentPlayer,
        ValidatingCard,
        RevertingInvalidMove,
        SwitchingToNextPlayer,
        FindingOutTrickWinner
    }
}
