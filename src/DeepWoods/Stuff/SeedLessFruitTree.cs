
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace DeepWoodsMod
{
    internal class SeedLessFruitTree : FruitTree
    {
        private bool isPerformingToolAction = false;

        public SeedLessFruitTree()
            : base()
        {
        }

        public SeedLessFruitTree(string id, int growthStage = 0)
            : base(id, growthStage)
        {
        }

        public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
        {
            isPerformingToolAction = true;
            string treeIdValueBackup = treeId.Value;
            treeId.Value = null;
            bool returnValue = base.performToolAction(t, explosion, tileLocation);
            treeId.Value = treeIdValueBackup;
            isPerformingToolAction = false;
            return returnValue;
        }

        public override void loadSprite()
        {
            if (!isPerformingToolAction)
            {
                base.loadSprite();
            }
        }
    }
}
