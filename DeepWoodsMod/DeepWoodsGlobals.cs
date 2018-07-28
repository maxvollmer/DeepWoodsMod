
using xTile.Dimensions;
using static DeepWoodsMod.DeepWoodsRandom;

namespace DeepWoodsMod
{
    class DeepWoodsGlobals
    {
        public readonly static string SAVE_FILE_NAME = "DeepWoodsModSave";
        public readonly static string DEFAULT_OUTDOOR_TILESHEET_ID = "DefaultOutdoor";
        public readonly static string LAKE_TILESHEET_ID = "WaterBorderTiles";
        public readonly static Location WOODS_WARP_LOCATION = new Location(26, 31);
        public readonly static int NUM_TILES_PER_LIGHTSOURCE = 16;
        public readonly static int MINIMUM_TILES_FOR_BAUBLE = 16;
        public readonly static int MINIMUM_TILES_FOR_LEAVES = 16;
        public readonly static int MINIMUM_TILES_FOR_MONSTER = 16;
        public readonly static int MINIMUM_TILES_FOR_TERRAIN_FEATURE = 4;
        public readonly static int NUM_MONSTER_SPAWN_TRIES = 6;
        public readonly static Chance CHANCE_FOR_WATER_LILY = new Chance(8);
        public readonly static Chance CHANCE_FOR_BLOSSOM_ON_WATER_LILY = new Chance(30);
        public readonly static int TIME_BEFORE_DELETION_ALLOWED_IF_OBELISK_SPAWNED = 100;
        public readonly static string UNIQUE_NAME_FOR_EASTER_EGG_ITEMS = "DeepWoodsModEasterEggItemIHopeThisNameIsUniqueEnoughToNotMessOtherModsUpw5365r6zgdhrt6u";
        public readonly static byte NETWORK_MESSAGE_DEEPWOODS_INIT = 98; // SMAPI will handle custom network messages in a future version, until then let's hope no other mod uses this value for a custom message :S 
        public readonly static byte NETWORK_MESSAGE_DEEPWOODS_WARP = 99; // SMAPI will handle custom network messages in a future version, until then let's hope no other mod uses this value for a custom message :S 
    }
}
