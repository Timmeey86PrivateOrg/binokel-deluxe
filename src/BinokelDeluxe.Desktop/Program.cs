namespace BinokelDeluxe.Desktop
{
    using System;

    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Core.Test.TestMethod();
            using (var game = new DesktopGame())
            {
                game.Run();
            }
        }
    }
}
