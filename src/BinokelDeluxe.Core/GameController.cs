namespace BinokelDeluxe.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// This class assembles the different parts of the Binokel business logic and controls the work done by these parts.
    /// Try not doing anything else in this class.
    /// This class also serves as the primary external interface to the Cross-Platform UI code.
    /// </summary>
    public class GameController
    {
        private const int UserPosition = 0;
        private const int InitialBidAmount = 150; // this seems to be fixed in all variants of rules out there.

        private static readonly Common.SerializableVersion CurrentStateStackVersion = new Common.SerializableVersion(1, 0);

        private readonly UI.IUserInterface userInterface;

        private readonly GameLogic.SingleGameStateBridge stateBridge = null;
        private GameStateStack currentStateStack = null;
        private int currentDealer = -1;
        private int currentBidAmount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameController"/> class.
        /// Note: This should best be created and used outside of the UI thread.
        /// </summary>
        /// <param name="userInterface">The user interface to be used.</param>
        public GameController(UI.IUserInterface userInterface)
        {
            this.userInterface = userInterface;
            this.stateBridge = new GameLogic.SingleGameStateBridge();
        }

        /// <summary>
        /// Starts a new game with teh given rules and AI strategies.
        /// </summary>
        /// <param name="ruleSettings">The rules for the game.</param>
        /// <param name="aiStrategyTypes">The strategies for the computer players. Should contain null entries for human players.</param>
        public void StartNewGame(GameLogic.RuleSettings ruleSettings, List<string> aiStrategyTypes)
        {
            this.currentStateStack = new GameStateStack()
            {
                CreationInfo = new GameCreationInfo()
                {
                    AIStrategyTypes = aiStrategyTypes,
                    RandomSeed = Guid.NewGuid().GetHashCode(),
                    RuleSettings = ruleSettings,
                    Version = CurrentStateStackVersion,
                },
            };
            // TODO: Serialize state stack to a persistent location right away.

            // Prepare a new game with a random player being the dealer

            ////this.currentRNG = new Random(this.currentStateStack.CreationInfo.RandomSeed);
            ////var playerCount = ruleSettings.GameType == GameLogic.GameType.ThreePlayerGame ? 3 : 4;
            ////this.currentDealer = _currentRNG.Next(playerCount);

            this.currentDealer = 0;
            this.currentBidAmount = 0;
            this.stateBridge.PrepareNewGame(ruleSettings, this.currentDealer);

            // Connect events
            this.ConnectEvents(this.stateBridge.EventSource);

            // Start displaying the main menu in a new thread
            var stopRequested = false;
            var thread = new Thread(() =>
            {
                while (!stopRequested)
                {
                    var userAction = this.userInterface.DisplayMainMenu();
                    if (userAction == UI.MainMenuActions.StartGame)
                    {
                        // Set up the basic UI
                        this.userInterface.PrepareTable(this.currentDealer);
                        this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.GameStarted);
                    }
                }
            });
            thread.Start();
        }

        private void ConnectEvents(GameLogic.ISingleGameEventSource sender)
        {
            sender.DealingStarted += this.EventSource_DealingStarted;
            sender.WaitingForFirstBidStarted += this.EventSource_WaitingForFirstBidStarted;
            sender.SwitchingPlayerBeforeFirstBidStarted += this.EventSource_SwitchingPlayerBeforeFirstBidStarted;
            sender.WaitingForBidOrPassStarted += this.EventSource_WaitingForBidOrPassStarted;
            sender.WaitingForCounterOrPassStarted += this.EventSource_WaitingForCounterOrPassStarted;
            sender.SwitchingCurrentBidPlayerStarted += this.EventSource_SwitchingCurrentBidPlayerStarted;
            sender.SwitchingCounterBidPlayerStarted += this.EventSource_SwitchingCounterBidPlayerStarted;
            sender.ExchangingCardsWithDabbStarted += this.EventSource_ExchangingCardsWithDabbStarted;
            sender.CalculatingGoingOutScoreStarted += this.EventSource_CalculatingGoingOutScoreStarted;
            sender.MeldingStarted += this.EventSource_MeldingStarted;
            sender.TrickTakingStarted += this.Sender_TrickTakingStarted;
            sender.WaitingForCardStarted += this.EventSource_WaitingForCardStarted;
            sender.ValidatingCardStarted += this.EventSource_ValidatingCardStarted;
            sender.RevertingInvalidMoveStarted += this.EventSource_RevertingInvalidMoveStarted;
            sender.SwitchingCurrentTrickPlayerStarted += this.EventSource_SwitchingCurrentTrickPlayerStarted;
            sender.StartingNewRoundStarted += this.EventSource_StartingNewRoundStarted;
            sender.CountingPlayerOrTeamScoresStarted += this.EventSource_CountingPlayerOrTeamScoresStarted;
            sender.GameFinished += this.EventSource_GameFinished;
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
                    string.Format("The UI allowed the user to send the {0} trigger which is not allowed in the {1} phase.", trigger.ToString(), phaseName)
                    );
            }
        }

        private static readonly HashSet<Common.GameTrigger> BidOrPassTriggers = new HashSet<Common.GameTrigger>()
        {
            Common.GameTrigger.BidPlaced,
            Common.GameTrigger.Passed,
        };

        private List<IEnumerable<Common.Card>> cardsPerPlayer;
        private List<Common.Card> cardsInDabb = new List<Common.Card>();
        private void EventSource_DealingStarted(object sender, GameLogic.PlayerPairEventArgs e)
        {
            var ruleSettings = this.currentStateStack.CreationInfo.RuleSettings;

            this.cardsPerPlayer = new List<IEnumerable<Common.Card>>();

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
                this.cardsPerPlayer.Add(playerCards);
            }
            for (; counter < amountOfCards; counter++)
            {
                this.cardsInDabb.Add(GetCard(amountOfCards, numberOfCardsPerSuit, counter));
            }

            this.userInterface.PlayDealingAnimation(this.currentDealer, this.cardsPerPlayer, this.cardsInDabb);
            this.userInterface.UncoverCardsForUser(this.cardsPerPlayer.First());
            this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.DealingFinished);
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
            if (this.IsUser(e.PlayerPosition))
            {
                trigger = this.userInterface.LetUserPlaceFirstBidOrPass(InitialBidAmount);
                this.ValidateTrigger(trigger, BidOrPassTriggers, "initial bid");
                this.currentStateStack.DeltaChanges.Add(new GameStateChangeInfo() { HumanTrigger = trigger });
            }
            else
            {
                // TODO: Implement AI
                trigger = Common.GameTrigger.BidPlaced;
            }

            if (trigger == Common.GameTrigger.Passed)
            {
                this.userInterface.DisplayPlayerAsPassed(e.PlayerPosition);
            }
            else
            {
                this.currentBidAmount = InitialBidAmount;
                if (!this.IsUser(e.PlayerPosition))
                {
                    this.userInterface.DisplayAIBid(e.PlayerPosition, this.currentBidAmount);
                }
            }
            this.stateBridge.TriggerSink.SendTrigger(trigger);
        }
        private void EventSource_SwitchingPlayerBeforeFirstBidStarted(object sender, GameLogic.PlayerPairEventArgs e)
        {
            // Nothing to do here at the moment
            this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
        }
        private void EventSource_WaitingForBidOrPassStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            this.HandleBidOrPass(Common.GameTrigger.BidPlaced, e);
        }

        private void HandleBidOrPass(Common.GameTrigger bidTriggerType, GameLogic.PlayerPositionEventArgs e)
        {
            var potentialBid = this.currentBidAmount + 10;
            Common.GameTrigger trigger;
            if (this.IsUser(e.PlayerPosition))
            {
                trigger = this.userInterface.LetUserDoCounterBidOrPass(potentialBid);
                this.ValidateTrigger(trigger, BidOrPassTriggers, "bidding");
                if (trigger == Common.GameTrigger.BidPlaced)
                {
                    trigger = bidTriggerType; // might be bid or counterbid
                }
                this.currentStateStack.DeltaChanges.Add(new GameStateChangeInfo() { HumanTrigger = trigger });
            }
            else
            {
                // TODO: Implement AI
                if (this.currentBidAmount < (150 + 20 * e.PlayerPosition))
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
                this.userInterface.DisplayPlayerAsPassed(e.PlayerPosition);
            }
            else
            {
                this.currentBidAmount = potentialBid;
                if (!this.IsUser(e.PlayerPosition))
                {
                    this.userInterface.DisplayAIBid(e.PlayerPosition, this.currentBidAmount);
                }
            }
            this.stateBridge.TriggerSink.SendTrigger(trigger);
        }

        private void EventSource_WaitingForCounterOrPassStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            this.HandleBidOrPass(Common.GameTrigger.BidCountered, e);
        }

        private void EventSource_SwitchingCurrentBidPlayerStarted(object sender, GameLogic.PlayerPairEventArgs e)
        {
            // Nothing to do here at the moment
            this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
        }

        private void EventSource_SwitchingCounterBidPlayerStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // Nothing to do here at the moment
            this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
        }

        private Common.CardSuit? trumpSuit = null;

        private bool playerWentOut = false;
        private void EventSource_ExchangingCardsWithDabbStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            this.userInterface.UncoverDabb(this.cardsInDabb);

            Common.GameTrigger trigger;
            if (this.IsUser(e.PlayerPosition))
            {
                IEnumerable<Common.Card> discardedCards = null;
                trigger = this.userInterface.LetUserExchangeCardsWithDabb(out discardedCards, out this.trumpSuit);
                // TODO: Validate and process discarded cards and selected trump suit
                var removedDabbCards = new HashSet<Common.Card>(this.cardsInDabb);
                var removedPlayerCards = new List<Common.Card>();
                foreach (var discardedCard in discardedCards)
                {
                    if (this.cardsInDabb.Contains(discardedCard))
                    {
                        // The card remains in dabb, remove it from the set of cards which could have been taken by the player
                        removedDabbCards.Remove(discardedCard);
                    }
                    {
                        // The card used to be a player card but was discarded
                        removedPlayerCards.Add(discardedCard);
                    }
                }
                this.cardsInDabb = this.cardsInDabb.Except(removedDabbCards.ToList()).Concat(removedPlayerCards).ToList();
                this.cardsPerPlayer[0] = this.cardsPerPlayer[0].Except(removedPlayerCards).Concat(removedDabbCards.ToList());
                // TODO: Actually rearrange cards
                this.userInterface.RearrangeCardsForUser(this.cardsPerPlayer.First());
            }
            else
            {
                // TODO: Implement AI
                trigger = Common.GameTrigger.TrumpSelected;
                this.trumpSuit = Common.CardSuit.Hearts;
            }

            this.ValidateTrigger(
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
            this.playerWentOut = trigger == Common.GameTrigger.GoingOut;

            this.stateBridge.TriggerSink.SendTrigger(trigger);
        }

        private void EventSource_CalculatingGoingOutScoreStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // TODO: Calculate actual score
            this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.ScoreCalculationFinished);
        }

        private void EventSource_MeldingStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // TODO: Display actual melds
            this.userInterface.DisplayMelds(
                new List<Common.MeldData>()
                {
                    new Common.MeldData()
                    {
                        Melds = new List<Common.SingleMeld>()
                        {
                            new Common.SingleMeld() { Cards = this.cardsPerPlayer[0].Take(5).ToList(), Points = 150 },
                            new Common.SingleMeld() { Cards = this.cardsPerPlayer[0].Skip(5).Take(2).ToList(), Points = 40 }
                        }
                    },
                    new Common.MeldData()
                    {
                        Melds = new List<Common.SingleMeld>() { new Common.SingleMeld() { Cards = this.cardsPerPlayer[1].Take(2).ToList(), Points = 20 } }
                    },
                    new Common.MeldData()
                    {
                        Melds = new List<Common.SingleMeld>() { new Common.SingleMeld() { Cards = this.cardsPerPlayer[2].Take(4).ToList(), Points = 80 } }
                    },
                    new Common.MeldData()
                    {
                        Melds = new List<Common.SingleMeld>() { new Common.SingleMeld() { Cards = this.cardsPerPlayer[3].Take(5).ToList(), Points = 50 } }
                    }
                });
            this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.MeldsSeenByAllPlayers);
        }

        private void Sender_TrickTakingStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            this.userInterface.PrepareTrickTaking(this.cardsPerPlayer, this.currentDealer);
            this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.ReadyForTrickTaking);
        }


        private Common.Card mostRecentCard = null;
        private void EventSource_WaitingForCardStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            this.userInterface.ActivatePlayer(e.PlayerPosition);
            if (this.IsUser(e.PlayerPosition))
            {
                this.mostRecentCard = this.userInterface.LetUserSelectCard();
            }
            else
            {
                // TODO: Implement AI
                this.mostRecentCard = this.cardsPerPlayer[e.PlayerPosition].First();
                this.cardsPerPlayer[e.PlayerPosition] = this.cardsPerPlayer[e.PlayerPosition].Except(new List<Common.Card>() { this.mostRecentCard });
            }
            this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.CardPlaced);
        }

        private void EventSource_ValidatingCardStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // TODO: Actually validate card
            bool cardIsValid = true;
            // Assumption: AI always provides valid cards.
            if (this.IsUser(e.PlayerPosition) && !cardIsValid)
            {
                this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.InvalidCardPlaced);
            }
            else
            {
                // TODO: Check if the card is currently leading
                bool cardIsWinning = true;
                if (cardIsWinning)
                {
                    this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.WinningCardPlaced);
                }
                else
                {
                    this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.LosingCardPlaced);
                }
                this.userInterface.PlaceCardInMiddle(e.PlayerPosition, this.mostRecentCard);
            }
        }

        private void EventSource_RevertingInvalidMoveStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            this.userInterface.HandleInvalidMove(new List<Common.Card>());
            this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.RevertingFinished);
        }

        private void EventSource_SwitchingCurrentTrickPlayerStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // Nothing to do here
            this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.PlayerSwitched);
        }

        private void EventSource_StartingNewRoundStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            this.userInterface.MoveCardsToTrickWinner(e.PlayerPosition);
            this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.NewRoundStarted);
        }

        private void EventSource_CountingPlayerOrTeamScoresStarted(object sender, GameLogic.PlayerPositionEventArgs e)
        {
            // TODO: Actually calculate score
            this.userInterface.MoveCardsToTrickWinner(e.PlayerPosition);
            this.stateBridge.TriggerSink.SendTrigger(Common.GameTrigger.ScoreCalculationFinished);
        }

        private void EventSource_GameFinished(object sender, EventArgs e)
        {
            if (this.playerWentOut)
            {
                // TODO: Actually provide scores
                this.userInterface.DisplayGoingOutScore(new List<Common.ScoreData>());
            }
            else
            {
                // TODO: Actually provide scores
                this.userInterface.DisplayGameScore(new List<Common.ScoreData>());
            }

        }
    }
}
