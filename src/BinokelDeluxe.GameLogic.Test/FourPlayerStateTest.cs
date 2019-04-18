using NUnit.Framework;
using System.Collections.Generic;

namespace BinokelDeluxe.GameLogic.Test
{
    public class FourPlayerStateTest
    {
        private SingleGameStateBridge _sut; // System under test
        private GameLogic.RuleSettings _ruleSettings;

        private int _currentPlayerNumber;
        private int _nextPlayerNumber;

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
            _sut.PrepareNewGame(_ruleSettings, dealerNumber: 0);

            var eventWasCalled = false;
            _sut.GetEventSender().DealingStarted += (o, e) => eventWasCalled = true;

            StartGame();
            Assert.That(eventWasCalled);
        }

        [Test]
        public void SingleGameStateBridge_AllPlayersPassing_DealerWins([Range(0,3)]int dealerNumber)
        {
            _sut.PrepareNewGame(_ruleSettings, dealerNumber);

            SkipDealingPhase();
            SkipPlayerSwitchingPhases();
            LetPlayersPassFirstBid(new List<int>{ 1, 2, 3}, dealerNumber);

            var eventWasCalled = false;
            var dealerWon = false;
            _sut.GetEventSender().ExchangingCardsWithDabbStarted += (o, e) =>
            {
                eventWasCalled = true;
                if (e.PlayerNumber == dealerNumber)
                {
                    dealerWon = true;
                }
            };

            StartGame();

            Assert.That(eventWasCalled, "The state machine did not reach the Dabb phase.");
            Assert.That(dealerWon, "The dealer did not win.");
        }




        private void StartGame()
        {
            _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.GameStarted);
        }

        private int GetPlayerNumber(int playerPosition, int dealerNumber)
        {
            return (dealerNumber + playerPosition) % 4;
        }

        private void SkipDealingPhase()
        {
            _sut.GetEventSender().DealingStarted += (o, e) =>
            {
                _currentPlayerNumber = e.CurrentPlayerNumber;
                _nextPlayerNumber = e.NextPlayerNumber;
                _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.DealingFinished);
            };
        }
        private void LetPlayersPassFirstBid(IList<int> playerPositions, int dealerNumber)
        {
            var playerNumbers = new List<int>();
            foreach (var playerPosition in playerPositions)
            {
                playerNumbers.Add(GetPlayerNumber(playerPosition, dealerNumber));
            }

            _sut.GetEventSender().WaitingForFirstBidStarted += (o, e) =>
            {
                if(playerNumbers.Contains(_currentPlayerNumber))
                {
                    _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.Passed);
                }
                else
                {
                    _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.BidPlaced);
                }
            };
        }
        private void SkipPlayerSwitchingPhases()
        {
            _sut.GetEventSender().SwitchingPlayerBeforeFirstBidStarted += (o, e) =>
            {
                _currentPlayerNumber = e.CurrentPlayerNumber;
                _nextPlayerNumber = e.NextPlayerNumber;
                _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.PlayerSwitched);
            };
            _sut.GetEventSender().SwitchingCounterBidPlayerStarted += (o, e) =>
            {
                _nextPlayerNumber = e.PlayerNumber;
                _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.PlayerSwitched);
            };
            _sut.GetEventSender().SwitchingCurrentBidPlayerStarted += (o, e) =>
            {
                _currentPlayerNumber = e.CurrentPlayerNumber;
                _nextPlayerNumber = e.NextPlayerNumber;
                _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.PlayerSwitched);
            };
            _sut.GetEventSender().SwitchingCurrentTrickPlayerStarted += (o, e) =>
            {
                _currentPlayerNumber = e.PlayerNumber;
                _sut.GetTriggerSink().SendTrigger(SingleGameTrigger.PlayerSwitched);
            };
        }
    }
}