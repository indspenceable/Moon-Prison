using System;

namespace MoonPrison
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
 
            using (MoonPrison game = new MoonPrison())
            {
                game.Run();
            }
        }
    }
}

