using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Linq;

namespace DeepWoodsMod
{
    class DeepWoodsRandom
    {
        private const int MAGIC_SALT = 854574563;

        public const int NEUTRAL_LUCK_WEIGHT = 0;
        public const int NEUTRAL_LUCK_LEVEL = 0;

        private readonly int seed;
        private readonly Random random;
        private int masterModeCounter;

        public struct WeightedValue
        {
            public readonly int value;
            public readonly int weight;
            public WeightedValue(int value, int weight)
            {
                this.value = value;
                this.weight = weight;
            }
            public static implicit operator WeightedValue(int[] i)
            {
                return new WeightedValue(i[0], i[1]);
            }
        }

        public class Probability
        {
            public const int PROCENT = 100;
            public const int PROMILLE = 1000;

            public readonly static Probability FIFTY_FIFTY = new Probability(50);

            private int probability;
            private int range;

            public Probability(int probability, int range = PROCENT)
            {
                this.probability = probability;
                this.range = range;
            }

            public int GetValue()
            {
                return this.probability;
            }

            public int GetRange()
            {
                return this.range;
            }
        }

        public class Luck
        {
            private int minProbability;
            private int maxProbability;
            private int range;

            public Luck(int minProbability, int maxProbability, int range = Probability.PROCENT)
            {
                this.minProbability = minProbability;
                this.maxProbability = maxProbability;
                this.range = range;
            }

            public int GetMinProbability()
            {
                return this.minProbability;
            }

            public int GetMaxProbability()
            {
                return this.maxProbability;
            }

            public int GetRange()
            {
                return this.range;
            }
        }

        public DeepWoodsRandom(int level, DeepWoodsEnterExit.EnterDirection enterDir, int? salt)
        {
            this.seed = CalculateSeed(level, enterDir, salt);
            this.random = new Random(this.seed);
            this.masterModeCounter = 0;
        }

        public DeepWoodsRandom(int seed)
        {
            this.seed = seed;
            this.random = new Random(this.seed);
            this.masterModeCounter = 0;
        }

        public bool IsInMasterMode()
        {
            return this.masterModeCounter > 0;
        }

        public int GetSeed()
        {
            return this.seed;
        }

        private int CalculateSeed(int level, DeepWoodsEnterExit.EnterDirection enterDir, int? salt)
        {
            if (level == 1)
            {
                // This is the "root" DeepWoods level, always use UniqueMultiplayerID as seed.
                // This makes sure the first level stays the same for the entire game, but still be different for each unique game experience.
                return GetHashFromUniqueMultiplayerID() ^ MAGIC_SALT;
            }
            else
            {
                // Calculate seed from multiplayer ID, DeepWoods level, enter direction and time since start.
                // This makes sure the seed is the same for all players entering the same DeepWoods level during the same game hour,
                // but still makes it unique for each game and pseudorandom enough for players to not be able to reasonably predict the woods.
                return GetHashFromUniqueMultiplayerID() ^ UniformAnyInt(level) ^ UniformAnyInt((int)enterDir) ^ UniformAnyInt(HoursSinceStart()) ^ (salt.HasValue ? salt.Value : MAGIC_SALT);
            }
        }

        private int UniformAnyInt(int x)
        {
            // From https://stackoverflow.com/a/12996028/9199167
            x = ((x >> 16) ^ x) * 0x45d9f3b;
            x = ((x >> 16) ^ x) * 0x45d9f3b;
            x = (x >> 16) ^ x;
            return x;
        }

        private int GetHashFromUniqueMultiplayerID()
        {
            ulong uniqueMultiplayerID = Game1.uniqueIDForThisGame; //Game1.MasterPlayer.UniqueMultiplayerID;
            return UniformAnyInt((int)((uniqueMultiplayerID >> 32) ^ uniqueMultiplayerID));
        }

        private int HoursSinceStart()
        {
            int hourOfDay = 1 + (Game1.timeOfDay - 600) / 100;
            return hourOfDay + SDate.Now().DaysSinceStart * 20;
        }

        public bool GetChance(Probability probability)
        {
            return GetRandomValue(0, probability.GetRange()) < probability.GetValue();
        }

        public bool GetLuck(Probability probability, int luckWeight = NEUTRAL_LUCK_WEIGHT, int luckLevel = NEUTRAL_LUCK_LEVEL)
        {
            // Daily luck in range from -100 to 100:
            int dailyLuck = Math.Min(100, Math.Max(-100, (int)((Game1.dailyLuck / 0.12) * 100.0)));

            // Player luck in range from 0 to 100:
            int playerLuck = Math.Min(100, Math.Max(0, luckLevel * 10));

            // Total luck in range from -100 to 100:
            int totalLuck = Math.Min(100, Math.Max(-100, (dailyLuck + playerLuck) / 2));

            // Luck modifier in range from -luckWeight to luckWeight:
            int luckModifier = Math.Min(luckWeight, Math.Max(-luckWeight, (totalLuck * luckWeight) / 100));

            // Use new probability modified with luck:
            return GetChance(new Probability(probability.GetValue() + luckModifier, probability.GetRange()));
        }

        public bool GetLuck(Luck luck, int luckLevel = NEUTRAL_LUCK_LEVEL)
        {
            int minProbability = luck.GetMinProbability();
            int maxProbability = luck.GetMaxProbability();
            int range = luck.GetRange();
            int delta = maxProbability - minProbability;
            return GetLuck(new Probability(minProbability + delta / 2, range), delta / 2, luckLevel);
        }

        public Random GetRandom()
        {
            if (this.IsInMasterMode())
            {
                return Game1.random;
            }
            else
            {
                return this.random;
            }
        }

        public int GetLuckValue(Luck luck, int luckLevel = NEUTRAL_LUCK_LEVEL)
        {
            // Daily luck in range from -100 to 100:
            int dailyLuck = Math.Min(100, Math.Max(-100, (int)((Game1.dailyLuck / 0.12) * 100.0)));

            // Player luck in range from 0 to 100:
            int playerLuck = Math.Min(100, Math.Max(0, luckLevel * 10));

            // Total luck in range from -100 to 100:
            int totalLuck = Math.Min(100, Math.Max(-100, (dailyLuck + playerLuck) / 2));

            // Total luck in range from 0 to 100:
            totalLuck = Math.Min(100, Math.Max(0, (totalLuck + 100) / 2));

            // Total misfortune in range from 0 to 100:
            int totalMisfortune = Math.Min(100, Math.Max(0, 100 - totalLuck));

            int min = (luck.GetMinProbability() * totalMisfortune) / 100;
            int max = (luck.GetMaxProbability() * totalLuck) / 100;

            return min + max;
        }

        public int GetRandomValue(int min, int max)
        {
            if (this.IsInMasterMode())
            {
                return GetRandom().Next(min, max);
            }
            else
            {
                return GetRandom().Next(min, max);
            }
        }

        public int GetRandomValue(Luck min, Luck max, int luckLevel = NEUTRAL_LUCK_LEVEL)
        {
            return GetRandomValue(GetLuckValue(min, luckLevel), GetLuckValue(max, luckLevel));
        }

        public int GetRandomValue(int[] values, Probability firstValueProbability = null)
        {
            if (firstValueProbability != null)
            {
                if (GetChance(firstValueProbability))
                {
                    return values[0];
                }
                else
                {
                    return values[GetRandomValue(1, values.Length)];
                }
            }
            else
            {
                return values[GetRandomValue(0, values.Length)];
            }
        }


        public int GetRandomValue(WeightedValue[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("values is null or empty");

            int total = values.Sum(wv => wv.weight);
            int n = GetRandomValue(0, total);

            int sum = 0;
            for (int i = 0; i < values.Length; i++)
            {
                sum += values[i].weight;
                if (n < sum)
                {
                    return values[i].value;
                }
            }

            throw new InvalidOperationException("Impossible to get here.");
        }

        public void EnterMasterMode()
        {
            // Master Mode is used when generating interactive content (monsters, terrain features, loot etc.)
            // These things are only generated by the server (while the map itself is generated on every client, hence the shared seed),
            // so when in master mode, we use Game1.random instead of our own random.
            // This ensures server-side only generation doesn't mess with shared generation (as the shared random stays in sync).
            this.masterModeCounter++;
        }

        public void LeaveMasterMode()
        {
            this.masterModeCounter--;
        }
    }
}
