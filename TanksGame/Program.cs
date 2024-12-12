namespace TanksGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Tanks Game";
            Console.CursorVisible = false;
            Console.Clear();

            Game game = new Game();
            game.Start();
        }
    }
}
