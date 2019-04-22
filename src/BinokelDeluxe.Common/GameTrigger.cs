using System;
using System.Collections.Generic;
using System.Text;

namespace BinokelDeluxe.Common
{
    /// <summary>
    /// Defines triggers the state machine will react to.
    /// See https://github.com/Timmeey86/binokel-deluxe/blob/statemachine/doc/modelio/img/08_01_SingleGameStateMachine.png for the used triggers and related transitions.
    /// Check the comment of each event to know which trigger to fire.
    /// </summary>
    public enum GameTrigger
    {
        None,
        GameStarted,
        DealingFinished,
        BidPlaced,
        BidCountered,
        Passed,
        PlayerSwitched,
        GoingOut,
        DurchAnnounced,
        BettelAnnounced,
        TrumpSelected,
        MeldsSeenByAllPlayers,
        CardPlaced,
        InvalidCardPlaced,
        WinningCardPlaced,
        LosingCardPlaced,
        RevertingFinished,
        NewRoundStarted,
        ScoreCalculationFinished,
        Internal
    }
}
