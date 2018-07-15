using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

/*
// ✘ Don't do this! It will crash on Linux/Mac.
string path = helper.DirectoryPath + "\assets\asset.xnb";

// ✓ This is OK
string path = Path.Combine(helper.DirectoryPath, "assets", "asset.xnb");

// ✘ Don't do this! It will crash if SMAPI rewrites the assembly (e.g. to update or crossplatform it).
string modFolder = Assembly.GetCallingAssembly().Location;

// ✓ This is OK
string modFolder = helper.DirectoryPath;
*/

namespace DeepWoodsMod
{
    public class ModEntry : Mod, IAssetEditor
    {
        private Dictionary<long, GameLocation> playerLocations = new Dictionary<long, GameLocation>();

        private static ModEntry mod = null;

        public static void Log(string message, LogLevel level = LogLevel.Debug)
        {
            ModEntry.mod?.Monitor?.Log(message, level);
        }

        public static IReflectionHelper GetReflection()
        {
            return ModEntry.mod?.Helper?.Reflection;
        }

        public override void Entry(IModHelper helper)
        {
            ModEntry.mod = this;
            SaveEvents.BeforeSave += this.SaveEvents_BeforeSave;
            SaveEvents.AfterSave += this.SaveEvents_AfterSave;
            SaveEvents.AfterLoad += this.SaveEvents_AfterLoad;
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
            TimeEvents.TimeOfDayChanged += this.TimeEvents_TimeOfDayChanged;
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
        }

        private void LoadAndAddDeepWoods()
        {
            this.Monitor.Log("LoadAndAddDeepWoods()", LogLevel.Error);
            DeepWoods.Load();
            DeepWoods.Add();
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs args)
        {
            this.Monitor.Log("SaveEvents_BeforeSave()", LogLevel.Error);
            DeepWoods.Save();
            DeepWoods.Remove();
            EasterEgg.RemoveAllEasterEggsFromGame();
        }

        private void SaveEvents_AfterSave(object sender, EventArgs args)
        {
            this.Monitor.Log("SaveEvents_AfterSave()", LogLevel.Error);
            DeepWoods.Add();
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs args)
        {
            this.Monitor.Log("SaveEvents_AfterLoad()", LogLevel.Error);

            /*
            // TODO: TEMPTEMPTEMP
            Game1.currentSeason = "winter";
            Game1.setGraphicsForSeason();
            */

            LoadAndAddDeepWoods();
        }

        private void TimeEvents_AfterDayStarted(object sender, EventArgs args)
        {
            this.Monitor.Log("TimeEvents_AfterDayStarted()", LogLevel.Error);

            DeepWoods.LocalDayUpdate(Game1.dayOfMonth);
            EasterEgg.InterceptIncubatorEggs();

            // TODO: TEMPTEMPTEMP
            // Game1.player.warpFarmer(new Warp(0, 0, "DeepWoods", DeepWoods.ENTER_LOCATION.X, DeepWoods.ENTER_LOCATION.Y, false));
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgs args)
        {
            this.Monitor.Log("TimeEvents_TimeOfDayChanged()", LogLevel.Error);

            DeepWoods.LocalTimeUpdate(Game1.timeOfDay);
        }

        private void GameEvents_UpdateTick(object sender, EventArgs args)
        {
            Dictionary<long, GameLocation> newPlayerLocations = new Dictionary<long, GameLocation>();
            foreach (Farmer farmer in Game1.getOnlineFarmers())
            {
                newPlayerLocations.Add(farmer.UniqueMultiplayerID, farmer.currentLocation);
            }

            // Detect any farmer who left, joined or changed location.
            foreach (var playerLocation in playerLocations)
            {
                if (!newPlayerLocations.ContainsKey(playerLocation.Key))
                {
                    // player left
                    PlayerWarped(Game1.getFarmer(playerLocation.Key), playerLocation.Value, null);
                }
                else if (playerLocation.Value != newPlayerLocations[playerLocation.Key])
                {
                    // player warped
                    PlayerWarped(Game1.getFarmer(playerLocation.Key), playerLocation.Value, newPlayerLocations[playerLocation.Key]);
                }
            }

            foreach (var newPlayerLocation in newPlayerLocations)
            {
                if (!playerLocations.ContainsKey(newPlayerLocation.Key))
                {
                    // player joined
                    PlayerWarped(Game1.getFarmer(newPlayerLocation.Key), null, newPlayerLocation.Value);
                }
            }

            // Update cache
            playerLocations = newPlayerLocations;

            // Fix lighting in Woods and DeepWoods
            DeepWoods.FixLighting();
        }

        private void PlayerWarped(Farmer who, GameLocation prevLocation, GameLocation newLocation)
        {
            this.Monitor.Log("PlayerWarped()", LogLevel.Error);
            this.Monitor.Log("Farmer " + who.uniqueMultiplayerID + " warped from " + prevLocation + " to " + newLocation, LogLevel.Error);

            DeepWoods.PlayerWarped(who, prevLocation as DeepWoods, newLocation as DeepWoods);

            if (newLocation is AnimalHouse animalHouse)
            {
                EasterEgg.CheckEggHatched(who, animalHouse);
            }
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Maps/Woods");
        }

        public void Edit<T>(IAssetData asset)
        {
            // Get map from asset:
            Map map = asset.GetData<Map>();

            // Get "Buildings" layer (used for map border and forest border):
            Layer buildingsLayer = map.GetLayer("Buildings");

            // Get tileSheet and tileIndex from a forest border tile
            TileSheet borderTileSheet = buildingsLayer.Tiles[29, 26].TileSheet;
            int borderTileIndex = buildingsLayer.Tiles[29, 26].TileIndex;

            // Delete some hidden forest border tiles to allow player walking into deep woods:
            buildingsLayer.Tiles[29, 25] = null;
            buildingsLayer.Tiles[29, 26] = null;

            // Add some new border tiles to prevent player from getting confused/lost/stuck inside the hole we created.
            // (Basically setup a new border so player can only go left/down into DeepWoods or right/up back.)
            for (int x = 24; x < 29; x++)
            {
                buildingsLayer.Tiles[x, 24] = new StaticTile(buildingsLayer, borderTileSheet, BlendMode.Alpha, borderTileIndex);
            }

            // Add warps to DeepWoods reachable through deleted border:
            PropertyValue warpPropertyValue;
            map.Properties.TryGetValue("Warp", out warpPropertyValue);
            string warpPropertyString;
            if (warpPropertyValue != null)
            {
                warpPropertyString = warpPropertyValue.ToString() + " " + GetWoodsToDeepWoodsWarps();
            }
            else
            {
                warpPropertyString = GetWoodsToDeepWoodsWarps();
            }
            Log("warpPropertyString: " + warpPropertyString);
            map.Properties["Warp"] = new PropertyValue(warpPropertyString);
        }

        private string GetWoodsToDeepWoodsWarps()
        {
            string warps = "";

            for (int i = -DeepWoodsSpaceManager.EXIT_RADIUS; i <= DeepWoodsSpaceManager.EXIT_RADIUS; i++)
            {
                warps += " " + (26 + i) + " 32 DeepWoods " + (DeepWoods.ENTER_LOCATION.X + i) + " 1";
            }

            return warps.Trim();
        }
    }
}
