using System;

namespace LevelEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Editor game = new Editor())
            {
                game.Run();
            }
        }
    }
}

