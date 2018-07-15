using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWoodsMod
{
    /// <summary>
    /// Heh, this is the most literal easter egg I've ever written :D
    /// </summary>
    class EasterEgg : TerrainFeature
    {
        // public const string FESTIVAL_TILESHEET_ID = "Festivals";
        // this.map.AddTileSheet(new TileSheet(FESTIVAL_TILESHEET_ID, this.map, "Maps\\Festivals", new Size(32, 32), new Size(16, 16)));

        private Texture2D texture;
        private int eggTileIndex;

        public EasterEgg()
        {
            this.texture = Game1.content.Load<Texture2D>("Maps\\Festivals");
            this.eggTileIndex = Game1.random.Next(67, 71);
        }

        public override Microsoft.Xna.Framework.Rectangle getBoundingBox(Vector2 tileLocation)
        {
            return new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
        }

        public override bool isPassable(Character c = null)
        {
            return false;
        }

        public override bool performUseAction(Vector2 tileLocation, GameLocation location)
        {
            if (Game1.player.addItemToInventoryBool(new EasterEggItem(), false))
            {
                Game1.player.animateOnce(279 + Game1.player.FacingDirection);
                Game1.player.canMove = false;
                Game1.player.currentLocation.playSound("coin");
                // DelayedAction.playSoundAfterDelay("coin", 260);
            }
            else
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
            }
            return true;
        }

        public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation, GameLocation location)
        {
            return false;
        }

        public override void draw(SpriteBatch b, Vector2 tileLocation)
        {
            Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64, tileLocation.Y * 64));

            Rectangle destinationRectangle = new Rectangle((int)local.X, (int)local.Y, 64, 64);
            Rectangle sourceRectangle = Game1.getSourceRectForStandardTileSheet(this.texture, this.eggTileIndex, 16, 16);

            b.Draw(this.texture, destinationRectangle, sourceRectangle, Color.White);
        }

        private static int RemoveAllEggsFromLocation(GameLocation location)
        {
            if (location == null)
                return 0;

            int eggsRemoved = 0;

            if (location is BuildableGameLocation buildableGameLocation)
            {
                foreach (Building building in buildableGameLocation.buildings)
                {
                    eggsRemoved += RemoveAllEggsFromLocation(building.indoors.Value);
                }
            }

            foreach (StardewValley.Object @object in location.objects.Values)
            {
                if (@object is Chest chest)
                {
                    for (int index = chest.items.Count - 1; index >= 0; --index)
                    {
                        if (chest.items[index] is EasterEggItem)
                        {
                            eggsRemoved += chest.items[index].Stack;
                            chest.items.RemoveAt(index);
                        }
                    }
                }
            }

            return eggsRemoved;
        }

        public static void RemoveAllEasterEggsFromGame()
        {
            int eggsRemovedFromLocations = 0;
            foreach (GameLocation location in Game1.locations)
            {
                eggsRemovedFromLocations += RemoveAllEggsFromLocation(location);
            }

            int eggsRemovedFromFarmers = 0;
            foreach (Farmer farmer in Game1.getOnlineFarmers())
            {
                for (int index = farmer.items.Count - 1; index >= 0; --index)
                {
                    if (farmer.items[index] is EasterEggItem)
                    {
                        eggsRemovedFromFarmers += farmer.items[index].Stack;
                        farmer.items.RemoveAt(index);
                    }
                }
            }

            ModEntry.Log("Removed " + eggsRemovedFromLocations + " eggs from locations and " + eggsRemovedFromFarmers + " eggs from farmers.");
        }

        public static void InterceptIncubatorEggs()
        {
            if (!Game1.IsMasterGame)
                return;

            foreach (GameLocation location in Game1.locations)
            {
                if (location is BuildableGameLocation buildableGameLocation)
                {
                    foreach (Building building in buildableGameLocation.buildings)
                    {
                        if (building is Coop coop)
                        {
                            AnimalHouse animalHouse = coop.indoors.Value as AnimalHouse;

                            // Seems that this is dead legacy code, we still keep it to support either way of hatching, in case future game code changes back to this.
                            if (animalHouse.incubatingEgg.Y == EasterEggItem.PARENT_SHEET_INDEX && animalHouse.incubatingEgg.X == 1)
                            {
                                animalHouse.incubatingEgg.X = 0;
                                animalHouse.incubatingEgg.Y = -1;
                                animalHouse.map.GetLayer("Front").Tiles[1, 2].TileIndex = 45;
                                long newId = Game1MultiplayerAccessProvider.GetMultiplayer().getNewID();
                                animalHouse.animals.Add(newId, new FarmAnimal("Rabbit", newId, coop.owner));
                            }
                        }
                    }
                }
            }
        }

        public static void CheckEggHatched(Farmer who, AnimalHouse animalHouse)
        {
            if (who != Game1.player)
                return;

            foreach (StardewValley.Object @object in animalHouse.objects.Values)
            {
                if (@object.bigCraftable && @object.Name.Contains("Incubator") && @object.heldObject.Value != null && @object.heldObject.Value.ParentSheetIndex == EasterEggItem.PARENT_SHEET_INDEX && @object.minutesUntilReady <= 0 && !animalHouse.isFull())
                {
                    @object.heldObject.Value = null;
                    @object.ParentSheetIndex = 101;

                    Game1.exitActiveMenu();
                    if (animalHouse.currentEvent != null)
                    {
                        animalHouse.currentEvent.CurrentCommand = animalHouse.currentEvent.eventCommands.Length - 1;
                        animalHouse.currentEvent = new Event("none/-1000 -1000/farmer 2 9 0/pause 750/end");
                    }

                    string str = "A new... wait a minute, a rabbit hatched?!";// TODO: Add to strings for l18n Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_Hatch_DuckEgg");

                    Game1.drawDialogueNoTyping(str);
                    Game1.afterDialogues = new Game1.afterFadeFunction(() => {
                        Game1.activeClickableMenu = new NamingMenu(new NamingMenu.doneNamingBehavior((string name) => {
                            AddNewHatchedRabbit(who, animalHouse, name);
                        }), Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236"), null);
                    });
                }
            }
        }

        private static void AddNewHatchedRabbit(Farmer who, AnimalHouse animalHouse, string animalName)
        {
            long animalId = Game1MultiplayerAccessProvider.GetMultiplayer().getNewID();

            FarmAnimal farmAnimal = new FarmAnimal("Rabbit", animalId, who.uniqueMultiplayerID);
            farmAnimal.Name = animalName;
            farmAnimal.displayName = animalName;

            Building building = animalHouse.getBuilding();
            farmAnimal.home = building;
            farmAnimal.homeLocation.Value = new Vector2(building.tileX, building.tileY);
            farmAnimal.setRandomPosition(animalHouse);

            animalHouse.animals.Add(animalId, farmAnimal);
            animalHouse.animalsThatLiveHere.Add(animalId);

            Game1.exitActiveMenu();
        }
    }
}
