using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using static DeepWoodsMod.DeepWoodsEnterExit;
using static DeepWoodsMod.DeepWoodsRandom;

namespace DeepWoodsMod
{
    class DeepWoodsBuilder
    {
        private static Probability CHANCE_FOR_NORMAL_GRASS = new Probability(80);
        private static Probability CHANCE_FOR_BIG_FORESTPATCH_IN_CENTER = new Probability(25);
        private static Probability CHANCE_FOR_FORESTPATCH_IN_GRID = new Probability(50);

        private static Probability CHANCE_FOR_NOLEAVE_FOREST_FILLER = new Probability(80);

        private DeepWoods deepWoods;
        private DeepWoodsRandom random;
        private DeepWoodsSpaceManager spaceManager;
        private Map map;
        private Dictionary<ExitDirection, Location> exitLocations;

        private DeepWoodsBuilder(DeepWoods deepWoods, DeepWoodsRandom random, DeepWoodsSpaceManager spaceManager, Map map, Dictionary<ExitDirection, Location>  exitLocations)
        {
            this.deepWoods = deepWoods;
            this.random = random;
            this.spaceManager = spaceManager;
            this.map = map;
            this.exitLocations = exitLocations;
        }

        public static void Build(DeepWoods deepWoods, DeepWoodsRandom random, DeepWoodsSpaceManager spaceManager, Map map, Dictionary<ExitDirection, Location> exitLocations)
        {
            new DeepWoodsBuilder(deepWoods, random, spaceManager, map, exitLocations).Build();
        }

        private void Build()
        {
            GenerateForestBorder();
            // GenerateForestPatches();
            GenerateGround();
        }

        private int GetRandomGrassTileIndex(bool dark)
        {
            if (dark)
            {
                return this.random.GetRandomValue(new int[] { 380, 156 }, CHANCE_FOR_NORMAL_GRASS);
            }
            else
            {
                return this.random.GetRandomValue(new int[] { 351, 304, 300 }, CHANCE_FOR_NORMAL_GRASS);
            }
        }

        private void GenerateGround()
        {
            TileSheet tileSheet = this.map.GetTileSheet(DeepWoods.DEFAULT_OUTDOOR_TILESHEET_ID);
            Layer backLayer = this.map.GetLayer("Back");

            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (backLayer.Tiles[x, y] == null)
                    {
                        backLayer.Tiles[x, y] = new StaticTile(backLayer, tileSheet, BlendMode.Alpha, GetRandomGrassTileIndex(false));
                    }
                }
            }
        }

        private void GenerateForestBorder()
        {
            TileSheet tileSheet = this.map.GetTileSheet(DeepWoods.DEFAULT_OUTDOOR_TILESHEET_ID);
            Layer buildingsLayer = this.map.GetLayer("Buildings");

            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            /*
            for (int x = 0; x < mapWidth; x++)
            {
                buildingsLayer.Tiles[x, 0] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 1144 + (x % 2));
            }

            for (int x = 0; x < mapWidth; x++)
            {
                buildingsLayer.Tiles[x, mapHeight - 1] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 946);
            }

            for (int y = 0; y < (mapHeight - 1); y++)
            {
                buildingsLayer.Tiles[0, y] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 946);
            }

            for (int y = 0; y < (mapHeight - 1); y++)
            {
                buildingsLayer.Tiles[mapWidth - 1, y] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 946);
            }
            */

            GenerateExits();

            Size topLeftCornerSize = GenerateForestCorner(0, 0, 1, 1);
            Size topRightCornerSize = GenerateForestCorner(mapWidth - 1, 0, -1, 1);
            Size bottomLeftCornerSize = GenerateForestCorner(0, mapHeight - 1, 1, -1);
            Size bottomRightCornerSize = GenerateForestCorner(mapWidth - 1, mapHeight - 1, -1, -1);

            GenerateForestRow(
                new Location(topLeftCornerSize.Width, 0),
                new Location(mapWidth - topRightCornerSize.Width, 0),
                1, 0,
                ExitDirection.TOP);

            GenerateForestRow(
                new Location(bottomLeftCornerSize.Width, mapHeight - 1),
                new Location(mapWidth - bottomRightCornerSize.Width, mapHeight - 1),
                1, 0,
                ExitDirection.BOTTOM);

            GenerateForestRow(
                new Location(0, topLeftCornerSize.Height),
                new Location(0, mapHeight - bottomLeftCornerSize.Height),
                0, 1,
                ExitDirection.LEFT);

            GenerateForestRow(
                new Location(mapWidth - 1, topRightCornerSize.Height),
                new Location(mapWidth - 1, mapHeight - bottomRightCornerSize.Height),
                0, 1,
                ExitDirection.RIGHT);
        }

        private void GenerateForestRow(Location startPos, Location stopPos, int xDir, int yDir, ExitDirection exitDir)
        {
            if (this.exitLocations.ContainsKey(exitDir))
            {
                Location exitPosition = this.exitLocations[exitDir];
                GenerateForestRow(startPos, exitPosition - new Location(xDir * 2, yDir * 2), xDir, yDir);
                GenerateForestRow(exitPosition + new Location(xDir * 3, yDir * 3), stopPos, xDir, yDir);
            }
            else
            {
                GenerateForestRow(startPos, stopPos, xDir, yDir);
            }
        }

        private void GenerateForestRow(Location startPos, Location stopPos, int xDir, int yDir)
        {
            // Out of range check
            if (
                (stopPos.X != startPos.X && (((stopPos.X - startPos.X) > 0) != (xDir > 0)))
                ||
                (stopPos.Y != startPos.Y && (((stopPos.Y - startPos.Y) > 0) != (yDir > 0)))
               )
            {
                ModEntry.Log("GenerateForestRow out of range!", StardewModdingAPI.LogLevel.Warn);
                return;
            }

            TileSheet tileSheet = this.map.GetTileSheet(DeepWoods.DEFAULT_OUTDOOR_TILESHEET_ID);
            Layer buildingsLayer = this.map.GetLayer("Buildings");

            for (int x = startPos.X, y = startPos.Y; x != stopPos.X || y != stopPos.Y; x+= xDir, y += yDir)
            {
                buildingsLayer.Tiles[x, y] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 1012);
            }
        }

        private Size GenerateForestCorner(int startXPos, int startYPos, int xDir, int yDir)
        {
            TileSheet tileSheet = this.map.GetTileSheet(DeepWoods.DEFAULT_OUTDOOR_TILESHEET_ID);
            Layer buildingsLayer = this.map.GetLayer("Buildings");
            Layer alwaysFrontLayer = this.map.GetLayer("AlwaysFront");

            int width = 8;// this.random.GetRandomValue(DeepWoodsSpaceManager.MIN_CORNER_SIZE, DeepWoodsSpaceManager.MAX_CORNER_SIZE);
            int height = 8;// this.random.GetRandomValue(DeepWoodsSpaceManager.MIN_CORNER_SIZE, DeepWoodsSpaceManager.MAX_CORNER_SIZE);

            float ratio = (float)height / (float)width;
            int chance = (int)((100f / (ratio + 1f)) * ratio);
            Probability probability = new Probability(chance, 100);

            int endXPos = startXPos + ((width - 1) * xDir);
            int endYPos = startYPos + ((height - 1) * yDir);

            int curXPos = endXPos;
            int curYPos = startYPos;

            //for (int x = 0; x < width; x++, curXPos -= xDir)
            while (curXPos != startXPos)
            {
                int deltaX = Math.Abs(curXPos - startXPos);
                int deltaY = Math.Abs(curYPos - endYPos);
                if (deltaX > 1 && deltaY > 2 && this.random.GetChance(probability))
                {
                    // go vertical
                    for (int y = startYPos; y != curYPos; y += yDir)
                    {
                        FillForestTile(curXPos, y);
                    }
                    buildingsLayer.Tiles[curXPos, curYPos + 0 * yDir] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 946);
                    alwaysFrontLayer.Tiles[curXPos, curYPos + 0 * yDir] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, 941);
                    alwaysFrontLayer.Tiles[curXPos, curYPos + 1 * yDir] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, 966);
                    alwaysFrontLayer.Tiles[curXPos, curYPos + 2 * yDir] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, 992);
                    curXPos -= xDir;
                    curYPos += yDir;
                }
                else if (deltaX > 1)
                {
                    // go horizontal
                    for (int y = startYPos; y != curYPos; y += yDir)
                    {
                        FillForestTile(curXPos, y);
                    }
                    buildingsLayer.Tiles[curXPos, curYPos + 0 * yDir] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 946);
                    alwaysFrontLayer.Tiles[curXPos, curYPos + 0 * yDir] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, 942);
                    alwaysFrontLayer.Tiles[curXPos, curYPos + 1 * yDir] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, 967);
                    curXPos -= xDir;
                }
                else
                {
                    // fill last corner
                    for (int y = startYPos; y != curYPos; y += yDir)
                    {
                        FillForestTile(curXPos - 1 * xDir, y);
                        FillForestTile(curXPos - 0 * xDir, y);
                    }
                    buildingsLayer.Tiles[curXPos - 1 * xDir, curYPos + 0 * yDir] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 946);
                    buildingsLayer.Tiles[curXPos - 0 * xDir, curYPos + 0 * yDir] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 946);
                    buildingsLayer.Tiles[curXPos - 1 * xDir, curYPos + 1 * yDir] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 946);
                    alwaysFrontLayer.Tiles[curXPos - 1 * xDir, curYPos + 0 * yDir] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, 940);
                    alwaysFrontLayer.Tiles[curXPos - 0 * xDir, curYPos + 0 * yDir] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, 941);
                    alwaysFrontLayer.Tiles[curXPos - 1 * xDir, curYPos + 1 * yDir] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, 965);
                    alwaysFrontLayer.Tiles[curXPos - 0 * xDir, curYPos + 1 * yDir] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, 966);
                    curYPos += yDir;
                    while (curYPos != endYPos)
                    {
                        buildingsLayer.Tiles[curXPos - 1 * xDir, curYPos + 1 * yDir] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 946);
                        alwaysFrontLayer.Tiles[curXPos - 1 * xDir, curYPos + 1 * yDir] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, 990);
                        alwaysFrontLayer.Tiles[curXPos - 0 * xDir, curYPos + 1 * yDir] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, 991);
                        curYPos += yDir;
                    }
                    curXPos -= xDir;
                }
            }

            Vector2 lightPos = new Vector2(startXPos, startYPos);
            /*
            for (int x = 0, y = 0; x < width && y < height; x++, y++)
            {
                if (buildingsLayer.Tiles[startXPos + x * xDir, startYPos + y * yDir] == null)
                {
                    lightPos = new Vector2(startXPos + (xDir * MathHelper.Max(0, x - 5)), startYPos + (yDir * MathHelper.Max(0, y - 5)));
                    break;
                }
            }
            */
            deepWoods.lightSources.Add(lightPos);

            return new Size(width, height);
        }

        private void GenerateExits()
        {
            Layer buildingsLayer = this.map.GetLayer("Buildings");
            foreach (var exit in this.exitLocations)
            {
                if (exit.Key == ExitDirection.TOP)
                {

                }
                // buildingsLayer.Tiles[exit.Value] = null;
            }
        }

        private void FillForestTile(int x, int y)
        {
            TileSheet tileSheet = this.map.GetTileSheet(DeepWoods.DEFAULT_OUTDOOR_TILESHEET_ID);
            Layer buildingsLayer = this.map.GetLayer("Buildings");
            Layer alwaysFrontLayer = this.map.GetLayer("AlwaysFront");

            buildingsLayer.Tiles[x, y] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 946);
            alwaysFrontLayer.Tiles[x, y] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, GetRandomForestFillerTileIndex());
        }

        private int GetRandomForestFillerTileIndex()
        {
            return this.random.GetRandomValue(new int[] { 946, 971, 996 }, CHANCE_FOR_NOLEAVE_FOREST_FILLER);
        }

        /*
        private void GenerateForestRow(Location location, int width, Transform t)
        {
            TileSheet tileSheet = this.map.GetTileSheet(DEFAULT_OUTDOOR_TILESHEET_ID);
            Layer alwaysFrontLayer = this.map.GetLayer("AlwaysFront");

            for (int x = xOffset; x < xOffset + width; x++)
            {
                alwaysFrontLayer.Tiles[x, yOffset] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, this.random.GetRandomValue(new int[] { 1042, 1043 }));
                alwaysFrontLayer.Tiles[x, yOffset+1] = new StaticTile(alwaysFrontLayer, tileSheet, BlendMode.Alpha, this.random.GetRandomValue(new int[] { 1067, 1068 }));
            }
        }
        */

        private void GenerateForestPatch(xTile.Dimensions.Rectangle rectangle)
        {
            TileSheet tileSheet = this.map.GetTileSheet(DeepWoods.DEFAULT_OUTDOOR_TILESHEET_ID);
            Layer buildingsLayer = this.map.GetLayer("Buildings");
            Layer alwaysFrontLayer = this.map.GetLayer("AlwaysFront");

            for (int x = 0; x < rectangle.Width; x++)
            {
                for (int y = 0; y < rectangle.Height; y++)
                {
                    buildingsLayer.Tiles[rectangle.X + x, rectangle.Y + y] = new StaticTile(buildingsLayer, tileSheet, BlendMode.Alpha, 946);
                }
            }
        }

        private void TryGenerateForestPatch(Location location)
        {
            int wishWidth = this.random.GetRandomValue(DeepWoodsSpaceManager.MIN_FOREST_PATCH_DIAMETER, DeepWoodsSpaceManager.MAX_FOREST_PATCH_DIAMETER);
            int wishHeight = this.random.GetRandomValue(DeepWoodsSpaceManager.MIN_FOREST_PATCH_DIAMETER, DeepWoodsSpaceManager.MAX_FOREST_PATCH_DIAMETER);

            xTile.Dimensions.Rectangle rectangle;
            if (this.spaceManager.TryGetFreeRectangleForForestPatch(location, wishWidth, wishHeight, out rectangle))
            {
                GenerateForestPatch(rectangle);
            }
        }

        private void GenerateForestPatches()
        {
            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            int numForestPatches;

            if (mapWidth > DeepWoodsSpaceManager.FOREST_PATCH_CENTER_MIN_DISTANCE_TO_MAPBORDER * 2 && mapHeight > DeepWoodsSpaceManager.FOREST_PATCH_CENTER_MIN_DISTANCE_TO_MAPBORDER * 2)
            {
                // Calculate maximum theoretical amount of forest patches for the current map.
                int maxForestPatches = (mapWidth * mapHeight) / DeepWoodsSpaceManager.MINIMUM_TILES_FOR_FORESTPATCH;

                // Get a random value from 0 to maxForestPatches, using a "two dice" method,
                // where the center numbers are more likely than the edges.
                numForestPatches =
                    this.random.GetRandomValue(0, maxForestPatches / 2)
                    + this.random.GetRandomValue(0, maxForestPatches / 2);
            }
            else
            {
                numForestPatches = this.random.GetRandomValue(0, 1);
            }

            // Try to generate forest patches at random positions.
            // Some of these may not generate anything due to overlaps, that's by design.
            for (int i = 0; i < numForestPatches; i++)
            {
                int x = this.random.GetRandomValue(DeepWoodsSpaceManager.FOREST_PATCH_CENTER_MIN_DISTANCE_TO_MAPBORDER, mapWidth - DeepWoodsSpaceManager.FOREST_PATCH_CENTER_MIN_DISTANCE_TO_MAPBORDER);
                int y = this.random.GetRandomValue(DeepWoodsSpaceManager.FOREST_PATCH_CENTER_MIN_DISTANCE_TO_MAPBORDER, mapHeight - DeepWoodsSpaceManager.FOREST_PATCH_CENTER_MIN_DISTANCE_TO_MAPBORDER);
                TryGenerateForestPatch(new Location(x, y));
            }
        }

    }
}
