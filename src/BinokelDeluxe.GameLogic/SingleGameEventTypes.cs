using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BinokelDeluxe.GameLogic
{
    /// <summary>
    /// Defines triggers the state machine will react to.
    /// See https://github.com/Timmeey86/binokel-deluxe/blob/statemachine/doc/modelio/img/08_01_SingleGameStateMachine.png for a used triggers and related transitions.
    /// Check the comment of each event to know which trigger to fire.
    /// </summary>
    public enum TriggerType
    {
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
        ScoreCalculationFinished
    }

    /// <summary>
    /// Thrown when there is no listener connected to an event.
    /// Since the state machine will not switch states without anyone sending a trigger, the application would be stuck in that case.
    /// </summary>
    [System.Serializable]
    public class UnconnectedEventException : Exception
    {
        public UnconnectedEventException(Type eventType)
            : base(String.Format(
                "Event {0} is not handled and would cause the application to be stuck. Connect something to this event and fire the right trigger at the end of it.", 
                eventType.FullName
                ))
        {
        }

        protected UnconnectedEventException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Defines events which are sent by the machine whenever it is waiting for a trigger.
    /// </summary>
    public static class Events
    {
        public class DealingStartedEventArgs : EventArgs
        {
            public int CurrentPlayerNumber { get; set; }
            public int NextPlayerNumber { get; set; }
        }
        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Start a dealing animation and uncover each player's own cards.
        /// Send a DealingFinished trigger when done.
        /// </summary>
        public static event EventHandler<DealingStartedEventArgs> DealingStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Offer the player the choice between placing a bid (the initial one or the previous one +10) and passing.
        /// Send a BidPlaced or Passed trigger when done.
        /// Make sure the player cannot accidentally send a trigger twice in this state.
        /// </summary>
        public static event EventHandler WaitingForBidOrPassStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Offer the player the choice between countering a bid or passing.
        /// Send a BidPlaced or Passed trigger when done.
        /// Make sure the player cannot accidentally send a trigger twice in this state.
        /// </summary>
        public static event EventHandler WaitingForCounterOrPassStarted;

        public class SwitchingCurrentPlayerEventArgs : EventArgs
        {
            public int CurrentPlayerNumber { get; set; }
            public int NextPlayerNumber { get; set; }
        }
        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Switch the current (and the next) player for bidding (but do not offer them options yet).
        /// Send a PlayerSwitched trigger when done.
        /// </summary>
        public static event EventHandler<SwitchingCurrentPlayerEventArgs> SwitchingCurrentPlayerStarted;

        public class SwitchingNextPlayerEventArgs : EventArgs
        {
            public int NextPlayerNumber { get; set; }
        }
        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Switch the next player for counter-bidding (but do not offer them options yet).
        /// Send a PlayerSwitched trigger when done.
        /// </summary>
        public static event EventHandler<SwitchingNextPlayerEventArgs> SwitchingNextPlayerStarted;


        public class PlayerNumberEventArgs : EventArgs
        {
            public int PlayerNumber { get; set; }
        }
        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Offer the player who won the bid the option to exchange cards with the dabb and give them the following choices:
        /// - Selecting a trump and playing normally
        /// - Selecting a trump and going out
        /// - Announcing a Durch
        /// - Announcing a Bettel
        /// Dependent on their choice, send one of the following triggers when done:
        /// - TrumpSelected
        /// - GoingOut
        /// - DurchAnnounced
        /// - BettelAnnouced
        /// </summary>
        public static event EventHandler<PlayerNumberEventArgs> ExchangingCardsWithDabbStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Calculate the score for each player after the player who won the bid goes out.
        /// Send a ScoreCalculationFinished trigger when done.
        /// </summary>
        public static event EventHandler<PlayerNumberEventArgs> CalculatingGoingOutScoreStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Display the cards which can be melded by each player and display the total meld score for each player.
        /// Ask every player for confirmation that they saw the melds.
        /// Send a MeldsSeenByAllPlayers trigger when done.
        /// </summary>
        public static event EventHandler MeldingStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Offer the player the chance to play a card of their choice (including invalid cards).
        /// Send a CardPlaced trigger when done.
        /// </summary>
        public static event EventHandler<PlayerNumberEventArgs> WaitingForCardStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Remove any potential highlighting, then validate the card which was placed by the player.
        /// In case of a valid card, find out if the card beats the current winning card.
        /// Send either an InvalidCardPlaced, a WinningCardPlaced or a LosingCardPlaced event when done.
        /// </summary>
        public static event EventHandler<PlayerNumberEventArgs> ValidatingCardStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Revert the last move and highlight the cards which are valid.
        /// Send a RevertingFinished trigger when done.
        /// </summary>
        public static event EventHandler<PlayerNumberEventArgs> RevertingInvalidMoveStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Switch to the player which is identified by the player number in the event arguments (but do not offer them choices yet).
        /// Send a PlayerSwitched trigger when done.
        /// </summary>
        public static event EventHandler<PlayerNumberEventArgs> SwitchingToNextPlayerStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Switch to the trick-winning player which is identified by the player number in the event arguments and remember the cards won by this player.
        /// Send a NewRoundStarted trigger when done.
        /// </summary>
        public static event EventHandler<PlayerNumberEventArgs> StartingNewRoundStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Calculate the scores for each player or team.
        /// Send a ScoreCalculationFinished trigger when done.
        /// </summary>
        public static event EventHandler CountingPlayerOrTeamScoresStarted;
    }
}
