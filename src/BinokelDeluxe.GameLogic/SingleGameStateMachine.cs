using System;

namespace BinokelDeluxe.GameLogic
{
    /// <summary>
    /// This class manages the central state machine and serves as an event bridge for it.
    /// </summary>
    public class SingleGameStateMachine : ISingleGameEventSender
    {
        // Interface implementation. See ISingleGameEventSender for comments.
        public event EventHandler<PlayerPairEventArgs> DealingStarted;
        public event EventHandler WaitingForBidOrPassStarted;
        public event EventHandler WaitingForCounterOrPassStarted;
        public event EventHandler<PlayerPairEventArgs> SwitchingCurrentPlayerStarted;
        public event EventHandler<PlayerNumberEventArgs> SwitchingNextPlayerStarted;
        public event EventHandler<PlayerNumberEventArgs> ExchangingCardsWithDabbStarted;
        public event EventHandler<PlayerNumberEventArgs> CalculatingGoingOutScoreStarted;
        public event EventHandler MeldingStarted;
        public event EventHandler<PlayerNumberEventArgs> WaitingForCardStarted;
        public event EventHandler<PlayerNumberEventArgs> ValidatingCardStarted;
        public event EventHandler<PlayerNumberEventArgs> RevertingInvalidMoveStarted;
        public event EventHandler<PlayerNumberEventArgs> SwitchingToNextPlayerStarted;
        public event EventHandler<PlayerNumberEventArgs> StartingNewRoundStarted;
        public event EventHandler CountingPlayerOrTeamScoresStarted;

        public struct SingleGameProperties
        {
            public int NumberOfPlayers { get; set; }
            public int DealerNumber { get; set; }
            public int RemainingCards { get; set; }
            public int CurrentPlayerNumber { get; set; }
            public int NextPlayerNumber { get; set; }
            public int TrickWinnerNumber { get; set; }
        }

        /// <summary>
        /// Fires an event which takes event arguments safely. If no event listener is connected, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the event arguments.</typeparam>
        /// <param name="eventHandler">The handler of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void FireEvent<T>(EventHandler<T> eventHandler, T eventArgs)
        {
            var handler = eventHandler;
            if (handler != null)
            {
                handler(this, eventArgs);
            }
            else
            {
                throw new UnconnectedEventException();
            }
        }

        /// <summary>
        /// Fires an event which does not take event arguments safely. If no event listener is connected, an exception will be thrown.
        /// </summary>
        /// <param name="eventHandler">The handler of the event.</param>
        private void FireEvent(EventHandler eventHandler)
        {
            var handler = eventHandler;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
            else
            {
                throw new UnconnectedEventException();
            }
        }

        private Stateless.StateMachine<SingleGameState, SingleGameTrigger> _stateMachine = null;

        /// <summary>
        /// Creates a state machine for the given rule settings.
        /// Check https://github.com/Timmeey86/binokel-deluxe/wiki/State-Machines for an overview of possible states and transitions.
        /// </summary>
        /// <param name="ruleSettings">The settings for this game.</param>
        /// <param name="dealerNumber">The number of the dealing player for this game, starting at zero.</param>
        /// <returns>A state machine for a single game.</returns>
        public void RefreshStateMachine(RuleSettings ruleSettings, int dealerNumber)
        {
            // Configure initial attributes
            var properties = new SingleGameProperties
            {
                NumberOfPlayers = ruleSettings.GameType == GameType.ThreePlayerGame ? 3 : 4,
                DealerNumber = dealerNumber,
                RemainingCards = ruleSettings.SevensAreIncluded ? 48 : 40,
                CurrentPlayerNumber = -1,
                NextPlayerNumber = -1,
                TrickWinnerNumber = -1
            };

            _stateMachine = new Stateless.StateMachine<SingleGameState, SingleGameTrigger>(SingleGameState.Initial);
            // Ignore triggers which should not be available.
            _stateMachine.OnUnhandledTrigger((state, trigger) => { });

            _stateMachine.Configure(SingleGameState.Initial)
                .Permit(SingleGameTrigger.GameStarted, SingleGameState.Dealing);

            // Dealing phase
            _stateMachine.Configure(SingleGameState.Dealing)
                .Permit(SingleGameTrigger.DealingFinished, SingleGameState.Bidding_WaitingForCurrentPlayer)
                .OnEntry(() =>
                {
                    properties.CurrentPlayerNumber = (properties.DealerNumber + 1) % properties.NumberOfPlayers;
                    properties.NextPlayerNumber = (properties.DealerNumber + 2) % properties.NumberOfPlayers;

                    FireEvent(DealingStarted, new PlayerPairEventArgs(properties.CurrentPlayerNumber,properties.NextPlayerNumber));
                });

            // Bidding phase
            _stateMachine.Configure(SingleGameState.Bidding_WaitingForCurrentPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(SingleGameTrigger.BidPlaced, SingleGameState.Bidding_WaitingForNextPlayer)
                .Permit(SingleGameTrigger.Passed, SingleGameState.Bidding_SwitchingCurrentPlayer)
                .OnEntry(() => FireEvent(WaitingForBidOrPassStarted));

            _stateMachine.Configure(SingleGameState.Bidding_WaitingForNextPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(SingleGameTrigger.BidCountered, SingleGameState.Bidding_WaitingForCurrentPlayer)
                .Permit(SingleGameTrigger.Passed, SingleGameState.Bidding_SwitchingNextPlayer)
                .OnEntry(() => FireEvent(WaitingForCounterOrPassStarted));

            _stateMachine.Configure(SingleGameState.Bidding_SwitchingCurrentPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(SingleGameTrigger.PlayerSwitched, SingleGameState.Bidding_WaitingForCurrentPlayer)
                .Permit(SingleGameTrigger.Internal_SwitchToDabbExchange, SingleGameState.ExchangingCardsWithTheDabb)
                .OnEntry(() =>
                {
                    xxx wenn der letzte current passed, sollte der next player noch zum current werden.
                    if ((properties.CurrentPlayerNumber + 1) % properties.NumberOfPlayers == dealerNumber)
                    {
                        // All players had the chance to bid and n-1 players passed. End the bidding phase.
                        _stateMachine.Fire(SingleGameTrigger.Internal_SwitchToDabbExchange);
                    }
                    else
                    {
                        properties.CurrentPlayerNumber = properties.NextPlayerNumber;
                        properties.NextPlayerNumber = (properties.CurrentPlayerNumber + 1) % properties.NumberOfPlayers;

                        FireEvent(SwitchingCurrentPlayerStarted, new PlayerPairEventArgs(properties.CurrentPlayerNumber, properties.NextPlayerNumber));
                    }
                });

            _stateMachine.Configure(SingleGameState.Bidding_SwitchingNextPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(SingleGameTrigger.PlayerSwitched, SingleGameState.Bidding_WaitingForNextPlayer)
                .Permit(SingleGameTrigger.Internal_SwitchToDabbExchange, SingleGameState.ExchangingCardsWithTheDabb)
                .OnEntry(() =>
                {
                    if (properties.NextPlayerNumber == dealerNumber)
                    {

                    }
                });
        }
    }
}
