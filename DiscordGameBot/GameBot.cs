using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNimBot
{
    public class GameBot
    {
        List<Game> Games;


        public GameBot()
        {
            Games = new List<Game>();

            Manager.client.UsingCommands(input =>
            {
                input.PrefixChar = '!';
                input.AllowMentionPrefix = true;
            });

            Manager.commands = Manager.client.GetService<CommandService>();
            Manager.commands.CreateCommand("Hello").Parameter("name", ParameterType.Optional).Do(async (e) =>
            {
                switch (e.Args[0])
                {
                    case "me":
                        await e.Channel.SendMessage($"Hello {e.User.NicknameMention} or should i call you {e.User.Id} ?");
                        break;
                    case "you":
                        await e.Channel.SendMessage($"Hello {Manager.client.CurrentUser.NicknameMention}");
                        break;
                    default:
                        await e.Channel.SendMessage($"World!");
                        break;
                }
            });
        }
    }
}
