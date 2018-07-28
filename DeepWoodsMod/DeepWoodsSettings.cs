
using StardewModdingAPI;
using System.Collections.Generic;
using xTile.Dimensions;
using static DeepWoodsMod.DeepWoodsRandom;

namespace DeepWoodsMod
{
    class DeepWoodsSettings
    {
        class WoodsSettings
        {
            public bool ExampleBoolean { get; set; } = true;
            public float ExampleFloat { get; set; } = 0.5f;
        }

        class OtherSettings
        {
            public bool ExampleBoolean2 { get; set; } = true;
            public float ExampleFloat2 { get; set; } = 0.5f;
        }

        class TestConfig
        {
            public WoodsSettings WoodsSettings { get; set; } = new WoodsSettings();
            public OtherSettings OtherSettings { get; set; } = new OtherSettings();
        }

        static TestConfig testConfig;

        public static void Load()
        {
            testConfig = ModEntry.GetHelper().ReadConfig<TestConfig>() ?? new TestConfig();
            ModEntry.Log("bla: " + testConfig.WoodsSettings.ExampleFloat);
        }

        public static void Save()
        {
            ModEntry.GetHelper().WriteJsonFile($"{Constants.CurrentSavePath}/test.json", testConfig);
        }


        public static HashSet<long> playersWhoGotStardropFromUnicorn = new HashSet<long>();


        // Other stuff
        public const int MIN_AXE_LEVEL_FOR_BUSH = 0;    // TODO: 0 for development
        public const string UNIQUE_NAME_FOR_EASTER_EGG_ITEMS = "DeepWoodsModEasterEggItemIHopeThisNameIsUniqueEnoughToNotMessOtherModsUpw5365r6zgdhrt6u";

        public const byte NETWORK_MESSAGE_DEEPWOODS_WARP = 99; // Let's hope no other mod uses this value for a custom message :S


        // DeepWoods
        public const string DEFAULT_OUTDOOR_TILESHEET_ID = "DefaultOutdoor";
        public const string LAKE_TILESHEET_ID = "WaterBorderTiles";

        public const int TIME_BEFORE_DELETION_ALLOWED_IF_OBELISK_SPAWNED = 100;

        public const int MIN_LEVEL_FOR_LICHTUNG = 10;
        public static Luck LUCK_FOR_LICHTUNG = new Luck(10, 50);

        public const int CRITTER_MULTIPLIER = 5;

        public static Location DEEPWOODS_ENTER_LOCATION = new Location(MIN_MAP_WIDTH / 2, 0);
        public static Location WOODS_WARP_LOCATION = new Location(26, 31);


        // DeepWoodsBuilder
        public static Probability CHANCE_FOR_NORMAL_GRASS = new Probability(80);
        public static Probability CHANCE_FOR_BIG_FORESTPATCH_IN_CENTER = new Probability(25);
        public static Probability CHANCE_FOR_FORESTPATCH_IN_GRID = new Probability(50);

        public static Probability CHANCE_FOR_NOLEAVE_FOREST_FILLER = new Probability(80);

        public static Probability CHANCE_FOR_FOREST_ROW_TREESTUMPS = new Probability(50);

        public static Probability CHANCE_FOR_WATER_LILY = new Probability(8);
        public static Probability CHANCE_FOR_BLOSSOM_ON_WATER_LILY = new Probability(30);

        public const int FOREST_ROW_MAX_INWARDS_BUMP = 2;
        public static Probability CHANCE_FOR_FOREST_ROW_BUMP = new Probability(50);


        // DeepWoodsMonsters
        public struct MonsterDecider
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

        public static readonly Luck LUCK_FOR_HALF_MONSTERS = new Luck(0, 50);
        public static readonly Luck LUCK_FOR_UNBUFFED_MONSTERS = new Luck(25, 75);

        public const int MIN_LEVEL_FOR_BUFFED_MONSTERS = 10;

        public static readonly MonsterDecider BAT = new MonsterDecider(1, 10);
        public static readonly MonsterDecider BIGSLIME = new MonsterDecider(2, 10);
        public static readonly MonsterDecider GRUB = new MonsterDecider(2, 10);
        public static readonly MonsterDecider FLY = new MonsterDecider(2, 10);
        public static readonly MonsterDecider BRUTE = new MonsterDecider(5, 10);
        public static readonly MonsterDecider GOLEM = new MonsterDecider(5, 10);
        public static readonly MonsterDecider ROCK_CRAB = new MonsterDecider(10, 10);
        public static readonly MonsterDecider GHOST = new MonsterDecider(10, 10);
        public static readonly MonsterDecider PURPLE_SLIME = new MonsterDecider(10, 10);


        // DeepWoodsSpaceManager
        public const int MIN_CORNER_SIZE = 3;
        public const int MAX_CORNER_SIZE = 8;

        /// <summary>Amount of tiles exits expand in each direction</summary>
        public const int DEEPWOODS_EXIT_RADIUS = 2;

        public const int MIN_CORNER_DISTANCE_FOR_ENTER_LOCATION = MAX_CORNER_SIZE + DEEPWOODS_EXIT_RADIUS + 2;   // => 12

        public const int MIN_MAP_WIDTH = MIN_CORNER_DISTANCE_FOR_ENTER_LOCATION * 2 + 4;   // => 28
        public const int MAX_MAP_WIDTH = 64;
        public const int MIN_MAP_HEIGHT = MIN_CORNER_DISTANCE_FOR_ENTER_LOCATION * 2 + 4;  // => 28
        public const int MAX_MAP_HEIGHT = 64;

        public const int MAX_MAP_SIZE_FOR_LICHTUNG = 32;

        public const int MIN_FOREST_PATCH_DIAMETER = 12;
        public const int MAX_FOREST_PATCH_DIAMETER = 24;
        public const int FOREST_PATCH_MIN_GAP_TO_MAPBORDER = 6;
        public const int FOREST_PATCH_MIN_GAP_TO_EACHOTHER = 4;
        public const int FOREST_PATCH_CENTER_MIN_DISTANCE_TO_MAPBORDER = FOREST_PATCH_MIN_GAP_TO_MAPBORDER + MIN_FOREST_PATCH_DIAMETER / 2;
        public const int FOREST_PATCH_CENTER_MIN_DISTANCE_TO_EACHOTHER = FOREST_PATCH_MIN_GAP_TO_EACHOTHER + MIN_FOREST_PATCH_DIAMETER / 2;
        public const int MINIMUM_TILES_FOR_FORESTPATCH = MIN_FOREST_PATCH_DIAMETER * MIN_FOREST_PATCH_DIAMETER;
        public const int FOREST_PATCH_SHRINK_STEP_SIZE = 2;

        public const int MINIMUM_TILES_FOR_MONSTER = 36;
        public const int MINIMUM_TILES_FOR_TERRAIN_FEATURE = 4;
        public const int MINIMUM_TILES_FOR_BAUBLE = 16;
        public const int MINIMUM_TILES_FOR_LEAVES = 16;

        public const int LICHTUNG_ENTRANCE_DEPTH = 5;
        public const int NUM_TILES_PER_LIGHTSOURCE_IN_FOREST_PATCH = 16;
        public const int NUM_TILES_PER_LIGHTSOURCE_IN_LICHTUNG = 16;



        // DeepWoodsStuffCreator
        public const int MIN_LEVEL_FOR_METEORITE = 20;
        public const int MIN_LEVEL_FOR_FLOWERS = 10;
        public const int MIN_LEVEL_FOR_FRUITS = 10;
        public const int MIN_LEVEL_FOR_GINGERBREAD_HOUSE = 20;
        public const int MIN_LEVEL_FOR_THORNY_BUSHES = 10;

        public const int MAX_EASTER_EGGS_PER_WOOD = 4;

        public readonly static Probability CHANCE_FOR_GINGERBREAD_HOUSE = new Probability(1);
        public readonly static Probability CHANCE_FOR_RESOURCECLUMP = new Probability(5);
        public readonly static Probability CHANCE_FOR_LARGE_BUSH = new Probability(10);
        public readonly static Probability CHANCE_FOR_MEDIUM_BUSH = new Probability(5);
        public readonly static Probability CHANCE_FOR_SMALL_BUSH = new Probability(5);
        public readonly static Probability CHANCE_FOR_GROWN_TREE = new Probability(25);
        public readonly static Probability CHANCE_FOR_MEDIUM_TREE = new Probability(10);
        public readonly static Probability CHANCE_FOR_SMALL_TREE = new Probability(10);
        public readonly static Probability CHANCE_FOR_GROWN_FRUIT_TREE = new Probability(1);
        public readonly static Probability CHANCE_FOR_SMALL_FRUIT_TREE = new Probability(5);
        public readonly static Probability CHANCE_FOR_WEED = new Probability(20);
        public readonly static Probability CHANCE_FOR_TWIG = new Probability(10);
        public readonly static Probability CHANCE_FOR_STONE = new Probability(10);
        public readonly static Probability CHANCE_FOR_MUSHROOM = new Probability(10);
        public readonly static Probability CHANCE_FOR_FLOWER = new Probability(7);
        public readonly static Probability CHANCE_FOR_FLOWER_IN_WINTER = new Probability(3);

        public readonly static Probability CHANCE_FOR_METEORITE = new Probability(1);
        public readonly static Probability CHANCE_FOR_BOULDER = new Probability(10);
        public readonly static Probability CHANCE_FOR_HOLLOWLOG = new Probability(30);

        public readonly static Probability CHANCE_FOR_FRUIT = new Probability(50);

        public readonly static Probability CHANCE_FOR_FLOWER_ON_LICHTUNG = new Probability(5);

        public readonly static Luck LUCK_FOR_EASTEREGG = new Luck(5, 25, 1000);
        public readonly static Luck LUCK_FOR_MAX_EASTER_EGGS_INCREASE = new Luck(0, 25);
        public readonly static Luck LUCK_FOR_MAX_EASTER_EGGS_NOT_HALFED = new Luck(75, 100);

        // Possibilities for stuff in a Lichtung
        public const int LICHTUNG_STUFF_NOTHING = 0;
        public const int LICHTUNG_STUFF_LAKE = 1;
        public const int LICHTUNG_STUFF_HEALING_FOUNTAIN = 2;
        public const int LICHTUNG_STUFF_GINGERBREAD_HOUSE = 3;
        public const int LICHTUNG_STUFF_TREASURE = 4;
        public const int LICHTUNG_STUFF_IRIDIUM_TREE = 5;
        public const int LICHTUNG_STUFF_UNICORN = 6;
        public const int LICHTUNG_STUFF_EXCALIBUR = 7;

        public readonly static WeightedValue[] LICHTUNG_STUFF = new WeightedValue[]{
            new WeightedValue(LICHTUNG_STUFF_NOTHING, 1500),
            new WeightedValue(LICHTUNG_STUFF_LAKE, 1500000),
            new WeightedValue(LICHTUNG_STUFF_HEALING_FOUNTAIN, 1000),
            new WeightedValue(LICHTUNG_STUFF_GINGERBREAD_HOUSE, 1000),
            new WeightedValue(LICHTUNG_STUFF_TREASURE, 500),
            new WeightedValue(LICHTUNG_STUFF_IRIDIUM_TREE, 500),
            new WeightedValue(LICHTUNG_STUFF_UNICORN, 250),
            new WeightedValue(LICHTUNG_STUFF_EXCALIBUR, 250)
        };

        public readonly static WeightedValue[] LICHTUNG_PILE_ITEM_IDS_FOR_TREASURE = new WeightedValue[] {
            new WeightedValue(384, 2000),   // Gold ore
            new WeightedValue(386, 300),    // Iridium ore
            new WeightedValue(80, 75),      // Quartz (25g)
            new WeightedValue(82, 75),      // Fire Quartz (80g)
            new WeightedValue(66, 75),      // Amethyst (100g)
            new WeightedValue(62, 50),      // Aquamarine (180g)
            new WeightedValue(60, 40),      // Emerald (250g)
            new WeightedValue(64, 30),      // Ruby (250g)
            new WeightedValue(72, 10),      // Diamond
            new WeightedValue(74, 1),       // Prismatic Shard
            new WeightedValue(166, 1),      // Treasure Chest
        };

        public readonly static WeightedValue[] LICHTUNG_PILE_ITEM_IDS_FOR_TRASH = new WeightedValue[] {
            new WeightedValue(168, 2000),   // Trash
            new WeightedValue(172, 1000),   // Old Newspaper
            new WeightedValue(170, 1000),   // Glasses
            new WeightedValue(171, 500),    // CD
            new WeightedValue(167, 100),    // Joja Cola
            new WeightedValue(122, 5),      // Ancient Dwarf Computer
            new WeightedValue(118, 5),      // Glass Shards
            // new WeightedValue(169, 2000),// Driftwood
        };

        public readonly static Probability CHANCE_FOR_LEWIS_SHORTS_IN_TRASH = new Probability(1, 1000);

        public readonly static Probability CHANCE_FOR_METALBARS_IN_TREASURE = new Probability(10);
        public readonly static Probability CHANCE_FOR_ELIXIRS_IN_TREASURE = new Probability(10);
        public readonly static Probability CHANCE_FOR_GOLDENMASK_IN_TREASURE = new Probability(10);
        public readonly static Probability CHANCE_FOR_DWARF_SCROLL_IN_TREASURE = new Probability(10);
        public readonly static Probability CHANCE_FOR_RING_IN_TREASURE = new Probability(10);

        // This value will be used to determine if a Lichtung has a pile of trash instead of treasure
        public readonly static Luck LUCK_FOR_LICHTUNG_TREASURE_NOT_TRASH = new Luck(0, 100);



        // Excalibur
        public static int EXCALIBUR_TILE_INDEX = 3;
        public static string EXCALIBUR_BASE_NAME = "Excalibur";
        public static string EXCALIBUR_DESCRIPTION = "It feels hopeful to wield.";
        public static string EXCALIBUR_DISPLAY_NAME = "Excalibur";
        public static int EXCALIBUR_MIN_DAMAGE = 120;
        public static int EXCALIBUR_MAX_DAMAGE = 180;
        public static float EXCALIBUR_KNOCKBACK = 1.5f;
        public static int EXCALIBUR_SPEED = 10;
        public static int EXCALIBUR_ADDED_PRECISION = 5;
        public static int EXCALIBUR_ADDED_DEFENSE = 5;
        public static int EXCALIBUR_ADDED_AREA_OF_EFFECT = 5;
        public static float EXCALIBUR_CRITICAL_CHANCE = .05f;
        public static float EXCALIBUR_CRITICAL_MULTIPLIER = 5;


        // GingerBreadHouse
        public const int GINGERBREAD_HOUSE_START_HEALTH = 200;
        public const int GINGERBREAD_HOUSE_SPAWN_FOOD_HEALTH_STEP_SIZE = 20;
        public const int GINGERBREAD_HOUSE_MINIMUM_AXE_LEVEL = 0; // 0 for debugging purposes for now


        // IridiumTree
        public const int IRIDIUM_TREE_START_HEALTH = 200;
        public const int IRIDIUM_TREE_SPAWN_IRIDIUM_ORE_HEALTH_STEP_SIZE = 20;
        public const int IRIDIUM_TREE_MINIMUM_AXE_LEVEL = 0; // 0 for debugging purposes for now


        // Thorny bush
        public const int THORNY_BUSH_MIN_AXE_LEVEL = 0;    // TODO: 0 for development
        public const int THORNY_BUSH_DAMAGE_PER_LEVEL = 5;


        // Unicorn
        public const int UNICORN_SCARE_DISTANCE = 8;
        public const int UNICORN_SCARE_SPEED = 3;
        public const int UNICORN_FLEE_SPEED = 12;


        // WoodsObelisk
        public static string WOODS_OBELISK_BUILDING_NAME = "Woods Obelisk";
        public static string WOODS_OBELISK_DISPLAY_NAME = "Woods Obelisk Displayname";
        public static string WOODS_OBELISK_DESCRIPTION = "Woods Obelisk Description";
        public const int WOODS_OBELISK_MONEY_REQUIRED = 10;
        public static Dictionary<int, int> WOODS_OBELISK_ITEMS_REQUIRED = new Dictionary<int, int>();
    }
}
