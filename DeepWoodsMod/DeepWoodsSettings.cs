
using StardewModdingAPI;
using System.Collections.Generic;
using xTile.Dimensions;
using static DeepWoodsMod.DeepWoodsRandom;
using static DeepWoodsMod.DeepWoodsGlobals;
using Newtonsoft.Json;
using StardewValley.Tools;

namespace DeepWoodsMod
{
    class MapSettings
    {
        public int MaxMapWidth { get; set; } = 64;
        public int MaxMapHeight { get; set; } = 64;
        public int MaxMapWidthForClearing { get; set; } = 32;

        public int MaxBumpSizeForForestBorder { get; set; } = 2;

        public int MinTilesForCorner { get; set; } = 3;
        public int MaxTilesForCorner { get; set; } = 8;

        public int ExitRadius { get; set; } = 2;
        public int ExitLength { get; set; } = 5;

        public int MinSizeForForestPatch { get; set; } = 12;
        public int MaxSizeForForestPatch { get; set; } = 24;

        [JsonIgnore]
        public int ForestPatchMinGapToMapBorder
        {
            get
            {
                return MaxBumpSizeForForestBorder * 2 + 2;
            }
        }

        [JsonIgnore]
        public int ForestPatchMinGapToEachOther
        {
            get
            {
                return MaxBumpSizeForForestBorder * 2;
            }
        }

        [JsonIgnore]
        public int ForestPatchCenterMinDistanceToMapBorder
        {
            get
            {
                return ForestPatchMinGapToMapBorder + MinSizeForForestPatch / 2;
            }
        }

        [JsonIgnore]
        public int ForestPatchCenterMinDistanceToEachOther
        {
            get
            {
                return ForestPatchMinGapToEachOther + MinSizeForForestPatch / 2;
            }
        }

        [JsonIgnore]
        public int MinCornerDistanceForEnterLocation
        {
            get
            {
                return MaxTilesForCorner + ExitRadius + 2;   // => 12
            }
        }

        [JsonIgnore]
        public int MinMapWidth
        {
            get
            {
                return MinCornerDistanceForEnterLocation * 2 + 4;   // => 28
            }
        }

        [JsonIgnore]
        public int MinMapHeight
        {
            get
            {
                return MinCornerDistanceForEnterLocation * 2 + 4;  // => 28
            }
        }

        [JsonIgnore]
        public int MinimumTilesForForestPatch
        {
            get
            {
                return MinSizeForForestPatch * MinSizeForForestPatch;
            }
        }

        [JsonIgnore]
        public Location RootLevelEnterLocation
        {
            get
            {
                return new Location(MinMapWidth / 2, 0);
            }
        }
    }

    class LevelSettings
    {
        public int MinLevelForFlowers { get; set; } = 3;
        public int MinLevelForFruits { get; set; } = 5;
        public int MinLevelForThornyBushes { get; set; } = 10;
        public int MinLevelForBuffedMonsters { get; set; } = 15;
        public int MinLevelForWoodsObelisk { get; set; } = 20;
        public int MinLevelForMeteorite { get; set; } = 25;
        public int MinLevelForClearing { get; set; } = 30;
        public int MinLevelForGingerbreadHouse { get; set; } = 50;
    }

    public class ExcaliburSettings
    {
        public int MinDamage { get; set; } = 120;
        public int MaxDamage { get; set; } = 180;
        public float Knockback { get; set; } = 1.5f;
        public int Speed { get; set; } = 10;
        public int Precision { get; set; } = 5;
        public int Defense { get; set; } = 5;
        public int AreaOfEffect { get; set; } = 5;
        public float CriticalChance { get; set; } = .05f;
        public float CriticalMultiplier { get; set; } = 5;
    }

    public class GingerBreadHouseSettings
    {
        public int MinimumAxeLevel { get; set; } = Axe.gold;
        public int Health { get; set; } = 200;
        public int DamageIntervalForFoodDrop { get; set; } = 20;
    }

    public class IridiumTreeSettings
    {
        public int MinimumAxeLevel { get; set; } = Axe.iridium;
        public int Health { get; set; } = 200;
        public int DamageIntervalForOreDrop { get; set; } = 20;
    }

    public class WoodsObeliskSettings
    {
        public int MoneyRequired { get; set; } = 10000000;
        public Dictionary<int, int> ItemsRequired { get; set; } = new Dictionary<int, int>()
        {
            { 388, 999},    // Wood
            {  92, 999},    // Sap
            { 337, 20}      // Iridium Bar
        };
    }

    public class UnicornSettings
    {
        public int FarmerScareDistance { get; set; } = 8;
        public int FarmerScareSpeed { get; set; } = 3;
        public int FleeSpeed { get; set; } = 12;
    }

    public class BushSettings
    {
        public int MinAxeLevel { get; set; } = Axe.steel;
        public int ThornyBushMinAxeLevel { get; set; } = Axe.iridium;
        public int ThornyBushDamagePerLevel { get; set; } = 5;
    }

    public class ObjectsSettings
    {
        public ExcaliburSettings Excalibur { get; set; } = new ExcaliburSettings();
        public GingerBreadHouseSettings GingerBreadHouse { get; set; } = new GingerBreadHouseSettings();
        public IridiumTreeSettings IridiumTree { get; set; } = new IridiumTreeSettings();
        public WoodsObeliskSettings WoodsObelisk { get; set; } = new WoodsObeliskSettings();
        public UnicornSettings Unicorn { get; set; } = new UnicornSettings();
        public BushSettings Bush { get; set; } = new BushSettings();
    }

    public class ResourceClumpLuckSettings
    {
        public Chance ChanceForMeteorite { get; set; } = new Chance(1, 500);
        public Chance ChanceForBoulder { get; set; } = new Chance(1);
        public Chance ChanceForHollowLog { get; set; } = new Chance(2);
        public Chance ChanceForStump { get; set; } = new Chance(3);
    }

    public class TerrainLuckSettings
    {
        public Chance ChanceForGingerbreadHouse { get; set; } = new Chance(1);
        public Chance ChanceForLargeBush { get; set; } = new Chance(10);
        public Chance ChanceForMediumBush { get; set; } = new Chance(5);
        public Chance ChanceForSmallBush { get; set; } = new Chance(5);
        public Chance ChanceForGrownTree { get; set; } = new Chance(25);
        public Chance ChanceForMediumTree { get; set; } = new Chance(10);
        public Chance ChanceForSmallTree { get; set; } = new Chance(10);
        public Chance ChanceForGrownFruitTree { get; set; } = new Chance(1);
        public Chance ChanceForSmallFruitTree { get; set; } = new Chance(5);
        public Chance ChanceForWeed { get; set; } = new Chance(20);
        public Chance ChanceForTwig { get; set; } = new Chance(10);
        public Chance ChanceForStone { get; set; } = new Chance(10);
        public Chance ChanceForMushroom { get; set; } = new Chance(10);
        public Chance ChanceForFlower { get; set; } = new Chance(7);
        public Chance ChanceForFlowerInWinter { get; set; } = new Chance(3);
        public Chance ChanceForFlowerOnClearing { get; set; } = new Chance(5);
        public ResourceClumpLuckSettings ResourceClump { get; set; } = new ResourceClumpLuckSettings();

        public WeightedInt[] FruitCount { get; set; } = new WeightedInt[]{
            new WeightedInt(0, 60),
            new WeightedInt(1, 30),
            new WeightedInt(2, 20),
            new WeightedInt(3, 10),
        };

        public WeightedInt[] WinterFlowers { get; set; } = new WeightedInt[] {
            new WeightedInt(429, 290),  // 597, // BlueJazz, spring (50g)
            new WeightedInt(425, 50),   // 595, // FairyRose, fall (290g)
        };

        public WeightedInt[] Flowers { get; set; } = new WeightedInt[] {
            new WeightedInt(427, 290), // 591, // Tulip, spring (30g)
            new WeightedInt(429, 140), // 597, // BlueJazz, spring (50g)
            new WeightedInt(455, 90),  // 593, // SummerSpangle, summer (90g)
            new WeightedInt(453, 50),  // 376, // Poppy, summer (140g)
            new WeightedInt(425, 30),  // 595, // FairyRose, fall (290g)
        };

        public Chance ChanceForEasterEgg { get; set; } = new Chance(new LuckValue(5, 25), 1000);
        public Chance ChanceForExtraEasterEgg { get; set; } = new Chance(new LuckValue(0, 25));
        public Chance ChanceForEasterEggsDoubled { get; set; } = new Chance(new LuckValue(75, 100));

        public LuckRange MaxEasterEggsPerLevel { get; set; } = new LuckRange(new LuckValue(0, 2), new LuckValue(2, 6));
    }

    public class ClearingLuckSettings
    {
        public Chance ChanceForClearing { get; set; } = new Chance(new LuckValue(1, 10));

        public WeightedValue<LichtungStuff>[] Perks { get; set; } = new WeightedValue<LichtungStuff>[]{
            new WeightedValue<LichtungStuff>(LichtungStuff.Nothing, 1500),
            new WeightedValue<LichtungStuff>(LichtungStuff.Lake, 1500000),
            new WeightedValue<LichtungStuff>(LichtungStuff.HealingFountain, 1000),
            new WeightedValue<LichtungStuff>(LichtungStuff.GingerbreadHouse, 1000),
            new WeightedValue<LichtungStuff>(LichtungStuff.Treasure, 500),
            new WeightedValue<LichtungStuff>(LichtungStuff.IridiumTree, 500),
            new WeightedValue<LichtungStuff>(LichtungStuff.Unicorn, 250),
            new WeightedValue<LichtungStuff>(LichtungStuff.ExcaliburStone, 250)
        };

        public Chance ChanceForTrashOrTreasure { get; set; } = new Chance(new LuckValue(0, 100));

        public TreasureLuckSettings Treasure { get; set; } = new TreasureLuckSettings();
        public TrashLuckSettings Trash { get; set; } = new TrashLuckSettings();
    }

    public class TreasureLuckSettings
    {
        public WeightedInt[] PileItems { get; set; } = new WeightedInt[] {
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

        public Chance ChanceForMetalBarsInChest { get; set; } = new Chance(new LuckValue(10, 50));
        public Chance ChanceForElixirsInChest { get; set; } = new Chance(new LuckValue(10, 30));
        public Chance ChanceForArtefactInChest { get; set; } = new Chance(new LuckValue(0, 20));
        public Chance ChanceForDwarfScrollInChest { get; set; } = new Chance(new LuckValue(0, 20));
        public Chance ChanceForRingInChest { get; set; } = new Chance(new LuckValue(0, 20));
        public Chance ChanceForRandomPileItemInChest { get; set; } = new Chance(new LuckValue(0, 10));

        public LuckRange MetalBarStackSize { get; set; } = new LuckRange(new LuckValue(0, 100), new LuckValue(0, 100));
        public LuckRange ElixirStackSize { get; set; } = new LuckRange(new LuckValue(0, 100), new LuckValue(0, 100));
        public LuckRange PileItemStackSize { get; set; } = new LuckRange(new LuckValue(0, 100), new LuckValue(0, 100));
    }

    public class TrashLuckSettings
    {
        public WeightedInt[] PileItems { get; set; } = new WeightedInt[] {
            new WeightedInt(168, 2000),   // Trash
            new WeightedInt(172, 1000),   // Old Newspaper
            new WeightedInt(170, 1000),   // Glasses
            new WeightedInt(171, 500),    // CD
            new WeightedInt(167, 100),    // Joja Cola
            new WeightedInt(122, 5),      // Ancient Dwarf Computer
            new WeightedInt(118, 5),      // Glass Shards
            // new WeightedInt(169, 2000),// Driftwood
        };

        public Chance ChanceForLewisShortsInGarbagebin { get; set; } = new Chance(new LuckValue(0, 1, 1), 1000);
        public Chance ChanceForBoneInGarbagebin { get; set; } = new Chance(new LuckValue(0, 10));
        public Chance ChanceForArtefactInGarbagebin { get; set; } = new Chance(new LuckValue(0, 10));
        public Chance ChanceForPuppetInGarbagebin { get; set; } = new Chance(new LuckValue(0, 10));
        public Chance ChanceForRandomPileItemInGarbagebin { get; set; } = new Chance(new LuckValue(0, 10));

        public LuckRange PileItemStackSize { get; set; } = new LuckRange(new LuckValue(0, 100), new LuckValue(0, 100));
    }

    public class MonstersSettings
    {
        public Chance ChanceForHalfMonsterCount { get; set; } = new Chance(new LuckValue(0, 0, 50));
        public Chance ChanceForUnbuffedMonster { get; set; } = new Chance(new LuckValue(0, 25, 75));

        public DeepWoodsMonsters.MonsterDecider Bat { get; set; } = new DeepWoodsMonsters.MonsterDecider(1, 10);
        public DeepWoodsMonsters.MonsterDecider BigSlime { get; set; } = new DeepWoodsMonsters.MonsterDecider(2, 10);
        public DeepWoodsMonsters.MonsterDecider Grub { get; set; } = new DeepWoodsMonsters.MonsterDecider(2, 10);
        public DeepWoodsMonsters.MonsterDecider Fly { get; set; } = new DeepWoodsMonsters.MonsterDecider(2, 10);
        public DeepWoodsMonsters.MonsterDecider Brute { get; set; } = new DeepWoodsMonsters.MonsterDecider(5, 10);
        public DeepWoodsMonsters.MonsterDecider Golem { get; set; } = new DeepWoodsMonsters.MonsterDecider(5, 10);
        public DeepWoodsMonsters.MonsterDecider RockCrab { get; set; } = new DeepWoodsMonsters.MonsterDecider(10, 10);
        public DeepWoodsMonsters.MonsterDecider Ghost { get; set; } = new DeepWoodsMonsters.MonsterDecider(10, 10);
        public DeepWoodsMonsters.MonsterDecider PurpleSlime { get; set; } = new DeepWoodsMonsters.MonsterDecider(10, 10);
    }

    public class LuckSettings
    {
        public TerrainLuckSettings Terrain { get; set; } = new TerrainLuckSettings();
        public ClearingLuckSettings Clearings { get; set; } = new ClearingLuckSettings();
    }

    public class I18NData
    {
        public string ExcaliburDisplayName { get; set; } = "Excalibur";
        public string ExcaliburDescription { get; set; } = "It feels hopeful to wield.";
        public string WoodsObeliskDisplayName { get; set; } = "Woods Obelisk";
        public string WoodsObeliskDescription { get; set; } = "Woods Obelisk Description";
        public string EasterEggDisplayName { get; set; } = "Easter Egg";
    }

    class DeepWoodsStateData
    {
        public HashSet<long> PlayersWhoGotStardropFromUnicorn { get; set; } = new HashSet<long>();
        public int DeepWoodsLevelReached { get; set; } = 0;
    }

    class DeepWoodsSettings
    {
        // Save stuff
        public static DeepWoodsStateData DeepWoodsState { get; set; } = new DeepWoodsStateData();

        // I18N
        public static I18NData I18N { get; set; } = new I18NData();

        // Configurable settings
        public static DeepWoodsSettings Settings { get; set; } = new DeepWoodsSettings();

        // Settings subcategories
        public LevelSettings Level { get; set; } = new LevelSettings();
        public MapSettings Map { get; set; } = new MapSettings();
        public ObjectsSettings Objects { get; set; } = new ObjectsSettings();
        public LuckSettings Luck { get; set; } = new LuckSettings();
        public MonstersSettings Monsters { get; set; } = new MonstersSettings();

        public static void DoSave()
        {
            ModEntry.GetHelper().WriteJsonFile($"{Constants.CurrentSavePath}/{SAVE_FILE_NAME}.json", DeepWoodsState);
        }

        public static void DoLoad()
        {
            DeepWoodsState = ModEntry.GetHelper().ReadJsonFile<DeepWoodsStateData>($"{Constants.CurrentSavePath}/{SAVE_FILE_NAME}.json") ?? new DeepWoodsStateData();
            Settings = ModEntry.GetHelper().ReadConfig<DeepWoodsSettings>() ?? new DeepWoodsSettings();
            I18N = ModEntry.GetHelper().ReadJsonFile<I18NData>("i18n.json") ?? new I18NData();
        }

        public static void InitFromServer(object[] data)
        {
            DeepWoodsState = data[0] as DeepWoodsStateData ?? new DeepWoodsStateData();
            Settings = data[1] as DeepWoodsSettings ?? new DeepWoodsSettings();
        }
    }
}
