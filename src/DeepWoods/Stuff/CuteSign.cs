
using DeepWoodsMod.API.Impl;
using DeepWoodsMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;

namespace DeepWoodsMod.Stuff
{
    public class CuteSign : LargeTerrainFeature
    {
        private NetInt row = new NetInt(0);

        public CuteSign()
           : base(false)
        {
            this.row.Value = new Random().Next(0, 4);
        }

        public CuteSign(Vector2 tileLocation)
            : this()
        {
            this.Tile = tileLocation;
        }

        public override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddField(this.row);
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
            DeepWoodsQuestMenu.OpenQuestMenu(I18N.EntrySignMessage, new Response[1]
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

            int column;
            if (Game1.IsWinter) 
            {
                column = 2;
            }
            else if (Game1.IsSpring)
            {
                column = 3;
            }
            else if (Game1.IsFall)
            {
                column = 1;
            }
            else//if (Game1.IsSummer)
            {
                column = 0;
            }

            Rectangle bottomSourceRectangle = new Rectangle(column * 32, row.Value * 37 + 21, 32, 16);
            Vector2 globalBottomPosition = new Vector2(globalPosition.X, globalPosition.Y);

            Rectangle topSourceRectangle = new Rectangle(column * 32, row.Value * 37, 32, 21);
            Vector2 globalTopPosition = new Vector2(globalPosition.X, globalPosition.Y - 84);

            spriteBatch.Draw(DeepWoodsTextures.Textures.CuteSign, Game1.GlobalToLocal(Game1.viewport, globalTopPosition), topSourceRectangle, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, ((Tile.Y + 1f) * 64f / 10000f + Tile.X / 100000f));
            spriteBatch.Draw(DeepWoodsTextures.Textures.CuteSign, Game1.GlobalToLocal(Game1.viewport, globalBottomPosition), bottomSourceRectangle, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, ((Tile.Y + 1f) * 64f / 10000f + Tile.X / 100000f));
        }
    }
}
