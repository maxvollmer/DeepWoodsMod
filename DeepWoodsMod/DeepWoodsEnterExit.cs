
using System.Collections.Generic;
using xTile.Dimensions;

namespace DeepWoodsMod
{
    class DeepWoodsEnterExit
    {
        public class DeepWoodsExit
        {
            public Location location;
            public DeepWoods deepWoods;
            public DeepWoodsExit(Location location)
            {
                this.location = location;
                this.deepWoods = null;
            }
        }


        public enum EnterDirection
        {
            FROM_LEFT,
            FROM_TOP,
            FROM_RIGHT,
            FROM_BOTTOM
        }

        public enum ExitDirection
        {
            RIGHT,
            BOTTOM,
            LEFT,
            TOP
        }

        public static EnterDirection ExitDirToEnterDir(ExitDirection exitDir)
        {
            switch (exitDir)
            {
                case ExitDirection.RIGHT:
                    return EnterDirection.FROM_LEFT;
                case ExitDirection.BOTTOM:
                    return EnterDirection.FROM_TOP;
                case ExitDirection.LEFT:
                    return EnterDirection.FROM_RIGHT;
                case ExitDirection.TOP:
                    return EnterDirection.FROM_BOTTOM;
                default:
                    return EnterDirection.FROM_TOP;
            }
        }

        public static ExitDirection EnterDirToExitDir(EnterDirection enterDir)
        {
            switch (enterDir)
            {
                case EnterDirection.FROM_LEFT:
                    return ExitDirection.RIGHT;
                case EnterDirection.FROM_TOP:
                    return ExitDirection.BOTTOM;
                case EnterDirection.FROM_RIGHT:
                    return ExitDirection.LEFT;
                case EnterDirection.FROM_BOTTOM:
                    return ExitDirection.TOP;
                default:
                    return ExitDirection.BOTTOM;
            }
        }

        public static int EnterDirToFacingDirection(EnterDirection enterDir)
        {
            switch (enterDir)
            {
                case EnterDirection.FROM_LEFT:
                    return 1;
                case EnterDirection.FROM_TOP:
                    return 2;
                case EnterDirection.FROM_RIGHT:
                    return 3;
                case EnterDirection.FROM_BOTTOM:
                    return 0;
                default:
                    return 2;
            }
        }

        public static ExitDirection CastEnterDirToExitDir(EnterDirection enterDir)
        {
            switch (enterDir)
            {
                case EnterDirection.FROM_LEFT:
                    return ExitDirection.LEFT;
                case EnterDirection.FROM_TOP:
                    return ExitDirection.TOP;
                case EnterDirection.FROM_RIGHT:
                    return ExitDirection.RIGHT;
                case EnterDirection.FROM_BOTTOM:
                    return ExitDirection.BOTTOM;
                default:
                    return ExitDirection.TOP;
            }
        }

        public static List<ExitDirection> AllExitDirsBut(ExitDirection exclude)
        {
            List<ExitDirection> possibleExitDirs = new List<ExitDirection>{
                ExitDirection.BOTTOM,
                ExitDirection.LEFT,
                ExitDirection.RIGHT,
                ExitDirection.TOP
            };
            possibleExitDirs.Remove(exclude);
            return possibleExitDirs;
        }

        public static Dictionary<ExitDirection, Location> CreateExitDictionary(EnterDirection enterDir, Location enterLocation, Dictionary<ExitDirection, DeepWoodsExit> exits)
        {
            Dictionary<ExitDirection, Location> exitDictionary = new Dictionary<ExitDirection, Location>();
            exitDictionary.Add(CastEnterDirToExitDir(enterDir), enterLocation);
            foreach (var exit in exits)
            {
                exitDictionary.Add(exit.Key, exit.Value.location);
            }
            return exitDictionary;
        }
    }
}
