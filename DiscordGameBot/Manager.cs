using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DiscordNimBot
{
    public static class Manager
    {
        public static GameBot bot;
        public static Server server;
        public static DiscordClient client;
        public static CommandService commands;
        public static List<Game> activeGames = new List<Game>();
        public static Dictionary<User, int> activePlayers = new Dictionary<User, int>();

        public static GameBot Bot
        {
            get
            {
                return bot;
            }

            set
            {
                bot = value;
            }
        }

        public static Server Server
        {
            get
            {
                return server;
            }

            set
            {
                server = value;
            }
        }

        public static List<Game> ActiveGames
        {
            get
            {
                return activeGames;
            }

            set
            {
                activeGames = value;
            }
        }

        public static Dictionary<User, int> ActivePlayers
        {
            get
            {
                return activePlayers;
            }

            set
            {
                activePlayers = value;
            }
        }

        public static void StartBot()
        {
            Manager.client = new DiscordClient(input =>
            {
                input.LogLevel = LogSeverity.Info;
                input.LogHandler = Log;
            });

            Manager.Bot = new GameBot();
            RegisterCommands();

            Manager.client.ExecuteAndWait(async () =>
            {
                await Manager.client.Connect("MjczNDg5NTExMjgxOTgzNDk5.C2kv1g.BTQGPRxwkos1godWUsmhiLut5Zk", TokenType.Bot);
            });
        }

        public static int reserveGameSlot()
        {
            int i;
            for (i = 0; i < ActiveGames.Count; i++)
            {
                if(ActiveGames[i] == null)
                {
                    return i;
                }
            }

            ActiveGames.Add(new Games.Dummy());

            return i;
        }

        public static void freeGameSlot(int i)
        {
            ActiveGames[i] = null;
        }

        public static Game findGame(User player)
        {
            foreach(User loopPlayer in ActivePlayers.Keys)
            {
                if(player.Id == loopPlayer.Id)
                {
                    return ActiveGames[ActivePlayers[player]];
                }
            }

            return null;
        }

        public static void RegisterCommands()
        {
            Manager.commands.CreateCommand("rules").Parameter("game", ParameterType.Optional).Do(async (e) =>
            {
                switch(e.Args[0])
                {
                    case "nim":
                        await e.Channel.SendMessage("Im Spiel befinden sich 13 Hölzer. Jeder Spieler darf während seines Zuges 1,2 oder 3 Hölzer ziehen. Der Spieler der das letzte Holz zieht gewinnt!");
                        break;
                    default:
                        await e.Channel.SendMessage("Usage: !rules <game> \r\n for für unterstützte Spiele benutze !games");
                        break;
                }
                
            });

            Manager.commands.CreateCommand("games").Do(async (e) =>
            {
                await e.Channel.SendMessage("\r\n Bisher untersützte Spiele: \r\n\t 1. nim");
            });

            Manager.commands.CreateCommand("start").Parameter("game", ParameterType.Required).Parameter("player", ParameterType.Multiple).Do(async (e) =>
            {
                //await e.Channel.SendMessage($"\r\n[DEBUG] Issued: \r\n{{\r\nType: {e.Command.ToString()}\r\n\t{{\r\n\tName: {e.Command.Text}, \r\n\tParameter 1: {e.Args[0]}, \r\n\tParameter 2: {e.Args[1]}\r\n\t}}\r\n}} ");

                switch(e.Args[0])
                {
                    case "nim":
                        if (e.Args[1] == "" || e.Args[2] == "")
                        {
                            await e.Channel.SendMessage("Usage: /start <player1> <player2> \r\n Es sind genau 2 Spieler anzugeben um spielen zu können!");
                        }
                        else
                        {
                            User player1 = e.Server.FindUsers(e.Args[1]).FirstOrDefault();
                            User player2 = e.Server.FindUsers(e.Args[2]).FirstOrDefault();

                            List<User> players = new List<User>();

                            players.Add(player1);
                            players.Add(player2);

                            Game game = new Games.Nim(players, e.Channel);

                            int slotId = Manager.reserveGameSlot();

                            if (slotId == -1)
                            {
                                Manager.ActiveGames.Add(game);
                            }
                            else
                            {
                                Manager.ActiveGames[slotId] = game;
                            }

                            ActivePlayers.Add(player1, slotId);
                            ActivePlayers.Add(player2, slotId);

                            game.Start();
                        }
                        break;
                    default:
                        await e.Channel.SendMessage("Spiel nicht gefunden für unterstützte Spiele nutze !games.");
                        break;
                }
                
            });

            Games.Nim.RegisterCommands();
        }

        private static void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

    }

 
}
