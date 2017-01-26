using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordGameBot
{
    public abstract class Game : IGame
    {
        private Queue<User> players;
        private Channel channel;

        public Queue<User> Players
        {
            get
            {
                return players;
            }

            set
            {
                players = value;
            }
        }

        public Channel Channel
        {
            get
            {
                return channel;
            }

            set
            {
                channel = value;
            }
        }

        public Game(IEnumerable<User> players, Channel channel)
        {
            this.Channel = channel;
            this.Players = new Queue<User>();
            foreach(User player in players)
            {
                this.Players.Enqueue(player);
            }

            this.Reset();
        }

        public Game()
        { }

        public abstract void Reset();

        public abstract void Start();
    }
}
