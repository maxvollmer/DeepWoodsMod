using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace DeepWoodsMod
{
    // Makes the hoedirt invisible, but shows the flower.
    // Removes itself when the flower was plucked.
    // Doesn't allow tools (watering, destroying flower with pickaxe etc.)
    // This way we can have flowers grow in the forest without grass being ruined by a dirt patch.
    public class Flower : HoeDirt
    {
        public Flower()
            : base()
        {
        }

        public Flower(int flowerType, GameLocation gameLocation, Vector2 location)
            : base(HoeDirt.watered, new Crop(flowerType.ToString(), (int)location.X, (int)location.Y, gameLocation))
        {
            this.crop.growCompletely();
        }

        public override bool tickUpdate(GameTime time)
        {
            if (this.crop == null || this.crop.dead.Value)
                return true;
            return base.tickUpdate(time);
        }

        public override bool performUseAction(Vector2 tileLocation)
        {
            if (this.crop == null || this.crop.dead.Value)
                return false;
            return base.performUseAction(tileLocation);
        }

        public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
        {
            if (t is MeleeWeapon)
            {
                return base.performToolAction(t, damage, tileLocation);
            }
            return this.crop == null || this.crop.dead.Value;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
        {
            this.crop?.drawInMenu(spriteBatch, positionOnScreen + new Vector2(64f * scale, 64f * scale), Color.White, 0.0f, scale, layerDepth + (float)(((double)positionOnScreen.Y + 64.0 * (double)scale) / 20000.0));
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            this.crop?.draw(spriteBatch, Tile, Color.White, this.getShakeRotation());
        }
    }
}
