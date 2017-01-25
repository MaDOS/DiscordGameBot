using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordNimBot.Games
{
    public class TicTacToe : Game
    {
        public TicTacToe(IEnumerable<User> players, Channel channel) : base(players, channel)
        {
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }
    }
}
