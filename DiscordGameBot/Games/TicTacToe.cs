using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DiscordGameBot.Games
{
    public class TicTacToe : Game
    {
        Random rnd;
        private char[,] field;
        private bool started = false;
        bool done = false;
        private Queue<char> activeChar;
        private int moves = 0;

        public bool Done
        {
            get
            {
                return done;
            }

            set
            {
                done = value;
            }
        }

        public TicTacToe(IEnumerable<User> players, Channel channel) : base(players, channel)
        {
            rnd = new Random();
            activeChar = new Queue<char>();
            activeChar.Enqueue('X');
            activeChar.Enqueue('O');
        }

        public override void Reset()
        {
            field = new char[3,3];
            for(int x = 0; x < 3; x++)
            {
                for(int y = 0; y < 3; y++)
                {
                    field[y, x] = ' ';
                }
            }
        }

        public override void Start()
        {
            this.started = true;

            int playercount = this.Players.Count;
            int shuffle = rnd.Next(0, playercount);

            for (int i = 0; i <= shuffle; i++)
            {
                this.Players.Enqueue(this.Players.Dequeue());
            }

            this.Channel.SendMessage($"Los Geht's! {this.Players.Peek().NicknameMention} fängt an!");
            this.printField();
        }

        private void printField()
        {
            this.Channel.SendMessage($"#\r\n" +
                                     $" {field[0, 0]}  |  {field[0, 1]}  |  {field[0, 2]} \r\n" +
                                     $"--- --- ---\r\n" +
                                     $" {field[1, 0]}  |  {field[1, 1]}  |  {field[1, 2]} \r\n" +
                                     $"--- --- ---\r\n" +
                                     $" {field[2, 0]}  |  {field[2, 1]}  |  {field[2, 2]} ");
        }

        private void MakeTurn(int x, int y, User player)
        {
            bool won = false;
            char setChr = this.activeChar.Peek();

            if (!started)
            {
                return;
            }

            if (player.Id != Players.Peek().Id)
            {
                this.Channel.SendMessage($"{player.NicknameMention} du bist nicht am Zug!");
                return;
            }

            if (field[y,x] != ' ' || x < 0 || y < 0 || x > 3 || y > 3)
            {
                this.Channel.SendMessage($"{player.NicknameMention} du kannst hier nicht setzen!");
                return;
            }

            this.field[y, x] = setChr;
            
            for (int i = 0; i < 3; i++)
            {
                if (this.field[x,i] != setChr)
                    break;
                if (i == 2)
                {
                    won = true;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                if (this.field[i,x] != setChr)
                    break;
                if (i == 2)
                {
                    won = true;
                }
            }

            if (x == y)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (this.field[i,i] != setChr)
                        break;
                    if (i == 2)
                    {
                        won = true;
                    }
                }
            }
            
            if (x + y == 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (this.field[i,2 - i] != setChr)
                        break;
                    if (i == 2)
                    {
                        won = true;
                    }
                }
            }
            
            if (this.moves == 9)
            {
                this.Done = true;
                this.printField();
                this.Channel.SendMessage($"Niemand hat gewonnen!");
                return;
            }

            if(won)
            {
                this.Done = true;
                this.printField();
                this.Channel.SendMessage($"{player.NicknameMention} hat gewonnen! gg");
                return;
            }

            this.Players.Enqueue(this.Players.Dequeue());
            this.activeChar.Enqueue(this.activeChar.Dequeue());
            this.moves += 1;

            this.Channel.SendMessage($"{Players.Peek().NicknameMention} ist jetzt am Zug.");

            this.printField();
        }

        public static void RegisterCommands()
        {
            Manager.commands.CreateCommand("set").Parameter("x", ParameterType.Required).Parameter("y", ParameterType.Required).Do(async (e) =>
            {
                int x = -1;
                int y = -1;

                try
                {
                    x = Convert.ToInt32(e.Args[0]);
                    y = Convert.ToInt32(e.Args[1]);
                }
                catch (Exception ex)
                {
                    await e.Channel.SendMessage($"{e.Args[0]} ist keine gültige Eingabe! \r\n\r\n {ex.Message}");
                }

                TicTacToe gameInstance = null;

                try
                {
                    gameInstance = (TicTacToe)Manager.findGame(e.User);
                }
                catch (Exception ex)
                {
                    await e.Channel.SendMessage($"Konnte {e.User.NicknameMention}s Spiel nicht finde! \r\n\r\n {ex.Message}");
                }

                gameInstance.MakeTurn(x, y, e.User);

                if (gameInstance.Done)
                {
                    gameInstance = null;
                }
            });
        }
    }
}
