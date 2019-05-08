// DOCUMENTED

namespace BinokelDeluxe.GameLogic
{
    using System;

    /// <summary>
    /// This class manages the central state machine and serves as an event bridge for it.
    /// This class is an internal namespace class and should not be exposed directly.
    /// See https://github.com/Timmeey86/binokel-deluxe/wiki/Glossary for the difference between player positions and numbers.
    /// </summary>
    internal class SingleGameStateMachine : ISingleGameEventSource, ISingleGameTriggerSink
    {
        private Stateless.StateMachine<SingleGameState, Common.GameTrigger> stateMachine = null;

        /// <inheritdoc/>
        public event EventHandler<PlayerPairEventArgs> DealingStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> WaitingForFirstBidStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPairEventArgs> SwitchingPlayerBeforeFirstBidStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> WaitingForBidOrPassStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> WaitingForCounterOrPassStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPairEventArgs> SwitchingCurrentBidPlayerStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> SwitchingCounterBidPlayerStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> ExchangingCardsWithDabbStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> CalculatingGoingOutScoreStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> MeldingStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> TrickTakingStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> WaitingForCardStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> ValidatingCardStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> RevertingInvalidMoveStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> SwitchingCurrentTrickPlayerStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> StartingNewRoundStarted;

        /// <inheritdoc/>
        public event EventHandler<PlayerPositionEventArgs> CountingPlayerOrTeamScoresStarted;

        /// <inheritdoc/>
        public event EventHandler GameFinished;

        /// <inheritdoc/>
        public void SendTrigger(Common.GameTrigger trigger)
        {
            if (this.stateMachine != null)
            {
                Console.WriteLine(string.Format("Received trigger {0}", trigger.ToString()));
                this.stateMachine.Fire(trigger);
            }
        }

        /// <summary>
        /// Creates a state machine for the given rule settings.
        /// Check https://github.com/Timmeey86/binokel-deluxe/wiki/(Concept)-State-Machines for an overview of possible states and transitions.
        /// </summary>
        /// <param name="ruleSettings">The settings for this game.</param>
        /// <param name="dealerPosition">The Position of the dealing player for this game, starting at zero.</param>
        public void RefreshStateMachine(RuleSettings ruleSettings, int dealerPosition)
        {
            var amountOfCards = ruleSettings.SevensAreIncluded ? 48 : 40;
            var dabbSize = ruleSettings.SevensAreIncluded && ruleSettings.GameType == GameType.ThreePlayerGame ? 6 : 4;

            // Configure initial attributes
            var properties = new SingleGameProperties
            {
                NumberOfPlayers = ruleSettings.GameType == GameType.ThreePlayerGame ? 3 : 4,
                DealerPosition = dealerPosition,
                RemainingCards = amountOfCards - dabbSize,
                CurrentPlayerPosition = -1,
                NextPlayerPosition = -1,
                TrickWinnerPosition = -1,
            };

            this.stateMachine = new Stateless.StateMachine<SingleGameState, Common.GameTrigger>(SingleGameState.Initial);

            // Ignore triggers which should not be available.
            this.stateMachine.OnUnhandledTrigger((state, trigger) => { });

            this.stateMachine.Configure(SingleGameState.Initial)
                .Permit(Common.GameTrigger.GameStarted, SingleGameState.Dealing);

            this.ConfigureDealingPhase(properties);
            this.ConfigureBiddingPhase(properties);
            this.ConfigureDabbPhase(properties);
            this.ConfigureDurchPhase();
            this.ConfigureBettelPhase();
            this.ConfigureMeldingPhase(properties);
            this.ConfigureTrickTakingPhase(properties);
            this.ConfigureEndPhase(properties);
        }

        /// <summary>
        /// Fires an event which takes event arguments safely. If no event listener is connected, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the event arguments.</typeparam>
        /// <param name="eventHandler">The handler of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        /// <param name="name">The name of the event (used for error handling).</param>
        private void FireEvent<T>(EventHandler<T> eventHandler, T eventArgs, string name)
        {
            if (eventHandler == null)
            {
                throw new UnconnectedEventException(name);
            }

            Console.WriteLine(string.Format("Sending event {0}", name));
            eventHandler(this, eventArgs);
        }

        /// <summary>
        /// Fires an event which does not take event arguments safely. If no event listener is connected, an exception will be thrown.
        /// </summary>
        /// <param name="eventHandler">The handler of the event.</param>
        /// <param name="name">The name of the event (used for error handling).</param>
        private void FireEvent(EventHandler eventHandler, string name) =>
            this.FireEvent(eventHandler == null ? null : new EventHandler<EventArgs>(eventHandler), EventArgs.Empty, name);

        private void ConfigureDealingPhase(SingleGameProperties properties)
        {
            // Dealing phase
            this.stateMachine.Configure(SingleGameState.Dealing)
                .Permit(Common.GameTrigger.DealingFinished, SingleGameState.Bidding_WaitingForFirstBid)
                .OnEntry(() =>
                {
                    properties.CurrentPlayerPosition = (properties.DealerPosition + 1) % properties.NumberOfPlayers;
                    properties.NextPlayerPosition = (properties.DealerPosition + 2) % properties.NumberOfPlayers;

                    this.FireEvent(this.DealingStarted, new PlayerPairEventArgs(properties.CurrentPlayerPosition, properties.NextPlayerPosition), "DealingStarted");
                });
        }

        private void ConfigureBiddingPhase(SingleGameProperties properties)
        {
            // Bidding phase
            this.stateMachine.Configure(SingleGameState.Bidding_WaitingForFirstBid)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(Common.GameTrigger.BidPlaced, SingleGameState.Bidding_WaitingForNextPlayer)
                .Permit(Common.GameTrigger.Passed, SingleGameState.Bidding_SwitchingFirstBidPlayer)

                // Let listeners know we are waiting for a player to either make the first bid or pass.
                .OnEntry(() => this.FireEvent(this.WaitingForFirstBidStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "WaitingForFirstBidStarted"));

            this.stateMachine.Configure(SingleGameState.Bidding_SwitchingFirstBidPlayer)
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
                        this.stateMachine.Fire(Common.GameTrigger.Internal);
                    }
                    else
                    {
                        // Let listeners know we are waiting for the UI to perform a player switch.
                        this.FireEvent(this.SwitchingPlayerBeforeFirstBidStarted, new PlayerPairEventArgs(properties.CurrentPlayerPosition, properties.NextPlayerPosition), "SwitchingPlayerBeforeFirstBidStarted");
                    }
                });

            this.stateMachine.Configure(SingleGameState.Bidding_WaitingForNextPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(Common.GameTrigger.BidCountered, SingleGameState.Bidding_WaitingForCurrentPlayer)
                .Permit(Common.GameTrigger.Passed, SingleGameState.Bidding_SwitchingNextPlayer)

                // Let the UI know we are waiting for the next player to either counter bid or pass.
                .OnEntry(() => this.FireEvent(this.WaitingForCounterOrPassStarted, new PlayerPositionEventArgs(properties.NextPlayerPosition), "WaitingForCounterOrPassStarted"));

            this.stateMachine.Configure(SingleGameState.Bidding_WaitingForCurrentPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(Common.GameTrigger.BidPlaced, SingleGameState.Bidding_WaitingForNextPlayer)
                .Permit(Common.GameTrigger.Passed, SingleGameState.Bidding_SwitchingCurrentPlayer)

                // Let the UI know we are waiting for the current player to either increase their bid (i.e. counter the next player) or pass.
                .OnEntry(() => this.FireEvent(this.WaitingForBidOrPassStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "WaitingForBidOrPassStarted"));

            this.stateMachine.Configure(SingleGameState.Bidding_SwitchingCurrentPlayer)
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
                        this.stateMachine.Fire(Common.GameTrigger.Internal);
                    }
                    else
                    {
                        // Let the UI know we are waiting for the current and next players to be shifted counterclockwise.
                        this.FireEvent(this.SwitchingCurrentBidPlayerStarted, new PlayerPairEventArgs(properties.CurrentPlayerPosition, properties.NextPlayerPosition), "SwitchingCurrentBidPlayerStarted");
                    }
                });

            this.stateMachine.Configure(SingleGameState.Bidding_SwitchingNextPlayer)
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
                        this.stateMachine.Fire(Common.GameTrigger.Internal);
                    }
                    else
                    {
                        // Let the UI know we are waiting for the next player to be switched.
                        this.FireEvent(this.SwitchingCounterBidPlayerStarted, new PlayerPositionEventArgs(properties.NextPlayerPosition), "SwitchingCounterBidPlayerStarted");
                    }
                });
        }

        private void ConfigureDabbPhase(SingleGameProperties properties)
        {
            this.stateMachine.Configure(SingleGameState.ExchangingCardsWithTheDabb)
                .Permit(Common.GameTrigger.GoingOut, SingleGameState.CountingGoingOutScore)
                .Permit(Common.GameTrigger.DurchAnnounced, SingleGameState.Durch)
                .Permit(Common.GameTrigger.BettelAnnounced, SingleGameState.Bettel)
                .Permit(Common.GameTrigger.TrumpSelected, SingleGameState.Melding)

                // Let the UI know we are waiting for the current player to exchange cards with the dabb and do a choice between
                // going out, selecting a trump or announcing a durch or bettel (if allowed).
                .OnEntry(() => this.FireEvent(this.ExchangingCardsWithDabbStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "ExchangingCardsWithDabbStarted"));

            this.stateMachine.Configure(SingleGameState.CountingGoingOutScore)
                .Permit(Common.GameTrigger.ScoreCalculationFinished, SingleGameState.End)

                // Let the UI know we are waiting for the going out score to be calculated.
                .OnEntry(() => this.FireEvent(this.CalculatingGoingOutScoreStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "CalculatingGoingOutScoreStarted"));
        }

        private void ConfigureDurchPhase()
        {
            this.stateMachine.Configure(SingleGameState.Durch)
                .OnEntry(() => throw new NotImplementedException());
        }

        private void ConfigureBettelPhase()
        {
            this.stateMachine.Configure(SingleGameState.Bettel)
                .OnEntry(() => throw new NotImplementedException());
        }

        private void ConfigureMeldingPhase(SingleGameProperties properties)
        {
            this.stateMachine.Configure(SingleGameState.Melding)
                .Permit(Common.GameTrigger.MeldsSeenByAllPlayers, SingleGameState.TrickTaking)

                // Let the UI know we are waiting to display the melds of all players and wait for confirmation of all
                // (human) players that they have seen the melds.
                .OnEntry(() =>
                {
                    this.FireEvent(this.MeldingStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "MeldingStarted");

                    // Change the current player to the right-hand player of the dealer since this player is always the first to place a card
                    // (except for maybe Bettel and Durch, which is not implemented yet).
                    properties.CurrentPlayerPosition = (properties.DealerPosition + 1) % properties.NumberOfPlayers;
                });
        }

        private void ConfigureTrickTakingPhase(SingleGameProperties properties)
        {
            this.stateMachine.Configure(SingleGameState.TrickTaking)
                .Permit(Common.GameTrigger.ReadyForTrickTaking, SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                .OnEntry(() => this.FireEvent(this.TrickTakingStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "TrickTakingStarted"));

            this.stateMachine.Configure(SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(Common.GameTrigger.CardPlaced, SingleGameState.TrickTaking_ValidatingCard)

                // Let the UI know we are waiting for the current player to place a card.
                .OnEntry(() => this.FireEvent(this.WaitingForCardStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "WaitingForCardStarted"));

            this.stateMachine.Configure(SingleGameState.TrickTaking_ValidatingCard)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(Common.GameTrigger.WinningCardPlaced, SingleGameState.TrickTaking_RememberingWinningPlayer)
                .Permit(Common.GameTrigger.LosingCardPlaced, SingleGameState.TrickTaking_SwitchingToNextPlayer)
                .Permit(Common.GameTrigger.InvalidCardPlaced, SingleGameState.TrickTaking_RevertingInvalidMove)

                // Let the UI know we are waiting for the card to be validated.
                .OnEntry(() => this.FireEvent(this.ValidatingCardStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "ValidatingCardStarted"));

            this.stateMachine.Configure(SingleGameState.TrickTaking_RememberingWinningPlayer)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(Common.GameTrigger.Internal, SingleGameState.TrickTaking_SwitchingToNextPlayer)
                .OnEntry(() =>
                {
                    properties.TrickWinnerPosition = properties.CurrentPlayerPosition;

                    // Automatically switch to the next state.
                    this.stateMachine.Fire(Common.GameTrigger.Internal);
                });

            this.stateMachine.Configure(SingleGameState.TrickTaking_SwitchingToNextPlayer)
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
                        this.stateMachine.Fire(Common.GameTrigger.Internal);
                    }
                    else
                    {
                        // Let the UI know we are waiting for the player who is allowed to place a card to be switched.
                        this.FireEvent(this.SwitchingCurrentTrickPlayerStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "SwitchingCurrentTrickPlayerStarted");
                    }
                });

            this.stateMachine.Configure(SingleGameState.TrickTaking_RevertingInvalidMove)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(Common.GameTrigger.RevertingFinished, SingleGameState.TrickTaking_WaitingForCurrentPlayer)

                // Let the UI know we are waiting for an invalid move to be reverted.
                .OnEntry(() => this.FireEvent(this.RevertingInvalidMoveStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "RevertingInvalidMoveStarted"));

            this.stateMachine.Configure(SingleGameState.TrickTaking_StartingNewRound)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(Common.GameTrigger.NewRoundStarted, SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                .Permit(Common.GameTrigger.Internal, SingleGameState.CountingGameScore)
                .OnEntry(() =>
                {
                    // Whoever won the trick is now allowed to place the first card in the next roud.
                    properties.CurrentPlayerPosition = properties.TrickWinnerPosition;

                    // If there are no more cards left, end the game.
                    if (properties.RemainingCards <= 0)
                    {
                        this.stateMachine.Fire(Common.GameTrigger.Internal);
                    }
                    else
                    {
                        // Let the UI know we are waiting for a new round to be started
                        this.FireEvent(this.StartingNewRoundStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "StartingNewRoundStarted");
                    }
                });
        }

        private void ConfigureEndPhase(SingleGameProperties properties)
        {
            this.stateMachine.Configure(SingleGameState.CountingGameScore)
                .Permit(Common.GameTrigger.ScoreCalculationFinished, SingleGameState.End)

                // Let the UI know we are waiting for the final score to be calcualted.
                .OnEntry(() => this.FireEvent(this.CountingPlayerOrTeamScoresStarted, new PlayerPositionEventArgs(properties.CurrentPlayerPosition), "CountingPlayerOrTeamScoresStarted"));

            this.stateMachine.Configure(SingleGameState.End)
                .OnEntry(() => this.FireEvent(this.GameFinished, "GameFinished"));
        }

        /// <summary>
        /// Stores properties for the current state of a single game.
        /// </summary>
        private class SingleGameProperties
        {
            /// <summary>
            /// Gets or sets the number of players.
            /// </summary>
            public int NumberOfPlayers { get; set; }

            /// <summary>
            /// Gets or sets the position of the dealer.
            /// </summary>
            public int DealerPosition { get; set; }

            /// <summary>
            /// Gets or sets the number of remaining cards in the trick taking phase.
            /// </summary>
            public int RemainingCards { get; set; }

            /// <summary>
            /// Gets or sets the position of the current player.
            /// </summary>
            public int CurrentPlayerPosition { get; set; }

            /// <summary>
            /// Gets or sets the position of the next player.
            /// </summary>
            public int NextPlayerPosition { get; set; }

            /// <summary>
            /// Gets or sets the position of the trick winner.
            /// </summary>
            public int TrickWinnerPosition { get; set; }
        }
    }
}
