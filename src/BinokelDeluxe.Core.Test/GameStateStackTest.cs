namespace BinokelDeluxe.Core.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using NUnit.Framework;

#pragma warning disable SA1600 // Test methods should have an expressive name rather than being documented.
#pragma warning disable CS1591 // Same reason as above

    /// <summary>
    /// This class tests the Game State Stack.
    /// </summary>
    public class GameStateStackTest
    {
        [Test]
        public void RestoringXmlFile_YieldsExpectedGameState()
        {
            var expectedGameCreationInfo = new Core.GameCreationInfo()
            {
                AIStrategyTypes = new List<string>()
                {
                    typeof(AI.TempAI1).FullName,
                    typeof(AI.TempAI2).FullName,
                    typeof(AI.TempAI3).FullName,
                },
                RandomSeed = 82745392,
                RuleSettings = new GameLogic.RuleSettings(),
                Version = new Common.SerializableVersion(1, 0),
            };
            var expectedGameStateStack = new Core.GameStateStack()
            {
                CreationInfo = expectedGameCreationInfo,
                DeltaChanges = new List<GameStateChangeInfo>()
                {
                    new GameStateChangeInfo()
                    {
                        HumanTrigger = Common.GameTrigger.BidPlaced,
                    },
                    new GameStateChangeInfo()
                    {
                        CardPlayedByHuman = new Common.Card()
                        {
                            Suit = Common.CardSuit.Acorns,
                            Type = Common.CardType.King,
                        },
                    },
                },
            };

            var xmlSerializer = new XmlSerializer(typeof(Core.GameStateStack));
            GameStateStack actualStack;
            using (var xmlReader = new StreamReader("./GameStateStackTest.xml"))
            {
                actualStack = (GameStateStack)xmlSerializer.Deserialize(xmlReader);
            }

            Assert.That(expectedGameStateStack.Equals(actualStack));
        }

        [Test]
        public void Card_ComparingToNull_IsNotEqual()
        {
            var card = new Common.Card()
            {
                DeckNumber = 0,
                Suit = Common.CardSuit.Acorns,
                Type = Common.CardType.Ace,
            };

            Assert.That(!card.Equals(null));
            Assert.AreNotEqual(
                Common.ValueComparer<Common.Card>.GetHashCode(card),
                Common.ValueComparer<Common.Card>.GetHashCode(null));
        }

        [Test]
        public void Card_ComparingToSame_IsEqual()
        {
            var card1 = new Common.Card()
            {
                DeckNumber = 0,
                Suit = Common.CardSuit.Bells,
                Type = Common.CardType.Unter,
            };

            var card2 = new Common.Card()
            {
                DeckNumber = 0,
                Suit = Common.CardSuit.Bells,
                Type = Common.CardType.Unter,
            };

            Assert.That(card1.Equals(card2));
            Assert.That(card2.Equals(card1));
            Assert.AreEqual(card1.GetHashCode(), card2.GetHashCode());
        }

        [Test]
        public void Card_ComparingToDifferentDeckNumber_IsNotEqual()
        {
            var card1 = new Common.Card()
            {
                DeckNumber = 0,
                Suit = Common.CardSuit.Hearts,
                Type = Common.CardType.Ober,
            };

            var card2 = new Common.Card()
            {
                DeckNumber = 1,
                Suit = Common.CardSuit.Hearts,
                Type = Common.CardType.Ober,
            };

            Assert.That(!card1.Equals(card2));
            Assert.That(!card2.Equals(card1));
            Assert.AreNotEqual(card1.GetHashCode(), card2.GetHashCode());
        }

        [Test]
        public void Card_ComparingToDifferentSuit_IsNotEqual()
        {
            var card1 = new Common.Card()
            {
                DeckNumber = 0,
                Suit = Common.CardSuit.Hearts,
                Type = Common.CardType.Ober,
            };

            var card2 = new Common.Card()
            {
                DeckNumber = 0,
                Suit = Common.CardSuit.Leaves,
                Type = Common.CardType.Ober,
            };

            Assert.That(!card1.Equals(card2));
            Assert.That(!card2.Equals(card1));
            Assert.AreNotEqual(card1.GetHashCode(), card2.GetHashCode());
        }

        [Test]
        public void Card_ComparingToDifferentType_IsNotEqual()
        {
            var card1 = new Common.Card()
            {
                DeckNumber = 0,
                Suit = Common.CardSuit.Hearts,
                Type = Common.CardType.Ober,
            };

            var card2 = new Common.Card()
            {
                DeckNumber = 0,
                Suit = Common.CardSuit.Hearts,
                Type = Common.CardType.Unter,
            };

            Assert.That(!card1.Equals(card2));
            Assert.That(!card2.Equals(card1));
            Assert.AreNotEqual(card1.GetHashCode(), card2.GetHashCode());
        }
    }
}
#pragma warning disable CS1591 // Same reason as above
#pragma warning restore SA1600 // Elements should be documented