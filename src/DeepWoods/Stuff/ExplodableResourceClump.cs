using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace DeepWoodsMod
{
    public class ExplodableResourceClump : ResourceClump
    {
        public ExplodableResourceClump()
            : base()
        {
        }

        public ExplodableResourceClump(int parentSheetIndex, int width, int height, Vector2 tile)
            : base(parentSheetIndex, width, height, tile)
        {
        }

        public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
        {
            if (t == null && damage > 0)
            {
                this.health.Value -= damage;

                if (this.health.Value <= 0)
                {
                    Game1.createRadialDebris(Location, GetDebrisType(), (int)tileLocation.X + Game1.random.Next(this.width.Value / 2 + 1), (int)tileLocation.Y + Game1.random.Next(this.height.Value / 2 + 1), Game1.random.Next(12, 20), false);
                    if (this.parentSheetIndex.Value == 600 || this.parentSheetIndex.Value == 602)
                        Location.playSound("stumpCrack");
                    else
                        Location.playSound("boulderBreak");
                    return true;
                }

                Game1.createRadialDebris(Location, GetDebrisType(), (int)tileLocation.X + Game1.random.Next(this.width.Value / 2 + 1), (int)tileLocation.Y + Game1.random.Next(this.height.Value / 2 + 1), Game1.random.Next(4, 9), false);
                return false;
            }

            return base.performToolAction(t, damage, tileLocation);
        }

        private int GetDebrisType()
        {
            switch (this.parentSheetIndex.Value)
            {
                case 622:
                case 672:
                case 752:
                case 754:
                case 756:
                case 758:
                    return 14;
                case 600:
                case 602:
                default:
                    return 12;
            }
        }
    }
}
