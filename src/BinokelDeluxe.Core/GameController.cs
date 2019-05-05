using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
        private SynchronizationContext _uiContext;
        private bool _stopRequested;


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
            // Capture the context of the UI thread (= main thread = thread which created this object).
            _uiContext = SynchronizationContext.Current;
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
            // TODO: Randomize dealer
            //_currentDealer = _currentRNG.Next(playerCount);
            _currentDealer = 0;
            _currentBidAmount = 0;
            _stateBridge.PrepareNewGame(ruleSettings, _currentDealer);

            // Connect events
            ConnectEvents(_stateBridge.EventSource);

            // Start displaying the main menu in a new thread
            var thread = new Thread(() =>
            {
                while (!_stopRequested)
                {
                    var userAction = _userInterface.DisplayMainMenu();
                    if (userAction == UI.MainMenuActions.StartGame)
                    {
                        // Set up the basic UI
                        _userInterface.PrepareTable(_currentDealer);
                        _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.GameStarted);
                    }
                }
            });
            thread.Start();
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
            sender.TrickTakingStarted += Sender_TrickTakingStarted;
            sender.WaitingForCardStarted += EventSource_WaitingForCardStarted;
            sender.ValidatingCardStarted += EventSource_ValidatingCardStarted;
            sender.RevertingInvalidMoveStarted += EventSource_RevertingInvalidMoveStarted;
            sender.SwitchingCurrentTrickPlayerStarted += EventSource_SwitchingCurrentTrickPlayerStarted;
            sender.StartingNewRoundStarted += EventSource_StartingNewRoundStarted;
            sender.CountingPlayerOrTeamScoresStarted += EventSource_CountingPlayerOrTeamScoresStarted;
            sender.GameFinished += EventSource_GameFinished;
        }

        private bool IsUser(int position)
        {
            return position == UserPosition;
        }

        private void ValidateTrigger(Common.GameTrigger trigger, HashSet<Common.GameTrigger> allowedTriggers, string phaseName)
        {
            if (!allowedTriggers.Contains(trigger))
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

        private List<IEnumerable<Common.Card>> _cardsPerPlayer;
        private List<Common.Card> _cardsInDabb = new List<Common.Card>();
        private void EventSource_DealingStarted(object sender, GameLogic.PlayerPairEventArgs e)
        {
            var ruleSettings = _currentStateStack.CreationInfo.RuleSettings;

            _cardsPerPlayer = new List<IEnumerable<Common.Card>>();

            // four players, no sevens: 40 cards, 4 dabb, 9 per player
            // three players, no sevens: 40 cards, 4 dabb, 12 per player
            // four players, sevens: 48 cards, 4 dabb, 11 per player
            // three players, sevens: 48 cards, 6 dabb, 14 per player
            var numberOfPlayers = ruleSettings.GameType == GameLogic.GameType.ThreePlayerGame ? 3 : 4;
            var amountOfCards = ruleSettings.SevensAreIncluded ? 48 : 40;
            var numberOfCardsInDabb = ruleSettings.GameType == GameLogic.GameType.ThreePlayerGame && ruleSettings.SevensAreIncluded ? 6 : 4;
            var numberOfCardsPerPlayer = (amountOfCards - numberOfCardsInDabb) / numberOfPlayers;
            var numberOfCardsPerSuit = amountOfCards / 8; // four suits, two decks


            // TODO: generate and remember cards. Mind the possible inclusion of sevens and the number of players
            // For now, cards will be distributed according to the enum order
            var counter = 0;
            for (int player = 0; player < numberOfPlayers; player++)
            {
                var playerCards = new List<Common.Card>();
                for (int cardIndex = 0; cardIndex < numberOfCardsPerPlayer; cardIndex++)
                {
                    playerCards.Add(GetCard(amountOfCards, numberOfCardsPerSuit, counter));

                    counter++;
                }
                _cardsPerPlayer.Add(playerCards);
            }
            for (; counter < amountOfCards; counter++)
            {
                _cardsInDabb.Add(GetCard(amountOfCards, numberOfCardsPerSuit, counter));
            }

            _userInterface.PlayDealingAnimation(_currentDealer, _cardsPerPlayer, _cardsInDabb);
            _userInterface.UncoverCardsForUser(_cardsPerPlayer.First());
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.DealingFinished);
        }

        private static Common.Card GetCard(int amountOfCards, int numberOfCardsPerSuit, int counter)
        {
            // Take a new suit every numberOfCardsPerSuit cards.
            var cardSuit = (Common.CardSuit)((counter / numberOfCardsPerSuit) % Enum.GetValues(typeof(Common.CardSuit)).Length);
            // Take a new type every card, repeat from the beginning once numberOfCardsPerSuit have been taken.
            var cardTypeOffset = Enum.GetValues(typeof(Common.CardType)).Length - numberOfCardsPerSuit;
            var cardType = (Common.CardType)(counter % numberOfCardsPerSuit) + cardTypeOffset;
            // Increase the deck number after half of the cards.
            var deckNumber = (short)(counter / (amountOfCards / 2));

            var card = new Common.Card()
            {
                DeckNumber = deckNumber,
                Suit = cardSuit,
                Type = cardType
            };
            return card;
        }
        private void EventSource_WaitingForFirstBidStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            Common.GameTrigger trigger;
            if (IsUser(e.PlayerPosition))
            {
                trigger = _userInterface.LetUserPlaceFirstBidOrPass(InitialBidAmount);
                ValidateTrigger(trigger, BidOrPassTriggers, "initial bid");
                _currentStateStack.DeltaChanges.Add(new GameStateChangeInfo() { HumanTrigger = trigger });
            }
            else
            {
                // TODO: Implement AI
                trigger = Common.GameTrigger.BidPlaced;
            }

            if (trigger == Common.GameTrigger.Passed)
            {
                _userInterface.DisplayPlayerAsPassed(e.PlayerPosition);
            }
            else
            {
                _currentBidAmount = InitialBidAmount;
                if (!IsUser(e.PlayerPosition))
                {
                    _userInterface.DisplayAIBid(e.PlayerPosition, _currentBidAmount);
                }
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
            HandleBidOrPass(Common.GameTrigger.BidPlaced, e);
        }

        private void HandleBidOrPass(Common.GameTrigger bidTriggerType, GameLogic.PlayerPositionEventArgs e)
        {
            var potentialBid = _currentBidAmount + 10;
            Common.GameTrigger trigger;
            if (IsUser(e.PlayerPosition))
            {
                trigger = _userInterface.LetUserDoCounterBidOrPass(potentialBid);
                ValidateTrigger(trigger, BidOrPassTriggers, "bidding");
                if (trigger == Common.GameTrigger.BidPlaced)
                {
                    trigger = bidTriggerType; // might be bid or counterbid
                }
                _currentStateStack.DeltaChanges.Add(new GameStateChangeInfo() { HumanTrigger = trigger });
            }
            else
            {
                // TODO: Implement AI
                if (_currentBidAmount < (150 + 20 * e.PlayerPosition))
                {
                    trigger = bidTriggerType;
                }
                else
                {
                    trigger = Common.GameTrigger.Passed;
                }
            }

            if (trigger == Common.GameTrigger.Passed)
            {
                _userInterface.DisplayPlayerAsPassed(e.PlayerPosition);
            }
            else
            {
                _currentBidAmount = potentialBid;
                if (!IsUser(e.PlayerPosition))
                {
                    _userInterface.DisplayAIBid(e.PlayerPosition, _currentBidAmount);
                }
            }
            _stateBridge.TriggerSink.SendTrigger(trigger);
        }

        private void EventSource_WaitingForCounterOrPassStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            HandleBidOrPass(Common.GameTrigger.BidCountered, e);
        }

        private void EventSource_SwitchingCurrentBidPlayerStarted(object sender, GameLogic.PlayerPairEventArgs e)
        {
            // Nothing to do here at the moment
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
        }

        private void EventSource_SwitchingCounterBidPlayerStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // Nothing to do here at the moment
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
        }

        private Common.CardSuit? _trumpSuit = null;

        private bool _playerWentOut = false;
        private void EventSource_ExchangingCardsWithDabbStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            _userInterface.UncoverDabb(_cardsInDabb);

            Common.GameTrigger trigger;
            if (IsUser(e.PlayerPosition))
            {
                IEnumerable<Common.Card> discardedCards = null;
                trigger = _userInterface.LetUserExchangeCardsWithDabb(out discardedCards, out _trumpSuit);
                // TODO: Validate and process discarded cards and selected trump suit
                var removedDabbCards = new HashSet<Common.Card>(_cardsInDabb);
                var removedPlayerCards = new List<Common.Card>();
                foreach (var discardedCard in discardedCards)
                {
                    if (_cardsInDabb.Contains(discardedCard))
                    {
                        // The card remains in dabb, remove it from the set of cards which could have been taken by the player
                        removedDabbCards.Remove(discardedCard);
                    }
                    {
                        // The card used to be a player card but was discarded
                        removedPlayerCards.Add(discardedCard);
                    }
                }
                _cardsInDabb = _cardsInDabb.Except(removedDabbCards.ToList()).Concat(removedPlayerCards).ToList();
                _cardsPerPlayer[0] = _cardsPerPlayer[0].Except(removedPlayerCards).Concat(removedDabbCards.ToList());
                // TODO: Actually rearrange cards
                _userInterface.RearrangeCardsForUser(_cardsPerPlayer.First());
            }
            else
            {
                // TODO: Implement AI
                trigger = Common.GameTrigger.TrumpSelected;
                _trumpSuit = Common.CardSuit.Hearts;
            }

            ValidateTrigger(
                trigger,
                new HashSet<Common.GameTrigger>()
                {
                    Common.GameTrigger.TrumpSelected,
                    Common.GameTrigger.GoingOut,
                    Common.GameTrigger.BettelAnnounced,
                    Common.GameTrigger.DurchAnnounced
                },
                "dabb exchange"
                );
            _playerWentOut = trigger == Common.GameTrigger.GoingOut;

            _stateBridge.TriggerSink.SendTrigger(trigger);
        }

        private void EventSource_CalculatingGoingOutScoreStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // TODO: Calculate actual score
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.ScoreCalculationFinished);
        }

        private void EventSource_MeldingStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // TODO: Display actual melds
            _userInterface.DisplayMelds(
                new List<Common.MeldData>()
                {
                    new Common.MeldData()
                    {
                        Melds = new List<Common.SingleMeld>()
                        {
                            new Common.SingleMeld() { Cards = _cardsPerPlayer[0].Take(5).ToList(), Points = 150 },
                            new Common.SingleMeld() { Cards = _cardsPerPlayer[0].Skip(5).Take(2).ToList(), Points = 40 }
                        }
                    },
                    new Common.MeldData()
                    {
                        Melds = new List<Common.SingleMeld>() { new Common.SingleMeld() { Cards = _cardsPerPlayer[1].Take(2).ToList(), Points = 20 } }
                    },
                    new Common.MeldData()
                    {
                        Melds = new List<Common.SingleMeld>() { new Common.SingleMeld() { Cards = _cardsPerPlayer[2].Take(4).ToList(), Points = 80 } }
                    },
                    new Common.MeldData()
                    {
                        Melds = new List<Common.SingleMeld>() { new Common.SingleMeld() { Cards = _cardsPerPlayer[3].Take(5).ToList(), Points = 50 } }
                    }
                });
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.MeldsSeenByAllPlayers);
        }

        private void Sender_TrickTakingStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            _userInterface.PrepareTrickTaking(_cardsPerPlayer, _currentDealer);
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.ReadyForTrickTaking);
        }


        private Common.Card _mostRecentCard = null;
        private void EventSource_WaitingForCardStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            _userInterface.ActivatePlayer(e.PlayerPosition);
            if (IsUser(e.PlayerPosition))
            {
                _mostRecentCard = _userInterface.LetUserSelectCard();
            }
            else
            {
                // TODO: Implement AI
                _mostRecentCard = _cardsPerPlayer[e.PlayerPosition].First();
                _cardsPerPlayer[e.PlayerPosition] = _cardsPerPlayer[e.PlayerPosition].Except(new List<Common.Card>() { _mostRecentCard });
            }
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.CardPlaced);
        }

        private void EventSource_ValidatingCardStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // TODO: Actually validate card
            bool cardIsValid = true;
            // Assumption: AI always provides valid cards.
            if (IsUser(e.PlayerPosition) && !cardIsValid)
            {
                _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.InvalidCardPlaced);
            }
            else
            {
                // TODO: Check if the card is currently leading
                bool cardIsWinning = true;
                if (cardIsWinning)
                {
                    _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.WinningCardPlaced);
                }
                else
                {
                    _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.LosingCardPlaced);
                }
                _userInterface.PlaceCardInMiddle(e.PlayerPosition, _mostRecentCard);
            }
        }

        private void EventSource_RevertingInvalidMoveStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            _userInterface.HandleInvalidMove(new List<Common.Card>());
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.RevertingFinished);
        }

        private void EventSource_SwitchingCurrentTrickPlayerStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // Nothing to do here
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
        }

        private void EventSource_StartingNewRoundStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            _userInterface.MoveCardsToTrickWinner(e.PlayerPosition);
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.NewRoundStarted);
        }

        private void EventSource_CountingPlayerOrTeamScoresStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // TODO: Actually calculate score
            _userInterface.MoveCardsToTrickWinner(e.PlayerPosition);
            _stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.ScoreCalculationFinished);
        }

        private void EventSource_GameFinished(object sender, EventArgs e)
        {
            if (_playerWentOut)
            {
                // TODO: Actually provide scores
                _userInterface.DisplayGoingOutScore(new List<Common.ScoreData>());
            }
            else
            {
                // TODO: Actually provide scores
                _userInterface.DisplayGameScore(new List<Common.ScoreData>());
            }

        }
    }
}
