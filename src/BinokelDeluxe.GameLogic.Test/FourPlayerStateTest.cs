namespace BinokelDeluxe.GameLogic.Test
{
    using System;
    using NUnit.Framework;

#pragma warning disable SA1600 // Test methods should have an expressive name rather than being documented.
#pragma warning disable CS1591 // Same reason as above

    /// <summary>
    /// Defines tests for the SingleGameStateBridge for four players.
    /// See https://github.com/Timmeey86/binokel-deluxe/wiki/Glossary for the difference between player positions and numbers.
    /// </summary>
    public class FourPlayerStateTest
    {
        private SingleGameStateBridge sut; // System under test
        private GameLogic.RuleSettings ruleSettings;

        [SetUp]
        public void Setup()
        {
            this.sut = new SingleGameStateBridge();
            this.ruleSettings = new GameLogic.RuleSettings()
            {
                BettelsAreAllowed = true,
                CountingType = GameLogic.CountingType.DecreasingPointsForAceToUnter,
                ExtraPointsForLastTrickInsteadOfFirst = true,
                ExtraPointsForOthersWhenGoingOut = true,
                ExtraPointsForSevenOfTrumps = false,
                GameType = GameLogic.GameType.FourPlayerCrossBinokelGame,
                ScoresWillBeRounded = true,
                SecondTrumpMustAlsoWin = true,
                SevenOfTrumpsCanBeMeldedAndDiscarded = false,
                SevensAreIncluded = false,
            };
        }

        [Test]
        public void StartingGame_SendsDealingStartedEvent()
        {
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition: 0);

            var eventWasCalled = false;
            this.sut.EventSource.DealingStarted += (o, e) => eventWasCalled = true;

            this.StartGame();
            Assert.That(eventWasCalled);
        }

        [Test]
        public void AllPlayersPassing_LetsDealerWinBiddingPhase([Range(0, 3)]int dealerPosition)
        {
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipPlayerSwitchingPhases();
            this.MakePlayersPassFirstBidUntilPlayerNumber(0, dealerPosition);

            var eventWasCalled = false;
            var dealerWon = false;
            this.sut.EventSource.ExchangingCardsWithDabbStarted += (o, e) =>
            {
                eventWasCalled = true;
                if (e.PlayerPosition == dealerPosition)
                {
                    dealerWon = true;
                }
            };

            this.StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Dabb phase.");
            Assert.That(dealerWon, "The dealer did not win the bidding phase.");
        }

        [Test]
        public void FirstPlayerBiddingAndRestPassing_LetsFirstPlayerWinBiddingPhase([Range(0, 3)]int dealerPosition)
        {
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipPlayerSwitchingPhases();
            this.MakePlayersPassFirstBidUntilPlayerNumber(1, dealerPosition);
            this.MakePlayersPassCounterBid();

            var eventWasCalled = false;
            var firstPlayerWon = false;
            this.sut.EventSource.ExchangingCardsWithDabbStarted += (o, e) =>
            {
                eventWasCalled = true;
                if (e.PlayerPosition == this.GetPlayerPosition(1, dealerPosition))
                {
                    firstPlayerWon = true;
                }
            };

            this.StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Dabb phase.");
            Assert.That(firstPlayerWon, "The first player did not win the bidding phase.");
        }

        [Test]
        public void EachPlayerBiddingOnceThenPassing_LetsDealerWinBiddingPhase([Range(0, 3)]int dealerPosition)
        {
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipPlayerSwitchingPhases();
            this.MakePlayersPassFirstBidUntilPlayerNumber(0, dealerPosition);
            this.MakePlayersCounterButPassWhenCountered();

            var eventWasCalled = false;
            var dealerWon = false;
            this.sut.EventSource.ExchangingCardsWithDabbStarted += (o, e) =>
            {
                eventWasCalled = true;
                if (e.PlayerPosition == this.GetPlayerPosition(0, dealerPosition))
                {
                    dealerWon = true;
                }
            };

            this.StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Dabb phase.");
            Assert.That(dealerWon, "The dealer did not win the bidding phase.");
        }

        [Test]
        public void GoingOutInDabbPhase_TriggersScoreCalculation([Range(0, 3)] int playerNumber, [Range(0, 3)] int dealerPosition)
        {
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipBiddingPhase(playerNumber, dealerPosition);
            this.MakePlayerGoOut();

            var eventWasCalled = false;
            var expectedPlayerWentOut = false;
            var playerPosition = this.GetPlayerPosition(playerNumber, dealerPosition);
            this.sut.EventSource.CalculatingGoingOutScoreStarted += (o, e) =>
            {
                eventWasCalled = true;
                if (e.PlayerPosition == playerPosition)
                {
                    expectedPlayerWentOut = true;
                }
            };

            this.StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Going-Out Score Calculation phase.");
            Assert.That(expectedPlayerWentOut, "A certain player was expected to go out, but another player did so.");
        }

        [Test]
        public void FinishingGoingOutScoreCalculation_FinishesGame()
        {
            var dealerPosition = 0;
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipBiddingPhase(0, dealerPosition);
            this.MakePlayerGoOut();
            this.SkipScoreCalculationPhases();

            var eventWasCalled = false;
            this.sut.EventSource.GameFinished += (o, e) => eventWasCalled = true;

            this.StartGame();

            Assert.That(eventWasCalled, "The game was expected to be finished but is still in a different state.");
        }

        [Test]
        public void SelectingTrump_StartsMeldingPhase()
        {
            var dealerPosition = 0;
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipBiddingPhase(0, dealerPosition);
            this.MakePlayerSelectTrump();

            var eventWasCalled = false;
            this.sut.EventSource.MeldingStarted += (o, e) => eventWasCalled = true;

            this.StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Melding phase.");
        }

        [Test]
        public void MeldsSeenByAllPlayers_StartsTrickTakingPhase()
        {
            var dealerPosition = 0;
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipBiddingPhase(0, dealerPosition);
            this.SkipDabbPhase();
            this.SkipMeldingPhase();

            var eventWasCalled = false;
            var righthandPlayerOfDealerPlacesFirstCard = false;
            this.sut.EventSource.WaitingForCardStarted += (o, e) =>
            {
                righthandPlayerOfDealerPlacesFirstCard = e.PlayerPosition == this.GetPlayerPosition(1, dealerPosition);
                eventWasCalled = true;
            };

            this.StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Trick Taking phase.");
            Assert.That(righthandPlayerOfDealerPlacesFirstCard, "The right-hand player of the dealer is not the first to place a card");
        }

        [Test]
        public void PlacingCard_StartsCardValidation()
        {
            var dealerPosition = 0;
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipBiddingPhase(playerNumber: 0, dealerPosition: dealerPosition);
            this.SkipDabbPhase();
            this.SkipMeldingPhase();
            this.SkipCardPlacingState();

            bool eventWasCalled = false;
            this.sut.EventSource.ValidatingCardStarted += (o, e) => eventWasCalled = true;

            this.StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Card Validation phase.");
        }

        [Test]
        public void InvalidCard_StartsReverting()
        {
            var dealerPosition = 0;
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipBiddingPhase(playerNumber: 0, dealerPosition: dealerPosition);
            this.SkipDabbPhase();
            this.SkipMeldingPhase();
            this.SkipCardPlacingState();
            this.SimulateInvalidCard();

            bool eventWasCalled = false;
            this.sut.EventSource.RevertingInvalidMoveStarted += (o, e) => eventWasCalled = true;

            this.StartGame();

            Assert.That(eventWasCalled, "The state machine did not start reverting an invalid move.");
        }

        [Test]
        public void RevertingInvalidMove_GoesBackToPlacingCard()
        {
            var dealerPosition = 0;
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipBiddingPhase(playerNumber: 0, dealerPosition: dealerPosition);
            this.SkipDabbPhase();
            this.SkipMeldingPhase();
            this.SkipCardPlacingStateOnce();
            this.SimulateInvalidCard();
            this.SkipRevertingState();

            int numberOfTimesCardPlacingStateWasEntered = 0;
            this.sut.EventSource.WaitingForCardStarted += (o, e) => numberOfTimesCardPlacingStateWasEntered++;

            this.StartGame();

            Assert.AreEqual(2, numberOfTimesCardPlacingStateWasEntered);
        }

        [Test]
        public void PlacingLosingCard_LetsNextPlayerPlaceCard()
        {
            var dealerPosition = 0;
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            var firstPlayerNumber = 1;
            var secondPlayerNumber = 2;
            var firstPlayerPosition = this.GetPlayerPosition(firstPlayerNumber, dealerPosition);
            var secondPlayerPosition = this.GetPlayerPosition(secondPlayerNumber, dealerPosition);

            this.SkipDealingPhase();
            this.SkipBiddingPhase(firstPlayerNumber, dealerPosition);
            this.SkipDabbPhase();
            this.SkipMeldingPhase();
            this.SkipCardPlacingStateOnce();
            this.SimulateLosingCard();

            int numberOfTimesFirstPlayerWasAllowedToPlaceCards = 0;
            int numberOfTimesSecondPlayerWasAllowedToPlaceCards = 0;

            this.sut.EventSource.WaitingForCardStarted += (o, e) =>
            {
                if (e.PlayerPosition == firstPlayerPosition)
                {
                    numberOfTimesFirstPlayerWasAllowedToPlaceCards++;
                }
                else if (e.PlayerPosition == secondPlayerPosition)
                {
                    numberOfTimesSecondPlayerWasAllowedToPlaceCards++;
                }
                else
                {
                    Assert.That(false, string.Format(
                        "A player at position {0} was allowed to place cards but only positions {1} and {2} were expected",
                        e.PlayerPosition,
                        firstPlayerPosition,
                        secondPlayerPosition));
                }
            };

            this.StartGame();

            Assert.AreEqual(1, numberOfTimesFirstPlayerWasAllowedToPlaceCards);
            Assert.AreEqual(1, numberOfTimesSecondPlayerWasAllowedToPlaceCards);
        }

        [Test]
        public void PlacingFourCards_StartsNewRound()
        {
            var dealerPosition = 0;
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipBiddingPhase(0, dealerPosition);
            this.SkipDabbPhase();
            this.SkipMeldingPhase();
            this.SkipCardPlacingState();
            this.SimulateWinningCard();

            bool eventWasCalled = false;
            this.sut.EventSource.StartingNewRoundStarted += (o, e) =>
            {
                eventWasCalled = true;
            };

            this.StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Starting New Round (Trick Taking) phase.");
        }

        [Test]
        public void PlacingCardsUntilNoneAreLeft_TriggersScoreCalculation()
        {
            var dealerPosition = 0;
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipBiddingPhase(0, dealerPosition);
            this.SkipDabbPhase();
            this.SkipMeldingPhase();
            this.SkipCardPlacingState();
            this.SimulateWinningCard();
            this.SkipStartingNewRoundState();

            // Make sure the score calculation start event is being fired
            bool eventWasCalled = false;
            this.sut.EventSource.CountingPlayerOrTeamScoresStarted += (o, e) =>
            {
                eventWasCalled = true;
            };

            // Also make sure that the amount of new rounds matches the expected amount
            // Note: The actual number of rounds is initialized by one, since the event is not fired for the very first round.
            var actualNumberOfRounds = 1;
            this.sut.EventSource.StartingNewRoundStarted += (o, e) =>
            {
                actualNumberOfRounds++;
            };

            this.StartGame();

            // The expected number of rounds was deduced from the table of amount of cards per player in the German Wikipedia: https://de.wikipedia.org/wiki/Binokel#Geben
            var expectedNumberOfRounds = this.ruleSettings.GameType == GameType.ThreePlayerGame ? 12 : 9;
            expectedNumberOfRounds += this.ruleSettings.SevensAreIncluded ? 2 : 0;

            Assert.That(eventWasCalled, "The state machine did not reach the Starting New Round (Trick Taking) phase.");
            Assert.AreEqual(expectedNumberOfRounds, actualNumberOfRounds, "The were more or less rounds played than expected before ending the game.");
        }

        [Test]
        public void FinishingScoreCalculation_EndsGame()
        {
            var dealerPosition = 0;
            this.sut.PrepareNewGame(this.ruleSettings, dealerPosition);

            this.SkipDealingPhase();
            this.SkipBiddingPhase(0, dealerPosition);
            this.SkipDabbPhase();
            this.SkipMeldingPhase();
            this.SkipCardPlacingState();
            this.SimulateWinningCard();
            this.SkipStartingNewRoundState();
            this.SkipScoreCalculationPhases();

            bool eventWasCalled = false;
            this.sut.EventSource.GameFinished += (o, e) =>
            {
                eventWasCalled = true;
            };

            this.StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the final state.");
        }

        // Starts the game. Since the state machine is currently synchronous, this call is blocking.
        private void StartGame()
        {
            this.sut.TriggerSink.SendTrigger(Common.GameTrigger.GameStarted);
        }

        /// <summary>
        /// Retrieves the position on the table of the player with the given number, where the dealerPosition represents number zero.
        /// </summary>
        /// <param name="playerNumber">The number of the player, where 0 = dealer, 1 = right-hand player of dealer, etc.</param>
        /// <param name="dealerPosition">The position on the table where 0 = human player (single game) or host (multiplayer).</param>
        /// <returns>The position for the given number in the current game.</returns>
        private int GetPlayerPosition(int playerNumber, int dealerPosition)
        {
            return (dealerPosition + playerNumber) % 4;
        }

        private void SkipDealingPhase()
        {
            this.sut.EventSource.DealingStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.DealingFinished);
            };
        }

        private void SkipBiddingPhase(int playerNumber, int dealerPosition)
        {
            this.MakePlayersPassFirstBidUntilPlayerNumber(playerNumber, dealerPosition);
            this.SkipPlayerSwitchingPhases();

            var playerPosition = this.GetPlayerPosition(playerNumber, dealerPosition);
            this.sut.EventSource.WaitingForCounterOrPassStarted += (o, e) =>
            {
                if (e.PlayerPosition == playerPosition)
                {
                    this.sut.TriggerSink.SendTrigger(Common.GameTrigger.BidCountered);
                }
                else
                {
                    this.sut.TriggerSink.SendTrigger(Common.GameTrigger.Passed);
                }
            };
            this.sut.EventSource.WaitingForBidOrPassStarted += (o, e) =>
            {
                if (e.PlayerPosition == playerPosition)
                {
                    this.sut.TriggerSink.SendTrigger(Common.GameTrigger.BidPlaced);
                }
                else
                {
                    this.sut.TriggerSink.SendTrigger(Common.GameTrigger.Passed);
                }
            };
        }

        private void MakePlayersPassFirstBidUntilPlayerNumber(int playerNumber, int dealerPosition)
        {
            var playerPosition = this.GetPlayerPosition(playerNumber, dealerPosition);

            this.sut.EventSource.WaitingForFirstBidStarted += (o, e) =>
            {
                if (e.PlayerPosition == playerPosition)
                {
                    this.sut.TriggerSink.SendTrigger(Common.GameTrigger.BidPlaced);
                }
                else
                {
                    this.sut.TriggerSink.SendTrigger(Common.GameTrigger.Passed);
                }
            };
        }

        private void MakePlayersPassCounterBid()
        {
            this.sut.EventSource.WaitingForCounterOrPassStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.Passed);
            };
        }

        private void MakePlayersCounterButPassWhenCountered()
        {
            this.sut.EventSource.WaitingForBidOrPassStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.Passed);
            };
            this.sut.EventSource.WaitingForCounterOrPassStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.BidCountered);
            };
        }

        private void SkipPlayerSwitchingPhases()
        {
            this.sut.EventSource.SwitchingPlayerBeforeFirstBidStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
            };
            this.sut.EventSource.SwitchingCounterBidPlayerStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
            };
            this.sut.EventSource.SwitchingCurrentBidPlayerStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
            };
            this.sut.EventSource.SwitchingCurrentTrickPlayerStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
            };
        }

        private void MakePlayerGoOut()
        {
            this.sut.EventSource.ExchangingCardsWithDabbStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.GoingOut);
            };
        }

        private void SkipScoreCalculationPhases()
        {
            this.sut.EventSource.CalculatingGoingOutScoreStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.ScoreCalculationFinished);
            };
            this.sut.EventSource.CountingPlayerOrTeamScoresStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.ScoreCalculationFinished);
            };
        }

        private void MakePlayerSelectTrump()
        {
            this.sut.EventSource.ExchangingCardsWithDabbStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.TrumpSelected);
            };
        }

        private void SkipDabbPhase()
        {
            this.MakePlayerSelectTrump();
        }

        private void SkipMeldingPhase()
        {
            this.sut.EventSource.MeldingStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.MeldsSeenByAllPlayers);
            };

            // This event is only interesting for the UI so it can prepare things. There is no user/AI interaction involved
            this.sut.EventSource.TrickTakingStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.ReadyForTrickTaking);
            };
        }

        private void SkipCardPlacingState()
        {
            this.sut.EventSource.WaitingForCardStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.CardPlaced);
            };
        }

        private void SkipCardPlacingStateOnce()
        {
            EventHandler<PlayerPositionEventArgs> handler = null;
            handler = (o, e) =>
            {
                this.sut.EventSource.WaitingForCardStarted -= handler;
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.CardPlaced);
            };
            this.sut.EventSource.WaitingForCardStarted += handler;
        }

        private void SimulateInvalidCard()
        {
            this.sut.EventSource.ValidatingCardStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.InvalidCardPlaced);
            };
        }

        private void SimulateLosingCard()
        {
            this.sut.EventSource.ValidatingCardStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.LosingCardPlaced);
            };
        }

        private void SimulateWinningCard()
        {
            this.sut.EventSource.ValidatingCardStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.WinningCardPlaced);
            };
        }

        private void SkipRevertingState()
        {
            this.sut.EventSource.RevertingInvalidMoveStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.RevertingFinished);
            };
        }

        private void SkipStartingNewRoundState()
        {
            this.sut.EventSource.StartingNewRoundStarted += (o, e) =>
            {
                this.sut.TriggerSink.SendTrigger(Common.GameTrigger.NewRoundStarted);
            };
        }
    }
}
#pragma warning disable CS1591 // Same reason as above
#pragma warning restore SA1600 // Elements should be documented