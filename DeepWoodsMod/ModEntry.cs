using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using static DeepWoodsMod.DeepWoodsSettings;
using static DeepWoodsMod.DeepWoodsGlobals;
using System.Collections.Concurrent;

namespace DeepWoodsMod
{
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
    {
        private static ModEntry mod = null;

        private bool isDeepWoodsGameRunning = false;
        private Dictionary<long, GameLocation> playerLocations = new Dictionary<long, GameLocation>();

        private static ConcurrentQueue<string> queuedErrorMessages = new ConcurrentQueue<string>();

        private static void WorkErrorMessageQueue()
        {
            string msg;
            while (queuedErrorMessages.TryDequeue(out msg))
            {
                Log(msg, LogLevel.Error);
            }
        }

        public static void Log(string message, LogLevel level = LogLevel.Debug)
        {
            ModEntry.mod?.Monitor?.Log(message, level);
        }

        public static void QueueErrorMessage(string message)
        {
            queuedErrorMessages.Enqueue(message);
        }

        public static IReflectionHelper GetReflection()
        {
            return ModEntry.mod?.Helper?.Reflection;
        }

        public static IModHelper GetHelper()
        {
            return ModEntry.mod?.Helper;
        }

        public override void Entry(IModHelper helper)
        {
            ModEntry.mod = this;
            Game1MultiplayerAccessProvider.InterceptMultiplayer();
            Textures.LoadAll();
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            SaveEvents.BeforeSave += this.SaveEvents_BeforeSave;
            SaveEvents.AfterSave += this.SaveEvents_AfterSave;
            SaveEvents.AfterLoad += this.SaveEvents_AfterLoad;
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
            TimeEvents.TimeOfDayChanged += this.TimeEvents_TimeOfDayChanged;
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs args)
        {
            DeepWoodsSettings.DoSave();
            DeepWoods.Remove();
            EasterEggFunctions.RemoveAllEasterEggsFromGame();
            WoodsObelisk.RemoveAllFromGame();
        }

        private void SaveEvents_AfterSave(object sender, EventArgs args)
        {
            DeepWoods.Add();
            EasterEggFunctions.RestoreAllEasterEggsInGame();
            WoodsObelisk.RestoreAllInGame();
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs args)
        {
            if (Game1.IsMasterGame)
            {
                DeepWoodsSettings.DoLoad();
                DeepWoods.Add();
                EasterEggFunctions.RestoreAllEasterEggsInGame();
                WoodsObelisk.RestoreAllInGame();
                isDeepWoodsGameRunning = true;
            }
            else
            {
                Game1.MasterPlayer.queueMessage(NETWORK_MESSAGE_DEEPWOODS, Game1.player, new object[] { NETWORK_MESSAGE_DEEPWOODS_INIT });
            }
        }

        public static void DeepWoodsInitServerAnswerReceived()
        {
            if (Game1.IsMasterGame || mod.isDeepWoodsGameRunning)
                return;

            DeepWoods.Add();
            EasterEggFunctions.RestoreAllEasterEggsInGame();
            // WoodsObelisk.RestoreAllInGame(); <- Not needed, server already sends correct building
            mod.isDeepWoodsGameRunning = true;
        }

        private void TimeEvents_AfterDayStarted(object sender, EventArgs args)
        {
            if (!isDeepWoodsGameRunning)
                return;

            DeepWoods.LocalDayUpdate(Game1.dayOfMonth);
            EasterEggFunctions.InterceptIncubatorEggs();

            // TODO: TEMPTEMPTEMP
            Game1.player.warpFarmer(new Warp(0, 0, "DeepWoods", Settings.Map.RootLevelEnterLocation.X, Settings.Map.RootLevelEnterLocation.Y, false));
            // Game1.player.warpFarmer(new Warp(0, 0, "WizardHouse", 9, 15, false));
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgs args)
        {
            if (!isDeepWoodsGameRunning)
                return;

            DeepWoods.LocalTimeUpdate(Game1.timeOfDay);
        }

        private void GameEvents_UpdateTick(object sender, EventArgs args)
        {
            if (!isDeepWoodsGameRunning)
                return;

            WorkErrorMessageQueue();

            DeepWoods.LocalTick();

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

            // Add woods obelisk to wizard shop if possible and necessary,
            // intercept Building.obeliskWarpForReal() calls.
            WoodsObelisk.InjectWoodsObeliskIntoGame();
        }

        private void PlayerWarped(Farmer who, GameLocation prevLocation, GameLocation newLocation)
        {
            if (!isDeepWoodsGameRunning)
                return;

            if (newLocation is Woods woods)
            {
                OpenPassageInSecretWoods(woods);
            }

            DeepWoods.PlayerWarped(who, prevLocation as DeepWoods, newLocation as DeepWoods);

            if (newLocation is AnimalHouse animalHouse)
            {
                EasterEggFunctions.CheckEggHatched(who, animalHouse);
            }
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Maps/Woods");
        }

        private void OpenPassageInSecretWoods(Woods woods)
        {
            if (!isDeepWoodsGameRunning)
                return;

            woods.map.GetLayer("Buildings").Tiles[29, 25] = null;
            woods.map.GetLayer("Buildings").Tiles[29, 26] = null;
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
            // Commented out, because we do that in OpenPassageInSecretWoods(Woods woods) now, because we don't want this open in multiplayer clients connected to a server without the DeepWoodsMod.
            // buildingsLayer.Tiles[29, 25] = null;
            // buildingsLayer.Tiles[29, 26] = null;

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

            for (int i = -Settings.Map.ExitRadius; i <= Settings.Map.ExitRadius; i++)
            {
                warps += " " + (26 + i) + " 32 DeepWoods " + (Settings.Map.RootLevelEnterLocation.X + i) + " 1";
            }

            return warps.Trim();
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals($"Buildings\\{WoodsObelisk.WOODS_OBELISK_BUILDING_NAME}")
                || asset.AssetNameEquals("Maps\\deepWoodsLakeTilesheet");
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals($"Buildings\\{WoodsObelisk.WOODS_OBELISK_BUILDING_NAME}"))
            {
                return (T)(object)Textures.woodsObelisk;
            }
            else if (asset.AssetNameEquals("Maps\\deepWoodsLakeTilesheet"))
            {
                return (T)(object)Textures.lakeTilesheet;
            }
            else
            {
                throw new ArgumentException("Can't load " + asset.AssetName);
            }
        }
    }
}
