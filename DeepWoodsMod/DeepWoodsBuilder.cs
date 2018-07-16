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

        private static Probability CHANCE_FOR_FOREST_ROW_TREESTUMPS = new Probability(50);

        private const int FOREST_ROW_MAX_INWARDS_BUMP = 2;
        private static Probability CHANCE_FOR_FOREST_ROW_BUMP = new Probability(50);

        private const int PLAIN_FOREST_BACKGROUND = 946;
        private static int[] FOREST_BACKGROUND = new int[] { PLAIN_FOREST_BACKGROUND, 971, 996 };

        private const int FOREST_ROW_TREESTUMP_LEFT = 1144;
        private const int FOREST_ROW_TREESTUMP_RIGHT = 1145;

        private const int NUM_TILES_PER_LIGHTSOURCE_IN_FOREST_PATCH = 16;

        private const int DEBUG_PINK_LEAVES = 68;

        private enum GrassType
        {
            BLACK,
            DARK,
            NORMAL,
            BRIGHT
        }

        private enum PlacingDirection
        {
            DOWNWARDS,
            UPWARDS,
            LEFTWARDS,
            RIGHTWARDS
        }

        private class Placing
        {
            public readonly Location location;
            public readonly PlacingDirection dir;
            public readonly PlacingDirection dirInward;

            public int XDir
            {
                get
                {
                    switch (dir)
                    {
                        case PlacingDirection.LEFTWARDS:
                            return -1;
                        case PlacingDirection.RIGHTWARDS:
                            return 1;
                        case PlacingDirection.DOWNWARDS:
                        case PlacingDirection.UPWARDS:
                            return 0;
                        default:
                            throw new InvalidOperationException("Invalid placing direction: " + this.dir);
                    }
                }
            }

            public int YDir
            {
                get
                {
                    switch (dir)
                    {
                        case PlacingDirection.LEFTWARDS:
                        case PlacingDirection.RIGHTWARDS:
                            return 0;
                        case PlacingDirection.DOWNWARDS:
                            return 1;
                        case PlacingDirection.UPWARDS:
                            return -1;
                        default:
                            throw new InvalidOperationException("Invalid placing direction: " + this.dir);
                    }
                }
            }

            public int XDirInward
            {
                get
                {
                    switch (dirInward)
                    {
                        case PlacingDirection.LEFTWARDS:
                            return -1;
                        case PlacingDirection.RIGHTWARDS:
                            return 1;
                        case PlacingDirection.DOWNWARDS:
                        case PlacingDirection.UPWARDS:
                            return 0;
                        default:
                            throw new InvalidOperationException("Invalid placing direction: " + this.dirInward);
                    }
                }
            }

            public int YDirInward
            {
                get
                {
                    switch (dirInward)
                    {
                        case PlacingDirection.LEFTWARDS:
                        case PlacingDirection.RIGHTWARDS:
                            return 0;
                        case PlacingDirection.DOWNWARDS:
                            return 1;
                        case PlacingDirection.UPWARDS:
                            return -1;
                        default:
                            throw new InvalidOperationException("Invalid placing direction: " + this.dirInward);
                    }
                }
            }

            public int DistanceTo(Location location)
            {
                switch(dir)
                {
                    case PlacingDirection.LEFTWARDS:
                    case PlacingDirection.RIGHTWARDS:
                        return Math.Abs(location.X - this.location.X);
                    case PlacingDirection.DOWNWARDS:
                    case PlacingDirection.UPWARDS:
                        return Math.Abs(location.Y - this.location.Y);
                    default:
                        throw new InvalidOperationException("Invalid placing direction: " + this.dirInward);
                }
            }

            public Placing Replace(Location location)
            {
                return new Placing(location, this.dir, this.dirInward);
            }

            public Placing(Location location, PlacingDirection dir, PlacingDirection dirInward)
            {
                this.location = location;
                this.dir = dir;
                this.dirInward = dirInward;
            }
            public Placing(Placing placing, int steps, int stepsInward)
            {
                this.location.X = placing.location.X + (placing.XDir * steps) + (placing.XDirInward * stepsInward);
                this.location.Y = placing.location.Y + (placing.YDir * steps) + (placing.YDirInward * stepsInward);
                this.dir = placing.dir;
                this.dirInward = placing.dirInward;
            }
        }

        private enum PlaceMode
        {
            DONT_OVERRIDE,
            OVERRIDE
        }

        private class DeepWoodsRowTileMatrix
        {
            public int[] FOREST_BACK;
            public int[] FOREST_FRONT;

            public int[] FOREST_LEFT_BACK;
            public int[] FOREST_LEFT_FRONT;

            public int[] FOREST_RIGHT_BACK;
            public int[] FOREST_RIGHT_FRONT;

            public int FOREST_LEFT_CORNER_BACK;
            public int FOREST_LEFT_CONCAVE_CORNER;
            public int FOREST_LEFT_CONVEX_CORNER;

            public int FOREST_RIGHT_CORNER_BACK;
            public int FOREST_RIGHT_CONCAVE_CORNER;
            public int FOREST_RIGHT_CONVEX_CORNER;

            public int DARK_GRASS_FRONT;
            public int DARK_GRASS_LEFT;
            public int DARK_GRASS_RIGHT;
            public int DARK_GRASS_LEFT_CONCAVE_CORNER;
            public int DARK_GRASS_LEFT_CONVEX_CORNER;
            public int DARK_GRASS_RIGHT_CONCAVE_CORNER;
            public int DARK_GRASS_RIGHT_CONVEX_CORNER;

            public int BLACK_GRASS_FRONT;
            public int BLACK_GRASS_LEFT;
            public int BLACK_GRASS_RIGHT;
            public int BLACK_GRASS_LEFT_CONCAVE_CORNER;
            public int BLACK_GRASS_LEFT_CONVEX_CORNER;
            public int BLACK_GRASS_RIGHT_CONCAVE_CORNER;
            public int BLACK_GRASS_RIGHT_CONVEX_CORNER;

            public int BRIGHT_GRASS_FRONT;
            public int BRIGHT_GRASS_LEFT;
            public int BRIGHT_GRASS_RIGHT;
            public int BRIGHT_GRASS_LEFT_CONCAVE_CORNER;
            public int BRIGHT_GRASS_LEFT_CONVEX_CORNER;
            public int BRIGHT_GRASS_RIGHT_CONCAVE_CORNER;
            public int BRIGHT_GRASS_RIGHT_CONVEX_CORNER;

            public bool HAS_BLACK_GRASS;

            private DeepWoodsRowTileMatrix() { }

            public static readonly DeepWoodsRowTileMatrix TOP = new DeepWoodsRowTileMatrix()
            {
                FOREST_BACK = new int[] { 941/*942, 943*/ },
                FOREST_FRONT = new int[] { 967, 968 },

                FOREST_LEFT_BACK = new int[] { 1015 },
                FOREST_LEFT_FRONT = new int[] { 991, 1016 },

                FOREST_RIGHT_BACK = new int[] { 995 },
                FOREST_RIGHT_FRONT = new int[] { 994, 1019 },

                FOREST_LEFT_CORNER_BACK = 940,
                FOREST_LEFT_CONCAVE_CORNER = 966,
                FOREST_LEFT_CONVEX_CORNER = 992,

                FOREST_RIGHT_CORNER_BACK = 945,
                FOREST_RIGHT_CONCAVE_CORNER = 969,
                FOREST_RIGHT_CONVEX_CORNER = 993,

                DARK_GRASS_FRONT = 405,
                DARK_GRASS_LEFT = 381,
                DARK_GRASS_RIGHT = 379,
                DARK_GRASS_LEFT_CONCAVE_CORNER = 357,
                DARK_GRASS_LEFT_CONVEX_CORNER = 406,
                DARK_GRASS_RIGHT_CONCAVE_CORNER = 407,
                DARK_GRASS_RIGHT_CONVEX_CORNER = 404,

                BLACK_GRASS_FRONT = 1119,
                BLACK_GRASS_LEFT = 1121,
                BLACK_GRASS_RIGHT = 1096,
                BLACK_GRASS_LEFT_CONCAVE_CORNER = 1092,
                BLACK_GRASS_LEFT_CONVEX_CORNER = 1120,
                BLACK_GRASS_RIGHT_CONCAVE_CORNER = 1093,
                BLACK_GRASS_RIGHT_CONVEX_CORNER = 1095,

                BRIGHT_GRASS_FRONT = 326,
                BRIGHT_GRASS_LEFT = 350,
                BRIGHT_GRASS_RIGHT = 352,
                BRIGHT_GRASS_LEFT_CONCAVE_CORNER = 325,
                BRIGHT_GRASS_LEFT_CONVEX_CORNER = 328,
                BRIGHT_GRASS_RIGHT_CONCAVE_CORNER = 327,
                BRIGHT_GRASS_RIGHT_CONVEX_CORNER = 378,

                HAS_BLACK_GRASS = true
            };

            public static readonly DeepWoodsRowTileMatrix BOTTOM = new DeepWoodsRowTileMatrix()
            {
                FOREST_BACK = new int[] { 1068, 1069 },
                FOREST_FRONT = new int[] { 1042, 1043 },

                FOREST_LEFT_BACK = new int[] { 1015 },
                FOREST_LEFT_FRONT = new int[] { 991, 1016 },

                FOREST_RIGHT_BACK = new int[] { 995 },
                FOREST_RIGHT_FRONT = new int[] { 994, 1019 },

                FOREST_LEFT_CORNER_BACK = 1065,
                FOREST_LEFT_CONCAVE_CORNER = 1041,
                FOREST_LEFT_CONVEX_CORNER = 1017,

                FOREST_RIGHT_CORNER_BACK = 1070,
                FOREST_RIGHT_CONCAVE_CORNER = 1044,
                FOREST_RIGHT_CONVEX_CORNER = 1018,

                DARK_GRASS_FRONT = 355,
                DARK_GRASS_LEFT = 381,
                DARK_GRASS_RIGHT = 379,
                DARK_GRASS_LEFT_CONCAVE_CORNER = 382,
                DARK_GRASS_LEFT_CONVEX_CORNER = 356,
                DARK_GRASS_RIGHT_CONCAVE_CORNER = 332,
                DARK_GRASS_RIGHT_CONVEX_CORNER = 354,

                BRIGHT_GRASS_FRONT = 376,
                BRIGHT_GRASS_LEFT = 350,
                BRIGHT_GRASS_RIGHT = 352,
                BRIGHT_GRASS_LEFT_CONCAVE_CORNER = 375,
                BRIGHT_GRASS_LEFT_CONVEX_CORNER = 403,
                BRIGHT_GRASS_RIGHT_CONCAVE_CORNER = 377,
                BRIGHT_GRASS_RIGHT_CONVEX_CORNER = 353,

                HAS_BLACK_GRASS = false
            };

            public static readonly DeepWoodsRowTileMatrix LEFT = new DeepWoodsRowTileMatrix()
            {
                FOREST_BACK = new int[] { 1015 },
                FOREST_FRONT = new int[] { 991, 1016 },

                FOREST_LEFT_BACK = new int[] { 941/*942, 943*/ },
                FOREST_LEFT_FRONT = new int[] { 967, 968 },

                FOREST_RIGHT_BACK = new int[] { 1068, 1069 },
                FOREST_RIGHT_FRONT = new int[] { 1042, 1043 },

                FOREST_LEFT_CORNER_BACK = 940,
                FOREST_LEFT_CONCAVE_CORNER = 966,
                FOREST_LEFT_CONVEX_CORNER = 992,

                FOREST_RIGHT_CORNER_BACK = 1040,
                FOREST_RIGHT_CONCAVE_CORNER = 1041,
                FOREST_RIGHT_CONVEX_CORNER = 1017,

                DARK_GRASS_FRONT = 381,
                DARK_GRASS_LEFT = 405,
                DARK_GRASS_RIGHT = 355,
                DARK_GRASS_LEFT_CONCAVE_CORNER = 357,
                DARK_GRASS_LEFT_CONVEX_CORNER = 406,
                DARK_GRASS_RIGHT_CONCAVE_CORNER = 382,
                DARK_GRASS_RIGHT_CONVEX_CORNER = 356,

                BRIGHT_GRASS_FRONT = 350,
                BRIGHT_GRASS_LEFT = 326,
                BRIGHT_GRASS_RIGHT = 376,
                BRIGHT_GRASS_LEFT_CONCAVE_CORNER = 325,
                BRIGHT_GRASS_LEFT_CONVEX_CORNER = 328,
                BRIGHT_GRASS_RIGHT_CONCAVE_CORNER = 375,
                BRIGHT_GRASS_RIGHT_CONVEX_CORNER = 403,

                HAS_BLACK_GRASS = false
            };

            public static readonly DeepWoodsRowTileMatrix RIGHT = new DeepWoodsRowTileMatrix()
            {
                FOREST_BACK = new int[] { 995 },
                FOREST_FRONT = new int[] { 994, 1019 },

                FOREST_LEFT_BACK = new int[] { 941/*942, 943*/ },
                FOREST_LEFT_FRONT = new int[] { 967, 968 },

                FOREST_RIGHT_BACK = new int[] { 1068, 1069 },
                FOREST_RIGHT_FRONT = new int[] { 1042, 1043 },

                FOREST_LEFT_CORNER_BACK = 945,
                FOREST_LEFT_CONCAVE_CORNER = 969,
                FOREST_LEFT_CONVEX_CORNER = 993,

                FOREST_RIGHT_CORNER_BACK = 1069,
                FOREST_RIGHT_CONCAVE_CORNER = 1044,
                FOREST_RIGHT_CONVEX_CORNER = 1018,

                DARK_GRASS_FRONT = 379,
                DARK_GRASS_LEFT = 405,
                DARK_GRASS_RIGHT = 355,
                DARK_GRASS_LEFT_CONCAVE_CORNER = 407,
                DARK_GRASS_LEFT_CONVEX_CORNER = 404,
                DARK_GRASS_RIGHT_CONCAVE_CORNER = 332,
                DARK_GRASS_RIGHT_CONVEX_CORNER = 354,

                BRIGHT_GRASS_FRONT = 352,
                BRIGHT_GRASS_LEFT = 326,
                BRIGHT_GRASS_RIGHT = 376,
                BRIGHT_GRASS_LEFT_CONCAVE_CORNER = 327,
                BRIGHT_GRASS_LEFT_CONVEX_CORNER = 378,
                BRIGHT_GRASS_RIGHT_CONCAVE_CORNER = 377,
                BRIGHT_GRASS_RIGHT_CONVEX_CORNER = 353,

                HAS_BLACK_GRASS = false
            };
        }

        private class DeepWoodsCornerTileMatrix
        {
            public int[] HORIZONTAL_BACK;
            public int[] HORIZONTAL_FRONT;
            public int[] VERTICAL_BACK;
            public int[] VERTICAL_FRONT;
            public int CONCAVE_CORNER_DIAGONAL_BACK;
            public int CONCAVE_CORNER_HORIZONTAL_BACK;
            public int CONCAVE_CORNER_VERTICAL_BACK;
            public int CONCAVE_CORNER;
            public int CONVEX_CORNER;

            public int DARK_GRASS_HORIZONTAL;
            public int DARK_GRASS_VERTICAL;
            public int DARK_GRASS_CONCAVE_CORNER;
            public int DARK_GRASS_CONVEX_CORNER;

            public int BLACK_GRASS_HORIZONTAL;
            public int BLACK_GRASS_VERTICAL;
            public int BLACK_GRASS_CONCAVE_CORNER;
            public int BLACK_GRASS_CONVEX_CORNER;

            public bool HAS_BLACK_GRASS;

            private DeepWoodsCornerTileMatrix() { }

            public static readonly DeepWoodsCornerTileMatrix TOP_LEFT = new DeepWoodsCornerTileMatrix()
            {
                HORIZONTAL_BACK = new int[] { 941/*942, 943*/ },
                HORIZONTAL_FRONT = new int[] { 967, 968 },
                VERTICAL_BACK = new int[] { /*990,*/ 1015 },
                VERTICAL_FRONT = new int[] { 991, 1016 },
                CONCAVE_CORNER_DIAGONAL_BACK = 940,
                CONCAVE_CORNER_HORIZONTAL_BACK = 941,
                CONCAVE_CORNER_VERTICAL_BACK = 965,
                CONCAVE_CORNER = 966,
                CONVEX_CORNER = 992,
                DARK_GRASS_HORIZONTAL = 405,
                DARK_GRASS_VERTICAL = 381,
                DARK_GRASS_CONCAVE_CORNER = 357,
                DARK_GRASS_CONVEX_CORNER = 406,
                BLACK_GRASS_HORIZONTAL = 1119,
                BLACK_GRASS_VERTICAL = 1121,
                BLACK_GRASS_CONCAVE_CORNER = 1092,
                BLACK_GRASS_CONVEX_CORNER = 1120,
                HAS_BLACK_GRASS = true
            };

            public static readonly DeepWoodsCornerTileMatrix TOP_RIGHT = new DeepWoodsCornerTileMatrix()
            {
                HORIZONTAL_BACK = new int[] { 942 },
                HORIZONTAL_FRONT = new int[] { 967, 968 },
                VERTICAL_BACK = new int[] { 995 },
                VERTICAL_FRONT = new int[] { 994, 1019 },
                CONCAVE_CORNER_DIAGONAL_BACK = 945,
                CONCAVE_CORNER_HORIZONTAL_BACK = 944 /*942?*/,
                CONCAVE_CORNER_VERTICAL_BACK = 970,
                CONCAVE_CORNER = 969,
                CONVEX_CORNER = 993,
                DARK_GRASS_HORIZONTAL = 405,
                DARK_GRASS_VERTICAL = 379,
                DARK_GRASS_CONCAVE_CORNER = 407,
                DARK_GRASS_CONVEX_CORNER = 404,
                BLACK_GRASS_HORIZONTAL = 1119,
                BLACK_GRASS_VERTICAL = 1096,
                BLACK_GRASS_CONCAVE_CORNER = 1093,
                BLACK_GRASS_CONVEX_CORNER = 1095,
                HAS_BLACK_GRASS = true
            };

            public static readonly DeepWoodsCornerTileMatrix BOTTOM_LEFT = new DeepWoodsCornerTileMatrix()
            {
                HORIZONTAL_BACK = new int[] { 1068, 1069 },
                HORIZONTAL_FRONT = new int[] { 1042, 1043 },
                VERTICAL_BACK = new int[] { 990, 1015 },
                VERTICAL_FRONT = new int[] { 991, 1016 },
                CONCAVE_CORNER_DIAGONAL_BACK = 1065,
                CONCAVE_CORNER_HORIZONTAL_BACK = 1066,
                CONCAVE_CORNER_VERTICAL_BACK = 1040,
                CONCAVE_CORNER = 1041,
                CONVEX_CORNER = 1017,
                DARK_GRASS_HORIZONTAL = 355,
                DARK_GRASS_VERTICAL = 381,
                DARK_GRASS_CONCAVE_CORNER = 382,
                DARK_GRASS_CONVEX_CORNER = 356,
                HAS_BLACK_GRASS = false
            };

            public static readonly DeepWoodsCornerTileMatrix BOTTOM_RIGHT = new DeepWoodsCornerTileMatrix()
            {
                HORIZONTAL_BACK = new int[] { 1068, 1069 },
                HORIZONTAL_FRONT = new int[] { 1042, 1043 },
                VERTICAL_BACK = new int[] { 995 },
                VERTICAL_FRONT = new int[] { 994, 1019 },
                CONCAVE_CORNER_DIAGONAL_BACK = 1070,
                CONCAVE_CORNER_HORIZONTAL_BACK = 1069,
                CONCAVE_CORNER_VERTICAL_BACK = 1045,
                CONCAVE_CORNER = 1044,
                CONVEX_CORNER = 1018,
                DARK_GRASS_HORIZONTAL = 355,
                DARK_GRASS_VERTICAL = 379,
                DARK_GRASS_CONCAVE_CORNER = 332,
                DARK_GRASS_CONVEX_CORNER = 354,
                HAS_BLACK_GRASS = false
            };
        }

        private DeepWoods deepWoods;
        private DeepWoodsRandom random;
        private DeepWoodsSpaceManager spaceManager;
        private Map map;
        private Dictionary<ExitDirection, Location> exitLocations;
        private TileSheet tileSheet;
        private Layer backLayer;
        private Layer buildingsLayer;
        private Layer frontLayer;
        private Layer alwaysFrontLayer;


        private DeepWoodsBuilder(DeepWoods deepWoods, DeepWoodsRandom random, DeepWoodsSpaceManager spaceManager, Map map, Dictionary<ExitDirection, Location>  exitLocations)
        {
            this.deepWoods = deepWoods;
            this.random = random;
            this.spaceManager = spaceManager;
            this.map = map;
            this.exitLocations = exitLocations;
            this.tileSheet = map.GetTileSheet(DeepWoods.DEFAULT_OUTDOOR_TILESHEET_ID);
            this.backLayer = map.GetLayer("Back");
            this.buildingsLayer = map.GetLayer("Buildings");
            this.frontLayer = map.GetLayer("Front");
            this.alwaysFrontLayer = map.GetLayer("AlwaysFront");
        }

        public static void Build(DeepWoods deepWoods, DeepWoodsRandom random, DeepWoodsSpaceManager spaceManager, Map map, Dictionary<ExitDirection, Location> exitLocations)
        {
            new DeepWoodsBuilder(deepWoods, random, spaceManager, map, exitLocations).Build();
        }

        private void Build()
        {
            GenerateForestBorder();
            GenerateForestPatches();
            GenerateGround();
        }

        private int GetRandomGrassTileIndex(GrassType grassType)
        {
            switch (grassType)
            {
                case GrassType.BLACK:
                    return 1094;

                case GrassType.DARK:
                    return this.random.GetRandomValue(new WeightedValue[] {
                        new WeightedValue(380, 80),
                        new WeightedValue(156, 20)
                    });

                case GrassType.BRIGHT:
                    return this.random.GetRandomValue(new WeightedValue[] {
                        new WeightedValue(175, 100),
                        new WeightedValue(275, 100),
                        new WeightedValue(402, 100),
                        new WeightedValue(400, 15),
                        new WeightedValue(401, 15),
                        new WeightedValue(150, 15),
                        new WeightedValue(254, 5),
                        new WeightedValue(255, 5),
                        new WeightedValue(256, 5)
                    });

                case GrassType.NORMAL:
                    return this.random.GetRandomValue(new WeightedValue[] {
                        new WeightedValue(351, 100),
                        new WeightedValue(300, 10),
                        new WeightedValue(304, 10),
                        new WeightedValue(305, 10),
                        new WeightedValue(329, 1)
                    });
            }

            throw new ArgumentException("Unknown GrassType: " + grassType);
        }

        private void GenerateGround()
        {
            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    PlaceTile(backLayer, GetRandomGrassTileIndex(GrassType.NORMAL), x, y);
                }
            }
        }

        private void GenerateForestBorder()
        {
            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            Size topLeftCornerSize = GenerateForestCorner(0, 0, 1, 1, DeepWoodsCornerTileMatrix.TOP_LEFT);
            Size topRightCornerSize = GenerateForestCorner(mapWidth - 1, 0, -1, 1, DeepWoodsCornerTileMatrix.TOP_RIGHT);
            Size bottomLeftCornerSize = GenerateForestCorner(0, mapHeight - 1, 1, -1, DeepWoodsCornerTileMatrix.BOTTOM_LEFT);
            Size bottomRightCornerSize = GenerateForestCorner(mapWidth - 1, mapHeight - 1, -1, -1, DeepWoodsCornerTileMatrix.BOTTOM_RIGHT);

            GenerateForestRow(
                new Placing(new Location(topLeftCornerSize.Width, 0), PlacingDirection.RIGHTWARDS, PlacingDirection.DOWNWARDS),
                mapWidth - (topLeftCornerSize.Width + topRightCornerSize.Width),
                ExitDirection.TOP,
                DeepWoodsRowTileMatrix.TOP);

            GenerateForestRow(
                new Placing(new Location(bottomLeftCornerSize.Width, mapHeight - 1), PlacingDirection.RIGHTWARDS, PlacingDirection.UPWARDS),
                mapWidth - (bottomLeftCornerSize.Width + bottomRightCornerSize.Width),
                ExitDirection.BOTTOM,
                DeepWoodsRowTileMatrix.BOTTOM);

            GenerateForestRow(
                new Placing(new Location(0, topLeftCornerSize.Height), PlacingDirection.DOWNWARDS, PlacingDirection.RIGHTWARDS),
                mapHeight - (topLeftCornerSize.Height + bottomLeftCornerSize.Height),
                ExitDirection.LEFT,
                DeepWoodsRowTileMatrix.LEFT);

            GenerateForestRow(
                new Placing(new Location(mapWidth - 1, topRightCornerSize.Height), PlacingDirection.DOWNWARDS, PlacingDirection.LEFTWARDS),
                mapHeight - (topRightCornerSize.Height + bottomRightCornerSize.Height),
                ExitDirection.RIGHT,
                DeepWoodsRowTileMatrix.RIGHT);

            GenerateExits();
        }

        private void GenerateForestRow(
            Placing placing,
            int numTiles,
            ExitDirection exitDir,
            DeepWoodsRowTileMatrix matrix)
        {
            if (this.exitLocations.ContainsKey(exitDir))
            {
                Location exitPosition = this.exitLocations[exitDir];

                int numTilesFromStartToExit = placing.DistanceTo(exitPosition);
                int numTilesFromExitToEnd = numTiles - numTilesFromStartToExit - 1;
                
                GenerateForestRow(placing, numTilesFromStartToExit - DeepWoodsSpaceManager.EXIT_RADIUS, matrix);
                GenerateForestRow(new Placing(placing.Replace(exitPosition), DeepWoodsSpaceManager.EXIT_RADIUS + 1, 0), numTilesFromExitToEnd - DeepWoodsSpaceManager.EXIT_RADIUS, matrix);
            }
            else
            {
                GenerateForestRow(placing, numTiles, matrix);
            }
        }

        private void GenerateForestRow(Placing placing, int numTiles, DeepWoodsRowTileMatrix matrix, int y = 0, bool noBlackGrass = false)
        {
            // Out of range check
            if (numTiles <= 0)
            {
                ModEntry.Log("GenerateForestRow out of range! dir: " + placing.dir + ", numTiles: " + numTiles, StardewModdingAPI.LogLevel.Warn);
                return;
            }

            bool lastStepWasBumpOut = false;
            for (int x = 0; x < numTiles; x++)
            {
                if (y > 0 && (x >= (numTiles - Math.Abs(y)) || (!lastStepWasBumpOut && this.random.GetChance(CHANCE_FOR_FOREST_ROW_BUMP))))
                {
                    // Bump back!
                    PlaceTile(buildingsLayer, PLAIN_FOREST_BACKGROUND, placing, x, y - 1);
                    PlaceTile(alwaysFrontLayer, matrix.FOREST_LEFT_CORNER_BACK, placing, x, y - 1);
                    PlaceTile(alwaysFrontLayer, matrix.FOREST_LEFT_CONCAVE_CORNER, placing, x, y + 0);
                    PlaceTile(alwaysFrontLayer, matrix.FOREST_LEFT_CONVEX_CORNER, placing, x, y + 1);
                    if (!noBlackGrass && matrix.HAS_BLACK_GRASS)
                    {
                        PlaceTile(backLayer, GetRandomGrassTileIndex(GrassType.BLACK), placing, x, y + 0, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.BLACK_GRASS_LEFT_CONCAVE_CORNER, placing, x, y + 1, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.BLACK_GRASS_LEFT_CONVEX_CORNER, placing, x, y + 2, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.DARK_GRASS_LEFT_CONVEX_CORNER, placing, x, y + 3, PlaceMode.OVERRIDE);
                    }
                    else
                    {
                        PlaceTile(backLayer, GetRandomGrassTileIndex(GrassType.DARK), placing, x, y + 0, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.DARK_GRASS_LEFT_CONCAVE_CORNER, placing, x, y + 1, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.DARK_GRASS_LEFT_CONVEX_CORNER, placing, x, y + 2, PlaceMode.OVERRIDE);
                    }
                    y--;
                    lastStepWasBumpOut = false;
                }
                else if (x < (numTiles - (2 + Math.Abs(y))) && y < FOREST_ROW_MAX_INWARDS_BUMP && this.random.GetChance(CHANCE_FOR_FOREST_ROW_BUMP))
                {
                    // Bump out!
                    y++;
                    PlaceTile(buildingsLayer, PLAIN_FOREST_BACKGROUND, placing, x, y - 1);
                    PlaceTile(alwaysFrontLayer, matrix.FOREST_RIGHT_CORNER_BACK, placing, x, y - 1);
                    PlaceTile(alwaysFrontLayer, matrix.FOREST_RIGHT_CONCAVE_CORNER, placing, x, y + 0);
                    PlaceTile(alwaysFrontLayer, matrix.FOREST_RIGHT_CONVEX_CORNER, placing, x, y + 1);
                    if (!noBlackGrass && matrix.HAS_BLACK_GRASS)
                    {
                        PlaceTile(backLayer, GetRandomGrassTileIndex(GrassType.BLACK), placing, x, y + 0, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.BLACK_GRASS_RIGHT_CONCAVE_CORNER, placing, x, y + 1, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.BLACK_GRASS_RIGHT_CONVEX_CORNER, placing, x, y + 2, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.DARK_GRASS_RIGHT_CONVEX_CORNER, placing, x, y + 3, PlaceMode.OVERRIDE);
                    }
                    else
                    {
                        PlaceTile(backLayer, GetRandomGrassTileIndex(GrassType.DARK), placing, x, y + 0, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.DARK_GRASS_RIGHT_CONCAVE_CORNER, placing, x, y + 1, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.DARK_GRASS_RIGHT_CONVEX_CORNER, placing, x, y + 2, PlaceMode.OVERRIDE);
                    }
                    lastStepWasBumpOut = true;
                }
                else
                {
                    // TODO: Randomly (and sparsely) add "halfed" forest patches
                    PlaceTile(buildingsLayer, PLAIN_FOREST_BACKGROUND, placing, x, y + 0);
                    PlaceTile(alwaysFrontLayer, matrix.FOREST_BACK, placing, x, y + 0);
                    PlaceTile(alwaysFrontLayer, matrix.FOREST_FRONT, placing, x, y + 1);
                    if (!noBlackGrass && matrix.HAS_BLACK_GRASS)
                    {
                        PlaceTile(backLayer, GetRandomGrassTileIndex(GrassType.BLACK), placing, x, y + 0, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, GetRandomGrassTileIndex(GrassType.BLACK), placing, x, y + 1, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.BLACK_GRASS_FRONT, placing, x, y + 2, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.DARK_GRASS_FRONT, placing, x, y + 3, PlaceMode.OVERRIDE);
                        if (!lastStepWasBumpOut && x > 1 && x < numTiles - 2 && x % 2 == 0 && this.random.GetChance(CHANCE_FOR_FOREST_ROW_TREESTUMPS))
                        {
                            PlaceTile(buildingsLayer, FOREST_ROW_TREESTUMP_LEFT, placing, x - 1, y + 1);
                            PlaceTile(buildingsLayer, FOREST_ROW_TREESTUMP_RIGHT, placing, x, y + 1);
                        }
                    }
                    else
                    {
                        PlaceTile(backLayer, GetRandomGrassTileIndex(GrassType.DARK), placing, x, y + 0, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, GetRandomGrassTileIndex(GrassType.DARK), placing, x, y + 1, PlaceMode.OVERRIDE);
                        PlaceTile(backLayer, matrix.DARK_GRASS_FRONT, placing, x, y + 2, PlaceMode.OVERRIDE);
                    }
                    lastStepWasBumpOut = false;
                }

                // Fill tiles behind "bumped out" row
                for (int yy = y; yy >= 0; yy--)
                {
                    if (PlaceTile(alwaysFrontLayer, GetRandomForestFillerTileIndex(), placing, x, yy))
                    {
                        PlaceTile(buildingsLayer, PLAIN_FOREST_BACKGROUND, placing, x, yy);
                        PlaceTile(backLayer, GetRandomGrassTileIndex(GrassType.BLACK), placing, x, yy, PlaceMode.OVERRIDE);
                    }
                }
            }
        }

        private Size GenerateForestCorner(int startXPos, int startYPos, int xDir, int yDir, DeepWoodsCornerTileMatrix matrix)
        {
            int width = DeepWoodsSpaceManager.MAX_CORNER_SIZE; // this.random.GetRandomValue(DeepWoodsSpaceManager.MIN_CORNER_SIZE, DeepWoodsSpaceManager.MAX_CORNER_SIZE);
            int height = DeepWoodsSpaceManager.MAX_CORNER_SIZE; //this.random.GetRandomValue(DeepWoodsSpaceManager.MIN_CORNER_SIZE, DeepWoodsSpaceManager.MAX_CORNER_SIZE);

            float ratio = (float)height / (float)width;
            int chance = (int)((100f / (ratio + 1f)) * ratio);
            Probability probability = new Probability(chance, 100);

            int endXPos = startXPos + ((width - 1) * xDir);
            int endYPos = startYPos + ((height - 1) * yDir);

            int curXPos = endXPos;
            int curYPos = startYPos;

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
                    PlaceTile(buildingsLayer, PLAIN_FOREST_BACKGROUND, curXPos, curYPos + 0 * yDir);
                    PlaceTile(alwaysFrontLayer, matrix.CONCAVE_CORNER_HORIZONTAL_BACK, curXPos, curYPos + 0 * yDir);
                    PlaceTile(alwaysFrontLayer, matrix.CONCAVE_CORNER, curXPos, curYPos + 1 * yDir);
                    PlaceTile(alwaysFrontLayer, matrix.CONVEX_CORNER, curXPos, curYPos + 2 * yDir);

                    PlaceTile(backLayer, GetRandomGrassTileIndex(matrix.HAS_BLACK_GRASS ? GrassType.BLACK : GrassType.DARK), curXPos, curYPos + 1 * yDir);
                    PlaceTile(backLayer, GetRandomGrassTileIndex(matrix.HAS_BLACK_GRASS ? GrassType.BLACK : GrassType.DARK), curXPos, curYPos + 2 * yDir);

                    PlaceTile(backLayer, matrix.HAS_BLACK_GRASS ? matrix.BLACK_GRASS_CONCAVE_CORNER : matrix.DARK_GRASS_CONCAVE_CORNER, curXPos + 1 * xDir, curYPos + 1 * yDir);
                    PlaceTile(backLayer, matrix.HAS_BLACK_GRASS ? matrix.BLACK_GRASS_CONVEX_CORNER : matrix.DARK_GRASS_CONVEX_CORNER, curXPos + 1 * xDir, curYPos + 2 * yDir);
                    PlaceTile(backLayer, matrix.HAS_BLACK_GRASS ? matrix.BLACK_GRASS_HORIZONTAL : matrix.DARK_GRASS_HORIZONTAL, curXPos + 0 * xDir, curYPos + 2 * yDir);

                    if (matrix.HAS_BLACK_GRASS)
                    {
                        // Add dark grass
                        PlaceTile(backLayer, matrix.DARK_GRASS_CONCAVE_CORNER, curXPos + 2 * xDir, curYPos + 2 * yDir);
                        PlaceTile(backLayer, matrix.DARK_GRASS_CONVEX_CORNER, curXPos + 2 * xDir, curYPos + 3 * yDir);
                        PlaceTile(backLayer, matrix.DARK_GRASS_HORIZONTAL, curXPos + 1 * xDir, curYPos + 3 * yDir);
                        PlaceTile(backLayer, matrix.DARK_GRASS_HORIZONTAL, curXPos + 0 * xDir, curYPos + 3 * yDir);
                    }

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
                    PlaceTile(buildingsLayer, PLAIN_FOREST_BACKGROUND, curXPos, curYPos + 0 * yDir);
                    PlaceTile(alwaysFrontLayer, this.random.GetRandomValue(matrix.HORIZONTAL_BACK), curXPos, curYPos + 0 * yDir);
                    PlaceTile(alwaysFrontLayer, this.random.GetRandomValue(matrix.HORIZONTAL_FRONT), curXPos, curYPos + 1 * yDir);

                    PlaceTile(backLayer, matrix.HAS_BLACK_GRASS ? matrix.BLACK_GRASS_HORIZONTAL : matrix.DARK_GRASS_HORIZONTAL, curXPos, curYPos + 1 * yDir);

                    if (matrix.HAS_BLACK_GRASS)
                    {
                        // Add dark grass
                        PlaceTile(backLayer, matrix.DARK_GRASS_HORIZONTAL, curXPos, curYPos + 2 * yDir);
                    }

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
                    PlaceTile(buildingsLayer, PLAIN_FOREST_BACKGROUND, curXPos - 1 * xDir, curYPos + 0 * yDir);
                    PlaceTile(buildingsLayer, PLAIN_FOREST_BACKGROUND, curXPos - 0 * xDir, curYPos + 0 * yDir);
                    PlaceTile(buildingsLayer, PLAIN_FOREST_BACKGROUND, curXPos - 1 * xDir, curYPos + 1 * yDir);
                    PlaceTile(alwaysFrontLayer, matrix.CONCAVE_CORNER_DIAGONAL_BACK, curXPos - 1 * xDir, curYPos + 0 * yDir);
                    PlaceTile(alwaysFrontLayer, matrix.CONCAVE_CORNER_HORIZONTAL_BACK, curXPos - 0 * xDir, curYPos + 0 * yDir);
                    PlaceTile(alwaysFrontLayer, matrix.CONCAVE_CORNER_VERTICAL_BACK, curXPos - 1 * xDir, curYPos + 1 * yDir);
                    PlaceTile(alwaysFrontLayer, matrix.CONCAVE_CORNER, curXPos - 0 * xDir, curYPos + 1 * yDir);
                    curYPos += yDir;
                    if (curYPos != endYPos)
                    {
                        height -= Math.Abs(curYPos - endYPos);
                    }
                    curXPos -= xDir;
                }
            }

            deepWoods.lightSources.Add(new Vector2(startXPos, startYPos));

            return new Size(width, height);
        }

        private void GenerateExits()
        {
            foreach (var exit in this.exitLocations)
            {
                switch (exit.Key)
                {
                    case ExitDirection.TOP:
                        GenerateExit(new Placing(exit.Value, PlacingDirection.RIGHTWARDS, PlacingDirection.DOWNWARDS), DeepWoodsRowTileMatrix.TOP);
                        break;
                    case ExitDirection.BOTTOM:
                        GenerateExit(new Placing(exit.Value, PlacingDirection.RIGHTWARDS, PlacingDirection.UPWARDS), DeepWoodsRowTileMatrix.BOTTOM);
                        break;
                    case ExitDirection.LEFT:
                        GenerateExit(new Placing(exit.Value, PlacingDirection.DOWNWARDS, PlacingDirection.RIGHTWARDS), DeepWoodsRowTileMatrix.LEFT);
                        break;
                    case ExitDirection.RIGHT:
                        GenerateExit(new Placing(exit.Value, PlacingDirection.DOWNWARDS, PlacingDirection.LEFTWARDS), DeepWoodsRowTileMatrix.RIGHT);
                        break;
                }
                deepWoods.lightSources.Add(new Vector2(exit.Value.X, exit.Value.Y));
            }
        }

        private void GenerateExit(Placing placing, DeepWoodsRowTileMatrix matrix)
        {
            // Add forest pieces left and right
            PlaceTile(alwaysFrontLayer, matrix.FOREST_LEFT_FRONT, placing, -DeepWoodsSpaceManager.EXIT_RADIUS, 0);
            PlaceTile(alwaysFrontLayer, matrix.FOREST_LEFT_CONVEX_CORNER, placing, -DeepWoodsSpaceManager.EXIT_RADIUS, 1);
            PlaceTile(alwaysFrontLayer, matrix.FOREST_RIGHT_FRONT, placing, DeepWoodsSpaceManager.EXIT_RADIUS, 0);
            PlaceTile(alwaysFrontLayer, matrix.FOREST_RIGHT_CONVEX_CORNER, placing, DeepWoodsSpaceManager.EXIT_RADIUS, 1);

            // Add bright grass some paces inwards
            int brightGrassPacesInwards = this.random.GetRandomValue(2, 3);

            PlaceTile(backLayer, matrix.BRIGHT_GRASS_RIGHT_CONCAVE_CORNER, placing, -1, 0);
            PlaceTile(backLayer, GetRandomGrassTileIndex(GrassType.BRIGHT), placing, 0, 0);
            PlaceTile(backLayer, matrix.BRIGHT_GRASS_LEFT_CONCAVE_CORNER, placing, 1, 0);

            for (int i = 1; i <= brightGrassPacesInwards; i++)
            {
                PlaceTile(backLayer, matrix.BRIGHT_GRASS_RIGHT, placing, -1, i);
                PlaceTile(backLayer, GetRandomGrassTileIndex(GrassType.BRIGHT), placing, 0, i);
                PlaceTile(backLayer, matrix.BRIGHT_GRASS_LEFT, placing, 1, i);
            }

            PlaceTile(backLayer, matrix.BRIGHT_GRASS_RIGHT_CONVEX_CORNER, placing, -1, 3);
            PlaceTile(backLayer, matrix.BRIGHT_GRASS_FRONT, placing, 0, 3);
            PlaceTile(backLayer, matrix.BRIGHT_GRASS_LEFT_CONVEX_CORNER, placing, 1, 3);

            // Add forest "shadow" (dark grass) left and right
            PlaceTile(backLayer, matrix.DARK_GRASS_LEFT, placing, -DeepWoodsSpaceManager.EXIT_RADIUS, 0);
            PlaceTile(backLayer, matrix.DARK_GRASS_LEFT, placing, -DeepWoodsSpaceManager.EXIT_RADIUS, 1);
            PlaceTile(backLayer, matrix.DARK_GRASS_RIGHT, placing, DeepWoodsSpaceManager.EXIT_RADIUS, 0);
            PlaceTile(backLayer, matrix.DARK_GRASS_RIGHT, placing, DeepWoodsSpaceManager.EXIT_RADIUS, 1);

            if (matrix.HAS_BLACK_GRASS)
            {
                // longer dark grass to fit row with black grass
                PlaceTile(backLayer, matrix.DARK_GRASS_LEFT, placing, -DeepWoodsSpaceManager.EXIT_RADIUS, 2);
                PlaceTile(backLayer, matrix.DARK_GRASS_LEFT_CONVEX_CORNER, placing, -DeepWoodsSpaceManager.EXIT_RADIUS, 3);
                PlaceTile(backLayer, matrix.DARK_GRASS_RIGHT, placing, DeepWoodsSpaceManager.EXIT_RADIUS, 2);
                PlaceTile(backLayer, matrix.DARK_GRASS_RIGHT_CONVEX_CORNER, placing, DeepWoodsSpaceManager.EXIT_RADIUS, 3);

                // Override black shadows from row with corner tiles
                PlaceTile(backLayer, matrix.BLACK_GRASS_LEFT, placing, -(DeepWoodsSpaceManager.EXIT_RADIUS + 1), 1, PlaceMode.OVERRIDE);
                PlaceTile(backLayer, matrix.BLACK_GRASS_LEFT_CONVEX_CORNER, placing, -(DeepWoodsSpaceManager.EXIT_RADIUS + 1), 2, PlaceMode.OVERRIDE);
                PlaceTile(backLayer, matrix.BLACK_GRASS_RIGHT, placing, DeepWoodsSpaceManager.EXIT_RADIUS + 1, 1, PlaceMode.OVERRIDE);
                PlaceTile(backLayer, matrix.BLACK_GRASS_RIGHT_CONVEX_CORNER, placing, DeepWoodsSpaceManager.EXIT_RADIUS + 1, 2, PlaceMode.OVERRIDE);
            }
            else
            {
                // simple dark grass corner to fit row shadow without black grass
                PlaceTile(backLayer, matrix.DARK_GRASS_LEFT_CONVEX_CORNER, placing, -DeepWoodsSpaceManager.EXIT_RADIUS, 2);
                PlaceTile(backLayer, matrix.DARK_GRASS_RIGHT_CONVEX_CORNER, placing, DeepWoodsSpaceManager.EXIT_RADIUS, 2);
            }
        }

        private bool PlaceTile(Layer layer, int[] tileIndices, Placing placing, int steps, int stepsInward, PlaceMode placeMode = PlaceMode.DONT_OVERRIDE)
        {
            return PlaceTile(layer, this.random.GetRandomValue(tileIndices), placing, steps, stepsInward, placeMode);
        }

        private bool PlaceTile(Layer layer, int tileIndex, Placing placing, int steps, int stepsInward, PlaceMode placeMode = PlaceMode.DONT_OVERRIDE)
        {
            int x = placing.location.X + (steps * placing.XDir) + (stepsInward * placing.XDirInward);
            int y = placing.location.Y + (steps * placing.YDir) + (stepsInward * placing.YDirInward);
            return PlaceTile(layer, tileIndex, x, y, placeMode);
        }

        private bool PlaceTile(Layer layer, int tileIndex, int x, int y, PlaceMode placeMode = PlaceMode.DONT_OVERRIDE)
        {
            if (x < 0 || y < 0 || x >= this.spaceManager.GetMapWidth() || y >= this.spaceManager.GetMapHeight())
            {
                // ModEntry.Log("Tile out of range: " +  x + ", " + y, StardewModdingAPI.LogLevel.Debug);
                return false;
            }

            if (placeMode == PlaceMode.OVERRIDE || layer.Tiles[x, y] == null)
            {
                layer.Tiles[x, y] = new StaticTile(layer, tileSheet, BlendMode.Alpha, tileIndex);
                return true;
            }

            return false;
        }

        private bool ClearTile(Layer layer, int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.spaceManager.GetMapWidth() || y >= this.spaceManager.GetMapHeight())
            {
                // ModEntry.Log("Tile out of range: " +  x + ", " + y, StardewModdingAPI.LogLevel.Debug);
                return false;
            }

            layer.Tiles[x, y] = null;
            return true;
        }

        private void FillForestTile(int x, int y)
        {
            PlaceTile(buildingsLayer, PLAIN_FOREST_BACKGROUND, x, y);
            PlaceTile(alwaysFrontLayer, GetRandomForestFillerTileIndex(), x, y);
        }

        private int GetRandomForestFillerTileIndex()
        {
            return this.random.GetRandomValue(FOREST_BACKGROUND, CHANCE_FOR_NOLEAVE_FOREST_FILLER);
        }

        private void GenerateForestPatch(xTile.Dimensions.Rectangle rectangle)
        {
            int offset = FOREST_ROW_MAX_INWARDS_BUMP + 1;

            GenerateForestRow(
                new Placing(new Location(rectangle.X + offset + 1, rectangle.Y + offset), PlacingDirection.DOWNWARDS, PlacingDirection.LEFTWARDS),
                rectangle.Height - offset * 2,
                DeepWoodsRowTileMatrix.RIGHT,
                0);

            GenerateForestRow(
                new Placing(new Location(rectangle.X + rectangle.Width - offset - 1, rectangle.Y + offset), PlacingDirection.DOWNWARDS, PlacingDirection.RIGHTWARDS),
                rectangle.Height - offset * 2,
                DeepWoodsRowTileMatrix.LEFT,
                0);

            GenerateForestRow(
                new Placing(new Location(rectangle.X + offset, rectangle.Y + offset + 1), PlacingDirection.RIGHTWARDS, PlacingDirection.UPWARDS),
                rectangle.Width - offset * 2,
                DeepWoodsRowTileMatrix.BOTTOM,
                0);

            GenerateForestRow(
                new Placing(new Location(rectangle.X + offset, rectangle.Y + rectangle.Height - offset - 1), PlacingDirection.RIGHTWARDS, PlacingDirection.DOWNWARDS),
                rectangle.Width - offset * 2,
                DeepWoodsRowTileMatrix.TOP,
                0,
                true);

            PlaceTile(backLayer, DeepWoodsRowTileMatrix.BOTTOM.DARK_GRASS_RIGHT_CONVEX_CORNER, rectangle.X + 2, rectangle.Y + 2);
            PlaceTile(backLayer, DeepWoodsRowTileMatrix.TOP.DARK_GRASS_RIGHT_CONVEX_CORNER, rectangle.X + 2, rectangle.Y + rectangle.Height - 3);
            PlaceTile(backLayer, DeepWoodsRowTileMatrix.BOTTOM.DARK_GRASS_LEFT_CONVEX_CORNER, rectangle.X + rectangle.Width - 3, rectangle.Y + 2);
            PlaceTile(backLayer, DeepWoodsRowTileMatrix.TOP.DARK_GRASS_LEFT_CONVEX_CORNER, rectangle.X + rectangle.Width - 3, rectangle.Y + rectangle.Height - 3);

            ClearTile(buildingsLayer, rectangle.X + 3, rectangle.Y + 3);
            ClearTile(buildingsLayer, rectangle.X + 3, rectangle.Y + rectangle.Height - 4);
            ClearTile(buildingsLayer, rectangle.X + rectangle.Width - 4, rectangle.Y + 3);
            ClearTile(buildingsLayer, rectangle.X + rectangle.Width - 4, rectangle.Y + rectangle.Height - 4);

            int minFillX = rectangle.X + offset + 2;
            int maxFillX = rectangle.X + rectangle.Width - offset - 1;
            int minFillY = rectangle.Y + offset + 2;
            int maxFillY = rectangle.Y + rectangle.Height - offset - 1;

            int numFillTiles = 0;
            for (int x = minFillX; x < maxFillX; x++)
            {
                for (int y = minFillY; y < maxFillY; y++)
                {
                    FillForestTile(x, y);
                    numFillTiles++;
                }
            }

            int maxLightSources = Math.Max(1, numFillTiles / NUM_TILES_PER_LIGHTSOURCE_IN_FOREST_PATCH);
            int numLightSources = 1 + this.random.GetRandomValue(0, maxLightSources);
            for (int i = 0; i < numLightSources; i++)
            {
                deepWoods.lightSources.Add(new Vector2(this.random.GetRandomValue(minFillX, maxFillX + 1), this.random.GetRandomValue(minFillY, maxFillY + 1)));
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
