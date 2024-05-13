using ArcTicketBot.Configurations;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace ArcTicketBot.Commands {
    [SlashCommandGroup("ticket", "Create/Close a ticket; Add/Remove a member.")]
    public class TicketCommands : ApplicationCommandModule {

        [SlashCommand("open", "Opens a new ticket to discuss things with staff.")]
        public async Task OpenTicketCommand(InteractionContext ctx) {

            if (ctx.Guild == null) {
                await ctx.DeleteResponseAsync();
                return;
            }

            await ctx.DeferAsync(true);

            var configurationManager = new ConfigurationManager();

            DiscordChannel ticketCategory = ctx.Guild.Channels.FirstOrDefault(x => x.Value.Id.ToString() == configurationManager.GetTicketCategory()).Value;

            if (ticketCategory == null) {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Something went wrong. Please notify an administrator.").AsEphemeral(true));
                return;
            }

            var everyoneRoleBuilder = new DiscordOverwriteBuilder(ctx.Guild.EveryoneRole);

            everyoneRoleBuilder.For(ctx.Guild.EveryoneRole);
            everyoneRoleBuilder.Deny(DiscordPermissions.AccessChannels);
            everyoneRoleBuilder.Deny(DiscordPermissions.SendMessages);

            var everyoneRoleList = new List<DiscordOverwriteBuilder> { everyoneRoleBuilder };

            var ticketChannel = await ctx.Guild.CreateChannelAsync($"{ctx.Member.Username}-ticket", DiscordChannelType.Text, ticketCategory, "", null, null, everyoneRoleList);

            var ticketCreatorBuilder = new DiscordOverwriteBuilder(ctx.Member);

            ticketCreatorBuilder.For(ctx.Member);
            ticketCreatorBuilder.Allow(DiscordPermissions.AccessChannels);
            ticketCreatorBuilder.Allow(DiscordPermissions.SendMessages);
            ticketCreatorBuilder.Allow(DiscordPermissions.AttachFiles);
            ticketCreatorBuilder.Allow(DiscordPermissions.EmbedLinks);
            ticketCreatorBuilder.Allow(DiscordPermissions.AddReactions);
            ticketCreatorBuilder.Allow(DiscordPermissions.UseExternalEmojis);
            ticketCreatorBuilder.Allow(DiscordPermissions.ReadMessageHistory);
            ticketCreatorBuilder.Allow(DiscordPermissions.UseApplicationCommands);

            await ticketChannel.AddOverwriteAsync(ctx.Member, ticketCreatorBuilder.Allowed, ticketCreatorBuilder.Denied);

            List<string> staffRoles = configurationManager.GetStaffRoles();

            foreach(var role in staffRoles) {

                var staff = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Id.ToString() == role).Value;

                if (staff != null) {
                    var overwriteBuilder = new DiscordOverwriteBuilder(staff);

                    overwriteBuilder.For(staff);
                    overwriteBuilder.Allow(DiscordPermissions.AccessChannels);
                    overwriteBuilder.Allow(DiscordPermissions.SendMessages);
                    overwriteBuilder.Allow(DiscordPermissions.AttachFiles);
                    overwriteBuilder.Allow(DiscordPermissions.EmbedLinks);
                    overwriteBuilder.Allow(DiscordPermissions.AddReactions);
                    overwriteBuilder.Allow(DiscordPermissions.UseExternalEmojis);
                    overwriteBuilder.Allow(DiscordPermissions.ReadMessageHistory);
                    overwriteBuilder.Allow(DiscordPermissions.UseApplicationCommands);

                    await ticketChannel.AddOverwriteAsync(staff, overwriteBuilder.Allowed, overwriteBuilder.Denied);

                }

            }

            var ticketEmbed = new DiscordEmbedBuilder {
                Title = $"{ctx.Member.DisplayName}'s Ticket",
                Description = $"Please explain the reason for your ticket below.",
                Color = DiscordColor.Cyan,
                Timestamp = DateTime.UtcNow
            };

            var transcriptEmoji = DiscordEmoji.FromName(ctx.Client, ":scroll:");
            var closeEmoji = DiscordEmoji.FromName(ctx.Client, ":x:");

            var buttons = new DiscordButtonComponent[]
            {
                new DiscordButtonComponent(DiscordButtonStyle.Primary, "transcriptbutton", $"Transcript", false, new DiscordComponentEmoji(transcriptEmoji)),
                new DiscordButtonComponent(DiscordButtonStyle.Primary, "closeticketbutton", $"Close Ticket", false, new DiscordComponentEmoji(closeEmoji)),
            };

            var ticketMessageBuilder = new DiscordMessageBuilder().WithContent(ctx.Member.Mention).AddEmbed(ticketEmbed).AddComponents(buttons);

            await ticketChannel.SendMessageAsync(ticketMessageBuilder);

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{ctx.Member.Mention} your ticket has been created! Click here to go to it: {ticketChannel.Mention}").AsEphemeral(true));

            var staffThread = await ticketChannel.CreateThreadAsync("staff-discussion", DiscordAutoArchiveDuration.Week, DiscordChannelType.Private, "Staff discussion.");

            foreach (var role in configurationManager.GetStaffRoles()) {

                var staff = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Id.ToString() == role).Value;

                if (staff != null) {

                    var staffMembers = ctx.Guild.Members.Where(x => x.Value.Roles.Contains(staff));

                    foreach (var member in staffMembers) {

                        await staffThread.AddThreadMemberAsync(member.Value);

                        await Task.Delay(1000);

                    }

                }

            }

            var logChannelId = configurationManager.GetLogChannel();

            var logChannel = ctx.Guild.Channels.FirstOrDefault(x => x.Value.Id.ToString() ==  logChannelId).Value;

            if (logChannel != null) {
                await logChannel.SendMessageAsync($"【{ctx.QualifiedName}】{ctx.Member.Mention} opened {ticketChannel}.");
            }

        }

        [SlashCommand("close", "[Staff] Close the current ticket.")]
        public async Task CloseTicketCommand(InteractionContext ctx) {

            if (ctx.Guild == null) {
                await ctx.DeleteResponseAsync();
                return;
            }

            await ctx.DeferAsync(true);

            ConfigurationManager configurationManager = new ConfigurationManager();

            var ticketCategoryId = configurationManager.GetTicketCategory();
            var logChannelId = configurationManager.GetLogChannel();
            var staffRoles = configurationManager.GetStaffRoles();

            if (ticketCategoryId == null || logChannelId == null || staffRoles == null) {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Something went wrong. Please notify an administrator.").AsEphemeral(true));
                return;
            }

            var logChannel = ctx.Guild.Channels.FirstOrDefault(x => x.Value.Id.ToString() == logChannelId).Value;

            if (logChannel == null) {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Something went wrong. Please notify an administrator.").AsEphemeral(true));
                return;
            }

            var roleCheck = ctx.Member.Roles.Any(x => staffRoles.Contains(x.Id.ToString()));

            if (!roleCheck) {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("You do not have permission to use this command.").AsEphemeral(true));
                return;
            }

            if (ctx.Channel.Parent.Id.ToString() != ticketCategoryId) {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("You can only use this command in the ticket area.").AsEphemeral(true));
                return;
            }

            if (!ctx.Channel.Name.ToLower().Contains("-ticket")) {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("You can only close a valid ticket.").AsEphemeral(true));
                return;
            }

            var messages = ctx.Channel.GetMessagesAsync();

            if (messages == null) {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Something went wrong. Please notify an administrator.").AsEphemeral(true));
                return;
            }

            string tempFilePath = Path.GetTempFileName();

            using (StreamWriter streamWriter = new StreamWriter(tempFilePath)) {
                await foreach(var message in messages) {
                    if (message.Content !=  null) {
                        await streamWriter.WriteLineAsync($"[{message.Author.Username}][{message.Id}][{message.Timestamp}]: {message.Content}");
                    }
                    if (message.Attachments.Count > 0) {
                        foreach(var attachment in message.Attachments) {
                            await streamWriter.WriteLineAsync($"[{message.Author.Username}][{message.Id}][{message.Timestamp}]: {attachment.Url}");
                        }
                    }
                }
            }

            FileStream fileStream = null;

            try {
                using (fileStream = File.OpenRead(tempFilePath)) {
                    DiscordMessage transcriptMessage = await logChannel.SendMessageAsync(new DiscordMessageBuilder().WithContent($"## {ctx.Channel.Name}").AddFile("transcript.txt", fileStream));
                }
            } finally {
                if (fileStream != null) {
                    fileStream.Close();
                }
            }

            File.Delete(tempFilePath);

            var ticketCreatorName = ctx.Channel.Name.Replace("-ticket", "");

            var ticketCreator = ctx.Guild.Members.FirstOrDefault(x => x.Value.Username.ToLower() == ticketCreatorName.ToLower()).Value;

            try {
                await ticketCreator.SendMessageAsync($"### Ticket Closed:\nYour ticket has been closed, if you require any further assistance or assistance in the future please be sure to open a new ticket in {ctx.Guild.Name}. \n*To open a new ticket do: `/ticket open` in any channel within {ctx.Guild.Name}.");

                await logChannel.SendMessageAsync($"[Ticket Closed] DM sent to {ticketCreator.Mention}.");
            } catch {
                await logChannel.SendMessageAsync($"[Ticket Closed] DM not sent to {ticketCreator.Mention}.");
            }

            await ctx.Channel.DeleteAsync();

        }

        public async Task TicketButtonInteractions(DiscordClient sender, DiscordGuild guild, ComponentInteractionCreateEventArgs e) {

            if (guild == null || e == null) {
                return;
            }

            ConfigurationManager configurationManager = new ConfigurationManager();

            if (e.Message == null || e.Message.Embeds.Count <= 0) {
                return;
            }

            if (e.Message.Embeds[0].Title == null) {
                return;
            }

            if (e.Message.Author == sender.CurrentUser && e.Message.Embeds[0].Title.ToLower().Contains("ticket")) {

                var member = (DiscordMember)e.User;

                if (member == null) {
                    return;
                }

                var ticketCategoryId = configurationManager.GetTicketCategory();
                var logChannelId = configurationManager.GetLogChannel();
                var staffRoles = configurationManager.GetStaffRoles();

                if (ticketCategoryId == null || logChannelId == null || staffRoles == null) {
                    return;
                }

                var logChannel = guild.Channels.FirstOrDefault(x => x.Value.Id.ToString() == logChannelId).Value;

                if (logChannel == null) {
                    return;
                }

                var roleCheck = member.Roles.Any(x => staffRoles.Contains(x.Id.ToString()));

                if (!roleCheck) {
                    return;
                }

                switch (e.Id.ToLower()) {
                    case "transcriptbutton":

                        if (e.Channel.Parent.Id.ToString() == ticketCategoryId && e.Channel.Name.ToLower().Contains("-ticket")) {

                            var messages = e.Channel.GetMessagesAsync();

                            string tempFilePath = Path.GetTempFileName();

                            using (StreamWriter streamWriter = new StreamWriter(tempFilePath)) {

                                await foreach (var message in messages) {
                                    if (message.Content != null && message.Content.Length > 0) {
                                        await streamWriter.WriteLineAsync($"[{message.Author.Username}][{message.Id}][{message.Timestamp}]: {message.Content}");
                                    }
                                    if (message.Attachments.Count > 0) {
                                        foreach (var attachment in message.Attachments) {
                                            await streamWriter.WriteLineAsync($"[{message.Author.Username}][{message.Id}][{message.Timestamp}]: {attachment.Url}");
                                        }
                                    }
                                }

                            }

                            FileStream fileStream = null;

                            try {
                                using (fileStream = File.OpenRead(tempFilePath)) {
                                    DiscordMessage transcriptMessage = await logChannel.SendMessageAsync(new DiscordMessageBuilder().WithContent($"## {e.Channel.Name}").AddFile("transcript.txt", fileStream));
                                }
                            } finally {
                                if (fileStream != null) {
                                    fileStream.Close();
                                }
                            }

                            File.Delete(tempFilePath);

                            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You created a transcript for {e.Channel.Mention}").AsEphemeral(true));

                        }

                        break;
                    case "closeticketbutton":

                        if (e.Channel.Parent.Id.ToString() == ticketCategoryId && e.Channel.Name.ToLower().Contains("-ticket")) {

                            var messages = e.Channel.GetMessagesAsync();

                            string tempFilePath = Path.GetTempFileName();

                            using (StreamWriter streamWriter = new StreamWriter(tempFilePath)) {

                                await foreach (var message in messages) {
                                    if (message.Content != null) {
                                        await streamWriter.WriteLineAsync($"[{message.Author.Username}][{message.Id}][{message.Timestamp}]: {message.Content}");
                                    }
                                    if (message.Attachments.Count > 0) {
                                        foreach (var attachment in message.Attachments) {
                                            await streamWriter.WriteLineAsync($"[{message.Author.Username}][{message.Id}][{message.Timestamp}]: {attachment.Url}");
                                        }
                                    }
                                }

                            }

                            FileStream fileStream = null;

                            try {
                                using (fileStream = File.OpenRead(tempFilePath)) {
                                    DiscordMessage transcriptMessage = await logChannel.SendMessageAsync(new DiscordMessageBuilder().WithContent($"## {e.Channel.Name}").AddFile("transcript.txt", fileStream));
                                }
                            } finally {
                                if (fileStream != null) {
                                    fileStream.Close();
                                }
                            }

                            File.Delete(tempFilePath);

                            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You have closed {e.Channel.Mention}").AsEphemeral(true));

                            var ticketCreatorName = e.Channel.Name.Replace("-ticket", "");

                            var ticketCreator = guild.Members.FirstOrDefault(x => x.Value.Username.ToLower() == ticketCreatorName.ToLower()).Value;

                            try {
                                await ticketCreator.SendMessageAsync($"### Ticket Closed:\nYour ticket has been closed, if you require any further assistance or assistance in the future please be sure to open a new ticket in {guild.Name}. \n*To open a new ticket do: `/ticket open` in any channel within {guild.Name}.");

                                await logChannel.SendMessageAsync($"[Ticket Closed] DM sent to {ticketCreator.Mention}.");
                            } catch {
                                await logChannel.SendMessageAsync($"[Ticket Closed] DM not sent to {ticketCreator.Mention}.");
                            }

                            await e.Channel.DeleteAsync();

                        }

                        break;
                    default:
                        break;
                }

            }

        }

    }
}
