using System;

// DOCUMENTED

namespace BinokelDeluxe.GameLogic
{
    /// <summary>
    /// This class manages the central state machine and serves as an event bridge for it.
    /// This class is an internal namespace class and should not be exposed directly.
    /// See https://github.com/Timmeey86/binokel-deluxe/wiki/Glossary for the difference between player positions and numbers
    /// </summary>
    class SingleGameStateMachine : ISingleGameEventSource, ISingleGameTriggerSink
    {
        // Interface implementation. See ISingleGameEventSource for comments.
        public event EventHandler<PlayerPairEventArgs> DealingStarted;
        public event EventHandler<PlayerPositionEventArgs> WaitingForFirstBidStarted;
        public event EventHandler<PlayerPairEventArgs> SwitchingPlayerBeforeFirstBidStarted;
        public event EventHandler<PlayerPositionEventArgs> WaitingForBidOrPassStarted;
        public event EventHandler<PlayerPositionEventArgs> WaitingForCounterOrPassStarted;
        public event EventHandler<PlayerPairEventArgs> SwitchingCurrentBidPlayerStarted;
        public event EventHandler<PlayerPositionEventArgs> SwitchingCounterBidPlayerStarted;
        public event EventHandler<PlayerPositionEventArgs> ExchangingCardsWithDabbStarted;
        public event EventHandler<PlayerPositionEventArgs> CalculatingGoingOutScoreStarted;
        public event EventHandler<PlayerPositionEventArgs> MeldingStarted;
        public event EventHandler<PlayerPositionEventArgs> WaitingForCardStarted;
        public event EventHandler<PlayerPositionEventArgs> ValidatingCardStarted;
        public event EventHandler<PlayerPositionEventArgs> RevertingInvalidMoveStarted;
        public event EventHandler<PlayerPositionEventArgs> SwitchingCurrentTrickPlayerStarted;
        public event EventHandler<PlayerPositionEventArgs> StartingNewRoundStarted;
        public event EventHandler<PlayerPositionEventArgs> CountingPlayerOrTeamScoresStarted;
        public event EventHandler GameFinished;

        // Interface implementation. See ISingleGameTriggerSink for comments.
        public void SendTrigger(Common.GameTrigger trigger)
        {
            if(_stateMachine != null)
            {
                Console.WriteLine(string.Format("Received trigger {0}", trigger.ToString()));
                _stateMachine.Fire(trigger);
            }
        }

        /// <summary>
        /// Stores properties for the current state of a single game.
        /// </summary>
        private class SingleGameProperties
        {
            public int NumberOfPlayers { get; set; }
            public int DealerPosition { get; set; }
            public int RemainingCards { get; set; }
            public int CurrentPlayerPosition { get; set; }
            public int NextPlayerPosition { get; set; }
            public int TrickWinnerPosition { get; set; }
        }

        /// <summary>
        /// Fires an event which takes event arguments safely. If no event listener is connected, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the event arguments.</typeparam>
        /// <param name="eventHandler">The handler of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void FireEvent<T>(EventHandler<T> eventHandler, T eventArgs, string name)
        {
            if (eventHandler == null)
            {
                throw new UnconnectedEventException(name);
            }
            Console.WriteLine(String.Format("Sending event {0}", name));
            eventHandler(this, eventArgs);
        }

        /// <summary>
        /// Fires an event which does not take event arguments safely. If no event listener is connected, an exception will be thrown.
        /// </summary>
        /// <param name="eventHandler">The handler of the event.</param>
        private void FireEvent(EventHandler eventHandler, string name) =>
            FireEvent(eventHandler == null ? null : new EventHandler<EventArgs>(eventHandler), EventArgs.Empty, name);

        private Stateless.StateMachine<SingleGameState, Common.GameTrigger> _stateMachine = null;

        /// <summary>
        /// Creates a state machine for the given rule settings.
        /// Check https://github.com/Timmeey86/binokel-deluxe/wiki/State-Machines for an overview of possible states and transitions.
        /// </summary>
        /// <param name="ruleSettings">The settings for this game.</param>
        /// <param name="dealerPosition">The Position of the dealing player for this game, starting at zero.</param>
        /// <returns>A state machine for a single game.</returns>
        public void RefreshStateMachine(RuleSettings ruleSettings, int dealerPosition)
        {
            // Configure initial attributes
            var properties = new SingleGameProperties
            {
                NumberOfPlayers = ruleSettings.GameType == GameType.ThreePlayerGame ? 3 : 4,
                DealerPosition = dealerPosition,
                RemainingCards = ruleSettings.SevensAreIncluded ? 48 : 40,
                CurrentPlayerPosition = -1,
                NextPlayerPosition = -1,
                TrickWinnerPosition = -1
            };

            _stateMachine = new Stateless.StateMachine<SingleGameState, Common.GameTrigger>(SingleGameState.Initial);
            // Ignore triggers which should not be available.
            _stateMachine.OnUnhandledTrigger((state, trigger) => { });

            _stateMachine.Configure(SingleGameState.Initial)
                .Permit(Common.GameTrigger.GameStarted, SingleGameState.Dealing);

            ConfigureDealingPhase(properties);
            ConfigureBiddingPhase(properties);
            ConfigureDabbPhase(properties);
            ConfigureDurchPhase();
            ConfigureBettelPhase();
            ConfigureMeldingPhase(properties);
            ConfigureTrickTakingPhase(properties);
            ConfigureEndPhase(properties);
        }

        private void ConfigureDealingPhase(SingleGameProperties properties)
        {
            // Dealing phase
            _stateMachine.Configure(SingleGameState.Dealing)
                .Permit(Common.GameTrigger.DealingFinished, SingleGameState.Bidding_WaitingForFirstBid)
                .OnEntry(() =>
                {
                    properties.CurrentPlayerPosition = (properties.DealerPosition + 1) % properties.NumberOfPlayers;
                    properties.NextPlayerPosition = (properties.DealerPosition + 2) % properties.NumberOfPlayers;

                    FireEvent(DealingStarted, new PlayerPairEventArgs(properties.CurrentPlayerPosition, properties.NextPlayerPosition), "DealingStarted");
                });
        }

        private void ConfigureBiddingPhase(SingleGameProperties properties)
        {
            // Bidding phase
            _stateMachine.Configure(SingleGameState.Bidding_WaitingForFirstBid)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(Common.GameTrigger.BidPlaced, SingleGameState.Bidding_WaitingForNextPlayer)
                .Permit(Common.GameTrigger.Passed, SingleGameState.Bidding_SwitchingFirstBidPlayer)
                // Let listeners know we are waiting for a player to either make the first bid or pass.
                .OnEntry(() => FireEvent(WaitingForFirstBidStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "WaitingForFirstBidStarted"));

            _stateMachine.Configure(SingleGameState.Bidding_SwitchingFirstBidPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(Common.GameTrigger.PlayerSwitched, SingleGameState.Bidding_WaitingForFirstBid)
                .Permit(Common.GameTrigger.Internal, SingleGameState.ExchangingCardsWithTheDabb)
                .OnEntry(() =>
                {
                    properties.CurrentPlayerPosition = properties.NextPlayerPosition;
                    properties.NextPlayerPosition = (properties.CurrentPlayerPosition + 1) % properties.NumberOfPlayers;

                    // If the dealer is the current player and there still is no bid, the dealer automatically wins the round for 0 points 
                    // (this is extremely rare and not clearly defined in any rules)
                    if (properties.CurrentPlayerPosition == properties.DealerPosition)
                    {
                        _stateMachine.Fire(Common.GameTrigger.Internal);
                    }
                    else
                    {
                        // Let listeners know we are waiting for the UI to perform a player switch.
                        FireEvent(SwitchingPlayerBeforeFirstBidStarted, new PlayerPairEventArgs(properties.CurrentPlayerPosition, properties.NextPlayerPosition), "SwitchingPlayerBeforeFirstBidStarted");
                    }
                });

            _stateMachine.Configure(SingleGameState.Bidding_WaitingForNextPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(Common.GameTrigger.BidCountered, SingleGameState.Bidding_WaitingForCurrentPlayer)
                .Permit(Common.GameTrigger.Passed, SingleGameState.Bidding_SwitchingNextPlayer)
                // Let the UI know we are waiting for the next player to either counter bid or pass.
                .OnEntry(() => FireEvent(WaitingForCounterOrPassStarted, new PlayerPositionEventArgs(properties.NextPlayerPosition), "WaitingForCounterOrPassStarted"));

            _stateMachine.Configure(SingleGameState.Bidding_WaitingForCurrentPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(Common.GameTrigger.BidPlaced, SingleGameState.Bidding_WaitingForNextPlayer)
                .Permit(Common.GameTrigger.Passed, SingleGameState.Bidding_SwitchingCurrentPlayer)
                // Let the UI know we are waiting for the current player to either increase their bid (i.e. counter the next player) or pass.
                .OnEntry(() => FireEvent(WaitingForBidOrPassStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "WaitingForBidOrPassStarted"));

            _stateMachine.Configure(SingleGameState.Bidding_SwitchingCurrentPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(Common.GameTrigger.PlayerSwitched, SingleGameState.Bidding_WaitingForNextPlayer)
                .Permit(Common.GameTrigger.Internal, SingleGameState.ExchangingCardsWithTheDabb)
                .OnEntry(() =>
                {
                    properties.CurrentPlayerPosition = properties.NextPlayerPosition;
                    properties.NextPlayerPosition = (properties.CurrentPlayerPosition + 1) % properties.NumberOfPlayers;

                    // if the new current player is the dealer, this means the dealer won the bidding round since the dealer countered the previous bid
                    // and whoever placed that bid passed.
                    if (properties.CurrentPlayerPosition == properties.DealerPosition)
                    {
                        _stateMachine.Fire(Common.GameTrigger.Internal);
                    }
                    else
                    {
                        // Let the UI know we are waiting for the current and next players to be shifted counterclockwise.
                        FireEvent(SwitchingCurrentBidPlayerStarted, new PlayerPairEventArgs(properties.CurrentPlayerPosition, properties.NextPlayerPosition), "SwitchingCurrentBidPlayerStarted");
                    }
                });

            _stateMachine.Configure(SingleGameState.Bidding_SwitchingNextPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(Common.GameTrigger.PlayerSwitched, SingleGameState.Bidding_WaitingForNextPlayer)
                .Permit(Common.GameTrigger.Internal, SingleGameState.ExchangingCardsWithTheDabb)
                .OnEntry(() =>
                {
                    properties.NextPlayerPosition = (properties.NextPlayerPosition + 1) % properties.NumberOfPlayers;

                    // if the new next player would be the player right of the dealer, this means the current player won the bidding roudn
                    // since every player after them (and before them) passed.
                    if (properties.NextPlayerPosition == (properties.DealerPosition + 1) % properties.NumberOfPlayers)
                    {
                        _stateMachine.Fire(Common.GameTrigger.Internal);
                    }
                    else
                    {
                        // Let the UI know we are waiting for the next player to be switched.
                        FireEvent(SwitchingCounterBidPlayerStarted, new PlayerPositionEventArgs(properties.NextPlayerPosition), "SwitchingCounterBidPlayerStarted");
                    }
                });
        }

        private void ConfigureDabbPhase(SingleGameProperties properties)
        {
            _stateMachine.Configure(SingleGameState.ExchangingCardsWithTheDabb)
                .Permit(Common.GameTrigger.GoingOut, SingleGameState.CountingGoingOutScore)
                .Permit(Common.GameTrigger.DurchAnnounced, SingleGameState.Durch)
                .Permit(Common.GameTrigger.BettelAnnounced, SingleGameState.Bettel)
                .Permit(Common.GameTrigger.TrumpSelected, SingleGameState.Melding)
                // Let the UI know we are waiting for the current player to exchange cards with the dabb and do a choice between
                // going out, selecting a trump or announcing a durch or bettel (if allowed).
                .OnEntry(() => FireEvent(ExchangingCardsWithDabbStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "ExchangingCardsWithDabbStarted"));

            _stateMachine.Configure(SingleGameState.CountingGoingOutScore)
                .Permit(Common.GameTrigger.ScoreCalculationFinished, SingleGameState.End)
                // Let the UI know we are waiting for the going out score to be calculated.
                .OnEntry(() => FireEvent(CalculatingGoingOutScoreStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "CalculatingGoingOutScoreStarted"));
        }

        private void ConfigureDurchPhase()
        {
            _stateMachine.Configure(SingleGameState.Durch)
                .OnEntry(() => throw new NotImplementedException());
        }

        private void ConfigureBettelPhase()
        {
            _stateMachine.Configure(SingleGameState.Bettel)
                .OnEntry(() => throw new NotImplementedException());
        }

        private void ConfigureMeldingPhase(SingleGameProperties properties)
        {
            _stateMachine.Configure(SingleGameState.Melding)
                .Permit(Common.GameTrigger.MeldsSeenByAllPlayers, SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                // Let the UI know we are waiting to display the melds of all players and wait for confirmation of all
                // (human) players that they have seen the melds.
                .OnEntry(() => FireEvent(MeldingStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "MeldingStarted"));
        }

        private void ConfigureTrickTakingPhase(SingleGameProperties properties)
        {
            _stateMachine.Configure(SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(Common.GameTrigger.CardPlaced, SingleGameState.TrickTaking_ValidatingCard)
                // Let the UI know we are waiting for the current player to place a card.
                .OnEntry(() => FireEvent(WaitingForCardStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "WaitingForCardStarted"));

            _stateMachine.Configure(SingleGameState.TrickTaking_ValidatingCard)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(Common.GameTrigger.WinningCardPlaced, SingleGameState.TrickTaking_RememberingWinningPlayer)
                .Permit(Common.GameTrigger.LosingCardPlaced, SingleGameState.TrickTaking_SwitchingToNextPlayer)
                .Permit(Common.GameTrigger.InvalidCardPlaced, SingleGameState.TrickTaking_RevertingInvalidMove)
                // Let the UI know we are waiting for the card to be validated.
                .OnEntry(() => FireEvent(ValidatingCardStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "ValidatingCardStarted"));

            _stateMachine.Configure(SingleGameState.TrickTaking_RememberingWinningPlayer)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(Common.GameTrigger.Internal, SingleGameState.TrickTaking_SwitchingToNextPlayer)
                .OnEntry(() =>
                {
                    properties.TrickWinnerPosition = properties.CurrentPlayerPosition;
                    // Automatically switch to the next state.
                    _stateMachine.Fire(Common.GameTrigger.Internal);
                });

            _stateMachine.Configure(SingleGameState.TrickTaking_SwitchingToNextPlayer)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(Common.GameTrigger.PlayerSwitched, SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                .Permit(Common.GameTrigger.Internal, SingleGameState.TrickTaking_StartingNewRound)
                .OnEntry(() =>
                {
                    properties.CurrentPlayerPosition = (properties.CurrentPlayerPosition + 1) % properties.NumberOfPlayers;
                    properties.RemainingCards--;

                    // If all players placed a card, start a new round
                    if (properties.RemainingCards % properties.NumberOfPlayers == 0)
                    {
                        _stateMachine.Fire(Common.GameTrigger.Internal);
                    }
                    else
                    {
                        // Let the UI know we are waiting for the player who is allowed to place a card to be switched.
                        FireEvent(SwitchingCurrentTrickPlayerStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "SwitchingCurrentTrickPlayerStarted");
                    }
                });

            _stateMachine.Configure(SingleGameState.TrickTaking_RevertingInvalidMove)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(Common.GameTrigger.RevertingFinished, SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                // Let the UI know we are waiting for an invalid move to be reverted.
                .OnEntry(() => FireEvent(RevertingInvalidMoveStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "RevertingInvalidMoveStarted"));

            _stateMachine.Configure(SingleGameState.TrickTaking_StartingNewRound)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(Common.GameTrigger.NewRoundStarted, SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                .Permit(Common.GameTrigger.Internal, SingleGameState.CountingGameScore)
                .OnEntry(() =>
                {
                    // Whoever won the trick is now allowed to place the first card in the next roud.
                    properties.CurrentPlayerPosition = properties.TrickWinnerPosition;

                    // If there are no more cards left, end the game.
                    if (properties.RemainingCards < 0)
                    {
                        _stateMachine.Fire(Common.GameTrigger.Internal);
                    }
                    else
                    {
                        // Let the UI know we are waiting for a new round to be started
                        FireEvent(StartingNewRoundStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "StartingNewRoundStarted");
                    }
                });
        }

        private void ConfigureEndPhase(SingleGameProperties properties)
        {
            _stateMachine.Configure(SingleGameState.CountingGameScore)
                .Permit(Common.GameTrigger.ScoreCalculationFinished, SingleGameState.End)
                // Let the UI know we are waiting for the final score to be calcualted.
                .OnEntry(() => FireEvent(CountingPlayerOrTeamScoresStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "CountingPlayerOrTeamScoresStarted"));

            _stateMachine.Configure(SingleGameState.End)
                .OnEntry(() => FireEvent(GameFinished, "GameFinished"));
        }
    }
}
