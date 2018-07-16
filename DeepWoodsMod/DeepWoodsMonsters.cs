using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DeepWoodsMod.DeepWoodsRandom;

namespace DeepWoodsMod
{
    class DeepWoodsMonsters
    {
        private struct MonsterDecider
        {
            public readonly int minLevel;
            public readonly Probability probability;
            public MonsterDecider(int minLevel, int probability)
            {
                this.minLevel = minLevel;
                this.probability = new Probability(probability);
            }
        }

        public const int NUM_MONSTER_SPAWN_TRIES = 6;

        private static readonly Luck LUCK_FOR_HALF_MONSTERS = new Luck(0, 50);
        private static readonly Luck LUCK_FOR_UNBUFFED_MONSTERS = new Luck(25, 75);

        private const int MIN_LEVEL_FOR_BUFFED_MONSTERS = 10;

        private static readonly MonsterDecider BAT = new MonsterDecider(1, 10);
        private static readonly MonsterDecider BIGSLIME = new MonsterDecider(2, 10);
        private static readonly MonsterDecider GRUB = new MonsterDecider(2, 10);
        private static readonly MonsterDecider FLY = new MonsterDecider(2, 10);
        private static readonly MonsterDecider BRUTE = new MonsterDecider(5, 10);
        private static readonly MonsterDecider GOLEM = new MonsterDecider(5, 10);
        private static readonly MonsterDecider ROCK_CRAB = new MonsterDecider(10, 10);
        private static readonly MonsterDecider GHOST = new MonsterDecider(10, 10);
        private static readonly MonsterDecider PURPLE_SLIME = new MonsterDecider(10, 10);

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

            if (deepWoods.isLichtung)
                return;

            random.EnterMasterMode();

            deepWoods.characters.Clear();

            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            // Calculate maximum theoretical amount of monsters for the current map.
            int maxMonsters = (mapWidth * mapHeight) / DeepWoodsSpaceManager.MINIMUM_TILES_FOR_MONSTER;
            int minMonsters = Math.Min(deepWoods.GetLevel(), maxMonsters);

            // Get a random value from 0 to maxMonsters, using a "two dice" method, where the center numbers are more likely than the edges.
            // If, for example, maxMonsters is 100, it is much more likely to get something close to 50 than close to 100 or 0.
            // We then take the maximum of either minMonsters or the result, making sure we always have at least minMonsters monsters.
            int numMonsters = Math.Max(minMonsters, this.random.GetRandomValue(0, maxMonsters / 2) + this.random.GetRandomValue(0, maxMonsters / 2));

            if (deepWoods.GetCombatLevel() <= 1 || this.random.GetLuck(LUCK_FOR_HALF_MONSTERS, deepWoods.GetLuckLevel()))
            {
                numMonsters /= 2;
            }

            for (int i = 0; i < numMonsters; i++)
            {
                deepWoods.AddMonsterAtRandomLocation(CreateRandomMonster());
            }

            random.LeaveMasterMode();
        }

        Monster CreateRandomMonster()
        {
            Monster monster = null;

            if (Game1.isDarkOut() && CanHazMonster(BAT))
            {
                monster = new Bat(new Vector2());
            }
            else if (Game1.isDarkOut() && CanHazMonster(GHOST))
            {
                monster = new Ghost(new Vector2());
            }
            else if (CanHazMonster(BIGSLIME))
            {
                monster = new BigSlime(new Vector2(), GetSlimeLevel());
            }
            else if (CanHazMonster(GRUB))
            {
                monster = new Grub(new Vector2(), true);
            }
            else if (CanHazMonster(FLY))
            {
                monster = new Fly(new Vector2(), true);
            }
            else if (CanHazMonster(BRUTE))
            {
                monster = new ShadowBrute(new Vector2());
            }
            else if (CanHazMonster(GOLEM))
            {
                monster = new RockGolem(new Vector2(), deepWoods.GetCombatLevel());
            }
            else if (CanHazMonster(ROCK_CRAB))
            {
                monster = new RockCrab(new Vector2(), GetRockCrabType());
            }
            else
            {
                monster = new GreenSlime(new Vector2(), GetSlimeLevel());
            }

            if (deepWoods.GetLevel() >= MIN_LEVEL_FOR_BUFFED_MONSTERS && !this.random.GetLuck(LUCK_FOR_UNBUFFED_MONSTERS, deepWoods.GetLuckLevel()))
            {
                BuffMonster(monster);
            }

            return monster;
        }

        private void BuffMonster(Monster monster)
        {
            int maxAddedSpeed = deepWoods.GetCombatLevel() / 3 + (deepWoods.GetLevel() - MIN_LEVEL_FOR_BUFFED_MONSTERS) / 10;
            int minAddedSpeed = maxAddedSpeed / 3;

            float maxBuff = deepWoods.GetCombatLevel() * 0.5f + (deepWoods.GetLevel() - MIN_LEVEL_FOR_BUFFED_MONSTERS) * 0.1f;
            float minBuff = maxBuff * 0.25f;

            monster.addedSpeed = Math.Max(monster.addedSpeed, monster.addedSpeed + Game1.random.Next(minAddedSpeed, maxAddedSpeed));
            monster.missChance.Value = Math.Max(monster.missChance.Value, monster.missChance.Value * GetBuff(minBuff, maxBuff));
            monster.resilience.Value = Math.Max(monster.resilience.Value, (int)(monster.resilience.Value * GetBuff(minBuff, maxBuff)));
            monster.Health = Math.Max(monster.Health, (int)(monster.Health * GetBuff(minBuff, maxBuff)));
            monster.DamageToFarmer = Math.Max(monster.DamageToFarmer, (int)(monster.DamageToFarmer * GetBuff(minBuff, maxBuff)));
        }

        private float GetBuff(float minBuff, float maxBuff)
        {
            return Math.Max(1.0f, (float)((Game1.random.NextDouble() * (maxBuff - minBuff)) + minBuff));
        }

        private bool CanHazMonster(MonsterDecider which)
        {
            return deepWoods.GetLevel() >= which.minLevel && this.random.GetChance(which.probability);
        }

        private string GetRockCrabType()
        {
            return "Iridium Crab";
        }

        private int GetSlimeLevel()
        {
            if (CanHazMonster(PURPLE_SLIME))
            {
                return 121;
            }
            else if (Game1.currentSeason == "winter" && this.random.GetChance(Probability.FIFTY_FIFTY))
            {
                return 79;
            }
            else
            {
                return 39;
            }
        }
    }
}
