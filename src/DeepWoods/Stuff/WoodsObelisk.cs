﻿using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Reflection;
using static DeepWoodsMod.DeepWoodsSettings;
using static DeepWoodsMod.DeepWoodsGlobals;
using StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;
using System.Linq;

namespace DeepWoodsMod
{
    public class WoodsObelisk
    {
        private static void ObeliskWarpForRealOverride()
        {
            Game1.activeClickableMenu = new WoodsObeliskMenu();
        }

        public static void SendLetterIfNecessaryAndPossible()
        {
            if (DeepWoodsState.LowestLevelReached >= Settings.Level.MinLevelForWoodsObelisk
                && !Game1.player.hasOrWillReceiveMail(WOODS_OBELISK_WIZARD_MAIL_ID)
                && (Game1.player.mailReceived.Contains("hasPickedUpMagicInk") || Game1.player.hasMagicInk))
            {
                Game1.addMailForTomorrow(WOODS_OBELISK_WIZARD_MAIL_ID);
            }
        }

        public static void InjectWoodsObeliskIntoGame()
        {
            /*
            foreach (var a in Game1.delayedActions)
            {
                if (a.behavior == a.doGlobalFade && a.afterFadeBehavior != null
                    && a.afterFadeBehavior.GetMethodInfo() == typeof(Building).GetMethod("obeliskWarpForReal", BindingFlags.Instance | BindingFlags.NonPublic)
                    && a.afterFadeBehavior.Target is Building building
                    && building.buildingType.Value == WOODS_OBELISK_BUILDING_NAME)
                {
                    a.afterFadeBehavior = new Game1.afterFadeFunction(ObeliskWarpForRealOverride);
                }
            }

            if (DeepWoodsState.LowestLevelReached >= Settings.Level.MinLevelForWoodsObelisk
                && Game1.player.mailReceived.Contains(WOODS_OBELISK_WIZARD_MAIL_ID)
                && Game1.activeClickableMenu is CarpenterMenu carpenterMenu
                && IsMagical(carpenterMenu)
                && !HasBluePrint(carpenterMenu))
            {
                // Create a new Woods Obelisk BluePrint, but override the values as settings might have been loaded after blueprints
                CarpenterMenu.BlueprintEntry woodsObeliskBluePrint = new CarpenterMenu.BlueprintEntry(WOODS_OBELISK_BUILDING_NAME)
                {
                    displayName = I18N.WoodsObeliskDisplayName,
                    description = I18N.WoodsObeliskDescription,
                    moneyRequired = Settings.Objects.WoodsObelisk.MoneyRequired
                };
                woodsObeliskBluePrint.itemsRequired.Clear();
                foreach (var item in Settings.Objects.WoodsObelisk.ItemsRequired)
                {
                    woodsObeliskBluePrint.itemsRequired.Add(item.Key, item.Value);
                }
                SetBluePrintField(woodsObeliskBluePrint, "textureName", "Buildings\\" + WOODS_OBELISK_BUILDING_NAME);
                SetBluePrintField(woodsObeliskBluePrint, "texture", Game1.content.Load<Texture2D>(woodsObeliskBluePrint.textureName));

                // Add Woods Obelisk directly after the other obelisks
                int lastObeliskIndex = carpenterMenu.Blueprints.FindLastIndex(bluePrint => bluePrint.DisplayName.Contains("Obelisk"));
                carpenterMenu.Blueprints.Insert(lastObeliskIndex + 1, woodsObeliskBluePrint);
            }
            */
        }

        private static bool IsMagical(CarpenterMenu carpenterMenu)
        {
            return carpenterMenu.Blueprints.Exists(b => b.MagicalConstruction);
        }

        private static void SetBluePrintField(CarpenterMenu.BlueprintEntry bluePrint, string fieldName, object value)
        {
            //typeof(BluePrint).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(bluePrint, value);
        }

        private static bool HasBluePrint(CarpenterMenu carpenterMenu)
        {
            return carpenterMenu.Blueprints.Exists(bluePrint => bluePrint.DisplayName == WOODS_OBELISK_BUILDING_NAME);
        }



        private enum ProcessMethod
        {
            Remove,
            Restore
        }

        private static HashSet<GameLocation> processedLocations = new HashSet<GameLocation>();

        private static void ProcessAllInLocation(GameLocation location, ProcessMethod method)
        {
            if (location == null)
                return;

            ModEntry.Log("WoodsObelisk.ProcessAllInLocation(" + location.Name + ", " + method + ")", StardewModdingAPI.LogLevel.Trace);

            if (processedLocations.Contains(location))
            {
                ModEntry.Log("WoodsObelisk.ProcessAllInLocation(" + location.Name + ", " + method + "): Already processed this location (infinite recursion?), aborting!", StardewModdingAPI.LogLevel.Warn);
                return;
            }
            processedLocations.Add(location);

            if (location is GameLocation buildableGameLocation)
            {
                foreach (Building building in buildableGameLocation.buildings)
                {
                    if (building != null)
                    {
                        ProcessAllInLocation(building.indoors.Value, method);
                        if (method == ProcessMethod.Remove && building.buildingType.Value == WOODS_OBELISK_BUILDING_NAME)
                        {
                            building.buildingType.Value = EARTH_OBELISK_BUILDING_NAME;
                            DeepWoodsState.WoodsObeliskLocations.Add(new XY(building.tileX.Value, building.tileY.Value));
                        }
                        else if (method == ProcessMethod.Restore && building.buildingType.Value == EARTH_OBELISK_BUILDING_NAME)
                        {
                            if (DeepWoodsState.WoodsObeliskLocations.Contains(new XY(building.tileX.Value, building.tileY.Value)))
                            {
                                building.buildingType.Value = WOODS_OBELISK_BUILDING_NAME;
                                building.resetTexture();
                            }
                        }
                    }
                }
            }
        }

        public static void RemoveAllFromGame()
        {
            if (!Game1.IsMasterGame)
                return;

            ModEntry.Log("WoodsObelisk.RemoveAllFromGame()", StardewModdingAPI.LogLevel.Trace);

            DeepWoodsState.WoodsObeliskLocations.Clear();
            processedLocations.Clear();

            foreach (GameLocation location in Game1.locations)
            {
                ProcessAllInLocation(location, ProcessMethod.Remove);
            }

            processedLocations.Clear();
        }

        public static void RestoreAllInGame()
        {
            if (!Game1.IsMasterGame)
                return;

            ModEntry.Log("WoodsObelisk.RestoreAllInGame()", StardewModdingAPI.LogLevel.Trace);

            processedLocations.Clear();

            foreach (GameLocation location in Game1.locations)
            {
                ProcessAllInLocation(location, ProcessMethod.Restore);
            }

            processedLocations.Clear();
        }
    }
}
