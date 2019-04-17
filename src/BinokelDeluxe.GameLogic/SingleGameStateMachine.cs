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
        public event EventHandler WaitingForFirstBidStarted;
        public event EventHandler<PlayerPairEventArgs> SwitchingPlayerBeforeFirstBidStarted;
        public event EventHandler WaitingForBidOrPassStarted;
        public event EventHandler WaitingForCounterOrPassStarted;
        public event EventHandler<PlayerPairEventArgs> SwitchingCurrentBidPlayerStarted;
        public event EventHandler<PlayerNumberEventArgs> SwitchingCounterBidPlayerStarted;
        public event EventHandler<PlayerNumberEventArgs> ExchangingCardsWithDabbStarted;
        public event EventHandler<PlayerNumberEventArgs> CalculatingGoingOutScoreStarted;
        public event EventHandler MeldingStarted;
        public event EventHandler<PlayerNumberEventArgs> WaitingForCardStarted;
        public event EventHandler<PlayerNumberEventArgs> ValidatingCardStarted;
        public event EventHandler<PlayerNumberEventArgs> RevertingInvalidMoveStarted;
        public event EventHandler<PlayerNumberEventArgs> SwitchingCurrentTrickPlayerStarted;
        public event EventHandler<PlayerNumberEventArgs> StartingNewRoundStarted;
        public event EventHandler<PlayerNumberEventArgs> CountingPlayerOrTeamScoresStarted;

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

            ConfigureDealingPhase(properties);
            ConfigureBiddingPhase(properties);
            ConfigureDabbPhase(properties);
            ConfigureDurchPhase();
            ConfigureBettelPhase();
            ConfigureMeldingPhase();
            ConfigureTrickTakingPhase(properties);
            ConfigureEndPhase(properties);
        }

        private void ConfigureDealingPhase(SingleGameProperties properties)
        {
            // Dealing phase
            _stateMachine.Configure(SingleGameState.Dealing)
                .Permit(SingleGameTrigger.DealingFinished, SingleGameState.Bidding_WaitingForCurrentPlayer)
                .OnEntry(() =>
                {
                    properties.CurrentPlayerNumber = (properties.DealerNumber + 1) % properties.NumberOfPlayers;
                    properties.NextPlayerNumber = (properties.DealerNumber + 2) % properties.NumberOfPlayers;

                    FireEvent(DealingStarted, new PlayerPairEventArgs(properties.CurrentPlayerNumber, properties.NextPlayerNumber));
                });
        }

        private void ConfigureBiddingPhase(SingleGameProperties properties)
        {
            // Bidding phase
            _stateMachine.Configure(SingleGameState.Bidding_WaitingForFirstBid)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(SingleGameTrigger.BidPlaced, SingleGameState.Bidding_WaitingForNextPlayer)
                .Permit(SingleGameTrigger.Passed, SingleGameState.Bidding_SwitchingFirstBidPlayer)
                // Let listeners know we are waiting for a player to either make the first bid or pass.
                .OnEntry(() => FireEvent(WaitingForFirstBidStarted));

            _stateMachine.Configure(SingleGameState.Bidding_SwitchingFirstBidPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(SingleGameTrigger.PlayerSwitched, SingleGameState.Bidding_WaitingForFirstBid)
                .Permit(SingleGameTrigger.Internal, SingleGameState.ExchangingCardsWithTheDabb)
                .OnEntry(() =>
                {
                    properties.CurrentPlayerNumber = properties.NextPlayerNumber;
                    properties.NextPlayerNumber = (properties.CurrentPlayerNumber + 1) % properties.NumberOfPlayers;

                    // If the dealer is the current player and there still is no bid, the dealer automatically wins the round for 0 points 
                    // (this is extremely rare and not clearly defined in any rules)
                    if (properties.CurrentPlayerNumber == properties.DealerNumber)
                    {
                        _stateMachine.Fire(SingleGameTrigger.Internal);
                    }
                    else
                    {
                        // Let listeners know we are waiting for the UI to perform a player switch.
                        FireEvent(SwitchingPlayerBeforeFirstBidStarted, new PlayerPairEventArgs(properties.CurrentPlayerNumber, properties.NextPlayerNumber));
                    }
                });

            _stateMachine.Configure(SingleGameState.Bidding_WaitingForNextPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(SingleGameTrigger.BidCountered, SingleGameState.Bidding_WaitingForCurrentPlayer)
                .Permit(SingleGameTrigger.Passed, SingleGameState.Bidding_SwitchingNextPlayer)
                // Let the UI know we are waiting for the next player to either counter bid or pass.
                .OnEntry(() => FireEvent(WaitingForCounterOrPassStarted));

            _stateMachine.Configure(SingleGameState.Bidding_WaitingForCurrentPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(SingleGameTrigger.BidPlaced, SingleGameState.Bidding_WaitingForNextPlayer)
                .Permit(SingleGameTrigger.Passed, SingleGameState.Bidding_SwitchingCurrentPlayer)
                // Let the UI know we are waiting for the current player to either increase their bid (i.e. counter the next player) or pass.
                .OnEntry(() => FireEvent(WaitingForBidOrPassStarted));

            _stateMachine.Configure(SingleGameState.Bidding_SwitchingCurrentPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(SingleGameTrigger.PlayerSwitched, SingleGameState.Bidding_WaitingForNextPlayer)
                .Permit(SingleGameTrigger.Internal, SingleGameState.ExchangingCardsWithTheDabb)
                .OnEntry(() =>
                {
                    properties.CurrentPlayerNumber = properties.NextPlayerNumber;
                    properties.NextPlayerNumber = (properties.CurrentPlayerNumber + 1) % properties.NumberOfPlayers;

                    // if the new current player is the dealer, this means the dealer won the bidding round since the dealer countered the previous bid
                    // and whoever placed that bid passed.
                    if (properties.CurrentPlayerNumber == properties.DealerNumber)
                    {
                        _stateMachine.Fire(SingleGameTrigger.Internal);
                    }
                    else
                    {
                        // Let the UI know we are waiting for the current and next players to be shifted counterclockwise.
                        FireEvent(SwitchingCurrentBidPlayerStarted, new PlayerPairEventArgs(properties.CurrentPlayerNumber, properties.NextPlayerNumber));
                    }
                });

            _stateMachine.Configure(SingleGameState.Bidding_SwitchingNextPlayer)
                .SubstateOf(SingleGameState.Bidding)
                .Permit(SingleGameTrigger.PlayerSwitched, SingleGameState.Bidding_WaitingForNextPlayer)
                .Permit(SingleGameTrigger.Internal, SingleGameState.ExchangingCardsWithTheDabb)
                .OnEntry(() =>
                {
                    properties.NextPlayerNumber = (properties.NextPlayerNumber + 1) % properties.NumberOfPlayers;

                    // if the new next player would be the player right of the dealer, this means the current player won the bidding roudn
                    // since every player after them (and before them) passed.
                    if (properties.NextPlayerNumber == (properties.DealerNumber + 1) % properties.NumberOfPlayers)
                    {
                        _stateMachine.Fire(SingleGameTrigger.Internal);
                    }
                    else
                    {
                        // Let the UI know we are waiting for the next player to be switched.
                        FireEvent(SwitchingCounterBidPlayerStarted, new PlayerNumberEventArgs(properties.NextPlayerNumber));
                    }
                });
        }

        private void ConfigureDabbPhase(SingleGameProperties properties)
        {
            _stateMachine.Configure(SingleGameState.ExchangingCardsWithTheDabb)
                .Permit(SingleGameTrigger.GoingOut, SingleGameState.CountingGoingOutScore)
                .Permit(SingleGameTrigger.DurchAnnounced, SingleGameState.Durch)
                .Permit(SingleGameTrigger.BettelAnnounced, SingleGameState.Bettel)
                .Permit(SingleGameTrigger.TrumpSelected, SingleGameState.Melding)
                // Let the UI know we are waiting for the current player to exchange cards with the dabb and do a choice between
                // going out, selecting a trump or announcing a durch or bettel (if allowed).
                .OnEntry(() => FireEvent(ExchangingCardsWithDabbStarted, new PlayerNumberEventArgs(properties.CurrentPlayerNumber)));

            _stateMachine.Configure(SingleGameState.CountingGoingOutScore)
                .Permit(SingleGameTrigger.ScoreCalculationFinished, SingleGameState.End)
                // Let the UI know we are waiting for the going out score to be calculated.
                .OnEntry(() => FireEvent(CalculatingGoingOutScoreStarted, new PlayerNumberEventArgs(properties.CurrentPlayerNumber)));
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

        private void ConfigureMeldingPhase()
        {
            _stateMachine.Configure(SingleGameState.Melding)
                .Permit(SingleGameTrigger.MeldsSeenByAllPlayers, SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                // Let the UI know we are waiting to display the melds of all players and wait for confirmation of all
                // (human) players that they have seen the melds.
                .OnEntry(() => FireEvent(MeldingStarted));
        }

        private void ConfigureTrickTakingPhase(SingleGameProperties properties)
        {
            _stateMachine.Configure(SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(SingleGameTrigger.CardPlaced, SingleGameState.TrickTaking_ValidatingCard)
                // Let the UI know we are waiting for the current player to place a card.
                .OnEntry(() => FireEvent(WaitingForCardStarted, new PlayerNumberEventArgs(properties.CurrentPlayerNumber)));

            _stateMachine.Configure(SingleGameState.TrickTaking_ValidatingCard)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(SingleGameTrigger.WinningCardPlaced, SingleGameState.TrickTaking_RememberingWinningPlayer)
                .Permit(SingleGameTrigger.LosingCardPlaced, SingleGameState.TrickTaking_SwitchingToNextPlayer)
                .Permit(SingleGameTrigger.InvalidCardPlaced, SingleGameState.TrickTaking_RevertingInvalidMove)
                // Let the UI know we are waiting for the card to be validated.
                .OnEntry(() => FireEvent(ValidatingCardStarted, new PlayerNumberEventArgs(properties.CurrentPlayerNumber)));

            _stateMachine.Configure(SingleGameState.TrickTaking_RememberingWinningPlayer)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(SingleGameTrigger.Internal, SingleGameState.TrickTaking_SwitchingToNextPlayer)
                .OnEntry(() =>
                {
                    properties.TrickWinnerNumber = properties.CurrentPlayerNumber;
                    // Automatically switch to the next state.
                    _stateMachine.Fire(SingleGameTrigger.Internal);
                });

            _stateMachine.Configure(SingleGameState.TrickTaking_SwitchingToNextPlayer)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(SingleGameTrigger.PlayerSwitched, SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                .Permit(SingleGameTrigger.Internal, SingleGameState.TrickTaking_StartingNewRound)
                .OnEntry(() =>
                {
                    properties.CurrentPlayerNumber = (properties.CurrentPlayerNumber + 1) % properties.NumberOfPlayers;
                    properties.RemainingCards--;

                    // If all players placed a card, start a new round
                    if (properties.RemainingCards % properties.NumberOfPlayers == 0)
                    {
                        _stateMachine.Fire(SingleGameTrigger.Internal);
                    }
                    else
                    {
                        // Let the UI know we are waiting for the player who is allowed to place a card to be switched.
                        FireEvent(SwitchingCurrentTrickPlayerStarted, new PlayerNumberEventArgs(properties.CurrentPlayerNumber));
                    }
                });

            _stateMachine.Configure(SingleGameState.TrickTaking_RevertingInvalidMove)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(SingleGameTrigger.RevertingFinished, SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                // Let the UI know we are waiting for an invalid move to be reverted.
                .OnEntry(() => FireEvent(RevertingInvalidMoveStarted, new PlayerNumberEventArgs(properties.CurrentPlayerNumber)));

            _stateMachine.Configure(SingleGameState.TrickTaking_StartingNewRound)
                .SubstateOf(SingleGameState.TrickTaking)
                .Permit(SingleGameTrigger.NewRoundStarted, SingleGameState.TrickTaking_WaitingForCurrentPlayer)
                .Permit(SingleGameTrigger.Internal, SingleGameState.CountingGameScore)
                .OnEntry(() =>
                {
                    // Whoever won the trick is now allowed to place the first card in the next roud.
                    properties.CurrentPlayerNumber = properties.TrickWinnerNumber;

                    // If there are no more cards left, end the game.
                    if (properties.RemainingCards < 0)
                    {
                        _stateMachine.Fire(SingleGameTrigger.Internal);
                    }
                    else
                    {
                        // Let the UI know we are waiting for a new round to be started
                        FireEvent(StartingNewRoundStarted, new PlayerNumberEventArgs(properties.CurrentPlayerNumber));
                    }
                });
        }

        private void ConfigureEndPhase(SingleGameProperties properties)
        {
            _stateMachine.Configure(SingleGameState.CountingGameScore)
                .Permit(SingleGameTrigger.ScoreCalculationFinished, SingleGameState.End)
                // Let the UI know we are waiting for the final score to be calcualted.
                .OnEntry(() => FireEvent(CalculatingGoingOutScoreStarted, new PlayerNumberEventArgs(properties.CurrentPlayerNumber)));
        }
    }
}
