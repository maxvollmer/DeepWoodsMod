using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DeepWoodsMod.DeepWoodsRandom;

namespace DeepWoodsMod
{
    class GingerBreadHouse : ResourceClump
    {
        private const int START_HEALTH = 200;
        private const int SPAWN_FOOD_HEALTH_STEP_SIZE = 20;
        private const int MINIMUM_AXE_LEVEL = 0; // 0 for debugging purposes for now

        private Texture2D texture;
        private new int parentSheetIndex;
        private float nextSpawnFoodHealth;

        private int lol;

        public GingerBreadHouse()
            : base()
        {
            this.texture = Game1.content.Load<Texture2D>("Buildings\\Plank Cabin");
            this.parentSheetIndex = 0;
            this.health.Value = START_HEALTH;
            this.nextSpawnFoodHealth = START_HEALTH - SPAWN_FOOD_HEALTH_STEP_SIZE;
            this.lol = Game1.random.Next();
        }

        public GingerBreadHouse(Vector2 tile)
            : base(602, 5, 3, tile)
        {
            this.texture = Game1.content.Load<Texture2D>("Buildings\\Plank Cabin");
            this.parentSheetIndex = 0;
            this.health.Value = START_HEALTH;
            this.nextSpawnFoodHealth = START_HEALTH - SPAWN_FOOD_HEALTH_STEP_SIZE;
            this.lol = Game1.random.Next();
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            Vector2 globalPosition = this.tile.Value * 64f;
            if (this.shakeTimer > 0)
            {
                globalPosition.X += (float)Math.Sin(2.0 * Math.PI / this.shakeTimer) * 4f;
            }

            Rectangle upperHousePartRectangle = Game1.getSourceRectForStandardTileSheet(texture, this.parentSheetIndex, 16, 16);
            upperHousePartRectangle.Width = 5 * 16;
            upperHousePartRectangle.Height = 4 * 16;

            Rectangle bottomHousePartRectangle = Game1.getSourceRectForStandardTileSheet(texture, this.parentSheetIndex, 16, 16);
            bottomHousePartRectangle.Y += 4 * 16;
            bottomHousePartRectangle.Width = 5 * 16;
            bottomHousePartRectangle.Height = 3 * 16;

            Vector2 upperHousePartPosition = globalPosition;
            upperHousePartPosition.Y -= 4 * 64;

            spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, upperHousePartPosition), upperHousePartRectangle, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, ((this.tile.Y + 1f) * 64f / 10000f + this.tile.X / 100000f));
            spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, globalPosition), bottomHousePartRectangle, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, ((this.tile.Y + 1f) * 64f / 10000f + this.tile.X / 100000f));
        }

        public override bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location)
        {
            if (!(t is Axe))
                return false;

            if (t.upgradeLevel < MINIMUM_AXE_LEVEL)
            {
                location.playSound("axe");
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13948"));
                Game1.player.jitterStrength = 1f;
                return false;
            }

            Vector2 debrisLocation = CalculateDebrisLocation(tileLocation);

            location.playSound("axchop");
            Game1.createRadialDebris(Game1.currentLocation, Debris.woodDebris, (int)debrisLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 9), false, -1, false, -1);
            this.health.Value -= SPAWN_FOOD_HEALTH_STEP_SIZE;// Math.Max(1f, (t.upgradeLevel + 1) * 0.75f);

            if (this.health > 0)
            {
                if (this.health <= this.nextSpawnFoodHealth)
                {
                    location.playSound("stumpCrack");

                    t.getLastFarmerToUse().gainExperience(Farmer.foragingSkill, 25);

                    SpawnFoodItem(t, (int)tileLocation.X, (int)tileLocation.Y);

                    this.nextSpawnFoodHealth = this.health - SPAWN_FOOD_HEALTH_STEP_SIZE;
                }

                this.shakeTimer = 100f;
                return false;
            }

            PlayDestroyedSounds(location);

            // TODO: Spawn lots of stuffs :3
            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                {
                    SpawnFoodItems(t, (int)(this.tile.X + x), (int)(this.tile.Y + y));
                    Game1.createRadialDebris(Game1.currentLocation, Debris.woodDebris, (int)(this.tile.X + x), (int)(this.tile.Y + y), Game1.random.Next(4, 9), false, -1, false, -1);
                }
            }

            ModEntry.Log("GingerBreadHouse died: " + this.lol);
            return true;
        }

        private int GetRandomFoodType()
        {
            DeepWoodsRandom random = new DeepWoodsRandom(Game1.random.Next());
            random.EnterMasterMode();

            return random.GetRandomValue(new WeightedValue[] {
                // ITEM NAME // SELL PRICE // WEIGHT
                CreateWeightedValueForFootType(245), // Sugar               //  50 // 2000
                CreateWeightedValueForFootType(246), // Wheat Flour         //  50 // 2000
                CreateWeightedValueForFootType(229), // Tortilla            //  50 // 2000
                CreateWeightedValueForFootType(216), // Bread               //  60 // 1666
                CreateWeightedValueForFootType(223), // Cookie              // 140 //  714
                CreateWeightedValueForFootType(234), // Blueberry Tart      // 150 //  666
                CreateWeightedValueForFootType(220), // Chocolate Cake      // 200 //  500
                CreateWeightedValueForFootType(243), // Miner's Treat       // 200 //  500
                CreateWeightedValueForFootType(203), // Strange Bun         // 225 //  444
                CreateWeightedValueForFootType(651), // Poppyseed Muffin    // 250 //  400
                CreateWeightedValueForFootType(611), // Blackberry Cobbler  // 260 //  384
                CreateWeightedValueForFootType(607), // Roasted Hazelnuts   // 270 //  370
                CreateWeightedValueForFootType(731), // Maple Bar           // 300 //  333
                CreateWeightedValueForFootType(608), // Pumpkin Pie         // 385 //  259
                CreateWeightedValueForFootType(222), // Rhubarb Pie         // 400 //  250
                CreateWeightedValueForFootType(221), // Pink Cake           // 480 //  208
                // Non-food items with hardcoded weight (their price is too low, they would always spawn)
                new WeightedValue(388, 3000),  // Wood // 2 // 3000 (50000)
                new WeightedValue(92, 3000),   // Sap  // 2 // 3000 (50000)
                /* // TODO: Figure out how to add these!
                // Non-food items with hardcoded weight (they don't have a sell price and are not included in ObjectInformation)
                new WeightedValue(40, 10000),   // Big Green Cane // x // 100 (x)
                new WeightedValue(41, 10000),   // Green Canes    // x // 100 (x)
                new WeightedValue(42, 10000),   // Mixed Cane     // x // 100 (x)
                new WeightedValue(43, 10000),   // Red Canes      // x // 100 (x)
                new WeightedValue(44, 10000),   // Big Red Cane   // x // 100 (x)
                */
            });
        }

        private WeightedValue CreateWeightedValueForFootType(int type)
        {
            int price = 0;
            if (Game1.objectInformation.ContainsKey(type))
            {
                price = Convert.ToInt32(Game1.objectInformation[type].Split('/')[StardewValley.Object.objectInfoPriceIndex]);
            }
            // We invert the price to get higher weights for cheaper items and vice versa.
            return new WeightedValue(type, 100000 / price);
        }

        private void SpawnFoodItems(Tool t, int x, int y)
        {
            for (int i = 0, n = Game1.random.Next(1,4); i < n; i++)
            {
                SpawnFoodItem(t, x, y);
            }
        }

        private void SpawnFoodItem(Tool t, int x, int y)
        {
            if (Game1.IsMultiplayer)
                Game1.createMultipleObjectDebris(GetRandomFoodType(), x, y, 1, t.getLastFarmerToUse().UniqueMultiplayerID);
            else
                Game1.createMultipleObjectDebris(GetRandomFoodType(), x, y, 1);
        }

        private void PlayDestroyedSounds(GameLocation location)
        {
            DelayedAction.playSoundAfterDelay("stumpCrack", 0, location);
            DelayedAction.playSoundAfterDelay("boulderBreak", 10, location);
            DelayedAction.playSoundAfterDelay("breakingGlass", 20, location);
            DelayedAction.playSoundAfterDelay("stumpCrack", 50, location);
            DelayedAction.playSoundAfterDelay("boulderBreak", 60, location);
            DelayedAction.playSoundAfterDelay("breakingGlass", 70, location);
            DelayedAction.playSoundAfterDelay("boulderBreak", 110, location);
            DelayedAction.playSoundAfterDelay("breakingGlass", 120, location);
            DelayedAction.playSoundAfterDelay("boulderBreak", 160, location);

            DelayedAction.playSoundAfterDelay("cacklingWitch", 2000, location);
        }

        private Vector2 CalculateDebrisLocation(Vector2 tileLocation)
        {
            int xOffset = Game1.random.Next(0, 2);
            int yOffset = Game1.random.Next(0, 2);

            Vector2 debrisLocation = new Vector2(tileLocation.X, tileLocation.Y);

            if ((tileLocation.X + xOffset) > (this.tile.X + this.width - 1))
            {
                debrisLocation.X = debrisLocation.X - xOffset;
            }
            else if ((tileLocation.X - xOffset) < this.tile.X)
            {
                debrisLocation.X = debrisLocation.X + xOffset;
            }
            else
            {
                debrisLocation.X = debrisLocation.X + (Game1.random.NextDouble() < 0.5 ? xOffset : -xOffset);
            }

            if ((tileLocation.Y + yOffset) > (this.tile.Y + this.height - 1))
            {
                debrisLocation.Y = debrisLocation.Y - yOffset;
            }
            else if ((tileLocation.Y - yOffset) < this.tile.Y)
            {
                debrisLocation.Y = debrisLocation.Y + yOffset;
            }
            else
            {
                debrisLocation.Y = debrisLocation.Y + (Game1.random.NextDouble() < 0.5 ? yOffset : -yOffset);
            }

            return debrisLocation;
        }

        public override bool performUseAction(Vector2 tileLocation, GameLocation location)
        {
            return true;
        }
    }
}
