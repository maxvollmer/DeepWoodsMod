
using DeepWoodsMod.API.Impl;
using DeepWoodsMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace DeepWoodsMod.Stuff
{
    public class BigWoodenSign : LargeTerrainFeature
    {
        public BigWoodenSign()
           : base(false)
        {
        }

        public BigWoodenSign(Vector2 tileLocation)
            : this()
        {
            this.Tile = tileLocation;
        }

        public override bool isActionable()
        {
            return true;
        }

        public override Rectangle getBoundingBox()
        {
            return new Rectangle((int)Tile.X * 64 + 8, (int)Tile.Y * 64, 128 - 24, 64);
        }

        public override bool isPassable(Character c = null)
        {
            return false;
        }

        public override bool performUseAction(Vector2 tileLocation)
        {
            DeepWoodsQuestMenu.OpenQuestMenuWithModInfo(I18N.BigWoodenSignMessage, new Response[1]
            {
                new Response("No", I18N.MessageBoxClose).SetHotKey(Keys.Escape)
            });

            return true;
        }

        public override bool tickUpdate(GameTime time)
        {
            return false;
        }

        public override void dayUpdate()
        {
        }

        public override bool seasonUpdate(bool onLoad)
        {
            return false;
        }

        public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
        {
            return false;
        }

        public override void performPlayerEntryAction()
        {
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            Vector2 globalPosition = Tile * 64f;

            // 28 x 40 pixel

            Rectangle bottomSourceRectangle = new Rectangle(5, 28, 28, 22);
            Vector2 globalBottomPosition = new Vector2(globalPosition.X, globalPosition.Y);

            Rectangle topSourceRectangle = new Rectangle(5, 8, 28, 20);
            Vector2 globalTopPosition = new Vector2(globalPosition.X, globalPosition.Y - 80);

            spriteBatch.Draw(DeepWoodsTextures.Textures.BigWoodenSign, Game1.GlobalToLocal(Game1.viewport, globalTopPosition), topSourceRectangle, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, ((Tile.Y + 1f) * 64f / 10000f + Tile.X / 100000f));
            spriteBatch.Draw(DeepWoodsTextures.Textures.BigWoodenSign, Game1.GlobalToLocal(Game1.viewport, globalBottomPosition), bottomSourceRectangle, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, ((Tile.Y + 1f) * 64f / 10000f + Tile.X / 100000f));
        }
    }
}
