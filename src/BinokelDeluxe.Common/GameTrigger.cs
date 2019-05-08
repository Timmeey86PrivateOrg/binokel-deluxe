// DOCUMENTED

namespace BinokelDeluxe.Common
{
    /// <summary>
    /// Defines triggers the state machine will react to.
    /// See https://raw.githubusercontent.com/Timmeey86/binokel-deluxe/master/doc/modelio/img/08_01_SingleGameStateMachine.png for the used triggers and related transitions.
    /// Check the comment of each event to know which trigger to fire.
    /// </summary>
    public enum GameTrigger
    {
        /// <summary>
        /// This is used to indicate that no trigger was associated with an action, rather than having a nullable GameTrigger member.
        /// </summary>
        None,

        /// <summary>
        /// Send this trigger to start the game.
        /// </summary>
        GameStarted,

        /// <summary>
        /// Send this trigger once a dealing animation was finished.
        /// </summary>
        DealingFinished,

        /// <summary>
        /// Send this trigger whenever a bid was placed by any player.
        /// </summary>
        BidPlaced,

        /// <summary>
        /// Send this trigger whenever the bid of the current player was countered.
        /// </summary>
        BidCountered,

        /// <summary>
        /// Send this trigger whenever any player passed.
        /// </summary>
        Passed,

        /// <summary>
        /// Send this trigger whenever a player switch was finished (e.g. when another player is shown as the active player).
        /// </summary>
        PlayerSwitched,

        /// <summary>
        /// Send this trigger when a player decided to go out in the "Dabb" phase.
        /// </summary>
        GoingOut,

        /// <summary>
        /// Send this trigger when a player announced a "Durch" in the "Dabb" phase.
        /// </summary>
        DurchAnnounced,

        /// <summary>
        /// Send this trigger when a player announced a "Bettel" in the "Dabb" phase.
        /// </summary>
        BettelAnnounced,

        /// <summary>
        /// Send this trigger when a player finishes the "Dabb" phase by selecting a trump for the trick taking phase.
        /// </summary>
        TrumpSelected,

        /// <summary>
        /// Send this trigger as soon as all players confirmed seeing the melds of all players.
        /// </summary>
        MeldsSeenByAllPlayers,

        /// <summary>
        /// Send this trigger as soon as the UI is ready for trick taking. This is a separate step since the UI usually has to initialize
        /// several things between displaying melds and letting a player place the first card.
        /// </summary>
        ReadyForTrickTaking,

        /// <summary>
        /// Send this trigger whenever a card was placed by the active player.
        /// </summary>
        CardPlaced,

        /// <summary>
        /// Send this trigger whenever an invalid card was placed.
        /// </summary>
        InvalidCardPlaced,

        /// <summary>
        /// Send this trigger when the card being placed beats all other cards currently in the middle. The first card is always a winning card.
        /// </summary>
        WinningCardPlaced,

        /// <summary>
        /// Send this trigger when the card being placed does not beat all other cards currently in the middle.
        /// </summary>
        LosingCardPlaced,

        /// <summary>
        /// Send this trigger when reverting an invalid move was finished.
        /// </summary>
        RevertingFinished,

        /// <summary>
        /// Send this trigger whenever a new trick taking round started.
        /// </summary>
        NewRoundStarted,

        /// <summary>
        /// Send this trigger whenever the score calculation was finished.
        /// </summary>
        ScoreCalculationFinished,

        /// <summary>
        /// Internal trigger used for automatic state transitions. Do not use this outside of the state machine.
        /// </summary>
        Internal,
    }
}
