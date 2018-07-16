
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using xTile.Dimensions;
using static DeepWoodsMod.DeepWoodsEnterExit;

namespace DeepWoodsMod
{
    class DeepWoodsSpaceManager
    {
        private const int TILE_SIZE_IN_ACTUAL_PIXEL = 64;

        public const int MIN_CORNER_SIZE = 3;
        public const int MAX_CORNER_SIZE = 8;

        /// <summary>Amount of tiles exits expand in each direction</summary>
        public const int EXIT_RADIUS = 2;

        private const int MIN_CORNER_DISTANCE_FOR_ENTER_LOCATION = MAX_CORNER_SIZE + EXIT_RADIUS + 2;   // => 12

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
        private const int FOREST_PATCH_SHRINK_STEP_SIZE = 2;

        public const int MINIMUM_TILES_FOR_MONSTER = 36;
        public const int MINIMUM_TILES_FOR_TERRAIN_FEATURE = 4;
        public const int MINIMUM_TILES_FOR_BAUBLE = 16;
        public const int MINIMUM_TILES_FOR_LEAVES = 16;

        private int mapWidth;
        private int mapHeight;
        private List<xTile.Dimensions.Rectangle> occupiedRectangles = new List<xTile.Dimensions.Rectangle>();

        public DeepWoodsSpaceManager(int mapWidth, int mapHeight)
        {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
        }

        public int GetMapWidth()
        {
            return this.mapWidth;
        }

        public int GetMapHeight()
        {
            return this.mapHeight;
        }

        private bool IntersectsWorld(xTile.Dimensions.Rectangle rectangle)
        {
            if (rectangle.X < FOREST_PATCH_MIN_GAP_TO_MAPBORDER)
                return true;

            if (rectangle.Y < FOREST_PATCH_MIN_GAP_TO_MAPBORDER)
                return true;

            if ((this.mapWidth - (rectangle.X + rectangle.Width)) < FOREST_PATCH_MIN_GAP_TO_MAPBORDER)
                return true;

            if ((this.mapHeight - (rectangle.Y + rectangle.Height)) < FOREST_PATCH_MIN_GAP_TO_MAPBORDER)
                return true;

            return false;
        }

        private bool IntersectsAnyOccupiedRectangle(xTile.Dimensions.Rectangle rectangle)
        {
            foreach (xTile.Dimensions.Rectangle occupiedRectangle in this.occupiedRectangles)
            {
                if (occupiedRectangle.Intersects(rectangle))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IntersectsAny(xTile.Dimensions.Rectangle rectangle)
        {
            return IntersectsWorld(rectangle) || IntersectsAnyOccupiedRectangle(rectangle);
        }

        public bool TryGetFreeRectangleForForestPatch(Location location, int wishWidth, int wishHeight, out xTile.Dimensions.Rectangle rectangle)
        {
            int minWidth = MIN_FOREST_PATCH_DIAMETER;
            int minHeight = MIN_FOREST_PATCH_DIAMETER;

            rectangle = new xTile.Dimensions.Rectangle(location.X - wishWidth / 2, location.Y - wishHeight / 2, wishWidth, wishHeight);

            while (IntersectsAny(rectangle))
            {
                int reachedEndCount = 0; // i have a knot in my brain, this should be cleaner
                if (rectangle.Width >= minWidth + FOREST_PATCH_SHRINK_STEP_SIZE)
                {
                    rectangle.Width -= FOREST_PATCH_SHRINK_STEP_SIZE;
                }
                else
                {
                    reachedEndCount++; // i have a knot in my brain, this should be cleaner
                }
                if (rectangle.Height >= minHeight + FOREST_PATCH_SHRINK_STEP_SIZE)
                {
                    rectangle.Height -= FOREST_PATCH_SHRINK_STEP_SIZE;
                }
                else
                {
                    reachedEndCount++; // i have a knot in my brain, this should be cleaner
                }
                if (reachedEndCount == 2) // i have a knot in my brain, this should be cleaner
                {
                    break;
                }
                rectangle.X = location.X - rectangle.Width / 2;
                rectangle.Y = location.Y - rectangle.Height / 2;
            }

            if (rectangle.Width >= minWidth && rectangle.Height >= minHeight && !IntersectsAny(rectangle))
            {
                this.occupiedRectangles.Add(rectangle);
                return true;
            }

            return false;
        }

        public Location GetRandomEnterLocation(EnterDirection enterDir, DeepWoodsRandom random)
        {
            int x, y;
            if (enterDir == EnterDirection.FROM_BOTTOM || enterDir == EnterDirection.FROM_TOP)
            {
                x = random.GetRandomValue(MIN_CORNER_DISTANCE_FOR_ENTER_LOCATION, this.mapWidth - MIN_CORNER_DISTANCE_FOR_ENTER_LOCATION);
                if (enterDir == EnterDirection.FROM_BOTTOM)
                {
                    y = this.mapHeight - 1;
                }
                else
                {
                    y = 0;
                }
            }
            else
            {
                y = random.GetRandomValue(MIN_CORNER_DISTANCE_FOR_ENTER_LOCATION, this.mapHeight - MIN_CORNER_DISTANCE_FOR_ENTER_LOCATION);
                if (enterDir == EnterDirection.FROM_RIGHT)
                {
                    x = this.mapWidth - 1;
                }
                else
                {
                    x = 0;
                }
            }
            return new Location(x, y);
        }

        public Location GetRandomExitLocation(ExitDirection exitDir, DeepWoodsRandom random)
        {
            if (exitDir == ExitDirection.BOTTOM)
            {
                return GetRandomEnterLocation(EnterDirection.FROM_BOTTOM, random);
            }
            else if (exitDir == ExitDirection.LEFT)
            {
                return GetRandomEnterLocation(EnterDirection.FROM_LEFT, random);
            }
            else if (exitDir == ExitDirection.RIGHT)
            {
                return GetRandomEnterLocation(EnterDirection.FROM_RIGHT, random);
            }
            else
            {
                return GetRandomEnterLocation(EnterDirection.FROM_TOP, random);
            }
        }

        public Location GetActualTitleSafeTopleftCorner()
        {
            Microsoft.Xna.Framework.Rectangle titleSafeArea = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
            int currentMapWidthInPixel = this.mapWidth * TILE_SIZE_IN_ACTUAL_PIXEL;
            Location location = new Location(titleSafeArea.Left, titleSafeArea.Top);
            if (currentMapWidthInPixel < titleSafeArea.Width)
            {
                location.X += (titleSafeArea.Width - currentMapWidthInPixel) / 2;
            }
            return location;
        }
    }
}
