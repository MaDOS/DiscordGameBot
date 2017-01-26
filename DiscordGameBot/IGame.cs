using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordGameBot
{
    public interface IGame
    {
        void Start();
        void Reset();
    }
}
