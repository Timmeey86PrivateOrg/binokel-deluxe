using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

// DOCUMENTED

namespace BinokelDeluxe.GameLogic
{
    /// <summary>
    /// This interface can be used to send triggers to the internal state machine.
    /// See https://github.com/Timmeey86/binokel-deluxe/blob/statemachine/doc/modelio/img/08_01_SingleGameStateMachine.png for the used triggers and related transitions.
    /// </summary>
    public interface ISingleGameTriggerSink
    {
        void SendTrigger(Common.GameTrigger trigger);
    }

    /// <summary>
    /// Thrown when there is no listener connected to an event.
    /// Since the state machine will not switch states without anyone sending a trigger, the application would be stuck in that case.
    /// </summary>
    [System.Serializable]
    public class UnconnectedEventException : Exception
    {
        public UnconnectedEventException(string eventName)
            : base(String.Format(
                "Nothing is connected to the {0} event. This would cause the application to be stuck. Connect something to this event and fire the right trigger at the end of it.",
                eventName
                ))
        {
        }

        protected UnconnectedEventException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }

    public class PlayerPairEventArgs : EventArgs
    {
        public PlayerPairEventArgs(int currentPlayerPosition, int nextPlayerPosition)
        {
            CurrentPlayerPosition = currentPlayerPosition;
            NextPlayerPosition = nextPlayerPosition;
        }

        public int CurrentPlayerPosition { get; set; }
        public int NextPlayerPosition { get; set; }
    }
    public class PlayerPositionEventArgs : EventArgs
    {
        public PlayerPositionEventArgs(int playerPosition)
        {
            PlayerPosition = playerPosition;
        }

        public int PlayerPosition { get; set; }
    }
    /// <summary>
    /// Defines events which are sent by the machine whenever it is waiting for a trigger.
    /// </summary>
    public interface ISingleGameEventSource
    {
        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Start a dealing animation and uncover each player's own cards.
        /// Send a DealingFinished trigger when done.
        /// </summary>
        event EventHandler<PlayerPairEventArgs> DealingStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Offer the player the choice between placing the initial bid or passing.
        /// Send a BidPlaced or Passed trigger when done.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> WaitingForFirstBidStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Switch the current (and the next) player (but do not offer them options yet).
        /// Send a PlayerSwitched trigger when done.
        /// </summary>
        event EventHandler<PlayerPairEventArgs> SwitchingPlayerBeforeFirstBidStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Offer the player the choice between placing a bid (the initial one or the previous one +10) and passing.
        /// Send a BidPlaced or Passed trigger when done.
        /// Make sure the player cannot accidentally send a trigger twice in this state.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> WaitingForBidOrPassStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Offer the player the choice between countering a bid or passing.
        /// Send a BidPlaced or Passed trigger when done.
        /// Make sure the player cannot accidentally send a trigger twice in this state.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> WaitingForCounterOrPassStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Switch the current (and the next) player for bidding (but do not offer them options yet).
        /// Send a PlayerSwitched trigger when done.
        /// </summary>
        event EventHandler<PlayerPairEventArgs> SwitchingCurrentBidPlayerStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Switch the next player for counter-bidding (but do not offer them options yet).
        /// Send a PlayerSwitched trigger when done.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> SwitchingCounterBidPlayerStarted;


        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Uncover the dabb and wait for confirmation of all players that they have seen it.
        /// Then, offer the player who won the bid the option to exchange cards with the dabb and give them the following choices:
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
        event EventHandler<PlayerPositionEventArgs> ExchangingCardsWithDabbStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Calculate the score for each player after the player who won the bid goes out.
        /// Send a ScoreCalculationFinished trigger when done.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> CalculatingGoingOutScoreStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Display the cards which can be melded by each player and display the total meld score for each player.
        /// Ask every player for confirmation that they saw the melds.
        /// Send a MeldsSeenByAllPlayers trigger when done.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> MeldingStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Set every player up for the trick taking phase by showing them only their cards, with nothing in the middle.
        /// Send a ReadyForTrickTaking trigger when done.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> TrickTakingStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Offer the player the chance to play a card of their choice (including invalid cards).
        /// Send a CardPlaced trigger when done.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> WaitingForCardStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Remove any potential highlighting, then validate the card which was placed by the player.
        /// In case of a valid card, find out if the card beats the current winning card.
        /// Send either an InvalidCardPlaced, a WinningCardPlaced or a LosingCardPlaced event when done.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> ValidatingCardStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Revert the last move and highlight the cards which are valid.
        /// Send a RevertingFinished trigger when done.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> RevertingInvalidMoveStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Switch to the player which is identified by the player Position in the event arguments (but do not offer them choices yet).
        /// Send a PlayerSwitched trigger when done.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> SwitchingCurrentTrickPlayerStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Switch to the trick-winning player which is identified by the player Position in the event arguments and remember the cards won by this player.
        /// Send a NewRoundStarted trigger when done.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> StartingNewRoundStarted;

        /// <summary>
        /// Event listeners should be implemented as follows:
        /// Calculate the scores for each player or team.
        /// The player argument identifies the player who won the last trick.
        /// Send a ScoreCalculationFinished trigger when done.
        /// </summary>
        event EventHandler<PlayerPositionEventArgs> CountingPlayerOrTeamScoresStarted;

        /// <summary>
        /// Lets event listeners know that the game was finished and a new one can be started.
        /// You need to prepare a new game for the next round.
        /// </summary>
        event EventHandler GameFinished;
    }
}
