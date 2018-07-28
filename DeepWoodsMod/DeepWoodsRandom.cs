using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Linq;

namespace DeepWoodsMod
{
    class DeepWoodsRandom
    {
        private const int MAGIC_SALT = 854574563;

        private readonly DeepWoods deepWoods;
        private readonly int seed;
        private readonly Random random;
        private int masterModeCounter;

        public class LuckValue
        {
            public int BadLuck { get; }
            public int Neutral { get; }
            public int GoodLuck { get; }

            public LuckValue(int badLuck, int goodLuck)
            {
                BadLuck = badLuck;
                GoodLuck = goodLuck;
                Neutral = (BadLuck + GoodLuck) / 2;
            }

            public LuckValue(int badLuck, int neutral, int goodLuck)
            {
                BadLuck = badLuck;
                Neutral = neutral;
                GoodLuck = goodLuck;
            }
        }

        public class LuckRange
        {
            public LuckValue LowerBound { get; }
            public LuckValue UpperBound { get; }

            public LuckRange(LuckValue lowerBound, LuckValue upperBound)
            {
                LowerBound = lowerBound;
                UpperBound = upperBound;
            }
        }

        public class WeightedValue<T>
        {
            public T Value { get; }
            public LuckValue Weight { get; }

            public WeightedValue(T value, LuckValue weight)
            {
                Value = value;
                Weight = weight;
            }

            public WeightedValue(T value, int weight)
            {
                Value = value;
                Weight = new LuckValue(weight, weight, weight);
            }
        }

        public class WeightedInt : WeightedValue<int>
        {
            public WeightedInt(int value, LuckValue weight)
                : base(value, weight)
            {
            }

            public WeightedInt(int value, int weight)
                : base(value, weight)
            {
            }
        }

        public class Chance
        {
            public const int PROCENT = 100;
            public const int PROMILLE = 1000;

            public readonly static Chance FIFTY_FIFTY = new Chance(50);

            public LuckValue Value { get; }
            public int Range { get; }

            public Chance(LuckValue value, int range = PROCENT)
            {
                Value = value;
                Range = range;
            }

            public Chance(int value, int range = PROCENT)
            {
                Value = new LuckValue(value, value, value);
                Range = range;
            }
        }

        public DeepWoodsRandom(DeepWoods deepWoods, int level, DeepWoodsEnterExit.EnterDirection enterDir, int? salt)
        {
            this.deepWoods = deepWoods;
            this.seed = CalculateSeed(level, enterDir, salt);
            this.random = new Random(this.seed);
            this.masterModeCounter = 0;
        }

        public DeepWoodsRandom(DeepWoods deepWoods, int seed)
        {
            this.deepWoods = deepWoods;
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

        private Random GetRandom()
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

        private int GetAbsoluteLuckValue(LuckValue value)
        {
            // Daily luck in range from -100 to 100:
            int dailyLuck = Math.Min(100, Math.Max(-100, (int)((Game1.dailyLuck / 0.12) * 100.0)));

            // Player luck in range from 0 to 100:
            int playerLuck = Math.Min(100, Math.Max(0, deepWoods.GetLuckLevel() * 10));

            // Total luck in range from -100 to 100:
            int totalLuck = Math.Min(100, Math.Max(-100, (dailyLuck + playerLuck) / 2));

            if (totalLuck < 0)
            {
                int badLuckFactor = -totalLuck;
                int neutralFactor = 100 - badLuckFactor;
                return ((value.BadLuck * badLuckFactor) + (value.Neutral * neutralFactor)) / 100;
            }
            else
            {
                int goodLuckFactor = totalLuck;
                int neutralFactor = 100 - goodLuckFactor;
                return ((value.GoodLuck * goodLuckFactor) + (value.Neutral * neutralFactor)) / 100;
            }
        }

        public int GetRandomValue()
        {
            return GetRandom().Next();
        }

        public int GetRandomValue(int min, int max)
        {
            return GetRandom().Next(min, max);
        }

        public bool CheckChance(Chance chance)
        {
            return GetRandomValue(0, chance.Range) < GetAbsoluteLuckValue(chance.Value);
        }

        public int GetRandomValue(LuckRange range)
        {
            return GetRandomValue(GetAbsoluteLuckValue(range.LowerBound), GetAbsoluteLuckValue(range.UpperBound));
        }

        public T GetRandomValue<T>(T[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("values is null or empty");

            return values[GetRandomValue(0, values.Length)];
        }

        public int GetRandomValue(WeightedInt[] values)
        {
            return GetRandomValue<int>(values);
        }

        public T GetRandomValue<T>(WeightedValue<T>[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("values is null or empty");

            int total = values.Sum(wv => GetAbsoluteLuckValue(wv.Weight));
            int n = GetRandomValue(0, total);

            int sum = 0;
            for (int i = 0; i < values.Length; i++)
            {
                sum += GetAbsoluteLuckValue(values[i].Weight);
                if (n < sum)
                {
                    return values[i].Value;
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
