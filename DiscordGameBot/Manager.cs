using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.Commands;

namespace DiscordGameBot
{
    public static class Manager
    {
        private static string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static DirectoryInfo appFolder = new DirectoryInfo(appPath);

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
            string APIKEY = File.ReadAllText(appFolder.FullName + @"\apikey.txt");

            Manager.client = new DiscordClient(input =>
            {
                input.LogLevel = LogSeverity.Info;
                input.LogHandler = Log;
            });

            Manager.Bot = new GameBot();
            RegisterCommands();

            Manager.client.ExecuteAndWait(async () =>
            {
                await Manager.client.Connect(APIKEY, TokenType.Bot);
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
            Manager.commands.CreateCommand("help").Do(async (e) =>
            {
                await e.Channel.SendMessage("GameBot erkennt folgende Commands:\r\n\t-!games\r\n\t-!rules\r\n\t-!start");
            });

            Manager.commands.CreateCommand("rules").Parameter("game", ParameterType.Optional).Do(async (e) =>
            {
                switch(e.Args[0])
                {
                    case "nim":
                        await e.Channel.SendMessage("Im Spiel befinden sich 13 Hölzer. Jeder Spieler darf während seines Zuges 1,2 oder 3 Hölzer ziehen. Der Spieler der das letzte Holz zieht gewinnt!\r\nHölzer werden mit !take <anzahl> gezogen.");
                        break;
                    case "tictactoe":
                        await e.Channel.SendMessage("Standart 3x3 TicTactToe. Der erste Spieler mit 3 Steine in einer Zeile, Spalte oder Diagonale gewinnt.\r\nGesetzt wird mit !set <x> <y> mit x,y von 0-2.");
                        break;
                    default:
                        await e.Channel.SendMessage("Usage: !rules <game> \r\nFür unterstützte Spiele benutze !games");
                        break;
                }
                
            });

            Manager.commands.CreateCommand("games").Do(async (e) =>
            {
                await e.Channel.SendMessage("\r\n Bisher untersützte Spiele: \r\n\t 1. nim\r\n\t 2. tictactoe");
            });

            Manager.commands.CreateCommand("start").Parameter("game", ParameterType.Required).Parameter("player", ParameterType.Multiple).Do(async (e) =>
            {
                //await e.Channel.SendMessage($"\r\n[DEBUG] Issued: \r\n{{\r\nType: {e.Command.ToString()}\r\n\t{{\r\n\tName: {e.Command.Text}, \r\n\tParameter 1: {e.Args[0]}, \r\n\tParameter 2: {e.Args[1]}\r\n\t}}\r\n}} ");

                switch(e.Args[0])
                {
                    case "nim":
                    {
                        List<User> players = new List<User>();

                        for (int i = 1; i < e.Args.Count(); i++)
                        {
                            players.Add(e.Server.FindUsers(e.Args[i]).FirstOrDefault());
                            if (players[i - 1] == null)
                            {
                                await e.Channel.SendMessage($"Konnte {e.Args[i]} nicht finden!");
                                return;
                            }
                        }

                        if (players.Count < 2)
                        {
                            await e.Channel.SendMessage("Usage: /start <player1> <player2> [player3]\r\nEs sind min 2 Spieler anzugeben um spielen zu können!");
                            return;
                        }

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

                        foreach (User player in players)
                        {
                            ActivePlayers.Add(player, slotId);
                        }

                        game.Start();
                        break;
                    }
                    case "tictactoe":
                    {
                        List<User> players = new List<User>();

                        for (int i = 1; i < e.Args.Count(); i++)
                        {
                            players.Add(e.Server.FindUsers(e.Args[i]).FirstOrDefault());
                            if (players[i - 1] == null)
                            {
                                await e.Channel.SendMessage($"Konnte {e.Args[i]} nicht finden!");
                                return;
                            }
                        }

                        if (players.Count != 2)
                        {
                            await e.Channel.SendMessage("Usage: /start <player1> <player2>\r\nEs sind genau 2 Spieler anzugeben um spielen zu können!");
                            return;
                        }

                        Game game = new Games.TicTacToe(players, e.Channel);

                        int slotId = Manager.reserveGameSlot();

                        if (slotId == -1)
                        {
                            Manager.ActiveGames.Add(game);
                        }
                        else
                        {
                            Manager.ActiveGames[slotId] = game;
                        }

                        foreach (User player in players)
                        {
                            ActivePlayers.Add(player, slotId);
                        }

                        game.Start();
                        break;
                    }
                    default:
                        await e.Channel.SendMessage("Spiel nicht gefunden für unterstützte Spiele nutze !games.");
                        break;
                }
                
            });

            Games.Nim.RegisterCommands();
            Games.TicTacToe.RegisterCommands();
        }

        private static void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

    }

 
}
