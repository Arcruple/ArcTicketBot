using ArcTicketBot.Configurations;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ArcTicketBot.Commands {
    [SlashCommandGroup("setup", "[Administrators] Setup commands for the Bot.")]
    public class SetupCommands : ApplicationCommandModule {

        [SlashCommand("addrole", "Add a staff role to the config file.")]
        public async Task AddRoleCommand(InteractionContext ctx, [Option("role", "The role you are adding.")] DiscordRole role) {

            if (ctx.Guild == null || role == null) {
                await ctx.DeleteResponseAsync();
                return;
            }

            var configurationManager = new ConfigurationManager();

            List<string> staffRoles = configurationManager.GetStaffRoles();

            if (staffRoles.Contains(role.Id.ToString())) {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{role.Mention} already exists in the configuration file.").AsEphemeral(true));
            } else {
                configurationManager.AddStaffRole(role.Id.ToString());
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{role.Mention} has been added to the configuration file.").AsEphemeral(true));
            }

        }//Fucking dick

        [SlashCommand("removerole", "Remove a staff role to the config file.")]
        public async Task RemoveRoleCommand(InteractionContext ctx, [Option("role", "The role you are removing.")] DiscordRole role) {

            if (ctx.Guild == null || role == null) {
                await ctx.DeleteResponseAsync();
                return;
            }

            var configurationManager = new ConfigurationManager();

            List<string> staffRoles = configurationManager.GetStaffRoles();

            if (staffRoles.Contains(role.Id.ToString())) {
                configurationManager.RemoveStaffRole(role.Id.ToString());
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{role.Mention} has been removed from the configuration file.").AsEphemeral(true));
            } else {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{role.Mention} does not exist in the configuration file.").AsEphemeral(true));
            }

        }

        [SlashCommand("setcategory", "The category you want new tickets to be created under.")]
        public async Task SetCategoryCommand(InteractionContext ctx, [ChannelTypes(DiscordChannelType.Category)][Option("category", "The category you are choosing.")] DiscordChannel channel) {

            if (ctx.Guild == null || channel == null) {
                await ctx.DeleteResponseAsync();
                return;
            }

            var configurationManager = new ConfigurationManager();

            configurationManager.SetTicketCategory(channel.Id.ToString());
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{channel.Mention} has been set as the `ticket category` in the configuration file.").AsEphemeral(true));

        }

        [SlashCommand("setlogs", "The channel you want the log information set to.")]
        public async Task SetLogsCommand(InteractionContext ctx, [ChannelTypes(DiscordChannelType.Text)][Option("channel", "The channel you are choosing.")] DiscordChannel channel) {

            if (ctx.Guild == null || channel == null) {
                await ctx.DeleteResponseAsync();
                return;
            }

            var configurationManager = new ConfigurationManager();

            configurationManager.SetLogChannel(channel.Id.ToString());
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{channel.Mention} has been set as the `log channel` in the configuration file.").AsEphemeral(true));

        }

    }
}
