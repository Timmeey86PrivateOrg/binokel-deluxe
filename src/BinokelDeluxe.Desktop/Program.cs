using System;

namespace BinokelDeluxe.Desktop
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // TEMP TEST CODE

            var ruleSettings = new GameLogic.RuleSettings();

            // /TEMP TEST CODE




            using (var game = new DesktopGame())
            {
                game.Run();
            }
        }
    }
}
