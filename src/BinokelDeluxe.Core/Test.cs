using System;
using System.IO;
using System.Xml.Serialization;

namespace BinokelDeluxe.Core
{
    public static class Test
    {
        public static void TestMethod()
        {
            Type type = null;
            //var stateBridge = new GameLogic.SingleGameStateBridge();

            //var ruleSettings = new GameLogic.RuleSettings();

            //var gameStateStack = new GameStateStack()
            //{
            //    RuleSettings = ruleSettings.Clone()
            //};


            ////var xmlSerializer = new XmlSerializer(typeof(GameStateStack));
            ////var reader = new StreamReader("C:\\Temp\\test.xml");

            ////var gameStateStack = (GameStateStack)xmlSerializer.Deserialize(reader);
            //stateBridge.PrepareNewGame(ruleSettings, dealerPosition: 0);

            //var gameStateEntry = new GameStateEntry()
            //{
            //    SingleGameStateInfo = stateBridge.CurrentState.Clone()
            //};

            //gameStateStack.push(gameStateEntry);

            //var xmlSerializer = new XmlSerializer(typeof(GameStateStack));
            //var writer = new StreamWriter("C:\\Temp\\test.xml");

            //xmlSerializer.Serialize(writer, gameStateStack);
        }
    }
}
