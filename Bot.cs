using DSharpPlus.Interactivity;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using System.Text;
using Newtonsoft.Json;
using DSharpPlus.Interactivity.Extensions;
using ArcTicketBot.Commands;
using DSharpPlus.EventArgs;

namespace ArcTicketBot {
    internal class Bot {

        public DiscordClient? Client { get; private set; }
        public InteractivityExtension? Interactivity { get; private set; }
        public SlashCommandsExtension? Slash { get; private set; }

        public async Task RunAsync() {

            //Pull Token
            var json = string.Empty;

            using (var fileReader = File.OpenRead("config.json"))
            using (var streamReader = new StreamReader(fileReader, new UTF8Encoding(false)))
                json = await streamReader.ReadToEndAsync();

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            //Discord Configuration
            var config = new DiscordConfiguration {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All,
                LogUnknownAuditlogs = false,
                LogUnknownEvents = false                
            };

            //Event Registering
            Client = new DiscordClient(config);

            Client.ComponentInteractionCreated += OnComponentInteraction;

            //Interactivity Setup
            Client.UseInteractivity(new InteractivityConfiguration {
                Timeout = TimeSpan.FromMinutes(5)
            });

            //Slash Command Registering

            Slash = Client.UseSlashCommands();

            try {
                Slash.RegisterCommands<SetupCommands>();
                Slash.RegisterCommands<TicketCommands>();
            }catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            //Connect to Discord
            await Client.ConnectAsync();

            //Run Indefinitely
            await Task.Delay(-1);

        }

        public async Task OnComponentInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e) {

            var ticketInstance = new TicketCommands();

            _ = Task.Run(() => ticketInstance.TicketButtonInteractions(sender, e.Guild, e));

            await Task.CompletedTask;
        }

    }
}
