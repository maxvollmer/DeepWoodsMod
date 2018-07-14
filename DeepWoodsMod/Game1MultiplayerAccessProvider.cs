using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWoodsMod
{
    class Game1MultiplayerAccessProvider : Game1
    {
        private Game1MultiplayerAccessProvider() { }
        public static Multiplayer GetMultiplayer()
        {
            return Game1.multiplayer;
        }
    }
}
