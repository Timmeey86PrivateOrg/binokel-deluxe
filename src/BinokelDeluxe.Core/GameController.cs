using System;
using System.Collections.Generic;

namespace BinokelDeluxe.Core
{
    /// <summary>
    /// This class assembles the different parts of the Binokel business logic and controls the work done by these parts.
    /// Try not doing anything else in this class.
    /// This class also serves as the primary external interface to the Cross-Platform UI code.
    /// </summary>
    public class GameController
    {
        private static Common.SerializableVersion CurrentStateStackVersion = new Common.SerializableVersion(1, 0);
        private const int UserPosition = 0;
        private const int InitialBidAmount = 150; // this seems to be fixed in all variants of rules out there.

        private readonly UI.IUserInterface _userInterface;
        private GameLogic.SingleGameStateBridge _stateBridge = null;
        private Random _currentRNG = null;
        private GameStateStack _currentStateStack = null;
        private int _currentDealer = -1;
        private int _currentBidAmount = 0;

        /// <summary>
        /// Creates a new game controller for the given user interface. Note: This should best be created and used outside of the UI thread.
        /// </summary>
        /// <param name="userInterface">The user interface to be used.</param>
        public GameController(UI.IUserInterface userInterface)
        {
            _userInterface = userInterface;
            _stateBridge = new GameLogic.SingleGameStateBridge();
        }

        /// <summary>
        /// Starts a new game with teh given rules and AI strategies.
        /// </summary>
        /// <param name="ruleSettings">The rules for the game.</param>
        /// <param name="aiStrategyTypes">The strategies for the computer players. Should contain null entries for human players.</param>
        public void StartNewGame(GameLogic.RuleSettings ruleSettings, List<string> aiStrategyTypes)
        {
            _currentStateStack = new GameStateStack()
            {
                CreationInfo = new GameCreationInfo()
                {
                    AIStrategyTypes = aiStrategyTypes,
                    RandomSeed = Guid.NewGuid().GetHashCode(),
                    RuleSettings = ruleSettings,
                    Version = CurrentStateStackVersion
                }
            };
            // TODO: Serialize state stack to a persistent location right away.

            // Prepare a new game with a random player being the dealer
            _currentRNG = new Random(_currentStateStack.CreationInfo.RandomSeed);
            var playerCount = ruleSettings.GameType == GameLogic.GameType.ThreePlayerGame ? 3 : 4;
            _currentDealer = _currentRNG.Next(playerCount);
            _currentBidAmount = 0;
            _stateBridge.PrepareNewGame(ruleSettings, _currentDealer);

            // Set up the basic UI
            _userInterface.PrepareTable(_currentDealer);

            // Connect events
            ConnectEvents(_stateBridge.EventSource);
        }

        private void ConnectEvents(GameLogic.ISingleGameEventSource sender)
        {
            sender.DealingStarted += EventSource_DealingStarted;
            sender.WaitingForFirstBidStarted += EventSource_WaitingForFirstBidStarted;
            sender.SwitchingPlayerBeforeFirstBidStarted += EventSource_SwitchingPlayerBeforeFirstBidStarted;
            sender.WaitingForBidOrPassStarted += EventSource_WaitingForBidOrPassStarted;
            sender.WaitingForCounterOrPassStarted += EventSource_WaitingForCounterOrPassStarted;
            sender.SwitchingCurrentBidPlayerStarted += EventSource_SwitchingCurrentBidPlayerStarted;
            sender.SwitchingCounterBidPlayerStarted += EventSource_SwitchingCounterBidPlayerStarted;
            sender.ExchangingCardsWithDabbStarted += EventSource_ExchangingCardsWithDabbStarted;
            sender.CalculatingGoingOutScoreStarted += EventSource_CalculatingGoingOutScoreStarted;
            sender.MeldingStarted += EventSource_MeldingStarted;
            sender.WaitingForCardStarted += EventSource_WaitingForCardStarted;
            sender.ValidatingCardStarted += EventSource_ValidatingCardStarted;
            sender.RevertingInvalidMoveStarted += EventSource_RevertingInvalidMoveStarted;
            sender.SwitchingCurrentTrickPlayerStarted += EventSource_SwitchingCurrentTrickPlayerStarted;
            sender.StartingNewRoundStarted += EventSource_StartingNewRoundStarted;
            sender.CountingPlayerOrTeamScoresStarted += EventSource_CountingPlayerOrTeamScoresStarted;
            sender.GameFinished += EventSource_GameFinished;
        }

        // TODO: The following methods, or at least their implementations, need to go to another class.
        private bool IsUser(int position)
        {
            return position == UserPosition;
        }

        private void ValidateTrigger(Common.GameTrigger trigger, HashSet<Common.GameTrigger> allowedTriggers, string phaseName)
        {
            if( !allowedTriggers.Contains(trigger))
            {
                throw new ArgumentException(
                    String.Format("The UI allowed the user to send the {0} trigger which is not allowed in the {1} phase.", trigger.ToString(), phaseName)
                    );
            }
        }

        private static readonly HashSet<Common.GameTrigger> BidOrPassTriggers = new HashSet<Common.GameTrigger>()
        {
            Common.GameTrigger.BidPlaced,
            Common.GameTrigger.Passed
        };

        private void EventSource_DealingStarted(object sender, GameLogic.PlayerPairEventArgs e)
        {
            var numberOfCardsPerPlayer = _currentStateStack.CreationInfo.RuleSettings.SevensAreIncluded ? 12 : 10;
            _userInterface.PlayDealingAnimation(_currentDealer, numberOfCardsPerPlayer, numberOfCardsInDabb: 4);
            // TODO: generate and remember user cards
            _userInterface.UncoverCardsForUser(new List<Common.Card>()
            {
                new Common.Card() { DeckNumber = 0, Suit = Common.CardSuit.Acorns, Type = Common.CardType.Ace },
                new Common.Card() { DeckNumber = 0, Suit = Common.CardSuit.Acorns, Type = Common.CardType.Ten },
                new Common.Card() { DeckNumber = 0, Suit = Common.CardSuit.Acorns, Type = Common.CardType.King },
                new Common.Card() { DeckNumber = 0, Suit = Common.CardSuit.Acorns, Type = Common.CardType.Ober },
                new Common.Card() { DeckNumber = 0, Suit = Common.CardSuit.Acorns, Type = Common.CardType.Unter },
                new Common.Card() { DeckNumber = 0, Suit = Common.CardSuit.Bells, Type = Common.CardType.Ace },
                new Common.Card() { DeckNumber = 0, Suit = Common.CardSuit.Bells, Type = Common.CardType.Ten },
                new Common.Card() { DeckNumber = 0, Suit = Common.CardSuit.Bells, Type = Common.CardType.King },
                new Common.Card() { DeckNumber = 0, Suit = Common.CardSuit.Bells, Type = Common.CardType.Ober },
                new Common.Card() { DeckNumber = 0, Suit = Common.CardSuit.Bells, Type = Common.CardType.Unter }
            });
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.DealingFinished);
        }
        private void EventSource_WaitingForFirstBidStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            Common.GameTrigger trigger;
            if (IsUser(e.PlayerPosition))
            {
                trigger = _userInterface.LetUserPlaceFirstBidOrPass(InitialBidAmount);
                ValidateTrigger(trigger, BidOrPassTriggers, "initial bid phase");
                _currentStateStack.DeltaChanges.Add(new GameStateChangeInfo() { HumanTrigger = trigger });
            }
            else
            {
                // TODO: Implement AI
                trigger = Common.GameTrigger.Passed;
            }

            if (trigger == Common.GameTrigger.Passed)
            {
                _userInterface.DisplayPlayerAsPassed(e.PlayerPosition);
            }
            else
            {
                _currentBidAmount = InitialBidAmount;
            }
            _stateBridge.TriggerSink.SendTrigger(trigger);
        }
        private void EventSource_SwitchingPlayerBeforeFirstBidStarted(object sender, GameLogic.PlayerPairEventArgs e)
        {
            // Nothing to do here at the moment
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
        }
        private void EventSource_WaitingForBidOrPassStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            var potentialBid = _currentBidAmount + 10;
            Common.GameTrigger trigger;
            if(IsUser(e.PlayerPosition))
            {
                trigger = _userInterface.LetUserDoCounterBidOrPass(potentialBid);
                ValidateTrigger(trigger, BidOrPassTriggers, "bidding phase");
                _currentStateStack.DeltaChanges.Add(new GameStateChangeInfo() { HumanTrigger = trigger });
            }
            else
            {
                // TODO: Implement AI
                trigger = Common.GameTrigger.Passed;
            }

            if (trigger == Common.GameTrigger.Passed)
            {
                _userInterface.DisplayPlayerAsPassed(e.PlayerPosition);
            }
            else
            {
                _currentBidAmount = potentialBid;
            }
            _stateBridge.TriggerSink.SendTrigger(trigger);
        }
        private void EventSource_WaitingForCounterOrPassStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // The UI does not care if a bid is a counter or a pass.
            EventSource_WaitingForBidOrPassStarted(sender, e);
        }

        private void EventSource_SwitchingCurrentBidPlayerStarted(object sender, GameLogic.PlayerPairEventArgs e)
        {
            /// <summary>
            /// Event listeners should be implemented as follows:
            /// Switch the current (and the next) player for bidding (but do not offer them options yet).
            /// Send a PlayerSwitched trigger when done.
            /// </summary>
            throw new NotImplementedException();
        }

        private void EventSource_SwitchingCounterBidPlayerStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            /// <summary>
            /// Event listeners should be implemented as follows:
            /// Switch the next player for counter-bidding (but do not offer them options yet).
            /// Send a PlayerSwitched trigger when done.
            /// </summary>
            throw new NotImplementedException();
        }

        private void EventSource_ExchangingCardsWithDabbStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
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
            throw new NotImplementedException();
        }

        private void EventSource_CalculatingGoingOutScoreStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            /// <summary>
            /// Event listeners should be implemented as follows:
            /// Calculate the score for each player after the player who won the bid goes out.
            /// Send a ScoreCalculationFinished trigger when done.
            /// </summary>
            throw new NotImplementedException();
        }

        private void EventSource_MeldingStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            /// <summary>
            /// Event listeners should be implemented as follows:
            /// Display the cards which can be melded by each player and display the total meld score for each player.
            /// Ask every player for confirmation that they saw the melds.
            /// Send a MeldsSeenByAllPlayers trigger when done.
            /// </summary>
            throw new NotImplementedException();
        }

        private void EventSource_WaitingForCardStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            /// <summary>
            /// Event listeners should be implemented as follows:
            /// Offer the player the chance to play a card of their choice (including invalid cards).
            /// Send a CardPlaced trigger when done.
            /// </summary>
            throw new NotImplementedException();
        }

        private void EventSource_ValidatingCardStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            /// <summary>
            /// Event listeners should be implemented as follows:
            /// Remove any potential highlighting, then validate the card which was placed by the player.
            /// In case of a valid card, find out if the card beats the current winning card.
            /// Send either an InvalidCardPlaced, a WinningCardPlaced or a LosingCardPlaced event when done.
            /// </summary>
            throw new NotImplementedException();
        }

        private void EventSource_RevertingInvalidMoveStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            /// <summary>
            /// Event listeners should be implemented as follows:
            /// Revert the last move and highlight the cards which are valid.
            /// Send a RevertingFinished trigger when done.
            /// </summary>
            throw new NotImplementedException();
        }

        private void EventSource_SwitchingCurrentTrickPlayerStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            /// <summary>
            /// Event listeners should be implemented as follows:
            /// Switch to the player which is identified by the player Position in the event arguments (but do not offer them choices yet).
            /// Send a PlayerSwitched trigger when done.
            /// </summary>
            throw new NotImplementedException();
        }

        private void EventSource_StartingNewRoundStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            /// <summary>
            /// Event listeners should be implemented as follows:
            /// Switch to the trick-winning player which is identified by the player Position in the event arguments and remember the cards won by this player.
            /// Send a NewRoundStarted trigger when done.
            /// </summary>
            throw new NotImplementedException();
        }

        private void EventSource_CountingPlayerOrTeamScoresStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            /// <summary>
            /// Event listeners should be implemented as follows:
            /// Calculate the scores for each player or team.
            /// The player argument identifies the player who won the last trick.
            /// Send a ScoreCalculationFinished trigger when done.
            /// </summary>
            throw new NotImplementedException();
        }

        private void EventSource_GameFinished(object sender, EventArgs e)
        {
            /// <summary>
            /// Lets event listeners know that the game was finished and a new one can be started.
            /// You need to prepare a new game for the next round.
            /// </summary>
            throw new NotImplementedException();
        }
    }
}
