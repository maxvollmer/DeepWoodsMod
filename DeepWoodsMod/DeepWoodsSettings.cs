
using StardewModdingAPI;
using System.Collections.Generic;
using xTile.Dimensions;
using static DeepWoodsMod.DeepWoodsRandom;

namespace DeepWoodsMod
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

    class DeepWoodsSettings
    {
        public WoodsSettings WoodsSettings { get; set; } = new WoodsSettings();
        public OtherSettings OtherSettings { get; set; } = new OtherSettings();

        public static DeepWoodsSettings Settings { get; set; } = new DeepWoodsSettings();

        public static void Load()
        {
            Settings = ModEntry.GetHelper().ReadConfig<DeepWoodsSettings>() ?? new DeepWoodsSettings();
            ModEntry.Log("bla: " + Settings.WoodsSettings.ExampleFloat);
        }

        public static void Save()
        {
            // ModEntry.GetHelper().WriteJsonFile($"{Constants.CurrentSavePath}/test.json", testConfig);
        }


        public readonly static HashSet<long> playersWhoGotStardropFromUnicorn = new HashSet<long>();


        // Other stuff
        public readonly static string UNIQUE_NAME_FOR_EASTER_EGG_ITEMS = "DeepWoodsModEasterEggItemIHopeThisNameIsUniqueEnoughToNotMessOtherModsUpw5365r6zgdhrt6u";

        public readonly static byte NETWORK_MESSAGE_DEEPWOODS_WARP = 99; // Let's hope no other mod uses this value for a custom message :S


        // DeepWoods
        public readonly static string DEFAULT_OUTDOOR_TILESHEET_ID = "DefaultOutdoor";
        public readonly static string LAKE_TILESHEET_ID = "WaterBorderTiles";

        public readonly static int TIME_BEFORE_DELETION_ALLOWED_IF_OBELISK_SPAWNED = 100;

        public readonly static int MIN_LEVEL_FOR_LICHTUNG = 10;
        public readonly static Chance LUCK_FOR_LICHTUNG = new Chance(new LuckValue(10, 50));

        public readonly static int CRITTER_MULTIPLIER = 5;

        public readonly static Location DEEPWOODS_ENTER_LOCATION = new Location(MIN_MAP_WIDTH / 2, 0);
        public readonly static Location WOODS_WARP_LOCATION = new Location(26, 31);


        // DeepWoodsBuilder
        public readonly static Chance CHANCE_FOR_BIG_FORESTPATCH_IN_CENTER = new Chance(25);
        public readonly static Chance CHANCE_FOR_FORESTPATCH_IN_GRID = new Chance(50);

        public readonly static Chance CHANCE_FOR_WATER_LILY = new Chance(8);
        public readonly static Chance CHANCE_FOR_BLOSSOM_ON_WATER_LILY = new Chance(30);

        public readonly static int FOREST_ROW_MAX_INWARDS_BUMP = 2;
        public readonly static Chance CHANCE_FOR_FOREST_ROW_BUMP = new Chance(50);


        // DeepWoodsMonsters
        public struct MonsterDecider
        {
            public readonly int minLevel;
            public readonly Chance probability;
            public MonsterDecider(int minLevel, int probability)
            {
                this.minLevel = minLevel;
                this.probability = new Chance(probability);
            }
        }

        public readonly static int NUM_MONSTER_SPAWN_TRIES = 6;

        public readonly static Chance LUCK_FOR_HALF_MONSTERS = new Chance(new LuckValue(0, 50));
        public readonly static Chance LUCK_FOR_UNBUFFED_MONSTERS = new Chance(new LuckValue(25, 75));

        public readonly static int MIN_LEVEL_FOR_BUFFED_MONSTERS = 10;

        public readonly static MonsterDecider BAT = new MonsterDecider(1, 10);
        public readonly static MonsterDecider BIGSLIME = new MonsterDecider(2, 10);
        public readonly static MonsterDecider GRUB = new MonsterDecider(2, 10);
        public readonly static MonsterDecider FLY = new MonsterDecider(2, 10);
        public readonly static MonsterDecider BRUTE = new MonsterDecider(5, 10);
        public readonly static MonsterDecider GOLEM = new MonsterDecider(5, 10);
        public readonly static MonsterDecider ROCK_CRAB = new MonsterDecider(10, 10);
        public readonly static MonsterDecider GHOST = new MonsterDecider(10, 10);
        public readonly static MonsterDecider PURPLE_SLIME = new MonsterDecider(10, 10);


        // DeepWoodsSpaceManager
        public readonly static int MIN_CORNER_SIZE = 3;
        public readonly static int MAX_CORNER_SIZE = 8;

        /// <summary>Amount of tiles exits expand in each direction</summary>
        public readonly static int DEEPWOODS_EXIT_RADIUS = 2;

        public readonly static int MIN_CORNER_DISTANCE_FOR_ENTER_LOCATION = MAX_CORNER_SIZE + DEEPWOODS_EXIT_RADIUS + 2;   // => 12

        public readonly static int MIN_MAP_WIDTH = MIN_CORNER_DISTANCE_FOR_ENTER_LOCATION * 2 + 4;   // => 28
        public readonly static int MAX_MAP_WIDTH = 64;
        public readonly static int MIN_MAP_HEIGHT = MIN_CORNER_DISTANCE_FOR_ENTER_LOCATION * 2 + 4;  // => 28
        public readonly static int MAX_MAP_HEIGHT = 64;

        public readonly static int MAX_MAP_SIZE_FOR_LICHTUNG = 32;

        public readonly static int MIN_FOREST_PATCH_DIAMETER = 12;
        public readonly static int MAX_FOREST_PATCH_DIAMETER = 24;
        public readonly static int FOREST_PATCH_MIN_GAP_TO_MAPBORDER = 6;
        public readonly static int FOREST_PATCH_MIN_GAP_TO_EACHOTHER = 4;
        public readonly static int FOREST_PATCH_CENTER_MIN_DISTANCE_TO_MAPBORDER = FOREST_PATCH_MIN_GAP_TO_MAPBORDER + MIN_FOREST_PATCH_DIAMETER / 2;
        public readonly static int FOREST_PATCH_CENTER_MIN_DISTANCE_TO_EACHOTHER = FOREST_PATCH_MIN_GAP_TO_EACHOTHER + MIN_FOREST_PATCH_DIAMETER / 2;
        public readonly static int MINIMUM_TILES_FOR_FORESTPATCH = MIN_FOREST_PATCH_DIAMETER * MIN_FOREST_PATCH_DIAMETER;
        public readonly static int FOREST_PATCH_SHRINK_STEP_SIZE = 2;

        public readonly static int MINIMUM_TILES_FOR_MONSTER = 36;
        public readonly static int MINIMUM_TILES_FOR_TERRAIN_FEATURE = 4;
        public readonly static int MINIMUM_TILES_FOR_BAUBLE = 16;
        public readonly static int MINIMUM_TILES_FOR_LEAVES = 16;

        public readonly static int LICHTUNG_ENTRANCE_DEPTH = 5;
        public readonly static int NUM_TILES_PER_LIGHTSOURCE_IN_FOREST_PATCH = 16;
        public readonly static int NUM_TILES_PER_LIGHTSOURCE_IN_LICHTUNG = 16;



        // DeepWoodsStuffCreator
        public readonly static int MIN_LEVEL_FOR_METEORITE = 20;
        public readonly static int MIN_LEVEL_FOR_FLOWERS = 10;
        public readonly static int MIN_LEVEL_FOR_FRUITS = 10;
        public readonly static int MIN_LEVEL_FOR_GINGERBREAD_HOUSE = 20;
        public readonly static int MIN_LEVEL_FOR_THORNY_BUSHES = 10;

        public readonly static int MAX_EASTER_EGGS_PER_WOOD = 4;

        public readonly static Chance CHANCE_FOR_GINGERBREAD_HOUSE = new Chance(1);
        public readonly static Chance CHANCE_FOR_RESOURCECLUMP = new Chance(5);
        public readonly static Chance CHANCE_FOR_LARGE_BUSH = new Chance(10);
        public readonly static Chance CHANCE_FOR_MEDIUM_BUSH = new Chance(5);
        public readonly static Chance CHANCE_FOR_SMALL_BUSH = new Chance(5);
        public readonly static Chance CHANCE_FOR_GROWN_TREE = new Chance(25);
        public readonly static Chance CHANCE_FOR_MEDIUM_TREE = new Chance(10);
        public readonly static Chance CHANCE_FOR_SMALL_TREE = new Chance(10);
        public readonly static Chance CHANCE_FOR_GROWN_FRUIT_TREE = new Chance(1);
        public readonly static Chance CHANCE_FOR_SMALL_FRUIT_TREE = new Chance(5);
        public readonly static Chance CHANCE_FOR_WEED = new Chance(20);
        public readonly static Chance CHANCE_FOR_TWIG = new Chance(10);
        public readonly static Chance CHANCE_FOR_STONE = new Chance(10);
        public readonly static Chance CHANCE_FOR_MUSHROOM = new Chance(10);
        public readonly static Chance CHANCE_FOR_FLOWER = new Chance(7);
        public readonly static Chance CHANCE_FOR_FLOWER_IN_WINTER = new Chance(3);

        public readonly static Chance CHANCE_FOR_METEORITE = new Chance(1);
        public readonly static Chance CHANCE_FOR_BOULDER = new Chance(10);
        public readonly static Chance CHANCE_FOR_HOLLOWLOG = new Chance(30);

        public readonly static WeightedInt[] FRUIT_COUNT_CHANCES = new WeightedInt[]{
            new WeightedInt(0, 60),
            new WeightedInt(1, 30),
            new WeightedInt(2, 20),
            new WeightedInt(3, 10),
        };

        public readonly static Chance CHANCE_FOR_FLOWER_ON_LICHTUNG = new Chance(5);

        public readonly static Chance LUCK_FOR_EASTEREGG = new Chance(new LuckValue(5, 25), 1000);
        public readonly static Chance LUCK_FOR_MAX_EASTER_EGGS_INCREASE = new Chance(new LuckValue(0, 25));
        public readonly static Chance LUCK_FOR_MAX_EASTER_EGGS_NOT_HALFED = new Chance(new LuckValue(75, 100));

        // Possibilities for stuff in a Lichtung
        public readonly static WeightedValue<LichtungStuff>[] LICHTUNG_STUFF = new WeightedValue<LichtungStuff>[]{
            new WeightedValue<LichtungStuff>(LichtungStuff.Nothing, 1500),
            new WeightedValue<LichtungStuff>(LichtungStuff.Lake, 1500000),
            new WeightedValue<LichtungStuff>(LichtungStuff.HealingFountain, 1000),
            new WeightedValue<LichtungStuff>(LichtungStuff.GingerbreadHouse, 1000),
            new WeightedValue<LichtungStuff>(LichtungStuff.Treasure, 500),
            new WeightedValue<LichtungStuff>(LichtungStuff.IridiumTree, 500),
            new WeightedValue<LichtungStuff>(LichtungStuff.Unicorn, 250),
            new WeightedValue<LichtungStuff>(LichtungStuff.ExcaliburStone, 250)
        };

        public readonly static WeightedInt[] LICHTUNG_PILE_ITEM_IDS_FOR_TREASURE = new WeightedInt[] {
            new WeightedInt(384, 2000),   // Gold ore
            new WeightedInt(386, 300),    // Iridium ore
            new WeightedInt(80, 75),      // Quartz (25g)
            new WeightedInt(82, 75),      // Fire Quartz (80g)
            new WeightedInt(66, 75),      // Amethyst (100g)
            new WeightedInt(62, 50),      // Aquamarine (180g)
            new WeightedInt(60, 40),      // Emerald (250g)
            new WeightedInt(64, 30),      // Ruby (250g)
            new WeightedInt(72, 10),      // Diamond
            new WeightedInt(74, 1),       // Prismatic Shard
            new WeightedInt(166, 1),      // Treasure Chest
        };

        public readonly static WeightedInt[] LICHTUNG_PILE_ITEM_IDS_FOR_TRASH = new WeightedInt[] {
            new WeightedInt(168, 2000),   // Trash
            new WeightedInt(172, 1000),   // Old Newspaper
            new WeightedInt(170, 1000),   // Glasses
            new WeightedInt(171, 500),    // CD
            new WeightedInt(167, 100),    // Joja Cola
            new WeightedInt(122, 5),      // Ancient Dwarf Computer
            new WeightedInt(118, 5),      // Glass Shards
            // new WeightedInt(169, 2000),// Driftwood
        };

        public readonly static Chance CHANCE_FOR_METALBARS_IN_TREASURE = new Chance(10);
        public readonly static Chance CHANCE_FOR_ELIXIRS_IN_TREASURE = new Chance(10);
        public readonly static Chance CHANCE_FOR_GOLDENMASK_IN_TREASURE = new Chance(10);
        public readonly static Chance CHANCE_FOR_DWARF_SCROLL_IN_TREASURE = new Chance(10);
        public readonly static Chance CHANCE_FOR_RING_IN_TREASURE = new Chance(10);
        public readonly static Chance CHANCE_FOR_PILE_ITEM_IN_TREASURE = new Chance(10);

        public readonly static LuckRange TREASURECHEST_METALBAR_STACK_SIZE = new LuckRange(new LuckValue(0, 100), new LuckValue(0, 100));
        public readonly static LuckRange TREASURECHEST_ELIXIR_STACK_SIZE = new LuckRange(new LuckValue(0, 100), new LuckValue(0, 100));
        public readonly static LuckRange TREASURECHEST_PILE_ITEM_STACK_SIZE = new LuckRange(new LuckValue(0, 100), new LuckValue(0, 100));

        public readonly static Chance CHANCE_FOR_LEWIS_SHORTS_IN_TRASH = new Chance(1, 1000);
        public readonly static Chance CHANCE_FOR_BONE_IN_TRASH = new Chance(10);
        public readonly static Chance CHANCE_FOR_ARTEFACT_IN_TRASH = new Chance(10);
        public readonly static Chance CHANCE_FOR_PUPPET_IN_TRASH = new Chance(10);
        public readonly static Chance CHANCE_FOR_PILE_ITEM_IN_TRASH = new Chance(10);

        public readonly static LuckRange TRASHCAN_PILE_ITEM_STACK_SIZE = new LuckRange(new LuckValue(0, 100), new LuckValue(0, 100));

        // This value will be used to determine if a Lichtung has a pile of trash instead of treasure
        public readonly static Chance LUCK_FOR_LICHTUNG_TREASURE_NOT_TRASH = new Chance(new LuckValue(0, 100));

        public readonly static WeightedInt[] WINTER_FLOWERS = new WeightedInt[] {
            new WeightedInt(429, 290),  // 597, // BlueJazz, spring (50g)
            new WeightedInt(425, 50),   // 595, // FairyRose, fall (290g)
        };

        public readonly static WeightedInt[] FLOWERS = new WeightedInt[] {
            new WeightedInt(427, 290), // 591, // Tulip, spring (30g)
            new WeightedInt(429, 140), // 597, // BlueJazz, spring (50g)
            new WeightedInt(455, 90),  // 593, // SummerSpangle, summer (90g)
            new WeightedInt(453, 50),  // 376, // Poppy, summer (140g)
            new WeightedInt(425, 30),  // 595, // FairyRose, fall (290g)
        };



        // Excalibur
        public class ExcaliburSettings
        {
            public bool ExampleBoolean { get; set; } = true;
        }
        public readonly static string EXCALIBUR_BASE_NAME = "Excalibur";
        public readonly static string EXCALIBUR_DESCRIPTION = "It feels hopeful to wield.";
        public readonly static string EXCALIBUR_DISPLAY_NAME = "Excalibur";
        public readonly static int EXCALIBUR_MIN_DAMAGE = 120;
        public readonly static int EXCALIBUR_MAX_DAMAGE = 180;
        public readonly static float EXCALIBUR_KNOCKBACK = 1.5f;
        public readonly static int EXCALIBUR_SPEED = 10;
        public readonly static int EXCALIBUR_ADDED_PRECISION = 5;
        public readonly static int EXCALIBUR_ADDED_DEFENSE = 5;
        public readonly static int EXCALIBUR_ADDED_AREA_OF_EFFECT = 5;
        public readonly static float EXCALIBUR_CRITICAL_CHANCE = .05f;
        public readonly static float EXCALIBUR_CRITICAL_MULTIPLIER = 5;


        // GingerBreadHouse
        public readonly static int GINGERBREAD_HOUSE_START_HEALTH = 200;
        public readonly static int GINGERBREAD_HOUSE_SPAWN_FOOD_HEALTH_STEP_SIZE = 20;
        public readonly static int GINGERBREAD_HOUSE_MINIMUM_AXE_LEVEL = 0; // 0 for debugging purposes for now


        // IridiumTree
        public readonly static int IRIDIUM_TREE_START_HEALTH = 200;
        public readonly static int IRIDIUM_TREE_SPAWN_IRIDIUM_ORE_HEALTH_STEP_SIZE = 20;
        public readonly static int IRIDIUM_TREE_MINIMUM_AXE_LEVEL = 0; // 0 for debugging purposes for now


        // Thorny bush
        public readonly static int DESTROYABLE_BUSH_MIN_AXE_LEVEL = 0;    // TODO: 0 for development
        public readonly static int THORNY_BUSH_MIN_AXE_LEVEL = 0;    // TODO: 0 for development
        public readonly static int THORNY_BUSH_DAMAGE_PER_LEVEL = 5;


        // Unicorn
        public readonly static int UNICORN_SCARE_DISTANCE = 8;
        public readonly static int UNICORN_SCARE_SPEED = 3;
        public readonly static int UNICORN_FLEE_SPEED = 12;


        // WoodsObelisk
        public readonly static string WOODS_OBELISK_BUILDING_NAME = "Woods Obelisk";
        public readonly static string WOODS_OBELISK_DISPLAY_NAME = "Woods Obelisk Displayname";
        public readonly static string WOODS_OBELISK_DESCRIPTION = "Woods Obelisk Description";
        public readonly static int WOODS_OBELISK_MONEY_REQUIRED = 10;
        public readonly static Dictionary<int, int> WOODS_OBELISK_ITEMS_REQUIRED = new Dictionary<int, int>();




        // StuffCreatore
    }
}
