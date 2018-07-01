using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWoodsMod
{
    // Makes the hoedirt invisible, but shows the flower.
    // This way we can have flowers grow in the forest without grass being ruined by a dirt patch.
    class Flower : HoeDirt
    {
        public Flower()
            : base()
        {
        }

        public Flower(int flowerType, Vector2 location)
            : base(HoeDirt.watered, new Crop(flowerType, (int)location.X, (int)location.Y))
        {
            this.crop.growCompletely();
        }

        public override bool performUseAction(Vector2 tileLocation, GameLocation location)
        {
            base.performUseAction(tileLocation, location);
            return this.crop.dead || this.crop == null;
        }

        public override bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location)
        {
            base.performToolAction(t, damage, tileLocation, location);
            return this.crop.dead || this.crop == null;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
        {
            this.crop?.drawInMenu(spriteBatch, positionOnScreen + new Vector2(64f * scale, 64f * scale), Color.White, 0.0f, scale, layerDepth + (float)(((double)positionOnScreen.Y + 64.0 * (double)scale) / 20000.0));
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            this.crop?.draw(spriteBatch, tileLocation, Color.White, this.getShakeRotation());
        }
    }
}
