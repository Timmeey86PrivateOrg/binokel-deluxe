using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace BinokelDeluxe.Core
{
    public static class Test
    {
        public static void TestMethod()
        {
#if false
            var ruleSettings = new GameLogic.RuleSettings();

            var xmlSerializer = new XmlSerializer(typeof(GameLogic.RuleSettings));
            var writer = new StreamWriter("C:\\Temp\\test.xml");

            xmlSerializer.Serialize(writer, ruleSettings);
#endif

        }
    }
}
