namespace ArcTicketBot {
    internal class Program {
        //Main program
        static void Main(string[] args) {
            //Creates the Bot from the Bot.cs class and runs it
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}