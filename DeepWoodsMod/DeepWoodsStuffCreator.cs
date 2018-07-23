using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using static DeepWoodsMod.DeepWoodsRandom;

namespace DeepWoodsMod
{
    class DeepWoodsStuffCreator
    {
        private const int MIN_LEVEL_FOR_METEORITE = 10;
        private const int MIN_LEVEL_FOR_FLOWERS = 10;
        private const int MIN_LEVEL_FOR_FRUITS = 10;
        private const int MIN_LEVEL_FOR_GINGERBREAD_HOUSE = 20;

        private const int MAX_EASTER_EGGS_PER_WOOD = 4;

        private readonly static Probability CHANCE_FOR_GINGERBREAD_HOUSE = new Probability(1);
        private readonly static Probability CHANCE_FOR_RESOURCECLUMP = new Probability(5);
        private readonly static Probability CHANCE_FOR_LARGE_BUSH = new Probability(10);
        private readonly static Probability CHANCE_FOR_MEDIUM_BUSH = new Probability(5);
        private readonly static Probability CHANCE_FOR_SMALL_BUSH = new Probability(5);
        private readonly static Probability CHANCE_FOR_GROWN_TREE = new Probability(25);
        private readonly static Probability CHANCE_FOR_MEDIUM_TREE = new Probability(10);
        private readonly static Probability CHANCE_FOR_SMALL_TREE = new Probability(10);
        private readonly static Probability CHANCE_FOR_GROWN_FRUIT_TREE = new Probability(1);
        private readonly static Probability CHANCE_FOR_SMALL_FRUIT_TREE = new Probability(5);
        private readonly static Probability CHANCE_FOR_WEED = new Probability(20);
        private readonly static Probability CHANCE_FOR_TWIG = new Probability(10);
        private readonly static Probability CHANCE_FOR_STONE = new Probability(10);
        private readonly static Probability CHANCE_FOR_MUSHROOM = new Probability(10);
        private readonly static Probability CHANCE_FOR_FLOWER = new Probability(7);
        private readonly static Probability CHANCE_FOR_FLOWER_IN_WINTER = new Probability(3);

        private readonly static Probability CHANCE_FOR_METEORITE = new Probability(1);
        private readonly static Probability CHANCE_FOR_BOULDER = new Probability(10);
        private readonly static Probability CHANCE_FOR_HOLLOWLOG = new Probability(30);

        private readonly static Probability CHANCE_FOR_FRUIT = new Probability(50);

        private readonly static Probability CHANCE_FOR_FLOWER_ON_LICHTUNG = new Probability(5);

        private readonly static Luck LUCK_FOR_EASTEREGG = new Luck(5, 25, 1000);
        private readonly static Luck LUCK_FOR_MAX_EASTER_EGGS_INCREASE = new Luck(0, 25);
        private readonly static Luck LUCK_FOR_MAX_EASTER_EGGS_NOT_HALFED = new Luck(75, 100);

        // Possibilities for stuff in a Lichtung
        private const int LICHTUNG_STUFF_NOTHING = 0;
        private const int LICHTUNG_STUFF_LAKE = 1;
        private const int LICHTUNG_STUFF_HEALING_FOUNTAIN = 2;
        private const int LICHTUNG_STUFF_GINGERBREAD_HOUSE = 3;
        private const int LICHTUNG_STUFF_TREASURE = 4;
        private const int LICHTUNG_STUFF_IRIDIUM_TREE = 5;
        private const int LICHTUNG_STUFF_UNICORN = 6;
        private const int LICHTUNG_STUFF_EXCALIBUR = 7;

        private readonly static WeightedValue[] LICHTUNG_STUFF = new WeightedValue[]{
            new WeightedValue(LICHTUNG_STUFF_NOTHING, 1500),
            new WeightedValue(LICHTUNG_STUFF_LAKE, 1500 * 1000),
            new WeightedValue(LICHTUNG_STUFF_HEALING_FOUNTAIN, 1000),
            new WeightedValue(LICHTUNG_STUFF_GINGERBREAD_HOUSE, 1000),
            new WeightedValue(LICHTUNG_STUFF_TREASURE, 500),
            new WeightedValue(LICHTUNG_STUFF_IRIDIUM_TREE, 500),
            new WeightedValue(LICHTUNG_STUFF_UNICORN, 250),
            new WeightedValue(LICHTUNG_STUFF_EXCALIBUR, 250)
        };

        private readonly static WeightedValue[] LICHTUNG_PILE_ITEM_IDS_FOR_TREASURE = new WeightedValue[] {
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

        private readonly static WeightedValue[] LICHTUNG_PILE_ITEM_IDS_FOR_TRASH = new WeightedValue[] {
            new WeightedValue(168, 2000),   // Trash
            new WeightedValue(172, 1000),   // Old Newspaper
            new WeightedValue(170, 1000),   // Glasses
            new WeightedValue(171, 500),    // CD
            new WeightedValue(167, 100),    // Joja Cola
            new WeightedValue(122, 5),      // Ancient Dwarf Computer
            new WeightedValue(118, 5),      // Glass Shards
            // new WeightedValue(169, 2000),// Driftwood
        };

        private readonly static Probability CHANCE_FOR_LEWIS_SHORTS_IN_TRASH = new Probability(1, 1000);

        private readonly static Probability CHANCE_FOR_METALBARS_IN_TREASURE = new Probability(10);
        private readonly static Probability CHANCE_FOR_ELIXIRS_IN_TREASURE = new Probability(10);
        private readonly static Probability CHANCE_FOR_GOLDENMASK_IN_TREASURE = new Probability(10);
        private readonly static Probability CHANCE_FOR_DWARF_SCROLL_IN_TREASURE = new Probability(10);
        private readonly static Probability CHANCE_FOR_RING_IN_TREASURE = new Probability(10);

        // If daily luck is negative, this value will be used to determine if a Lichtung has a pile of trash instead of something awesome
        private readonly static Luck LUCK_FOR_LICHTUNG_STUFF_NOT_TRASH = new Luck(0, 100);

        private DeepWoods deepWoods;
        private DeepWoodsRandom random;
        private DeepWoodsSpaceManager spaceManager;
        private DeepWoodsBuilder deepWoodsBuilder;

        private DeepWoodsStuffCreator(DeepWoods deepWoods, DeepWoodsRandom random, DeepWoodsSpaceManager spaceManager, DeepWoodsBuilder deepWoodsBuilder)
        {
            this.deepWoods = deepWoods;
            this.random = random;
            this.spaceManager = spaceManager;
            this.deepWoodsBuilder = deepWoodsBuilder;
        }

        public static void AddStuff(DeepWoods deepWoods, DeepWoodsRandom random, DeepWoodsSpaceManager spaceManager, DeepWoodsBuilder deepWoodsBuilder)
        {
            new DeepWoodsStuffCreator(deepWoods, random, spaceManager, deepWoodsBuilder).ClearAndAddStuff();
        }

        private void ClearAndAddStuff()
        {
            if (!Game1.IsMasterGame)
                return;

            this.random.EnterMasterMode();

            ClearStuff();
            AddStuff();

            this.random.LeaveMasterMode();
        }

        private void ClearStuff()
        {
            deepWoods.resourceClumps.Clear();
            deepWoods.largeTerrainFeatures.Clear();
            deepWoods.terrainFeatures.Clear();
            deepWoods.objects.Clear();
            deepWoods.debris.Clear();
            deepWoods.overlayObjects.Clear();
        }

        private void AddStuff()
        {
            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            // TODO: Add thorny bushes around entrance area.
            // deepWoods.terrainFeatures[new Vector2(10, 10), new ThornyBush(new Vector2(10, 10), deepWoods));

            if (deepWoods.isLichtung)
            {
                // Add something awesome in the lichtung center
                AddSomethingAwesomeForLichtung(new Vector2(deepWoods.lichtungCenter.X, deepWoods.lichtungCenter.Y));
            }

            if (!deepWoods.isLichtung && deepWoods.GetLevel() >= MIN_LEVEL_FOR_GINGERBREAD_HOUSE && this.random.GetChance(CHANCE_FOR_GINGERBREAD_HOUSE))
            {
                // Add a gingerbread house
                deepWoods.resourceClumps.Add(new GingerBreadHouse(new Vector2(mapWidth / 2, mapHeight / 2)));
            }

            // Calculate maximum theoretical amount of terrain features for the current map.
            int maxTerrainFeatures = (mapWidth * mapHeight) / DeepWoodsSpaceManager.MINIMUM_TILES_FOR_TERRAIN_FEATURE;

            int numEasterEggs = 0;
            int maxEasterEggs = 0;
            if (IsEasterEggDay())
            {
                maxEasterEggs = 1 + this.random.GetRandomValue(0, MAX_EASTER_EGGS_PER_WOOD);
                if (this.random.GetLuck(LUCK_FOR_MAX_EASTER_EGGS_INCREASE))
                {
                    maxEasterEggs++;
                }
                if (!this.random.GetLuck(LUCK_FOR_MAX_EASTER_EGGS_NOT_HALFED))
                {
                    maxEasterEggs = Math.Max(1, maxEasterEggs / 2);
                }
            }

            for (int i = 0; i < maxTerrainFeatures; i++)
            {
                int x = this.random.GetRandomValue(1, mapWidth - 1);
                int y = this.random.GetRandomValue(1, mapHeight - 1);
                Vector2 location = new Vector2(x, y);

                // Check if location is free
                if (deepWoods.terrainFeatures.ContainsKey(location) || !deepWoods.isTileLocationTotallyClearAndPlaceable(location))
                    continue;

                // Don't place anything on the bright grass in Lichtungen
                if (deepWoods.isLichtung && DeepWoodsBuilder.IsTileIndexBrightGrass(deepWoods.map.GetLayer("Back").Tiles[x, y]?.TileIndex ?? 0))
                    continue;

                // Don't place anything on water
                if (deepWoods.doesTileHaveProperty(x, y, "Water", "Back") != null)
                    continue;

                if (deepWoods.isLichtung)
                {
                    if (this.random.GetChance(CHANCE_FOR_FLOWER_ON_LICHTUNG))
                    {
                        deepWoods.terrainFeatures[location] = new Flower(GetRandomFlowerType(), location);
                    }
                    else if (IsEasterEggDay() && numEasterEggs < maxEasterEggs && this.random.GetLuck(LUCK_FOR_EASTEREGG))
                    {
                        deepWoods.terrainFeatures[location] = new EasterEgg();
                        numEasterEggs++;
                    }
                    else
                    {
                        deepWoods.terrainFeatures[location] = new LootFreeGrass(GetSeasonGrassType(), this.random.GetRandomValue(1, 3));
                    }
                }
                else
                {
                    if (this.random.GetChance(CHANCE_FOR_RESOURCECLUMP) && IsSpaceFree(location, new Size(2, 2)))
                    {
                        ResourceClump resourceClump = new ResourceClump(GetRandomResourceClumpType(), 2, 2, location);
                        deepWoods.resourceClumps.Add(resourceClump);
                    }
                    else if (this.random.GetChance(CHANCE_FOR_LARGE_BUSH) && IsSpaceFree(location, new Size(3, 1)))
                    {
                        deepWoods.largeTerrainFeatures.Add(new DestroyableBush(location, Bush.largeBush, deepWoods));
                    }
                    else if (this.random.GetChance(CHANCE_FOR_MEDIUM_BUSH) && IsSpaceFree(location, new Size(2, 1)))
                    {
                        deepWoods.largeTerrainFeatures.Add(new DestroyableBush(location, Bush.mediumBush, deepWoods));
                    }
                    else if (this.random.GetChance(CHANCE_FOR_SMALL_BUSH))
                    {
                        deepWoods.largeTerrainFeatures.Add(new DestroyableBush(location, Bush.smallBush, deepWoods));
                    }
                    else if (this.random.GetChance(CHANCE_FOR_GROWN_TREE) && IsRegionTreeFree(location, 1))
                    {
                        deepWoods.terrainFeatures[location] = new Tree(GetRandomTreeType(), Tree.treeStage);
                    }
                    else if (this.random.GetChance(CHANCE_FOR_MEDIUM_TREE))
                    {
                        deepWoods.terrainFeatures[location] = new Tree(GetRandomTreeType(), Tree.bushStage);
                    }
                    else if (this.random.GetChance(CHANCE_FOR_SMALL_TREE))
                    {
                        deepWoods.terrainFeatures[location] = new Tree(GetRandomTreeType(), this.random.GetRandomValue(Tree.sproutStage, Tree.saplingStage));
                    }
                    else if (this.random.GetChance(CHANCE_FOR_GROWN_FRUIT_TREE) && IsRegionTreeFree(location, 2))
                    {
                        int numFruits = 0;
                        if (deepWoods.GetLevel() >= MIN_LEVEL_FOR_FRUITS && this.random.GetChance(CHANCE_FOR_FRUIT))
                        {
                            numFruits = this.random.GetRandomValue(new int[] { 1, 2, 3 }, Probability.FIFTY_FIFTY);
                        }
                        AddFruitTree(location, FruitTree.treeStage, numFruits);
                    }
                    else if (this.random.GetChance(CHANCE_FOR_SMALL_FRUIT_TREE))
                    {
                        AddFruitTree(location, FruitTree.bushStage);
                    }
                    else if (this.random.GetChance(CHANCE_FOR_WEED))
                    {
                        deepWoods.objects[location] = new StardewValley.Object(location, GetRandomWeedType(), 1);
                    }
                    else if (this.random.GetChance(CHANCE_FOR_TWIG))
                    {
                        deepWoods.objects[location] = new StardewValley.Object(location, GetRandomTwigType(), 1);
                    }
                    else if (this.random.GetChance(CHANCE_FOR_STONE))
                    {
                        deepWoods.objects[location] = new StardewValley.Object(location, GetRandomStoneType(), 1);
                    }
                    else if (this.random.GetChance(CHANCE_FOR_MUSHROOM))
                    {
                        deepWoods.objects[location] = new StardewValley.Object(location, GetRandomMushroomType(), 1) { IsSpawnedObject = true };
                    }
                    else if (deepWoods.GetLevel() >= MIN_LEVEL_FOR_FLOWERS && this.random.GetChance(Game1.currentSeason == "winter" ? CHANCE_FOR_FLOWER_IN_WINTER : CHANCE_FOR_FLOWER))
                    {
                        deepWoods.terrainFeatures[location] =new Flower(GetRandomFlowerType(), location);
                    }
                    else if (IsEasterEggDay() && numEasterEggs < maxEasterEggs && this.random.GetLuck(LUCK_FOR_EASTEREGG))
                    {
                        deepWoods.terrainFeatures[location] = new EasterEgg();
                        numEasterEggs++;
                    }
                    else
                    {
                        deepWoods.terrainFeatures[location] = new LootFreeGrass(GetSeasonGrassType(), this.random.GetRandomValue(1, 3));
                    }
                }
            }

            // Fill up with grass (if not a Lichtung)
            if (!deepWoods.isLichtung)
            {
                int maxGrass = maxTerrainFeatures * 2;
                if (Game1.currentSeason == "winter")
                {
                    // Leaveless trees and snow ground make winter forest look super empty and open,
                    // so we fill it with plenty of icy grass to give it a better atmosphere.
                    maxGrass *= 2;
                }
                for (int i = 0; i < maxGrass; i++)
                {
                    Vector2 location = new Vector2(this.random.GetRandomValue(1, mapWidth - 1), this.random.GetRandomValue(1, mapHeight - 1));
                    if (deepWoods.terrainFeatures.ContainsKey(location) || !deepWoods.isTileLocationTotallyClearAndPlaceable(location))
                        continue;

                    deepWoods.terrainFeatures[location] = new LootFreeGrass(GetSeasonGrassType(), this.random.GetRandomValue(1, 3));
                }
            }
        }

        private void AddSomethingAwesomeForLichtung(Vector2 location)
        {
            if (true || this.random.GetLuck(LUCK_FOR_LICHTUNG_STUFF_NOT_TRASH, deepWoods.GetLuckLevel()))
            {
                switch (this.random.GetRandomValue(LICHTUNG_STUFF))
                {
                    case LICHTUNG_STUFF_LAKE:
                        this.deepWoodsBuilder.AddLakeToLichtung();
                        break;
                    case LICHTUNG_STUFF_TREASURE:
                        deepWoods.objects[location] = new TreasureChest(location, CreateRandomTreasureChestItems());
                        AddLichtungStuffPile(location, LICHTUNG_PILE_ITEM_IDS_FOR_TREASURE);
                        break;
                    case LICHTUNG_STUFF_GINGERBREAD_HOUSE:
                        deepWoods.resourceClumps.Add(new GingerBreadHouse(location - new Vector2(2, 4)));
                        AddGingerBreadHouseDeco(location - new Vector2(2, 4));
                        break;
                    case LICHTUNG_STUFF_HEALING_FOUNTAIN:
                        deepWoods.largeTerrainFeatures.Add(new HealingFountain(location - new Vector2(2, 0)));
                        AddRipeFruitTreesAroundFountain(location - new Vector2(2, 2));
                        if (Game1.currentSeason == "winter")
                            AddWinterFruitsAroundFountain(location - new Vector2(2, 2));
                        break;
                    case LICHTUNG_STUFF_IRIDIUM_TREE:
                        deepWoods.resourceClumps.Add(new IridiumTree(location));
                        AddIridiumNodesAroundTree(location);
                        Game1.currentLightSources.Add(new LightSource(LightSource.sconceLight, location, 6, new Color(1f, 0f, 0f)));
                        break;
                    case LICHTUNG_STUFF_UNICORN:
                        if (!Game1.isRaining)
                        {
                            deepWoods.characters.Add(new Unicorn(location));
                        }
                        break;
                    case LICHTUNG_STUFF_EXCALIBUR:
                        deepWoods.largeTerrainFeatures.Add(new ExcaliburStone(location));
                        break;
                    case LICHTUNG_STUFF_NOTHING:
                    default:
                        break;
                }
            }
            else
            {
                // Bad luck: Pile of trash
                deepWoods.objects[location] = new TreasureChest(location, CreateRandomTrashCanItems(), true);
                AddLichtungStuffPile(location, LICHTUNG_PILE_ITEM_IDS_FOR_TRASH);
            }
        }

        private Location GetTreasurePileOffset()
        {
            int y = this.random.GetRandomValue(0, 2) + this.random.GetRandomValue(0, 2);
            int x;
            if (y == 0)
                x = this.random.GetRandomValue(0, 2) == 1 ? -1 : 1;
            else if (y == 1)
                x = this.random.GetRandomValue(-2, 3);
            else
                x = this.random.GetRandomValue(-1, 2);
            return new Location(x, y);
        }

        private struct OffsetVariation
        {
            public readonly int minX;
            public readonly int maxX;
            public readonly int minY;
            public readonly int maxY;
            public OffsetVariation(int minX, int maxX, int minY, int maxY)
            {
                this.minX = minX;
                this.maxX = maxX;
                this.minY = minY;
                this.maxY = maxY;
            }
        }

        private OffsetVariation GetTreasurePileOffsetVariation(Location offset)
        {
            if (offset.X == -1 && offset.Y <= 1)
                return new OffsetVariation(-32, 0, -32, 32);

            if (offset.X == 1 && offset.Y <= 1)
                return new OffsetVariation(0, 32, -32, 32);

            if (offset.X == 0 && offset.Y == 1)
                return new OffsetVariation(-32, 32, 0, 32);

            return new OffsetVariation(-32, 32, -32, 32);
        }

        private void AddLichtungStuffPile(Vector2 location, WeightedValue[] itemIds)
        {
            int x = (int)location.X;
            int y = (int)location.Y;
            int numStuff = this.random.GetRandomValue(32, 128);
            for (int i = 0; i < numStuff; i++)
            {
                Location offset = GetTreasurePileOffset();
                OffsetVariation offsetVariation = GetTreasurePileOffsetVariation(offset);
                int debrisX = (x + offset.X) * 64 + this.random.GetRandomValue(offsetVariation.minX, offsetVariation.maxX);
                int debrisY = (x + offset.Y) * 64 + this.random.GetRandomValue(offsetVariation.minY, offsetVariation.maxY);

                Vector2 debrisLocation = new Vector2(debrisX, debrisY);

                Debris debris = new Debris(this.random.GetRandomValue(itemIds), debrisLocation, debrisLocation) {
                    chunkFinalYLevel = debrisY,
                    chunkFinalYTarget = debrisY,
                    movingFinalYLevel = false
                };

                for (int j = 0; j < 100; j++)
                {
                    debris.timeSinceDoneBouncing += 100;
                    debris.updateChunks(Game1.currentGameTime, deepWoods);
                }

                foreach (Chunk chunk in debris.Chunks)
                {
                    chunk.xVelocity.Value = 0;
                    chunk.yVelocity.Value = 0;
                    chunk.rotationVelocity = 0;
                    chunk.position.X = debrisX;
                    chunk.position.Y = debrisY;
                    chunk.hasPassedRestingLineOnce.Value = true;
                    chunk.bounces = 3;
                }

                deepWoods.debris.Add(debris);
            }
        }

        private void AddGingerBreadHouseDeco(Vector2 location)
        {
            for (int y = 1; y <= 5; y++)
            {
                Vector2 leftPos = new Vector2(location.X - 2, location.Y + y);
                Vector2 rightPos = new Vector2(location.X + 6, location.Y + y);
                if (y == 1 || y == 5)
                {
                    int id = this.random.GetRandomValue(new int[] { 40, 44 });  // Big Green Cane or Big Red Cane
                    deepWoods.objects[leftPos] = new StardewValley.Object(leftPos, id) { Flipped = true };
                    deepWoods.objects[rightPos] = new StardewValley.Object(rightPos, id) { Flipped = false };
                }
                else
                {
                    deepWoods.objects[leftPos] = new StardewValley.Object(leftPos, GetRandomSmallCaneType()) { Flipped = this.random.GetChance(Probability.FIFTY_FIFTY) };
                    deepWoods.objects[rightPos] = new StardewValley.Object(rightPos, GetRandomSmallCaneType()) { Flipped = this.random.GetChance(Probability.FIFTY_FIFTY) };
                }
                if (y >= 3)
                {
                    Vector2 centerPos = new Vector2(location.X + 2, location.Y + y);
                    deepWoods.objects[centerPos] = new StardewValley.Object(centerPos, 409, 1) { Flipped = this.random.GetChance(Probability.FIFTY_FIFTY) }; // Crystal Floor
                }
            }

            for (int x = -1; x <= 5; x++)
            {
                if (x != 2)
                {
                    Vector2 pos = new Vector2(location.X + x, location.Y + 5);
                    deepWoods.terrainFeatures[pos] = new Flower(431, pos);   // Sunflower
                }
                if (x == -1 || x == 5)
                {
                    Vector2 pos = new Vector2(location.X + x, location.Y + 1);
                    deepWoods.terrainFeatures[pos] = new Flower(431, pos);   // Sunflower
                }
            }
        }

        private int GetRandomSmallCaneType()
        {
            // Green Canes, Mixed Canes or Red Canes
            return this.random.GetRandomValue(41, 43+1);
        }

        private void AddIridiumNodesAroundTree(Vector2 location)
        {
            int numIridiumNodes = 5;
            for (int dist = 1; numIridiumNodes > 0; dist++, numIridiumNodes /= 2)
            {
                for (int i = 0; i < numIridiumNodes; i++)
                {
                    int nodeX, nodeY;
                    do
                    {
                        nodeX = this.random.GetRandomValue(-dist, dist + 2);
                        nodeY = this.random.GetRandomValue(-dist, dist + 1);
                    }
                    while ((nodeX == 0 || nodeX == 1) && nodeY == 0);

                    Vector2 nodeLocation = new Vector2(location.X + nodeX, location.Y + nodeY);

                    if (deepWoods.objects.ContainsKey(nodeLocation))
                        continue;

                    deepWoods.objects[nodeLocation] = new StardewValley.Object(nodeLocation, 765, "Stone", true, false, false, false) { MinutesUntilReady = 16 };
                }
            }
        }

        private void AddWinterFruitsAroundFountain(Vector2 location)
        {
            int x = (int)location.X;
            int y = (int)location.Y;
            xTile.Dimensions.Rectangle fountainRectangle = new xTile.Dimensions.Rectangle(x, y, 6, 5);
            int minX = x - 6;
            int maxX = x + 9;
            int minY = y - 6;
            int maxY = y + 9;
            int numWinterFruits = this.random.GetRandomValue(9, 16);
            for (int i = 0; i < numWinterFruits; i++)
            {
                int fruitX = this.random.GetRandomValue(minX, maxX);
                int fruitY = this.random.GetRandomValue(minX, maxX);
                if (fountainRectangle.Contains(new Location(fruitX, fruitY)))
                    continue;

                Vector2 fruitLocation = new Vector2(fruitX, fruitY);

                if (deepWoods.objects.ContainsKey(fruitLocation))
                    continue;

                deepWoods.objects[fruitLocation] = new StardewValley.Object(fruitLocation, 414, 1);
            }
        }

        private void AddRipeFruitTreesAroundFountain(Vector2 location)
        {
            AddRipeFruitTree(location + new Vector2(-3, -1));
            AddRipeFruitTree(location + new Vector2(-3, 6));
            AddRipeFruitTree(location + new Vector2(8, -1));
            AddRipeFruitTree(location + new Vector2(8, 6));
        }

        private void AddRipeFruitTree(Vector2 location)
        {
            AddFruitTree(location, FruitTree.treeStage, 3);
        }

        private void AddFruitTree(Vector2 location, int growthStage, int fruitsOnTree = 0)
        {
            FruitTree fruitTree = new FruitTree(GetRandomFruitTreeType(), FruitTree.treeStage);
            fruitTree.fruitsOnTree.Value = Game1.currentSeason == "winter" ? 0 : fruitsOnTree;
            fruitTree.daysUntilMature.Value = 28 - (growthStage * 7);
            deepWoods.terrainFeatures[location] = fruitTree;
        }

        private List<Item> CreateRandomTreasureChestItems()
        {
            List<Item> items = new List<Item>();
            int numItems = this.random.GetRandomValue(new Luck(0, 3), new Luck(2, 9), deepWoods.GetLuckLevel());
            bool goldenMaskInTreasure = false;
            bool dwarfScrollInTreasure = false;
            bool ringInTreasure = false;
            for (int i = 0; i < numItems; i++)
            {
                int id;
                int stack = 1;
                
                if (this.random.GetChance(CHANCE_FOR_METALBARS_IN_TREASURE))
                {
                    id = this.random.GetRandomValue(334, 338 + 1);                            // Metal bars
                    stack = this.random.GetRandomValue(new Luck(1, 2), new Luck(2, 7), deepWoods.GetLuckLevel());
                }
                else if (this.random.GetChance(CHANCE_FOR_ELIXIRS_IN_TREASURE))
                {
                    id = this.random.GetRandomValue(new int[] { 772, 773 });                // Elixirs
                    stack = this.random.GetRandomValue(new Luck(1, 2), new Luck(2, 7), deepWoods.GetLuckLevel());
                }
                else if (!goldenMaskInTreasure && this.random.GetChance(CHANCE_FOR_GOLDENMASK_IN_TREASURE))
                {
                    id = 124;                                           // Golden Mask (Artefact)
                    goldenMaskInTreasure = true;
                }
                else if (!dwarfScrollInTreasure && this.random.GetChance(CHANCE_FOR_DWARF_SCROLL_IN_TREASURE))
                {
                    id = this.random.GetRandomValue(96, 99 + 1);        // Dwarf Scrolls
                    dwarfScrollInTreasure = true;
                }
                else if (!ringInTreasure && this.random.GetChance(CHANCE_FOR_RING_IN_TREASURE))
                {
                    id = this.random.GetRandomValue(516, 534 + 1);      // Rings
                    ringInTreasure = true;
                }
                else
                {
                    id = this.random.GetRandomValue(LICHTUNG_PILE_ITEM_IDS_FOR_TREASURE);   // Treasure
                    stack = this.random.GetRandomValue(new Luck(1, 2), new Luck(2, 7), deepWoods.GetLuckLevel());
                }

                items.Add(new StardewValley.Object(id, stack));
            }
            return items;
        }

        private List<Item> CreateRandomTrashCanItems()
        {
            List<Item> items = new List<Item>();
            int numItems = this.random.GetRandomValue(new Luck(0, 3), new Luck(2, 9), deepWoods.GetLuckLevel());
            bool lewisShortsInTrash = false;
            for (int i = 0; i < numItems; i++)
            {
                int id;
                int stack = 1;
                if (!lewisShortsInTrash && this.random.GetChance(CHANCE_FOR_LEWIS_SHORTS_IN_TRASH))
                {
                    id = 789;   // Lucky Purple Shorts
                    lewisShortsInTrash = true;
                }
                else
                {
                    switch (this.random.GetRandomValue(0, 4))
                    {
                        case 0:
                            id = this.random.GetRandomValue(579, 585 + 1);                          // Bones
                            break;
                        case 1:
                            id = this.random.GetRandomValue(new int[] { 111, 112, 113, 115 });      // Artefacts
                            break;
                        case 2:
                            id = this.random.GetRandomValue(new int[] { 103, 126, 127 });           // Puppets
                            break;
                        case 3:
                        default:
                            id = this.random.GetRandomValue(LICHTUNG_PILE_ITEM_IDS_FOR_TRASH);      // Trash
                            stack = this.random.GetRandomValue(new Luck(1, 1), new Luck(2, 7), deepWoods.GetLuckLevel());
                            break;
                    }
                }

                items.Add(new StardewValley.Object(id, stack));
            }
            return items;
        }

        private bool IsEasterEggDay()
        {
            return Game1.currentSeason == "spring" && (Game1.dayOfMonth == 13 || Game1.dayOfMonth == 14);
        }

        private int GetRandomFlowerType()
        {
            // 427, // 591, // Tulip, spring (30g)
            // 429, // 597, // BlueJazz, spring (50g)
            // 455, // 593, // SummerSpangle, summer (90g)
            // 453, // 376, // Poppy, summer (140g)
            // 425, // 595, // FairyRose, fall (290g)

            if (Game1.currentSeason == "winter")
            {
                return this.random.GetRandomValue(new WeightedValue[] {
                    new WeightedValue(429, 290),  // 597, // BlueJazz, spring (50g)
                    new WeightedValue(425, 50),   // 595, // FairyRose, fall (290g)
                });
            }
            else
            {
                // TODO: Use season, sell price and player luck for probability
                return this.random.GetRandomValue(new WeightedValue[] {
                    new WeightedValue(427, 290), // 591, // Tulip, spring (30g)
                    new WeightedValue(429, 140), // 597, // BlueJazz, spring (50g)
                    new WeightedValue(455, 90),  // 593, // SummerSpangle, summer (90g)
                    new WeightedValue(453, 50),  // 376, // Poppy, summer (140g)
                    new WeightedValue(425, 30),  // 595, // FairyRose, fall (290g)
                });
            }
        }

        private bool IsSpaceFree(Vector2 location, Size resourceClumpSize)
        {
            for (int x = 0; x < resourceClumpSize.Width; x++)
            {
                for (int y = 0; y < resourceClumpSize.Height; y++)
                {
                    Vector2 check = new Vector2(location.X + x, location.Y + y);
                    if (deepWoods.terrainFeatures.ContainsKey(check)
                        || !deepWoods.isTileLocationTotallyClearAndPlaceable(check))
                        return false;
                }
            }
            return true;
        }

        private bool TileHasTree(Vector2 location)
        {
            return deepWoods.terrainFeatures.ContainsKey(location)
                && (deepWoods.terrainFeatures[location] is FruitTree
                    || ((deepWoods.terrainFeatures[location] as Tree)?.growthStage ?? 0) >= Tree.treeStage
                );
        }

        private bool IsRegionTreeFree(Vector2 location, int radius)
        {
            for (int x = -radius; x < radius*2; x++)
            {
                for (int y = -radius; y < radius * 2; y++)
                {
                    if (TileHasTree(new Vector2(location.X + x, location.Y + y)))
                        return false;
                }
            }
            return true;
        }

        private int GetSeasonGrassType()
        {
            return Game1.currentSeason == "winter" ? Grass.frostGrass : Grass.springGrass;
        }

        private int GetRandomWeedType()
        {
            return GameLocation.getWeedForSeason(this.random.GetRandom(), Game1.currentSeason);
        }

        private int GetRandomStoneType()
        {
            return this.random.GetRandomValue(new int[] { 343, 668, 670 });
        }

        private int GetRandomTwigType()
        {
            return this.random.GetRandomValue(new int[] { 294, 295 });
        }

        private int GetRandomTreeType()
        {
            return this.random.GetRandomValue(new int[] { Tree.bushyTree, Tree.leafyTree, Tree.pineTree });
        }

        private int GetRandomMushroomType()
        {
            return this.random.GetRandomValue(new WeightedValue[] {
                new WeightedValue(422, 1),  // Purple one
                new WeightedValue(420, 5),  // Red one
                new WeightedValue(257, 10), // Morel
                new WeightedValue(281, 20), // Big brown one
                new WeightedValue(404, 50), // Normal one
            });
        }

        private int GetRandomFruitTreeType()
        {
            Dictionary<int, string> fruitTrees = Game1.content.Load<Dictionary<int, string>>("Data\\fruitTrees");
            int[] fruitTreeTypes = fruitTrees.Keys.ToArray();
            Array.Sort(fruitTreeTypes);
            return this.random.GetRandomValue(fruitTreeTypes);
        }

        private int GetRandomResourceClumpType()
        {
            if (deepWoods.GetLevel() >= MIN_LEVEL_FOR_METEORITE && this.random.GetChance(CHANCE_FOR_METEORITE))
            {
                return ResourceClump.meteoriteIndex;
            }
            else if (this.random.GetChance(CHANCE_FOR_BOULDER))
            {
                return this.random.GetChance(Probability.FIFTY_FIFTY) ? ResourceClump.mineRock1Index : ResourceClump.mineRock2Index;
            }
            else if (this.random.GetChance(CHANCE_FOR_HOLLOWLOG))
            {
                return ResourceClump.hollowLogIndex;
            }
            else
            {
                return ResourceClump.stumpIndex;
            }
            /*
            return ResourceClump.mineRock1Index;
            return ResourceClump.mineRock2Index;
            return ResourceClump.mineRock3Index;
            return ResourceClump.mineRock4Index;
            */
        }
    }
}
