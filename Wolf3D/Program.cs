using System;

namespace Wolf3D
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new NezGame())
                game.Run();
        }
    }
}
