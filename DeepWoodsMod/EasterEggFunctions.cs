using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using static DeepWoodsMod.DeepWoodsSettings;
using static DeepWoodsMod.DeepWoodsGlobals;

namespace DeepWoodsMod
{
    class EasterEggFunctions
    {
        private enum ProcessMethod
        {
            Remove,
            Restore
        }

        private static void ProcessEggsInItems(NetObjectList<Item> items, ProcessMethod method)
        {
            for (int index = items.Count - 1; index >= 0; --index)
            {
                if (method == ProcessMethod.Remove && items[index] is EasterEggItem easterEggItem)
                {
                    items[index] = new StardewValley.Object(EASTER_EGG_REPLACEMENT_ITEM, easterEggItem.Stack) { name = UNIQUE_NAME_FOR_EASTER_EGG_ITEMS };
                }
                else if (method == ProcessMethod.Restore
                    && items[index] is StardewValley.Object @object
                    && @object.parentSheetIndex == EASTER_EGG_REPLACEMENT_ITEM
                    && @object.name == UNIQUE_NAME_FOR_EASTER_EGG_ITEMS)
                {
                    items[index] = new EasterEggItem() { Stack = @object.Stack };
                }
            }
        }

        private static void ProcessEggsInLocation(GameLocation location, ProcessMethod method)
        {
            if (location == null)
                return;

            if (location is BuildableGameLocation buildableGameLocation)
            {
                foreach (Building building in buildableGameLocation.buildings)
                {
                    ProcessEggsInLocation(building.indoors.Value, method);
                }
            }

            foreach (StardewValley.Object @object in location.objects.Values)
            {
                if (@object is Chest chest)
                {
                    ProcessEggsInItems(chest.items, method);
                }
            }
        }

        public static void RemoveAllEasterEggsFromGame()
        {
            foreach (GameLocation location in Game1.locations)
            {
                ProcessEggsInLocation(location, ProcessMethod.Remove);
            }

            foreach (Farmer farmer in Game1.getOnlineFarmers())
            {
                ProcessEggsInItems(farmer.items, ProcessMethod.Remove);
            }
        }

        public static void RestoreAllEasterEggsInGame()
        {
            foreach (GameLocation location in Game1.locations)
            {
                ProcessEggsInLocation(location, ProcessMethod.Restore);
            }

            foreach (Farmer farmer in Game1.getOnlineFarmers())
            {
                ProcessEggsInItems(farmer.items, ProcessMethod.Restore);
            }
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

                    Game1.drawDialogueNoTyping(I18N.EasterEggHatchedMessage);
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
