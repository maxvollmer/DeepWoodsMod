
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace DeepWoodsMod
{
    public class InfestedTree : FruitTree
    {
        public InfestedTree()
            : base()
        {
        }

        public InfestedTree(string saplingIndex)
           : base(saplingIndex, 4)
        {
            base.struckByLightningCountdown.Value = 4;
            base.fruit.Clear();
        }

        public override bool IsWinterTreeHere()
        {
            return Game1.GetSeasonForLocation(Location) == Season.Winter;
        }

        public void DeInfest()
        {
            base.struckByLightningCountdown.Value = 0;
            base.daysUntilMature.Value = 0;
            TryAddFruit();
            TryAddFruit();
            TryAddFruit();
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            if (Location is DeepWoods deepWoods && base.struckByLightningCountdown.Value > 0)
            {
                var seasonOverrideField = typeof(GameLocation).GetField("seasonOverride", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var backupSeason = seasonOverrideField.GetValue(Location);
                seasonOverrideField.SetValue(Location, new System.Lazy<Season?>(Season.Winter));
                deepWoods.infestedTreeIsDrawing = true;
                base.draw(spriteBatch);
                deepWoods.infestedTreeIsDrawing = false;
                seasonOverrideField.SetValue(Location, backupSeason);
            }
            else
            {
                base.draw(spriteBatch);
            }
        }
    }
}
