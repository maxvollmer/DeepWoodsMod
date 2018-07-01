using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWoodsMod
{
    class DeepWoodsMonsters
    {
        private DeepWoods deepWoods;
        private DeepWoodsRandom random;
        private DeepWoodsSpaceManager spaceManager;

        private DeepWoodsMonsters(DeepWoods deepWoods, DeepWoodsRandom random, DeepWoodsSpaceManager spaceManager)
        {
            this.deepWoods = deepWoods;
            this.random = random;
            this.spaceManager = spaceManager;
        }

        public static void AddMonsters(DeepWoods deepWoods, DeepWoodsRandom random, DeepWoodsSpaceManager spaceManager)
        {
            new DeepWoodsMonsters(deepWoods, random, spaceManager).AddMonsters();
        }

        private void AddMonsters()
        {
            if (!Game1.IsMasterGame)
                return;

            random.EnterMasterMode();

            deepWoods.characters.Clear();

            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            // Calculate maximum theoretical amount of monsters for the current map.
            int maxMonsters = (mapWidth * mapHeight) / DeepWoodsSpaceManager.MINIMUM_TILES_FOR_MONSTER;

            // TODO: Use DeepWoods level, luck and combat level to modify maxMonsters.
            // Game1.dailyLuck;
            // TODO: Use combat level of player(s) in level, not host!

            // Get a random value from 0 to maxMonsters, using a "two dice" method,
            // where the center numbers are more likely than the edges.
            int numMonsters =
                this.random.GetRandomValue(0, maxMonsters / 2)
                + this.random.GetRandomValue(0, maxMonsters / 2);

            for (int i = 0; i < numMonsters/10; i++)
            {
                // TODO: More monster types.
                // TODO: Use DeepWoods level, luck and combat level to modify monster strength.
                deepWoods.addCharacterAtRandomLocation(new GreenSlime(new Vector2(), 40));
                deepWoods.addCharacterAtRandomLocation(new BigSlime(new Vector2(), 40));
                deepWoods.addCharacterAtRandomLocation(new Grub(new Vector2()));
                deepWoods.addCharacterAtRandomLocation(new Fly(new Vector2()));
                deepWoods.addCharacterAtRandomLocation(new Bat(new Vector2()));
                deepWoods.addCharacterAtRandomLocation(new Ghost(new Vector2()));
                deepWoods.addCharacterAtRandomLocation(new ShadowBrute(new Vector2()));
                deepWoods.addCharacterAtRandomLocation(new ShadowShaman(new Vector2()));
                deepWoods.addCharacterAtRandomLocation(new SquidKid(new Vector2()));
                deepWoods.addCharacterAtRandomLocation(new RockGolem(new Vector2(), Game1.player.CombatLevel));
            }

            random.LeaveMasterMode();
        }
    }
}
