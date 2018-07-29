using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using static DeepWoodsMod.DeepWoodsSettings;

namespace DeepWoodsMod
{
    class EasterEggFunctions
    {
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
