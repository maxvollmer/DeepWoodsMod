﻿using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using static DeepWoodsMod.DeepWoodsRandom;
using static DeepWoodsMod.DeepWoodsSettings;
using DeepWoodsMod.API.Impl;
using DeepWoodsMod.Stuff;

namespace DeepWoodsMod
{
    class DeepWoodsStuffCreator
    {
        private DeepWoods deepWoods;
        private DeepWoodsRandom random;
        private DeepWoodsSpaceManager spaceManager;

        private DeepWoodsStuffCreator(DeepWoods deepWoods, DeepWoodsRandom random, DeepWoodsSpaceManager spaceManager)
        {
            this.deepWoods = deepWoods;
            this.random = random;
            this.spaceManager = spaceManager;
        }

        public static void AddStuff(DeepWoods deepWoods, DeepWoodsRandom random, HashSet<Location> blockedLocations)
        {
            DeepWoodsSpaceManager spaceManager = new DeepWoodsSpaceManager(deepWoods.mapWidth.Value, deepWoods.mapHeight.Value);
            new DeepWoodsStuffCreator(deepWoods, random, spaceManager).ClearAndAddStuff(blockedLocations);
        }

        public static void Infest(DeepWoods deepWoods, DeepWoodsRandom random, HashSet<Location> blockedLocations)
        {
            DeepWoodsSpaceManager spaceManager = new DeepWoodsSpaceManager(deepWoods.mapWidth.Value, deepWoods.mapHeight.Value);
            new DeepWoodsStuffCreator(deepWoods, random, spaceManager).Infest(blockedLocations);
        }

        public static void FillFirstLevel(DeepWoods deepWoods, DeepWoodsRandom random, HashSet<Location> blockedLocations)
        {
            DeepWoodsSpaceManager spaceManager = new DeepWoodsSpaceManager(deepWoods.mapWidth.Value, deepWoods.mapHeight.Value);
            new DeepWoodsStuffCreator(deepWoods, random, spaceManager).FillFirstLevel(blockedLocations);
        }

        public static void ClearAndGiftInfestedLevel(DeepWoods deepWoods, DeepWoodsRandom random)
        {
            DeepWoodsSpaceManager spaceManager = new DeepWoodsSpaceManager(deepWoods.mapWidth.Value, deepWoods.mapHeight.Value);
            new DeepWoodsStuffCreator(deepWoods, random, spaceManager).ClearAndGiftInfestedLevel();
        }

        private void ClearAndAddStuff(HashSet<Location> blockedLocations)
        {
            if (!Game1.IsMasterGame)
                return;

            ClearStuff();
            AddStuff(blockedLocations);

            if (Settings.Performance.GrassDensity < 100)
            {
                var terrainFeatures = deepWoods.terrainFeatures;
                var terrainFeatureLocations = new List<Vector2>(terrainFeatures.Keys);
                foreach (var location in terrainFeatureLocations)
                {
                    if (terrainFeatures[location] is Grass && (Settings.Performance.GrassDensity < random.GetRandomValue(1, 100)))
                    {
                        terrainFeatures.Remove(location);
                    }
                }
            }
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

        private void AddCuteSignToExit(HashSet<Location> blockedLocations, Location location, DeepWoodsEnterExit.ExitDirection direction)
        {
            Vector2 exitLocation = new Vector2(location.X, location.Y);
            Vector2 xDir = new Vector2();
            Vector2 yDir = new Vector2();
            switch (direction)
            {
                case DeepWoodsEnterExit.ExitDirection.BOTTOM:
                    xDir = new Vector2(-1, 0);
                    yDir = new Vector2(0, -1);
                    break;
                case DeepWoodsEnterExit.ExitDirection.TOP:
                    xDir = new Vector2(-1, 0);
                    yDir = new Vector2(0, 1);
                    break;
                case DeepWoodsEnterExit.ExitDirection.LEFT:
                    xDir = new Vector2(1, 0);
                    yDir = new Vector2(0, -1);
                    break;
                case DeepWoodsEnterExit.ExitDirection.RIGHT:
                    xDir = new Vector2(-1, 0);
                    yDir = new Vector2(0, -1);
                    break;
            }

            Vector2 cuteSignLocation = exitLocation + (xDir * Settings.Map.ExitRadius) + (yDir * Settings.Map.ExitRadius);
            deepWoods.largeTerrainFeatures.Add(new CuteSign(cuteSignLocation));
            deepWoods.AddLightSource(cuteSignLocation);

            blockedLocations.Add(new Location((int)cuteSignLocation.X, (int)cuteSignLocation.Y));
        }

        private void AddThornyBushesToExit(HashSet<Location> blockedLocations, DeepWoodsEnterExit.DeepWoodsExit exit)
        {
            Vector2 exitLocation = new Vector2(exit.Location.X, exit.Location.Y);
            Vector2 xDir = new Vector2();
            Vector2 yDir = new Vector2();
            switch (exit.ExitDir)
            {
                case DeepWoodsEnterExit.ExitDirection.BOTTOM:
                    xDir = new Vector2(1, 0);
                    yDir = new Vector2(0, -1);
                    break;
                case DeepWoodsEnterExit.ExitDirection.TOP:
                    xDir = new Vector2(1, 0);
                    yDir = new Vector2(0, 1);
                    break;
                case DeepWoodsEnterExit.ExitDirection.LEFT:
                    xDir = new Vector2(0, 1);
                    yDir = new Vector2(1, 0);
                    break;
                case DeepWoodsEnterExit.ExitDirection.RIGHT:
                    xDir = new Vector2(0, 1);
                    yDir = new Vector2(-1, 0);
                    break;
            }

            for (int x = -Settings.Map.ExitRadius; x <= Settings.Map.ExitRadius; x++)
            {
                for (int y = 0; y < Settings.Map.ExitLength-1; y++)
                {
                    if (y == 0 ||
                        (this.random.CheckChance(new Chance(Settings.Map.ExitLength - y, Settings.Map.ExitLength))
                        && this.random.CheckChance(new Chance(1 + Settings.Map.ExitRadius - Math.Abs(x), Settings.Map.ExitRadius))))
                    {
                        Vector2 thornyBushLocation = exitLocation + (xDir * x) + (yDir * y);
                        if (y == 0 || IsTileFree(blockedLocations, thornyBushLocation))
                        {
                            deepWoods.terrainFeatures[thornyBushLocation] = new ThornyBush(thornyBushLocation, deepWoods);
                            blockedLocations.Add(new Location((int)thornyBushLocation.X, (int)thornyBushLocation.Y));
                        }
                    }
                }
            }
        }

        private void AddStuff(HashSet<Location> blockedLocations)
        {
            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            /*
            if (!deepWoods.IsClearing)
            {
                // Add cute signs (we do this before the thorny bushes, so signs can spawn inside an overgrown exit :3
                foreach (var exit in deepWoods.exits)
                {
                    if (this.random.CheckChance(Settings.Level.ChanceForSignOnExit))
                    {
                        AddCuteSignToExit(blockedLocations, exit.Location, exit.ExitDir);
                    }
                }
                if (this.random.CheckChance(Settings.Level.ChanceForSignOnExit))
                {
                    AddCuteSignToExit(blockedLocations, deepWoods.EnterLocation, DeepWoodsEnterExit.CastEnterDirToExitDir(deepWoods.EnterDir));
                }
            }
            */

            // Add thorny bushes around exit areas.
            if (!deepWoods.isLichtung.Value && deepWoods.level.Value > Settings.Level.MinLevelForThornyBushes)
            {
                foreach (var exit in deepWoods.exits)
                {
                    if (this.random.CheckChance(Settings.Level.ChanceForThornyBushesOnExit))
                    {
                        AddThornyBushesToExit(blockedLocations, exit);
                    }
                }
            }

            if (deepWoods.IsClearing)
            {
                // Add something awesome in the lichtung center
                AddSomethingAwesomeForLichtung(new Vector2(deepWoods.lichtungCenter.X, deepWoods.lichtungCenter.Y));
            }

            if (!deepWoods.IsClearing && deepWoods.level.Value >= Settings.Level.MinLevelForGingerbreadHouse && this.random.CheckChance(Settings.Luck.Terrain.ChanceForGingerbreadHouse))
            {
                // Add a gingerbread house
                deepWoods.resourceClumps.Add(new GingerBreadHouse(new Vector2(mapWidth / 2, mapHeight / 2)));
            }

            List<int> allTilesInRandomOrder = Enumerable.Range(0, mapWidth * mapHeight).OrderBy(n => Game1.random.Next()).ToList();

            // Calculate maximum theoretical amount of terrain features for the current map.
            int maxTerrainFeatures = (mapWidth * mapHeight) / Math.Max(1, DeepWoodsSettings.Settings.Generation.MinTilesForTerrainFeature);

            int i = 0;
            for (; i < maxTerrainFeatures; i++)
            {
                int tileIndex = allTilesInRandomOrder[i];
                int x = tileIndex % mapWidth;
                int y = tileIndex / mapWidth;

                // the previous location either was filled or was already full,
                // add it to blockedLocations
                if (i > 0)
                {
                    int prevTileIndex = allTilesInRandomOrder[i - 1];
                    int prevX = prevTileIndex % mapWidth;
                    int prevY = prevTileIndex / mapWidth;
                    blockedLocations.Add(new Location(prevX, prevY));
                }

                Vector2 location = new Vector2(x, y);

                // Don't place anything here if tile is blocked
                if (!IsTileFree(blockedLocations, location))
                    continue;

                // Don't place anything on the bright grass in Lichtungen
                if (deepWoods.IsClearing && DeepWoodsBuilder.IsTileIndexBrightGrass(deepWoods.map.GetLayer("Back").Tiles[x, y]?.TileIndex ?? 0))
                    continue;

                if (deepWoods.IsClearing)
                {
                    if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForFlowerOnClearing))
                    {
                        deepWoods.terrainFeatures[location] = new Flower(GetRandomFlowerType(), deepWoods, location);
                    }
                    else
                    {
                        AddModOrGrass(location);
                    }
                }
                else
                {
                    if (deepWoods.level.Value >= Settings.Level.MinLevelForMeteorite && this.random.CheckChance(Settings.Luck.Terrain.ResourceClump.ChanceForMeteorite) && CheckAndBlockSpace(blockedLocations, location, new Size(2, 2)))
                    {
                        deepWoods.resourceClumps.Add(new ExplodableResourceClump(ResourceClump.meteoriteIndex, 2, 2, location));
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ResourceClump.ChanceForBoulder) && CheckAndBlockSpace(blockedLocations, location, new Size(2, 2)))
                    {
                        deepWoods.resourceClumps.Add(new ExplodableResourceClump(this.random.CheckChance(Chance.FIFTY_FIFTY) ? ResourceClump.mineRock1Index : ResourceClump.mineRock2Index, 2, 2, location));
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ResourceClump.ChanceForHollowLog) && CheckAndBlockSpace(blockedLocations, location, new Size(2, 2)))
                    {
                        deepWoods.resourceClumps.Add(new ExplodableResourceClump(ResourceClump.hollowLogIndex, 2, 2, location));
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ResourceClump.ChanceForStump) && CheckAndBlockSpace(blockedLocations, location, new Size(2, 2)))
                    {
                        deepWoods.resourceClumps.Add(new ExplodableResourceClump(ResourceClump.stumpIndex, 2, 2, location));
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForLargeBush) && CheckAndBlockSpace(blockedLocations, location, new Size(3, 1)))
                    {
                        deepWoods.largeTerrainFeatures.Add(new DestroyableBush(location, Bush.largeBush, deepWoods));
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForMediumBush) && CheckAndBlockSpace(blockedLocations, location, new Size(2, 1)))
                    {
                        deepWoods.largeTerrainFeatures.Add(new DestroyableBush(location, Bush.mediumBush, deepWoods));
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForSmallBush))
                    {
                        deepWoods.largeTerrainFeatures.Add(new DestroyableBush(location, Bush.smallBush, deepWoods));
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForGrownTree) && IsRegionTreeFree(location, 1))
                    {
                        deepWoods.terrainFeatures[location] = new Tree(GetRandomTreeType(), Tree.treeStage);
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForMediumTree))
                    {
                        deepWoods.terrainFeatures[location] = new Tree(GetRandomTreeType(), Tree.bushStage);
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForSmallTree))
                    {
                        deepWoods.terrainFeatures[location] = new Tree(GetRandomTreeType(), this.random.GetRandomValue(Tree.sproutStage, Tree.saplingStage));
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForGrownFruitTree) && IsRegionTreeFree(location, 2))
                    {
                        int numFruits = 0;
                        if (deepWoods.level.Value >= Settings.Level.MinLevelForFruits)
                        {
                            numFruits = this.random.GetRandomValue(Settings.Luck.Terrain.FruitCount);
                        }
                        AddFruitTree(location, FruitTree.treeStage, numFruits);
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForSmallFruitTree))
                    {
                        AddFruitTree(location, FruitTree.bushStage);
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForWeed))
                    {
                        deepWoods.objects[location] = CreateObject(GetRandomWeedType(), false);
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForTwig))
                    {
                        deepWoods.objects[location] = CreateObject(GetRandomTwigType(), false);
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForStone))
                    {
                        deepWoods.objects[location] = CreateObject(GetRandomStoneType(), false);
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForMushroom))
                    {
                        deepWoods.objects[location] = CreateObject(GetRandomMushroomType(), true);
                    }
                    else if (deepWoods.level.Value >= Settings.Level.MinLevelForFlowers && this.random.CheckChance(Game1.currentSeason == "winter" ? Settings.Luck.Terrain.ChanceForFlowerInWinter : Settings.Luck.Terrain.ChanceForFlower))
                    {
                        deepWoods.terrainFeatures[location] = new Flower(GetRandomFlowerType(), deepWoods, location);
                    }
                    else if (this.random.CheckChance(Settings.Luck.Terrain.ChanceForExtraForageable))
                    {
                        deepWoods.objects[location] = CreateObject(GetRandomForageable(), true);
                    }
                    else
                    {
                        AddModOrGrass(location);
                    }
                }
            }

            // Fill up with grass (if not a Lichtung)
            if (!deepWoods.IsClearing)
            {
                int maxGrass = (allTilesInRandomOrder.Count() - maxTerrainFeatures) / 3;
                if (Game1.currentSeason == "winter")
                {
                    // Leaveless trees and snow ground make winter forest look super empty and open,
                    // so we fill it with plenty of icy grass to give it a better atmosphere.
                    maxGrass *= 2;
                }
                for (int j = 0; j < maxGrass; j++, i++)
                {
                    int tileIndex = allTilesInRandomOrder[i];
                    int x = tileIndex % mapWidth;
                    int y = tileIndex / mapWidth;

                    Vector2 location = new Vector2(x, y);

                    if (blockedLocations.Contains(new Location(x, y)))
                        continue;

                    deepWoods.terrainFeatures[location] = new LootFreeGrass(GetSeasonGrassType(), this.random.GetRandomValue(1, 3));
                }
            }
        }

        private StardewValley.Object CreateObject(string id, bool canBePickedUp)
        {
            return new StardewValley.Object(id, 1, false, -1, GetRandomItemQuality())
            {
                IsSpawnedObject = canBePickedUp,
                CanBeSetDown = false,
                CanBeGrabbed = false,
                //TileLocation = tileLocation,
            };
        }

        private StardewValley.Object CreateFurniture(Vector2 location, string id, bool isFlipped)
        {
            return new StardewValley.Object(location, id) { Flipped = isFlipped };
        }

        private void AddModOrGrass(Vector2 location)
        {
            // We have 4 kinds of things that a mod can add:
            // largeterrainfeatures, terrainfeatures, resourceclumps and objects.
            // To create a truly random order, we shuffle the kind of things (using lambdas in a list),
            // and then we shuffle the callbacks provided by mods.
            foreach (var action in DeepWoodsAPI.ToShuffledList(new List<Func<bool>>()
            {
                () => {
                    foreach (var modLargeTerrainFeature in DeepWoodsAPI.ToShuffledList(ModEntry.GetAPI().LargeTerrainFeatures))
                    {
                        if (modLargeTerrainFeature.Item1(deepWoods, location))
                        {
                            deepWoods.largeTerrainFeatures.Add(modLargeTerrainFeature.Item2());
                            return true;
                        }
                    }
                    return false;
                },
                () => {
                    foreach (var modResourceClump in DeepWoodsAPI.ToShuffledList(ModEntry.GetAPI().ResourceClumps))
                    {
                        if (modResourceClump.Item1(deepWoods, location))
                        {
                            deepWoods.resourceClumps.Add(modResourceClump.Item2());
                            return true;
                        }
                    }
                    return false;
                },
                () => {
                    foreach (var modTerrainFeature in DeepWoodsAPI.ToShuffledList(ModEntry.GetAPI().TerrainFeatures))
                    {
                        if (modTerrainFeature.Item1(deepWoods, location))
                        {
                            deepWoods.terrainFeatures[location] = modTerrainFeature.Item2();
                            return true;
                        }
                    }
                    return false;
                },
                () => {
                    foreach (var modObject in DeepWoodsAPI.ToShuffledList(ModEntry.GetAPI().Objects))
                    {
                        if (modObject.Item1(deepWoods, location))
                        {
                            deepWoods.objects[location] = modObject.Item2();
                            return true;
                        }
                    }
                    return false;
                }
            }))
            {
                if (action())
                {
                    // A mod added something, return
                    return;
                }
            }

            // Mods didn't add anything, add grass
            deepWoods.terrainFeatures[location] = new LootFreeGrass(GetSeasonGrassType(), this.random.GetRandomValue(1, 3));
        }

        private void AddSomethingAwesomeForLichtung(Vector2 location)
        {
            // no stuff for infested or lakes!
            if (deepWoods.clearingType.Value != LichtungType.Default)
                return;

            // prevent obelisk based exploits
            if (deepWoods.spawnedFromObelisk.Value)
                return;

            string perk = this.random.GetRandomValue(Settings.Luck.Clearings.Perks);

            switch (perk)
            {
                case LichtungStuff.MushroomTrees:
                    AddMushroomTrees(location);
                    AddMushrooms();
                    break;
                case LichtungStuff.Treasure:
                    if (this.random.CheckChance(Settings.Luck.Clearings.ChanceForTrashOrTreasure))
                    {
                        deepWoods.objects[location] = new TreasureChest(location, CreateRandomTreasureChestItems());
                        AddLichtungStuffPile(location, Settings.Luck.Clearings.Treasure.PileItems);
                    }
                    else
                    {
                        deepWoods.objects[location] = new TreasureChest(location, CreateRandomTrashCanItems(), true);
                        AddLichtungStuffPile(location, Settings.Luck.Clearings.Trash.PileItems);
                    }
                    break;
                case LichtungStuff.GingerbreadHouse:
                    deepWoods.resourceClumps.Add(new GingerBreadHouse(location - new Vector2(2, 4)));
                    AddGingerBreadHouseDeco(location - new Vector2(2, 4));
                    break;
                case LichtungStuff.HealingFountain:
                    deepWoods.largeTerrainFeatures.Add(new HealingFountain(location - new Vector2(2, 0)));
                    AddRipeFruitTreesAroundFountain(location - new Vector2(2, 2));
                    if (Game1.currentSeason == "winter")
                        AddWinterFruitsAroundFountain(location - new Vector2(2, 2));
                    break;
                case LichtungStuff.IridiumTree:
                    deepWoods.resourceClumps.Add(new IridiumTree(location));
                    AddIridiumNodesAroundTree(location);
                    deepWoods.lightSources.Add(new LightSource($"DeepWoods{deepWoods.level}IridiumTreeLight", LightSource.sconceLight, location, 6, new Color(1f, 0f, 0f)));
                    break;
                case LichtungStuff.Unicorn:
                    if (!Game1.isRaining)
                    {
                        deepWoods.characters.Add(new Unicorn(location));
                    }
                    break;
                case LichtungStuff.ExcaliburStone:
                    deepWoods.largeTerrainFeatures.Add(new ExcaliburStone(location));
                    break;
                case LichtungStuff.Nothing:
                default:
                    break;
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

        private void AddLichtungStuffPile(Vector2 location, WeightedInt[] itemIds)
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
                    string id = this.random.GetRandomValue(new int[] { 40, 44 }).ToString();  // Big Green Cane or Big Red Cane
                    deepWoods.objects[leftPos] = CreateFurniture(leftPos, id, true);
                    deepWoods.objects[rightPos] = CreateFurniture(rightPos, id, false);
                }
                else
                {
                    deepWoods.objects[leftPos] = CreateFurniture(leftPos, GetRandomSmallCaneType(), this.random.CheckChance(Chance.FIFTY_FIFTY));
                    deepWoods.objects[rightPos] = CreateFurniture(rightPos, GetRandomSmallCaneType(), this.random.CheckChance(Chance.FIFTY_FIFTY));
                }
                if (y >= 3)
                {
                    Vector2 centerPos = new Vector2(location.X + 2, location.Y + y);
                    //deepWoods.objects[centerPos] = CreateFurniture(centerPos, "409", this.random.CheckChance(Chance.FIFTY_FIFTY)); // Crystal Floor
                    deepWoods.objects[centerPos] = CreateObject("409", true); // Crystal Floor
                }
            }

            for (int x = -1; x <= 5; x++)
            {
                if (x != 2)
                {
                    Vector2 pos = new Vector2(location.X + x, location.Y + 5);
                    deepWoods.terrainFeatures[pos] = new Flower(431, deepWoods, pos);   // Sunflower
                }
                if (x == -1 || x == 5)
                {
                    Vector2 pos = new Vector2(location.X + x, location.Y + 1);
                    deepWoods.terrainFeatures[pos] = new Flower(431, deepWoods, pos);   // Sunflower
                }
            }
        }

        private string GetRandomSmallCaneType()
        {
            // Green Canes, Mixed Canes or Red Canes
            return this.random.GetRandomValue(41, 43 + 1).ToString();
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

                    var iridiumNode = CreateObject("765", false);
                    iridiumNode.MinutesUntilReady = 16;
                    deepWoods.objects[nodeLocation] = iridiumNode;
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
            int numWinterFruits = this.random.GetRandomValue(6, 14);
            for (int i = 0; i < numWinterFruits; i++)
            {
                int fruitX = this.random.GetRandomValue(minX, maxX);
                int fruitY = this.random.GetRandomValue(minX, maxX);
                if (fountainRectangle.Contains(new Location(fruitX, fruitY)))
                    continue;

                Vector2 fruitLocation = new Vector2(fruitX, fruitY);

                if (deepWoods.objects.ContainsKey(fruitLocation))
                    continue;

                deepWoods.objects[fruitLocation] = CreateObject("414", true);
            }
        }

        private void AddMushroomTrees(Vector2 location)
        {
            int minX = Settings.Map.ForestPatchMinGapToMapBorder;
            int maxX = deepWoods.mapWidth.Value - Settings.Map.ForestPatchMinGapToMapBorder;
            int minY = Settings.Map.ForestPatchMinGapToMapBorder;
            int maxY = deepWoods.mapHeight.Value - Settings.Map.ForestPatchMinGapToMapBorder;

            int numMushroomTrees = this.random.GetRandomValue(3, 9);

            for (int i = 0; i < numMushroomTrees; i++)
            {
                int mushroomTreeX = this.random.GetRandomValue(minX, maxX);
                int mushroomTreeY = this.random.GetRandomValue(minX, maxX);

                Vector2 mushroomTreeLocation = new Vector2(mushroomTreeX, mushroomTreeY);

                if (!deepWoods.CanItemBePlacedHere(mushroomTreeLocation))
                    continue;

                Tree mushroomTree = new Tree(Tree.mushroomTree, Tree.treeStage);
                if (Game1.currentSeason == "winter")
                    mushroomTree.stump.Value = true;
                deepWoods.terrainFeatures[mushroomTreeLocation] = mushroomTree;
            }
        }

        private void AddMushrooms()
        {
            int minX = Settings.Map.MaxBumpSizeForForestBorder;
            int maxX = deepWoods.mapWidth.Value - Settings.Map.MaxBumpSizeForForestBorder;
            int minY = Settings.Map.MaxBumpSizeForForestBorder;
            int maxY = deepWoods.mapHeight.Value - Settings.Map.MaxBumpSizeForForestBorder;

            int numMushrooms = (Game1.currentSeason == "winter") ? this.random.GetRandomValue(12, 24) : this.random.GetRandomValue(9, 14);

            for (int i = 0; i < numMushrooms; i++)
            {
                int mushroomX = this.random.GetRandomValue(minX, maxX);
                int mushroomY = this.random.GetRandomValue(minX, maxX);

                Vector2 mushroomLocation = new Vector2(mushroomX, mushroomY);

                if (!deepWoods.CanItemBePlacedHere(mushroomLocation))
                    continue;

                // only purple mushrooms in winter
                string mushroomType = (Game1.currentSeason == "winter") ? "422" : GetRandomMushroomType();

                deepWoods.objects[mushroomLocation] = CreateObject(mushroomType, true);
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
            FruitTree fruitTree = new SeedLessFruitTree(GetRandomFruitTreeType(), growthStage);
            deepWoods.terrainFeatures[location] = fruitTree;

            fruitTree.fruit.Clear();
            if (Game1.season != Season.Winter)
            {
                for (int i = 0; i < fruitsOnTree; i++)
                {
                    fruitTree.TryAddFruit();
                }
            }

            if (growthStage == FruitTree.treeStage)
            {
                // itemQuality on fruit tree fruits
                if (deepWoods.level.Value >= Settings.Level.MinLevelForIridiumFruits
                    && this.random.CheckChance(Settings.Luck.Terrain.ChanceForIridiumFruits))
                {
                    fruitTree.daysUntilMature.Value = -336;     // itemQuality == 4
                }
                else if (deepWoods.level.Value >= Settings.Level.MinLevelForGoldFruits
                    && this.random.CheckChance(Settings.Luck.Terrain.ChanceForGoldFruits))
                {
                    fruitTree.daysUntilMature.Value = -224;     // itemQuality == 2
                }
                else if (deepWoods.level.Value >= Settings.Level.MinLevelForSilverFruits
                    && this.random.CheckChance(Settings.Luck.Terrain.ChanceForSilverFruits))
                {
                    fruitTree.daysUntilMature.Value = -112;     // itemQuality == 1
                }
                else
                {
                    fruitTree.daysUntilMature.Value = 0;        // itemQuality == 0
                }
            }
            else
            {
                fruitTree.daysUntilMature.Value = 28 - (growthStage * 7);
            }
        }

        private void AddItem(List<Item> items, int id, int stackSize = 1)
        {
            items.Add(new StardewValley.Object(id.ToString(), stackSize));
        }

        private List<Item> CreateRandomTreasureChestItems()
        {
            List<Item> items = new List<Item>();

            if (this.random.CheckChance(Settings.Luck.Clearings.Treasure.ChanceForMetalBarsInChest))
            {
                AddItem(items, this.random.GetRandomValue(334, 338 + 1), this.random.GetRandomValue(Settings.Luck.Clearings.Treasure.MetalBarStackSize));
            }

            if (this.random.CheckChance(Settings.Luck.Clearings.Treasure.ChanceForElixirsInChest))
            {
                AddItem(items, this.random.GetRandomValue(new int[] { 772, 773 }), this.random.GetRandomValue(Settings.Luck.Clearings.Treasure.ElixirStackSize));
            }

            if (this.random.CheckChance(Settings.Luck.Clearings.Treasure.ChanceForArtefactInChest))
            {
                AddItem(items, 124);
            }

            if (this.random.CheckChance(Settings.Luck.Clearings.Treasure.ChanceForDwarfScrollInChest))
            {
                AddItem(items, this.random.GetRandomValue(96, 99 + 1));
            }

            if (this.random.CheckChance(Settings.Luck.Clearings.Treasure.ChanceForRingInChest))
            {
                AddItem(items, this.random.GetRandomValue(516, 534 + 1));
            }

            if (this.random.CheckChance(Settings.Luck.Clearings.Treasure.ChanceForRandomPileItemInChest))
            {
                AddItem(items, this.random.GetRandomValue(Settings.Luck.Clearings.Treasure.PileItems), this.random.GetRandomValue(Settings.Luck.Clearings.Treasure.PileItemStackSize));
            }

            // Shuffle items around
            return new List<Item>(items.OrderBy<Item, int>(a => Game1.random.Next()));
        }

        private List<Item> CreateRandomTrashCanItems()
        {
            List<Item> items = new List<Item>();

            if (this.random.CheckChance(Settings.Luck.Clearings.Trash.ChanceForLewisShortsInGarbagebin))
            {
                AddItem(items, 789);
            }

            if (this.random.CheckChance(Settings.Luck.Clearings.Trash.ChanceForBoneInGarbagebin))
            {
                AddItem(items, this.random.GetRandomValue(579, 585 + 1));
            }

            if (this.random.CheckChance(Settings.Luck.Clearings.Trash.ChanceForArtefactInGarbagebin))
            {
                AddItem(items, this.random.GetRandomValue(new int[] { 111, 112, 113, 115 }));
            }

            if (this.random.CheckChance(Settings.Luck.Clearings.Trash.ChanceForPuppetInGarbagebin))
            {
                AddItem(items, this.random.GetRandomValue(new int[] { 103, 126, 127 }));
            }

            if (this.random.CheckChance(Settings.Luck.Clearings.Trash.ChanceForRandomPileItemInGarbagebin))
            {
                AddItem(items, this.random.GetRandomValue(Settings.Luck.Clearings.Trash.PileItems), this.random.GetRandomValue(Settings.Luck.Clearings.Trash.PileItemStackSize));
            }

            // Shuffle items around
            return new List<Item>(items.OrderBy<Item, int>(a => Game1.random.Next()));
        }

        private int GetRandomFlowerType()
        {
            if (Game1.currentSeason == "winter")
            {
                return this.random.GetRandomValue(Settings.Luck.Terrain.WinterFlowers);
            }
            else
            {
                return this.random.GetRandomValue(Settings.Luck.Terrain.Flowers);
            }
        }

        private bool IsTileFree(HashSet<Location> blockedLocations, Vector2 location)
        {
            if (blockedLocations.Contains(new Location((int)location.X, (int)location.Y)))
                return false;

            // No placements on tiles that are covered in forest.
            if (deepWoods.map.GetLayer("Buildings").Tiles[(int)location.X, (int)location.Y] != null)
                return false;

            // No placements on borders, exits and enter locations.
            if (deepWoods.IsLocationOnBorderOrExit(location))
                return false;

            // Don't place anything on water
            if (deepWoods.doesTileHaveProperty((int)location.X, (int)location.Y, "Water", "Back") != null)
                return false;

            return true;
        }

        private bool CheckAndBlockSpace(HashSet<Location> blockedLocations, Vector2 location, Size size)
        {
            // first we check if all tiles are free
            for (int x = 0; x < size.Width; x++)
            {
                for (int y = 0; y < size.Height; y++)
                {
                    if (!IsTileFree(blockedLocations, new Vector2(location.X + x, location.Y + y)))
                    {
                        // at least one tile is blocked, return false, leave rest free
                        return false;
                    }
                }
            }

            // if they are all free, we block them all and return true
            for (int x = 0; x < size.Width; x++)
            {
                for (int y = 0; y < size.Height; y++)
                {
                    blockedLocations.Add(new Location((int)location.X + x, (int)location.Y + y));
                }
            }
            return true;
        }

        private bool TileHasTree(Vector2 location)
        {
            return deepWoods.terrainFeatures.ContainsKey(location)
                && (deepWoods.terrainFeatures[location] is FruitTree
                    || ((deepWoods.terrainFeatures[location] as Tree)?.growthStage.Value ?? 0) >= Tree.treeStage
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

        private byte GetSeasonGrassType()
        {
            return Game1.currentSeason == "winter" ? Grass.frostGrass : Grass.springGrass;
        }

        private string GetRandomWeedType()
        {
            return GameLocation.getWeedForSeason(new Random(this.random.GetRandomValue()), Game1.season);
        }

        private string GetRandomForageable()
        {
            if (Game1.IsWinter && Settings.Luck.Terrain.WinterForageables.Length > 0)
            {
                return this.random.GetRandomValue(Settings.Luck.Terrain.WinterForageables).ToString();
            }
            else if (Game1.IsSummer && Settings.Luck.Terrain.SummerForageables.Length > 0)
            {
                return this.random.GetRandomValue(Settings.Luck.Terrain.SummerForageables).ToString();
            }
            else if (Game1.IsSpring && Settings.Luck.Terrain.SpringForageables.Length > 0)
            {
                return this.random.GetRandomValue(Settings.Luck.Terrain.SpringForageables).ToString();
            }
            else if (Game1.IsFall && Settings.Luck.Terrain.FallForageables.Length > 0)
            {
                return this.random.GetRandomValue(Settings.Luck.Terrain.FallForageables).ToString();
            }
            else
            {
                return GetRandomWeedType();
            }
        }

        private string GetRandomStoneType()
        {
            return this.random.GetRandomValue(new int[] { 343, 668, 670 }).ToString();
        }

        private string GetRandomTwigType()
        {
            return this.random.GetRandomValue(new int[] { 294, 295 }).ToString();
        }

        private string GetRandomTreeType()
        {
            return this.random.GetRandomValue(Settings.Luck.Terrain.TreeTypes).ToString();
        }

        private string GetRandomMushroomType()
        {
            return this.random.GetRandomValue(new WeightedInt[] {
                new WeightedInt(422, 1),  // Purple one
                new WeightedInt(420, 5),  // Red one
                new WeightedInt(257, 10), // Morel
                new WeightedInt(281, 20), // Big brown one
                new WeightedInt(404, 50), // Normal one
            }).ToString();
        }

        private int GetRandomItemQuality()
        {
            return this.random.GetRandomValue(new WeightedInt[] {
                new WeightedInt(StardewValley.Object.lowQuality, 100),
                new WeightedInt(StardewValley.Object.medQuality, 50),
                new WeightedInt(StardewValley.Object.highQuality, 10),
                new WeightedInt(StardewValley.Object.bestQuality, 1),
            });
        }

        private string GetRandomFruitTreeType()
        {
            return this.random.GetRandomValue(Settings.Luck.Terrain.FruitTreeTypes).ToString();
        }

        private void Infest(HashSet<Location> blockedLocations)
        {
            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            List<int> allTilesInRandomOrder = Enumerable.Range(0, deepWoods.mapWidth.Value * deepWoods.mapHeight.Value).OrderBy(n => Game1.random.Next()).ToList();

            int centerX = mapWidth / 2;
            int centerY = mapHeight / 2;

            CheckAndBlockSpace(blockedLocations, new Vector2(centerX, centerY), new Size(2, 1));
            deepWoods.largeTerrainFeatures.Add(new DeepWoodsOrbStone(new Vector2(centerX, centerY)));

            int numInfestedStuff = 40;

            for (int i = 0; i < numInfestedStuff; i++)
            {
                int tileIndex = allTilesInRandomOrder[i];
                int x = tileIndex % mapWidth;
                int y = tileIndex / mapWidth;

                if (!IsTileFree(blockedLocations, new Vector2(x, y)))
                    continue;

                switch (this.random.GetRandomValue(Settings.Luck.Clearings.InfestedPerks))
                {
                    case InfestedStuff.InfestedTree:
                        deepWoods.terrainFeatures[new Vector2(x, y)] = new InfestedTree(GetRandomFruitTreeType());
                        break;
                    case InfestedStuff.ThornyBush:
                        deepWoods.terrainFeatures[new Vector2(x, y)] = new ThornyBush(new Vector2(x, y), deepWoods);
                        break;
                }

                blockedLocations.Add(new Location(x, y));
            }
        }

        private void ClearAndGiftInfestedLevel()
        {
            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            SaveOrbStone(deepWoods.largeTerrainFeatures.Where(f => f is DeepWoodsOrbStone).Select(f => f as DeepWoodsOrbStone).FirstOrDefault());

            Array.ForEach(deepWoods.terrainFeatures.Pairs.ToArray(), pair =>
            {
                if (pair.Value is InfestedTree infestedTree)
                {
                    infestedTree.DeInfest();
                }
                else if (pair.Value is ThornyBush thornyBush)
                {
                    deepWoods.terrainFeatures.Remove(pair.Key);
                    deepWoods.terrainFeatures[pair.Key] = new Flower(GetRandomFlowerType(), deepWoods, pair.Key);
                }
            });
        }

        private void SaveOrbStone(DeepWoodsOrbStone orbStone)
        {
            orbStone.HasOrb = true;

            if (Game1.IsMasterGame)
            {
                var orbStonesSaved = DeepWoodsState.OrbStonesSaved;

                if (orbStonesSaved < DeepWoodsOrbStone.SavedColors.Length)
                {
                    orbStone.OrbColor = DeepWoodsOrbStone.SavedColors[orbStonesSaved];
                }
                else
                {
                    orbStone.OrbColor = Color.White;
                }

                DeepWoodsState.OrbStonesSaved = orbStonesSaved + 1;
            }

            deepWoods.lightSources.Add(new LightSource($"DeepWoods{deepWoods.level}OrbStone", LightSource.sconceLight, orbStone.Tile - new Vector2(0, - 2), 6, new Color(1f, 0f, 0f)));
        }


        private void FillFirstLevel(HashSet<Location> blockedLocations)
        {
            /*
             TODO:
                - a hut similar to the adventure's guild where players can talk to an NPC and get quests and info about the forest

                - a little seasonal garden that goes with the hut

                - a wooden welcome sign

                - a row of stone obelisks that serve a purpose i am not revealing yet

                - a minecart
             */

            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();



            // cute exit signs
            foreach (var exit in deepWoods.exits)
            {
                AddCuteSignToExit(blockedLocations, exit.Location, exit.ExitDir);
            }


            // hut with fruit trees
            var maxHutLocation = new Vector2(deepWoods.EnterLocation.X + Settings.Map.ExitRadius + 2, deepWoods.EnterLocation.Y + 1);
            DeepWoodsMaxHouse.MaxHutLocation = maxHutLocation;
            deepWoods.largeTerrainFeatures.Add(new MaxHut(maxHutLocation));

            // TODO: TEMP: 1.6: fruit trees drop seeds. disable fruit tree for now
            /*
            var fruitTree1Location = new Vector2(deepWoods.EnterLocation.X + Settings.Map.ExitRadius + 2 + 6, deepWoods.EnterLocation.Y + 8);
            AddRipeFruitTree(fruitTree1Location);
            */

            /*
            var fruitTree2Location = new Vector2(deepWoods.EnterLocation.X - Settings.Map.ExitRadius - 2 - 2, deepWoods.EnterLocation.Y + 7);
            AddRipeFruitTree(fruitTree2Location);

            var fruitTree3Location = new Vector2(deepWoods.EnterLocation.X - Settings.Map.ExitRadius - 2 - 6, deepWoods.EnterLocation.Y + 9);
            AddRipeFruitTree(fruitTree3Location);
            */


            // big welcome sign with minecart
            var woodenSignLocation = new Vector2(deepWoods.EnterLocation.X - Settings.Map.ExitRadius - 2, deepWoods.EnterLocation.Y + 3);
            deepWoods.largeTerrainFeatures.Add(new BigWoodenSign(woodenSignLocation));
            deepWoods.AddLightSource(woodenSignLocation);


            // orb stones
            float y = mapHeight - Settings.Map.ExitRadius - 6;
            for (int i = 0; i < 5; i++)
            {
                float x = mapWidth * (i + 1) / 6.0f;

                var orbStone = new DeepWoodsOrbStone(new Vector2(x, y), i);
                deepWoods.largeTerrainFeatures.Add(orbStone);
            }
        }
    }
}
