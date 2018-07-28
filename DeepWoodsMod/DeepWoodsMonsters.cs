using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using System;
using static DeepWoodsMod.DeepWoodsRandom;
using static DeepWoodsMod.DeepWoodsSettings;
using static DeepWoodsMod.DeepWoodsGlobals;

namespace DeepWoodsMod
{
    public class DeepWoodsMonsters
    {
        public class MonsterDecider
        {
            public int MinLevel { get; set; }
            public Chance Chance { get; set; }

            // For JSON serialization
            public MonsterDecider() { }

            public MonsterDecider(int minLevel, Chance chance)
            {
                MinLevel = minLevel;
                Chance = chance;
            }

            public MonsterDecider(int minLevel, int chance)
            {
                MinLevel = minLevel;
                Chance = new Chance(chance);
            }
        }

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
            int maxMonsters = (mapWidth * mapHeight) / MINIMUM_TILES_FOR_MONSTER;
            int minMonsters = Math.Min(deepWoods.GetLevel(), maxMonsters);

            // Get a random value from 0 to maxMonsters, using a "two dice" method, where the center numbers are more likely than the edges.
            // If, for example, maxMonsters is 100, it is much more likely to get something close to 50 than close to 100 or 0.
            // We then take the maximum of either minMonsters or the result, making sure we always have at least minMonsters monsters.
            int numMonsters = Math.Max(minMonsters, this.random.GetRandomValue(0, maxMonsters / 2) + this.random.GetRandomValue(0, maxMonsters / 2));

            if (deepWoods.GetCombatLevel() <= 1 || this.random.CheckChance(Settings.Monsters.ChanceForHalfMonsterCount))
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

            if (Game1.isDarkOut() && CanHazMonster(Settings.Monsters.Bat))
            {
                monster = new Bat(new Vector2());
            }
            else if (Game1.isDarkOut() && CanHazMonster(Settings.Monsters.Ghost))
            {
                monster = new Ghost(new Vector2());
            }
            else if (CanHazMonster(Settings.Monsters.BigSlime))
            {
                monster = new BigSlime(new Vector2(), GetSlimeLevel());
            }
            else if (CanHazMonster(Settings.Monsters.Grub))
            {
                monster = new Grub(new Vector2(), true);
            }
            else if (CanHazMonster(Settings.Monsters.Fly))
            {
                monster = new Fly(new Vector2(), true);
            }
            else if (CanHazMonster(Settings.Monsters.Brute))
            {
                monster = new ShadowBrute(new Vector2());
            }
            else if (CanHazMonster(Settings.Monsters.Golem))
            {
                monster = new RockGolem(new Vector2(), deepWoods.GetCombatLevel());
            }
            else if (CanHazMonster(Settings.Monsters.RockCrab))
            {
                monster = new RockCrab(new Vector2(), GetRockCrabType());
            }
            else
            {
                monster = new GreenSlime(new Vector2(), GetSlimeLevel());
            }

            if (deepWoods.GetLevel() >= Settings.Level.MinLevelForBuffedMonsters && !this.random.CheckChance(Settings.Monsters.ChanceForUnbuffedMonster))
            {
                BuffMonster(monster);
            }

            return monster;
        }

        private void BuffMonster(Monster monster)
        {
            int maxAddedSpeed = deepWoods.GetCombatLevel() / 3 + (deepWoods.GetLevel() - Settings.Level.MinLevelForBuffedMonsters) / 10;
            int minAddedSpeed = maxAddedSpeed / 3;

            float maxBuff = deepWoods.GetCombatLevel() * 0.5f + (deepWoods.GetLevel() - Settings.Level.MinLevelForBuffedMonsters) * 0.1f;
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
            return deepWoods.GetLevel() >= which.MinLevel && this.random.CheckChance(which.Chance);
        }

        private string GetRockCrabType()
        {
            return "Iridium Crab";
        }

        private int GetSlimeLevel()
        {
            if (CanHazMonster(Settings.Monsters.PurpleSlime))
            {
                return 121;
            }
            else if (Game1.currentSeason == "winter" && this.random.CheckChance(Chance.FIFTY_FIFTY))
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
