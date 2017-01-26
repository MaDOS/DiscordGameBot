using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DiscordGameBot.Games
{
    public class Nim : Game
    {
        Stack<Stick> sticks;
        bool started = false;
        bool done = false;
        Random rnd;        

        public int StickCount
        {
            get
            {
                return this.sticks.Count;
            }
        }

        public Nim(IEnumerable<User> players, Channel channel) : base(players, channel)
        {
            rnd = new Random();
        }

        private Stack<Stick> Sticks
        {
            get
            {
                return sticks;
            }

            set
            {
                sticks = value;
            }
        }

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

        public override void Reset()
        {
            this.Sticks = new Stack<Stick>();

            for (int i = 0; i < 13; i++)
            {
                sticks.Push(new Stick());
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

            if (this.Players.Peek().Id == Manager.client.CurrentUser.Id)
            {
                AITurn();
            }
        }

        public void printField()
        {
            string msg = "Hölzer: ";
            for(int i = 0; i < this.StickCount; i++)
            {
                msg += "|";
            }
            this.Channel.SendMessage(msg);
        }

        public void TakeSticks(int count, User player)
        {
            if(!started)
            {
                return;
            }

            if(player.Id != Players.Peek().Id)
            {
                this.Channel.SendMessage($"{player.NicknameMention} du bist nicht am Zug!");
                return;
            }

            if(count > 3 || count > this.StickCount)
            {
                this.Channel.SendMessage($"{player.NicknameMention} du kannst nicht so viele Hölzchen ziehen!");
                return;
            }

            if(count < 1)
            {
                this.Channel.SendMessage($"{player.NicknameMention} du musst nim. 1 Holz ziehen!");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                this.Sticks.Pop();
            }

            if(this.StickCount == 0)
            {
                this.Done = true;
                this.Channel.SendMessage($"{player.NicknameMention} hat gewonnen! gg");
                return;
            }

            this.Players.Enqueue(this.Players.Dequeue());

            this.Channel.SendMessage($"{Players.Peek().NicknameMention} ist jetzt am Zug.");

            this.printField();

            if(this.Players.Peek().Id == Manager.client.CurrentUser.Id)
            {
                AITurn();
            }
        }

        public void TakeSticks(int count)
        {
            if (!started)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                this.Sticks.Pop();
            }

            if (this.StickCount == 0)
            {
                this.Done = true;
                this.Channel.SendMessage($"{this.Players.Peek().NicknameMention} hat gewonnen! gg");
                return;
            }

            this.Players.Enqueue(this.Players.Dequeue());

            this.Channel.SendMessage($"{Players.Peek().NicknameMention} ist jetzt am Zug.");

            this.printField();
        }

        public async void AITurn()
        {
            int take = (this.StickCount % 4) == 0 ? 1 : this.StickCount % 4;

            await this.Channel.SendMessage($"Ich nehme {take} Hölzchen");
            this.TakeSticks(take);
        }

        public static void RegisterCommands()
        {
            Manager.commands.CreateCommand("take").Parameter("count", ParameterType.Required).Do(async (e) =>
            {
                int count = -1;

                try
                {
                    count = Convert.ToInt32(e.Args[0]);
                }
                catch(Exception ex)
                {
                    await e.Channel.SendMessage($"{e.Args[0]} ist keine gültige Eingabe! \r\n\r\n {ex.Message}");
                }

                Nim gameInstance = null;

                try
                {
                    gameInstance = (Nim)Manager.findGame(e.User);
                }
                catch(Exception ex)
                {
                    await e.Channel.SendMessage($"Konnte {e.User.NicknameMention}s Spiel nicht finde! \r\n\r\n {ex.Message}");
                }

                gameInstance.TakeSticks(count, e.User);

                if (gameInstance.Done)
                {
                    gameInstance = null;
                }
            });
        }

        private class Stick
        { }
    }
}
