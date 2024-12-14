using DeepWoodsMod.API.Impl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using static DeepWoodsMod.DeepWoodsRandom;
using static DeepWoodsMod.DeepWoodsSettings;

namespace DeepWoodsMod
{
    public class GingerBreadHouse : ResourceClump
    {
        private new int parentSheetIndex;
        private NetFloat nextSpawnFoodHealth = new NetFloat();
        DeepWoodsRandom random = null;

        public GingerBreadHouse()
            : base()
        {
            this.parentSheetIndex = 0;
        }

        public GingerBreadHouse(Vector2 tile)
            : base(602, 5, 3, tile)
        {
            this.parentSheetIndex = 0;
            this.health.Value = Settings.Objects.GingerBreadHouse.Health;
            this.nextSpawnFoodHealth.Value = Settings.Objects.GingerBreadHouse.Health - Settings.Objects.GingerBreadHouse.DamageIntervalForFoodDrop;
        }

        public override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddField(this.nextSpawnFoodHealth);
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            Vector2 globalPosition = this.Tile * 64f;
            if (this.shakeTimer > 0)
            {
                globalPosition.X += (float)Math.Sin(2.0 * Math.PI / this.shakeTimer) * 4f;
            }

            Rectangle upperHousePartRectangle = Game1.getSourceRectForStandardTileSheet(DeepWoodsTextures.Textures.GingerbreadHouse, this.parentSheetIndex, 16, 16);
            upperHousePartRectangle.Width = 5 * 16;
            upperHousePartRectangle.Height = 4 * 16;

            Rectangle bottomHousePartRectangle = Game1.getSourceRectForStandardTileSheet(DeepWoodsTextures.Textures.GingerbreadHouse, this.parentSheetIndex, 16, 16);
            bottomHousePartRectangle.Y += 4 * 16;
            bottomHousePartRectangle.Width = 5 * 16;
            bottomHousePartRectangle.Height = 3 * 16;

            Vector2 upperHousePartPosition = globalPosition;
            upperHousePartPosition.Y -= 4 * 64;

            spriteBatch.Draw(DeepWoodsTextures.Textures.GingerbreadHouse, Game1.GlobalToLocal(Game1.viewport, upperHousePartPosition), upperHousePartRectangle, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, ((this.Tile.Y + 1f) * 64f / 10000f + this.Tile.X / 100000f));
            spriteBatch.Draw(DeepWoodsTextures.Textures.GingerbreadHouse, Game1.GlobalToLocal(Game1.viewport, globalPosition), bottomHousePartRectangle, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, ((this.Tile.Y + 1f) * 64f / 10000f + this.Tile.X / 100000f));
        }

        public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
        {
            if (t == null && damage > 0)
            {
                // explosion
                this.shakeTimer = 100f;
                return false;
            }

            if (!(t is Axe))
                return false;

            if (t.UpgradeLevel < Settings.Objects.GingerBreadHouse.MinimumAxeLevel)
            {
                Location.playSound("axe", tileLocation);
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13948"));
                Game1.player.jitterStrength = 1f;
                return false;
            }

            Vector2 debrisLocation = CalculateDebrisLocation(tileLocation);

            Location.playSound("axchop", tileLocation);
            Game1.createRadialDebris(Game1.currentLocation, Debris.woodDebris, (int)debrisLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 9), false);
            this.health.Value -= Math.Max(1f, (t.UpgradeLevel + 1) * 0.75f);

            if (this.health.Value > 0)
            {
                if (this.health.Value <= this.nextSpawnFoodHealth.Value)
                {
                    Location.playSound("stumpCrack", tileLocation);

                    t.getLastFarmerToUse().gainExperience(Farmer.foragingSkill, 25);

                    SpawnFoodItem(Location as DeepWoods, t, (int)tileLocation.X, (int)tileLocation.Y);

                    this.nextSpawnFoodHealth.Value = this.health.Value - Settings.Objects.GingerBreadHouse.DamageIntervalForFoodDrop;
                }

                this.shakeTimer = 100f;
                return false;
            }

            PlayDestroyedSounds();

            for (int x = 0; x < this.width.Value; x++)
            {
                for (int y = 0; y < this.height.Value; y++)
                {
                    SpawnFoodItems(Location as DeepWoods, t, (int)(this.Tile.X + x), (int)(this.Tile.Y + y));
                    Game1.createRadialDebris(Game1.currentLocation, Debris.woodDebris, (int)(this.Tile.X + x), (int)(this.Tile.Y + y), Game1.random.Next(4, 9), false);
                }
            }

            return true;
        }

        private string GetRandomFoodType(DeepWoods deepWoods)
        {
            if (random == null)
                random = new DeepWoodsRandom(deepWoods, (deepWoods?.Seed ?? Game1.random.Next()) ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ (int)this.Tile.X ^ (int)this.Tile.Y);
            return random.GetRandomValue(Settings.Objects.GingerBreadHouse.FootItems).ToString();
        }

        public static WeightedInt CreateWeightedValueForFootType(int type)
        {
            int price = 1;
            try
            {
                if (Game1.objectData != null && Game1.objectData.ContainsKey(type.ToString()))
                {
                    price = Convert.ToInt32(Game1.objectData[type.ToString()].Price);
                }
            }
            catch (Exception) { /*i dont know i dont care*/ }
            // We invert the price to get higher weights for cheaper items and vice versa.
            return new WeightedInt(type, 100000 / Math.Max(1, price));
        }

        private void SpawnFoodItems(DeepWoods deepWoods, Tool t, int x, int y)
        {
            for (int i = 0, n = Game1.random.Next(1,4); i < n; i++)
            {
                SpawnFoodItem(deepWoods, t, x, y);
            }
        }

        private void SpawnFoodItem(DeepWoods deepWoods, Tool t, int x, int y)
        {
            if (Game1.IsMultiplayer)
                Game1.createMultipleObjectDebris(GetRandomFoodType(deepWoods), x, y, 1, t.getLastFarmerToUse().UniqueMultiplayerID);
            else
                Game1.createMultipleObjectDebris(GetRandomFoodType(deepWoods), x, y, 1);
        }

        private void PlayDestroyedSounds()
        {
            DelayedAction.playSoundAfterDelay("stumpCrack", 0, Location, Tile);
            DelayedAction.playSoundAfterDelay("boulderBreak", 10, Location, Tile);
            DelayedAction.playSoundAfterDelay("breakingGlass", 20, Location, Tile);
            DelayedAction.playSoundAfterDelay("stumpCrack", 50, Location, Tile);
            DelayedAction.playSoundAfterDelay("boulderBreak", 60, Location, Tile);
            DelayedAction.playSoundAfterDelay("breakingGlass", 70, Location, Tile);
            DelayedAction.playSoundAfterDelay("boulderBreak", 110, Location, Tile);
            DelayedAction.playSoundAfterDelay("breakingGlass", 120, Location, Tile);
            DelayedAction.playSoundAfterDelay("boulderBreak", 160, Location, Tile);

            DelayedAction.playSoundAfterDelay("cacklingWitch", 2000, Location);
        }

        private Vector2 CalculateDebrisLocation(Vector2 tileLocation)
        {
            int xOffset = Game1.random.Next(0, 2);
            int yOffset = Game1.random.Next(0, 2);

            Vector2 debrisLocation = new Vector2(tileLocation.X, tileLocation.Y);

            if ((tileLocation.X + xOffset) > (this.Tile.X + this.width.Value - 1))
            {
                debrisLocation.X = debrisLocation.X - xOffset;
            }
            else if ((tileLocation.X - xOffset) < this.Tile.X)
            {
                debrisLocation.X = debrisLocation.X + xOffset;
            }
            else
            {
                debrisLocation.X = debrisLocation.X + (Game1.random.NextDouble() < 0.5 ? xOffset : -xOffset);
            }

            if ((tileLocation.Y + yOffset) > (this.Tile.Y + this.height.Value - 1))
            {
                debrisLocation.Y = debrisLocation.Y - yOffset;
            }
            else if ((tileLocation.Y - yOffset) < this.Tile.Y)
            {
                debrisLocation.Y = debrisLocation.Y + yOffset;
            }
            else
            {
                debrisLocation.Y = debrisLocation.Y + (Game1.random.NextDouble() < 0.5 ? yOffset : -yOffset);
            }

            return debrisLocation;
        }

        public override bool performUseAction(Vector2 tileLocation)
        {
            return true;
        }
    }
}
