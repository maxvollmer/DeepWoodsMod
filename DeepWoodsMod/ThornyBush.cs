using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeepWoodsMod
{
    class ThornyBush : DestroyableBush
    {
        private const int MIN_AXE_LEVEL = 0;    // TODO: 0 for development
        private const int DAMAGE_PER_LEVEL = 5;

        public ThornyBush()
            : base()
        {
            minAxeLevel = MIN_AXE_LEVEL;
        }

        public ThornyBush(Vector2 tileLocation, GameLocation location)
            : base(tileLocation, Bush.smallBush, location)
        {
            minAxeLevel = MIN_AXE_LEVEL;
        }

        public override bool performUseAction(Vector2 tileLocation, GameLocation location)
        {
            base.performUseAction(tileLocation, location);
            DamageFarmer(Game1.player, location);
            return true;
        }

        public override void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who, GameLocation location)
        {
            if (who is Farmer farmer)
            {
                DamageFarmer(farmer, location);
            }
        }

        private void DamageFarmer(Farmer who, GameLocation location)
        {
            who.takeDamage(GetDamage(location as DeepWoods), false, null);
        }

        private int GetDamage(DeepWoods deepWoods)
        {
            int level = deepWoods?.GetLevel() ?? 1;
            return (1 + level / 10) * DAMAGE_PER_LEVEL;
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            base.draw(spriteBatch, tileLocation);

            // TODO: Texture with thorns to overlay over bush

            Rectangle destinationRectangle = this.getBoundingBox(tileLocation);
            Rectangle sourceRectangle = new Rectangle(0, 0, 16, 16);

            Vector2 local = Game1.GlobalToLocal(Game1.viewport, tileLocation * 64);

            spriteBatch.Draw(Textures.bushThorns, local, sourceRectangle, Color.White, GetShakeRotation(), Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((destinationRectangle.Center.Y + 48) / 10000 - tileLocation.X / 1000000) + float.Epsilon);
        }

        private float GetShakeRotation()
        {
            return (float)typeof(Bush).GetField("shakeRotation", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
        }
    }
}
