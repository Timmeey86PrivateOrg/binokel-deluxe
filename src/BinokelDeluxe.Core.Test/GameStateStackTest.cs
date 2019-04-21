using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace BinokelDeluxe.Core.Test
{
    public class GameStateStackTest
    {
        [Test]
        public void RestoringXmlFile_YieldsExpectedGameState()
        {
            var expectedGameCreationInfo = new Core.GameCreationInfo()
            {
                AIStrategyTypes = new List<String>()
                {
                    typeof(AI.TempAI1).FullName,
                    typeof(AI.TempAI2).FullName,
                    typeof(AI.TempAI3).FullName
                },
                RandomSeed = 82745392,
                RuleSettings = new GameLogic.RuleSettings(),
                Version = new Common.SerializableVersion(1, 0)
            };
            var expectedGameStateStack = new Core.GameStateStack()
            {
                CreationInfo = expectedGameCreationInfo,
                DeltaChanges = new List<GameStateChangeInfo>()
                {
                    new GameStateChangeInfo()
                    {
                        HumanTrigger = GameLogic.SingleGameTrigger.BidPlaced
                    },
                    new GameStateChangeInfo()
                    {
                        CardPlayedByHuman = new Common.Card()
                        {
                            Suit = Common.CardSuit.Acorns,
                            Type = Common.CardType.King
                        }
                    }
                }
            };

            var xmlSerializer = new XmlSerializer(typeof(Core.GameStateStack));
            GameStateStack actualStack;
            using (var xmlReader = new StreamReader("./GameStateStackTest.xml"))
            {
                actualStack = (GameStateStack)xmlSerializer.Deserialize(xmlReader);
            }

            Assert.That(expectedGameStateStack.Equals(actualStack));
        }
    }
}