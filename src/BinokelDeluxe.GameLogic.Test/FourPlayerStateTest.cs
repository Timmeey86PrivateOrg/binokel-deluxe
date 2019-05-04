using NUnit.Framework;
using System;

namespace BinokelDeluxe.GameLogic.Test
{
    /// <summary>
    /// Defines tests for the SingleGameStateBridge for four players.
    /// See https://github.com/Timmeey86/binokel-deluxe/wiki/Glossary for the difference between player positions and numbers.
    /// </summary>
    public class FourPlayerStateTest
    {
        private SingleGameStateBridge _sut; // System under test
        private GameLogic.RuleSettings _ruleSettings;

        [SetUp]
        public void Setup()
        {
            _sut = new SingleGameStateBridge();
            _ruleSettings = new GameLogic.RuleSettings()
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
                SevensAreIncluded = false
            };
        }

        [Test, MaxTime(2000)]
        public void StartingGame_SendsDealingStartedEvent()
        {
            _sut.PrepareNewGame(_ruleSettings, dealerPosition: 0);

            var eventWasCalled = false;
            _sut.EventSource.DealingStarted += (o, e) => eventWasCalled = true;

            StartGame();
            Assert.That(eventWasCalled);
        }

        [Test, MaxTime(2000)]
        public void AllPlayersPassing_LetsDealerWinBiddingPhase([Range(0, 3)]int dealerPosition)
        {
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipPlayerSwitchingPhases();
            MakePlayersPassFirstBidUntilPlayerNumber(0, dealerPosition);

            var eventWasCalled = false;
            var dealerWon = false;
            _sut.EventSource.ExchangingCardsWithDabbStarted += (o, e) =>
            {
                eventWasCalled = true;
                if (e.PlayerPosition == dealerPosition)
                {
                    dealerWon = true;
                }
            };

            StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Dabb phase.");
            Assert.That(dealerWon, "The dealer did not win the bidding phase.");
        }

        [Test, MaxTime(2000)]
        public void FirstPlayerBiddingAndRestPassing_LetsFirstPlayerWinBiddingPhase([Range(0, 3)]int dealerPosition)
        {
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipPlayerSwitchingPhases();
            MakePlayersPassFirstBidUntilPlayerNumber(1, dealerPosition);
            MakePlayersPassCounterBid();

            var eventWasCalled = false;
            var firstPlayerWon = false;
            _sut.EventSource.ExchangingCardsWithDabbStarted += (o, e) =>
            {
                eventWasCalled = true;
                if (e.PlayerPosition == GetPlayerPosition(1, dealerPosition))
                {
                    firstPlayerWon = true;
                }
            };

            StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Dabb phase.");
            Assert.That(firstPlayerWon, "The first player did not win the bidding phase.");
        }

        [Test, MaxTime(2000)]
        public void EachPlayerBiddingOnceThenPassing_LetsDealerWinBiddingPhase([Range(0, 3)]int dealerPosition)
        {
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipPlayerSwitchingPhases();
            MakePlayersPassFirstBidUntilPlayerNumber(0, dealerPosition);
            MakePlayersCounterButPassWhenCountered();

            var eventWasCalled = false;
            var dealerWon = false;
            _sut.EventSource.ExchangingCardsWithDabbStarted += (o, e) =>
            {
                eventWasCalled = true;
                if (e.PlayerPosition == GetPlayerPosition(0, dealerPosition))
                {
                    dealerWon = true;
                }
            };

            StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Dabb phase.");
            Assert.That(dealerWon, "The dealer did not win the bidding phase.");
        }

        [Test, MaxTime(2000)]
        public void GoingOutInDabbPhase_TriggersScoreCalculation([Range(0, 3)] int playerNumber, [Range(0, 3)] int dealerPosition)
        {
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipBiddingPhase(playerNumber, dealerPosition);
            MakePlayerGoOut();

            var eventWasCalled = false;
            var expectedPlayerWentOut = false;
            var playerPosition = GetPlayerPosition(playerNumber, dealerPosition);
            _sut.EventSource.CalculatingGoingOutScoreStarted += (o, e) =>
            {
                eventWasCalled = true;
                if (e.PlayerPosition == playerPosition)
                {
                    expectedPlayerWentOut = true;
                }
            };

            StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Going-Out Score Calculation phase.");
            Assert.That(expectedPlayerWentOut, "A certain player was expected to go out, but another player did so.");
        }

        [Test, MaxTime(2000)]
        public void FinishingGoingOutScoreCalculation_FinishesGame()
        {
            var dealerPosition = 0;
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipBiddingPhase(0, dealerPosition);
            MakePlayerGoOut();
            SkipScoreCalculationPhases();

            var eventWasCalled = false;
            _sut.EventSource.GameFinished += (o, e) => eventWasCalled = true;

            StartGame();

            Assert.That(eventWasCalled, "The game was expected to be finished but is still in a different state.");
        }

        [Test, MaxTime(2000)]
        public void SelectingTrump_StartsMeldingPhase()
        {
            var dealerPosition = 0;
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipBiddingPhase(0, dealerPosition);
            MakePlayerSelectTrump();

            var eventWasCalled = false;
            _sut.EventSource.MeldingStarted += (o, e) => eventWasCalled = true;

            StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Melding phase.");
        }

        [Test, MaxTime(2000)]
        public void MeldsSeenByAllPlayers_StartsTrickTakingPhase()
        {
            var dealerPosition = 0;
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipBiddingPhase(0, dealerPosition);
            SkipDabbPhase();
            SkipMeldingPhase();

            var eventWasCalled = false;
            var righthandPlayerOfDealerPlacesFirstCard = false;
            _sut.EventSource.WaitingForCardStarted += (o, e) =>
            {
                righthandPlayerOfDealerPlacesFirstCard = (e.PlayerPosition == GetPlayerPosition(1, dealerPosition));
                eventWasCalled = true;
            };

            StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Trick Taking phase.");
            Assert.That(righthandPlayerOfDealerPlacesFirstCard, "The right-hand player of the dealer is not the first to place a card");
        }

        [Test, MaxTime(2000)]
        public void PlacingCard_StartsCardValidation()
        {
            var dealerPosition = 0;
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipBiddingPhase(playerNumber: 0, dealerPosition: dealerPosition);
            SkipDabbPhase();
            SkipMeldingPhase();
            SkipCardPlacingState();

            bool eventWasCalled = false;
            _sut.EventSource.ValidatingCardStarted += (o, e) => eventWasCalled = true;

            StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Card Validation phase.");
        }

        [Test, MaxTime(2000)]
        public void InvalidCard_StartsReverting()
        {
            var dealerPosition = 0;
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipBiddingPhase(playerNumber: 0, dealerPosition: dealerPosition);
            SkipDabbPhase();
            SkipMeldingPhase();
            SkipCardPlacingState();
            SimulateInvalidCard();

            bool eventWasCalled = false;
            _sut.EventSource.RevertingInvalidMoveStarted += (o, e) => eventWasCalled = true;

            StartGame();

            Assert.That(eventWasCalled, "The state machine did not start reverting an invalid move.");
        }

        [Test, MaxTime(2000)]
        public void RevertingInvalidMove_GoesBackToPlacingCard()
        {
            var dealerPosition = 0;
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipBiddingPhase(playerNumber: 0, dealerPosition: dealerPosition);
            SkipDabbPhase();
            SkipMeldingPhase();
            SkipCardPlacingStateOnce();
            SimulateInvalidCard();
            SkipRevertingState();

            int numberOfTimesCardPlacingStateWasEntered = 0;
            _sut.EventSource.WaitingForCardStarted += (o, e) => numberOfTimesCardPlacingStateWasEntered++;

            StartGame();

            Assert.AreEqual(2, numberOfTimesCardPlacingStateWasEntered);
        }

        [Test, MaxTime(2000)]
        public void PlacingLosingCard_LetsNextPlayerPlaceCard()
        {
            var dealerPosition = 0;
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            var firstPlayerNumber = 1;
            var secondPlayerNumber = 2;
            var firstPlayerPosition = GetPlayerPosition(firstPlayerNumber, dealerPosition);
            var secondPlayerPosition = GetPlayerPosition(secondPlayerNumber, dealerPosition);

            SkipDealingPhase();
            SkipBiddingPhase(firstPlayerNumber, dealerPosition);
            SkipDabbPhase();
            SkipMeldingPhase();
            SkipCardPlacingStateOnce();
            SimulateLosingCard();

            int numberOfTimesFirstPlayerWasAllowedToPlaceCards = 0;
            int numberOfTimesSecondPlayerWasAllowedToPlaceCards = 0;

            _sut.EventSource.WaitingForCardStarted += (o, e) =>
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
                    Assert.That(false, String.Format(
                        "A player at position {0} was allowed to place cards but only positions {1} and {2} were expected",
                        e.PlayerPosition,
                        firstPlayerPosition,
                        secondPlayerPosition
                        ));
                }
            };

            StartGame();

            Assert.AreEqual(1, numberOfTimesFirstPlayerWasAllowedToPlaceCards);
            Assert.AreEqual(1, numberOfTimesSecondPlayerWasAllowedToPlaceCards);
        }

        [Test, MaxTime(2000)]
        public void PlacingFourCards_StartsNewRound()
        {
            var dealerPosition = 0;
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipBiddingPhase(0, dealerPosition);
            SkipDabbPhase();
            SkipMeldingPhase();
            SkipCardPlacingState();
            SimulateWinningCard();

            bool eventWasCalled = false;
            _sut.EventSource.StartingNewRoundStarted += (o, e) =>
            {
                eventWasCalled = true;
            };

            StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Starting New Round (Trick Taking) phase.");
        }

        [Test, MaxTime(2000)]
        public void PlacingCardsUntilNoneAreLeft_TriggersScoreCalculation()
        {
            var dealerPosition = 0;
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipBiddingPhase(0, dealerPosition);
            SkipDabbPhase();
            SkipMeldingPhase();
            SkipCardPlacingState();
            SimulateWinningCard();
            SkipStartingNewRoundState();

            // Make sure the score calculation start event is being fired
            bool eventWasCalled = false;
            _sut.EventSource.CountingPlayerOrTeamScoresStarted += (o, e) =>
            {
                eventWasCalled = true;
            };

            // Also make sure that the amount of new rounds matches the expected amount
            // Note: The actual number of rounds is initialized by one, since the event is not fired for the very first round.

            var actualNumberOfRounds = 1;
            _sut.EventSource.StartingNewRoundStarted += (o, e) =>
            {
                actualNumberOfRounds++;
            };

            StartGame();

            // The expected number of rounds was deduced from the table of amount of cards per player in the German Wikipedia: https://de.wikipedia.org/wiki/Binokel#Geben
            var expectedNumberOfRounds = _ruleSettings.GameType == GameType.ThreePlayerGame ? 12 : 9;
            expectedNumberOfRounds += _ruleSettings.SevensAreIncluded ? 2 : 0;

            Assert.That(eventWasCalled, "The state machine did not reach the Starting New Round (Trick Taking) phase.");
            Assert.AreEqual(expectedNumberOfRounds, actualNumberOfRounds, "The were more or less rounds played than expected before ending the game.");
        }

        [Test, MaxTime(2000)]
        public void FinishingScoreCalculation_EndsGame()
        {
            var dealerPosition = 0;
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipBiddingPhase(0, dealerPosition);
            SkipDabbPhase();
            SkipMeldingPhase();
            SkipCardPlacingState();
            SimulateWinningCard();
            SkipStartingNewRoundState();
            SkipScoreCalculationPhases();

            bool eventWasCalled = false;
            _sut.EventSource.GameFinished += (o, e) =>
            {
                eventWasCalled = true;
            };

            StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the final state.");
        }



        // Starts the game. Since the state machine is currently synchronous, this call is blocking.
        private void StartGame()
        {
            _sut.TriggerSink.SendTrigger(Common.GameTrigger.GameStarted);
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
            _sut.EventSource.DealingStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.DealingFinished);
            };
        }
        private void SkipBiddingPhase(int playerNumber, int dealerPosition)
        {
            MakePlayersPassFirstBidUntilPlayerNumber(playerNumber, dealerPosition);
            SkipPlayerSwitchingPhases();

            var playerPosition = GetPlayerPosition(playerNumber, dealerPosition);
            _sut.EventSource.WaitingForCounterOrPassStarted += (o, e) =>
            {
                if (e.PlayerPosition == playerPosition)
                {
                    _sut.TriggerSink.SendTrigger(Common.GameTrigger.BidCountered);
                }
                else
                {
                    _sut.TriggerSink.SendTrigger(Common.GameTrigger.Passed);
                }
            };
            _sut.EventSource.WaitingForBidOrPassStarted += (o, e) =>
            {
                if (e.PlayerPosition == playerPosition)
                {
                    _sut.TriggerSink.SendTrigger(Common.GameTrigger.BidPlaced);
                }
                else
                {
                    _sut.TriggerSink.SendTrigger(Common.GameTrigger.Passed);
                }
            };
        }
        private void MakePlayersPassFirstBidUntilPlayerNumber(int playerNumber, int dealerPosition)
        {
            var playerPosition = GetPlayerPosition(playerNumber, dealerPosition);

            _sut.EventSource.WaitingForFirstBidStarted += (o, e) =>
            {
                if (e.PlayerPosition == playerPosition)
                {
                    _sut.TriggerSink.SendTrigger(Common.GameTrigger.BidPlaced);
                }
                else
                {
                    _sut.TriggerSink.SendTrigger(Common.GameTrigger.Passed);
                }
            };
        }
        private void MakePlayersPassCounterBid()
        {
            _sut.EventSource.WaitingForCounterOrPassStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.Passed);
            };
        }
        private void MakePlayersCounterButPassWhenCountered()
        {
            _sut.EventSource.WaitingForBidOrPassStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.Passed);
            };
            _sut.EventSource.WaitingForCounterOrPassStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.BidCountered);
            };
        }
        private void SkipPlayerSwitchingPhases()
        {
            _sut.EventSource.SwitchingPlayerBeforeFirstBidStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
            };
            _sut.EventSource.SwitchingCounterBidPlayerStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
            };
            _sut.EventSource.SwitchingCurrentBidPlayerStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
            };
            _sut.EventSource.SwitchingCurrentTrickPlayerStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
            };
        }
        private void MakePlayerGoOut()
        {
            _sut.EventSource.ExchangingCardsWithDabbStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.GoingOut);
            };
        }
        private void SkipScoreCalculationPhases()
        {
            _sut.EventSource.CalculatingGoingOutScoreStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.ScoreCalculationFinished);
            };
            _sut.EventSource.CountingPlayerOrTeamScoresStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.ScoreCalculationFinished);
            };
        }
        private void MakePlayerSelectTrump()
        {
            _sut.EventSource.ExchangingCardsWithDabbStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.TrumpSelected);
            };
        }
        private void SkipDabbPhase()
        {
            MakePlayerSelectTrump();
        }
        private void SkipMeldingPhase()
        {
            _sut.EventSource.MeldingStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.MeldsSeenByAllPlayers);
            };
            // This event is only interesting for the UI so it can prepare things. There is no user/AI interaction involved
            _sut.EventSource.TrickTakingStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.ReadyForTrickTaking);
            };
        }
        private void SkipCardPlacingState()
        {
            _sut.EventSource.WaitingForCardStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.CardPlaced);
            };
        }
        private void SkipCardPlacingStateOnce()
        {
            EventHandler<PlayerPositionEventArgs> handler = null;
            handler = (o, e) =>
            {
                _sut.EventSource.WaitingForCardStarted -= handler;
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.CardPlaced);
            };
            _sut.EventSource.WaitingForCardStarted += handler;
        }
        private void SimulateInvalidCard()
        {
            _sut.EventSource.ValidatingCardStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.InvalidCardPlaced);
            };
        }
        private void SimulateLosingCard()
        {
            _sut.EventSource.ValidatingCardStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.LosingCardPlaced);
            };
        }
        private void SimulateWinningCard()
        {
            _sut.EventSource.ValidatingCardStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.WinningCardPlaced);
            };
        }
        private void SkipRevertingState()
        {
            _sut.EventSource.RevertingInvalidMoveStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.RevertingFinished);
            };
        }
        private void SkipStartingNewRoundState()
        {
            _sut.EventSource.StartingNewRoundStarted += (o, e) =>
            {
                _sut.TriggerSink.SendTrigger(Common.GameTrigger.NewRoundStarted);
            };
        }
    }
}