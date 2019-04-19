using NUnit.Framework;
using System.Collections.Generic;

namespace BinokelDeluxe.GameLogic.Test
{
    /// <summary>
    /// Defines tests for the SingleGameStateBridge for four players.
    /// See https://github.com/Timmeey86/binokel-deluxe/wiki/Glossary for the difference between player positions and numbers
    /// </summary>
    public class FourPlayerStateTest
    {
        private SingleGameStateBridge _sut; // System under test
        private GameLogic.RuleSettings _ruleSettings;

        private int _currentPlayerPosition;
        private int _nextPlayerPosition;

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

        [Test]
        public void SingleGameStateBridge_StartingGame_SendsDealingStartedEvent()
        {
            _sut.PrepareNewGame(_ruleSettings, dealerPosition: 0);

            var eventWasCalled = false;
            _sut.GetEventSource().DealingStarted += (o, e) => eventWasCalled = true;

            StartGame();
            Assert.That(eventWasCalled);
        }

        [Test]
        public void SingleGameStateBridge_AllPlayersPassing_LetsDealerWinBiddingPhase([Range(0, 3)]int dealerPosition)
        {
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipPlayerSwitchingPhases();
            MakePlayersPassFirstBidUntilPlayerNumber(0, dealerPosition);

            var eventWasCalled = false;
            var dealerWon = false;
            _sut.GetEventSource().ExchangingCardsWithDabbStarted += (o, e) =>
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

        [Test]
        public void SingleGameStateBridge_FirstPlayerBiddingAndRestPassing_LetsFirstPlayerWinBiddingPhase([Range(0, 3)]int dealerPosition)
        {
            _sut.PrepareNewGame(_ruleSettings, dealerPosition);

            SkipDealingPhase();
            SkipPlayerSwitchingPhases();
            MakePlayersPassFirstBidUntilPlayerNumber(1, dealerPosition);
            MakePlayersPassCounterBid();

            var eventWasCalled = false;
            var firstPlayerWon = false;
            _sut.GetEventSource().ExchangingCardsWithDabbStarted += (o, e) =>
            {
                eventWasCalled = true;
                if (e.PlayerPosition == GetPlayerPosition(1, dealerPosition))
                {
                    firstPlayerWon = true;
                }
            };

            Assert.That(eventWasCalled, "The state machine did not reach the Dabb phase.");
            Assert.That(firstPlayerWon, "The first player did not win the bidding phase.");
        }




        private void StartGame()
        {
            _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.GameStarted);
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
            _sut.GetEventSource().DealingStarted += (o, e) =>
            {
                _currentPlayerPosition = e.CurrentPlayerPosition;
                _nextPlayerPosition = e.NextPlayerPosition;
                _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.DealingFinished);
            };
        }
        private void MakePlayersPassFirstBidUntilPlayerNumber(int playerNumber, int dealerPosition)
        {
            var playerPosition = GetPlayerPosition(playerNumber, dealerPosition);

            _sut.GetEventSource().WaitingForFirstBidStarted += (o, e) =>
            {
                if (_currentPlayerPosition == playerPosition)
                {
                    _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.BidPlaced);
                }
                else
                {
                    _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.Passed);
                }
            };
        }
        private void MakePlayersPassCounterBid()
        {
            _sut.GetEventSource().WaitingForCounterOrPassStarted += (o, e) =>
            {
                _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.Passed);
            };
        }
        private void SkipPlayerSwitchingPhases()
        {
            _sut.GetEventSource().SwitchingPlayerBeforeFirstBidStarted += (o, e) =>
            {
                _currentPlayerPosition = e.CurrentPlayerPosition;
                _nextPlayerPosition = e.NextPlayerPosition;
                _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.PlayerSwitched);
            };
            _sut.GetEventSource().SwitchingCounterBidPlayerStarted += (o, e) =>
            {
                _nextPlayerPosition = e.PlayerPosition;
                _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.PlayerSwitched);
            };
            _sut.GetEventSource().SwitchingCurrentBidPlayerStarted += (o, e) =>
            {
                _currentPlayerPosition = e.CurrentPlayerPosition;
                _nextPlayerPosition = e.NextPlayerPosition;
                _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.PlayerSwitched);
            };
            _sut.GetEventSource().SwitchingCurrentTrickPlayerStarted += (o, e) =>
            {
                _currentPlayerPosition = e.PlayerPosition;
                _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.PlayerSwitched);
            };
        }
    }
}