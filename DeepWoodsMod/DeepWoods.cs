using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using static DeepWoodsMod.DeepWoodsEnterExit;
using static DeepWoodsMod.DeepWoodsGlobals;
using static DeepWoodsMod.DeepWoodsSettings;

namespace DeepWoodsMod
{
    public class DeepWoods : GameLocation
    {
        private static ConcurrentDictionary<DeepWoods, byte> allDeepWoods = new ConcurrentDictionary<DeepWoods, byte>();
        private static DeepWoods root;
        private static ConcurrentQueue<ExitChildData> queuedExitChildren = new ConcurrentQueue<ExitChildData>();
        private static bool lostMessageDisplayedToday = false;

        public static void WarpFarmerIntoDeepWoods(int level)
        {
            // Warp into root level if appropriate.
            if (level <= 1)
            {
                Game1.warpFarmer("DeepWoods", Settings.Map.RootLevelEnterLocation.X, Settings.Map.RootLevelEnterLocation.Y, false);
            }

            // First check if a level already exists and teleport player there.
            foreach (GameLocation gameLocation in Game1.locations)
            {
                if (gameLocation is DeepWoods deepWoods && deepWoods.GetLevel() == level)
                {
                    WarpFarmerIntoDeepWoods(deepWoods);
                    return;
                }
            }

            // Create a new level.
            CreateNewDeepWoodsAndWarpFarmerIntoIt(level);
        }

        public static void AddDeepWoodsFromObelisk(string name, int level, int seed)
        {
            DeepWoods deepWoods = new DeepWoods(name, level, seed);
            Game1.locations.Add(deepWoods);
            allDeepWoods.TryAdd(deepWoods, 1);
        }

        private static void CreateNewDeepWoodsAndWarpFarmerIntoIt(int level)
        {
            DeepWoods deepWoods = new DeepWoods(level);
            Game1.locations.Add(deepWoods);
            allDeepWoods.TryAdd(deepWoods, 1);
            if (!Game1.IsMasterGame)
            {
                Game1.MasterPlayer.queueMessage(NETWORK_MESSAGE_DEEPWOODS, Game1.player, new object[] { NETWORK_MESSAGE_DEEPWOODS_WARP, deepWoods.Name, deepWoods.level, deepWoods.GetSeed() });
            }
            else
            {
                WarpFarmerIntoDeepWoods(deepWoods);
            }
        }

        public static void WarpFarmerIntoDeepWoods(DeepWoods deepWoods)
        {
            if (deepWoods == null)
                return;

            Game1.player.FacingDirection = DeepWoodsEnterExit.EnterDirToFacingDirection(deepWoods.enterDir);
            if (deepWoods.enterDir == EnterDirection.FROM_TOP)
            {
                Game1.warpFarmer(deepWoods.Name, deepWoods.enterLocation.X, deepWoods.enterLocation.Y + 1, false);
            }
            else if (deepWoods.enterDir == EnterDirection.FROM_RIGHT)
            {
                Game1.warpFarmer(deepWoods.Name, deepWoods.enterLocation.X + 1, deepWoods.enterLocation.Y, false);
            }
            else
            {
                Game1.warpFarmer(deepWoods.Name, deepWoods.enterLocation.X, deepWoods.enterLocation.Y, false);
            }
        }

        public static void Remove()
        {
            foreach (var deepWood in DeepWoods.allDeepWoods)
            {
                Game1.locations.Remove(deepWood.Key);
            }
        }

        private static void CheckValid()
        {
            if (!IsValidForThisGame())
            {
                Remove();
                allDeepWoods.Clear();
                root = new DeepWoods(null, 1, EnterDirection.FROM_TOP);
                allDeepWoods.TryAdd(root, 1);
            }
        }

        public static void Add()
        {
            CheckValid();
            foreach (var deepWood in DeepWoods.allDeepWoods)
            {
                Game1.locations.Add(deepWood.Key);
            }
        }

        public static bool IsValidForThisGame()
        {
            return root != null && root.uniqueMultiplayerID == Game1.MasterPlayer.UniqueMultiplayerID;
        }

        // This is called by every client at the start of a new day
        public static void LocalDayUpdate(int dayOfMonth)
        {
            CheckValid();

            Remove();
            allDeepWoods.Clear();
            allDeepWoods.TryAdd(root, 1);
            Add();

            root.RandomizeExits();

            lostMessageDisplayedToday = false;
        }

        // This is called by every client everytime the time of day changes (10 ingame minute intervals)
        public static void LocalTimeUpdate(int timeOfDay)
        {
            CheckValid();

            // Check if it's a new hour
            if (timeOfDay % 100 == 0)
            {
                byte dummy = 0;
                foreach (DeepWoods deepWoods in new List<DeepWoods>(allDeepWoods.Keys))
                {
                    // Check which DeepWoods can be removed
                    if (deepWoods.TryRemove())
                    {
                        allDeepWoods.TryRemove(deepWoods, out dummy);
                    }
                    else
                    {
                        // Randomize all warps
                        deepWoods.RandomizeExits();
                    }
                }
            }
        }

        // This is called by every client every frame
        public static void LocalTick()
        {
            WorkExitChildrenQueue();
        }

        public static void FixLighting()
        {
            if (!(Game1.currentLocation is DeepWoods || Game1.currentLocation is Woods))
                return;

            int darkOutDelta = Game1.timeOfDay - Game1.getTrulyDarkTime();
            if (darkOutDelta > 0)
            {
                double delta = darkOutDelta / 100 + (darkOutDelta % 100 / 60.0) + ((Game1.gameTimeInterval / (double)Game1.realMilliSecondsPerGameTenMinutes) / 6.0);
                double maxDelta = (2400 - Game1.getTrulyDarkTime()) / 100.0;

                double ratio = Math.Min(1.0, delta / maxDelta);

                if (ratio <= 0.0)
                {
                    Game1.ambientLight = DAY_LIGHT;
                }
                else if (ratio >= 1.0)
                {
                    Game1.ambientLight = NIGHT_LIGHT;
                }
                else
                {
                    Color dayLightFactorized = DAY_LIGHT * (float)(1.0 - ratio);
                    Color nightLightFactorized = NIGHT_LIGHT * (float)ratio;
                    Game1.ambientLight.R = (byte)Math.Min(255, dayLightFactorized.R + nightLightFactorized.R);
                    Game1.ambientLight.G = (byte)Math.Min(255, dayLightFactorized.G + nightLightFactorized.G);
                    Game1.ambientLight.B = (byte)Math.Min(255, dayLightFactorized.B + nightLightFactorized.B);
                    Game1.ambientLight.A = 255;
                }
            }
            else
            {
                Game1.ambientLight = DAY_LIGHT;
            }

            Game1.outdoorLight = Game1.ambientLight;
        }


        // Called whenever a player warps, both from and to may be null (we just ignore the call then)
        public static void PlayerWarped(Farmer who, DeepWoods from, DeepWoods to)
        {
            from?.RemovePlayer(who);
            to?.AddPlayer(who);
            if (who == Game1.player
                && from != null
                && to != null
                && from.parent == null
                && to.parent == from
                && ExitDirToEnterDir(CastEnterDirToExitDir(from.enterDir)) == to.enterDir
                && lostMessageDisplayedToday == false)
            {
                Game1.addHUDMessage(new HUDMessage(I18N.LostMessage) { noIcon = true });
                lostMessageDisplayedToday = true;
            }
        }

        // Called when a new DeepWoods instance was constructed over network, after the server sent it to us.
        private static void InitializeMeAndReplaceLocalInstanceWithMe(DeepWoods networkInstance)
        {
            /*
            // Make sure we have local instances ready
            foreach (DeepWoods deepWoods in allDeepWoods)
            {
                deepWoods.ValidateAndIfNecessaryCreateExitChildren();
            }
            */

            // Get local instance for this DeepWoods level (by name)
            DeepWoods localInstance = new List<DeepWoods>(allDeepWoods.Keys).Find(deepWoods => deepWoods.Name == networkInstance.Name);
            if (localInstance == null)
            {
                // Something went seriously wrong
                ModEntry.Log("Got unknown DeepWoods level from server, can't recover, sorry.", StardewModdingAPI.LogLevel.Error);
                return;
            }

            // Initialize the network instance
            networkInstance.InternalInitialize(localInstance.parent, localInstance.level, localInstance.enterDir);

            // Copy player count
            networkInstance.playerCount = localInstance.playerCount;

            // Replace the local instance
            ReplaceLocalInstanceWithNetworkedInstance(localInstance, networkInstance);

            // If it's the root level, replace our root node aswell
            if (networkInstance.Name == "DeepWoods")
            {
                DeepWoods.root = networkInstance;
            }

            // Make sure level is valid
            // networkInstance.ValidateAndIfNecessaryCreateExitChildren();
        }

        private static void ReplaceLocalInstanceWithNetworkedInstance(DeepWoods localDeepWoods, DeepWoods networkDeepWoods)
        {
            // Replace the parent value of our children (if any)
            foreach (var exit in networkDeepWoods.exits)
            {
                exit.Value.deepWoods = localDeepWoods.exits[exit.Key].deepWoods;
                if (exit.Value.deepWoods != null)
                {
                    exit.Value.deepWoods.parent = networkDeepWoods;
                }
            }

            // Replace the instance in our parent's exit node
            if (networkDeepWoods.parent != null)
            {
                networkDeepWoods.parent.exits[EnterDirToExitDir(networkDeepWoods.enterDir)].deepWoods = networkDeepWoods;
            }

            // Replace the instance in our set
            byte dummy = 0;
            allDeepWoods.TryRemove(localDeepWoods, out dummy);
            allDeepWoods.TryAdd(networkDeepWoods, 1);

            // Replace the instance in the game locations list
            for (int index = 0; index < Game1.locations.Count; ++index)
            {
                if (Game1.locations[index].Equals(localDeepWoods))
                {
                    Game1.locations[index] = networkDeepWoods;
                    break;
                }
            }

            // A bit hacky, but ensures we are really in a clean state now
            Game1.locations.Remove(localDeepWoods);
            Game1.locations.Remove(networkDeepWoods);
            Game1.locations.Add(networkDeepWoods);
        }

        private class ExitChildData
        {
            public DeepWoods Parent { get; }
            public ExitDirection ExitDir { get; }
            public DeepWoods Child { get; }
            public ExitChildData(DeepWoods parent, ExitDirection exitDir, DeepWoods child)
            {
                Parent = parent;
                ExitDir = exitDir;
                Child = child;
            }
        }

        private static void WorkExitChildrenQueue()
        {
            byte dummy = 0;
            ExitChildData exitChildData;
            while (queuedExitChildren.TryDequeue(out exitChildData))
            {
                if (exitChildData.Parent.exits.ContainsKey(exitChildData.ExitDir))
                {
                    var exit = exitChildData.Parent.exits[exitChildData.ExitDir];
                    if (exit.deepWoods == null)
                    {
                        if (Game1.getLocationFromName(exitChildData.Child.Name) is DeepWoods existingChild)
                        {
                            exit.deepWoods = existingChild;
                            allDeepWoods.TryRemove(exitChildData.Child, out dummy);
                        }
                        else
                        {
                            exit.deepWoods = exitChildData.Child;
                            Game1.locations.Add(exitChildData.Child);
                            allDeepWoods.TryAdd(exitChildData.Child, 1);
                        }
                        if (Game1.IsMasterGame && !exit.deepWoods.hasMonstersAdded)
                        {
                            DeepWoodsMonsters.AddMonsters(exit.deepWoods, exit.deepWoods.random, exit.deepWoods.spaceManager);
                        }
                        exitChildData.Parent.AddExitWarps(exitChildData.ExitDir, exit.location, exit.deepWoods.Name, exit.deepWoods.enterLocation);
                    }
                }
            }
        }

        private object warpMutex = new object();
        private DeepWoodsRandom random;
        private DeepWoods parent;
        private int level;
        private int playerCount;
        private EnterDirection enterDir;
        private Location enterLocation;
        private ConcurrentDictionary<ExitDirection, DeepWoodsExit> exits = new ConcurrentDictionary<ExitDirection, DeepWoodsExit>();
        public List<Vector2> lightSources = new List<Vector2>();
        private DeepWoodsSpaceManager spaceManager;
        private DeepWoodsBuilder deepWoodsBuilder;
        private long uniqueMultiplayerID;
        private bool wasConstructedOverNetwork;
        public bool isLichtung;
        public bool hasMonstersAdded;
        public Location lichtungCenter;
        private int spawnTime;
        private int abandonedByParentTime;
        private bool spawnedFromObelisk;
        private bool validateAndIfNecessaryCreateExitChildrenHasRunSinceLastExitRandomization;

        public NetObjectList<ResourceClump> resourceClumps = new NetObjectList<ResourceClump>();

        public List<Vector2> baubles = new List<Vector2>();
        public List<WeatherDebris> weatherDebris = new List<WeatherDebris>();

        // We need a public default constructor for Stardew Valley's network code (it sends entire objects over the wire 🙄)
        // We don't initialize anything here, instead we set a flag and sort this out in resetLocalState() or resetSharedState() further down.
        // Stardew Valley will have initialized our netfields before calling resetLocalState() or resetSharedState(),
        // so we can use our name to copy everything we need from the local instance.
        public DeepWoods()
            : base()
        {
            this.wasConstructedOverNetwork = true;
        }

        private DeepWoods(DeepWoods parent, int level, EnterDirection enterDir)
            : base()
        {
            InternalInitialize(parent, level, enterDir);
        }

        private DeepWoods(int level)
            : base()
        {
            InternalInitialize(null, level, EnterDirection.FROM_TOP);
            if (Game1.IsMasterGame)
            {
                DeepWoodsMonsters.AddMonsters(this, this.random, this.spaceManager);
            }
            this.spawnedFromObelisk = true;
        }

        private DeepWoods(string name, int level, int seed)
            : base()
        {
            InternalInitialize(null, level, EnterDirection.FROM_TOP, seed);
            if (Game1.IsMasterGame)
            {
                DeepWoodsMonsters.AddMonsters(this, this.random, this.spaceManager);
            }
            this.spawnedFromObelisk = true;
        }

        private void InternalInitialize(DeepWoods parent, int level, EnterDirection enterDir, int seed = 0)
        {
            this.uniqueMultiplayerID = Game1.MasterPlayer.UniqueMultiplayerID;
            if (seed != 0)
            {
                this.random = new DeepWoodsRandom(this, seed);
            }
            else
            {
                this.random = new DeepWoodsRandom(this, level, enterDir, parent?.GetSeed());
            }
            if (level == 1)
            {
                this.name.Value = "DeepWoods";
            }
            else
            {
                this.name.Value = "DeepWoods_" + this.random.GetSeed();
            }
            this.parent = parent;
            this.level = level;
            DeepWoodsState.LowestLevelReached = Math.Max(DeepWoodsState.LowestLevelReached, this.level);
            this.playerCount = 0;
            this.enterDir = enterDir;
            this.spawnTime = Game1.timeOfDay;
            this.abandonedByParentTime = 2600;
            this.spawnedFromObelisk = false;
            this.hasMonstersAdded = false;
            this.validateAndIfNecessaryCreateExitChildrenHasRunSinceLastExitRandomization = false;

            InitializeBaseFields();

            CreateSpace();
            DetermineExits();
            GenerateMap();

            DeepWoodsStuffCreator.AddStuff(this, this.random, this.spaceManager, this.deepWoodsBuilder);
            if (Game1.IsMasterGame && level == 1)
            {
                DeepWoodsMonsters.AddMonsters(this, this.random, this.spaceManager);
            }
            DeepWoods.allDeepWoods.TryAdd(this, 1);

            if (parent == null && level > 1 && !this.exits.ContainsKey(CastEnterDirToExitDir(this.enterDir)))
            {
                this.exits.TryAdd(CastEnterDirToExitDir(this.enterDir), new DeepWoodsExit(this.enterLocation));
            }

            if (parent != null)
            {
                AddParentWarps();

                // TODO: Remove this
                ModEntry.Log("Child spawned, time: " + Game1.timeOfDay + " this: " + this.Name + ", level: " + this.level + ", parent: " + this.parent.Name + ", enterDir: " + this.enterDir);
            }
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(this.resourceClumps);
        }

        private void InitializeBaseFields()
        {
            this.IsOutdoors = true;
            this.ignoreDebrisWeather.Value = true;
            this.ignoreOutdoorLighting.Value = true;
            this.forceViewportPlayerFollow = true;
            this.critters = new List<Critter>();
        }

        private void RemovePlayer(Farmer who)
        {
            this.playerCount--;
        }

        private void AddPlayer(Farmer who)
        {
            this.playerCount++;
            if (this.playerCount == 1)
            {
                ValidateAndIfNecessaryCreateExitChildren();
            }
        }

        private void ValidateAndIfNecessaryCreateExitChildren()
        {
            if (this.playerCount <= 0)
                return;

            if (this.validateAndIfNecessaryCreateExitChildrenHasRunSinceLastExitRandomization)
                return;

            this.validateAndIfNecessaryCreateExitChildrenHasRunSinceLastExitRandomization = true;

            if (this.level > 1 && this.parent == null && !this.exits.ContainsKey(CastEnterDirToExitDir(this.enterDir)))
            {
                this.abandonedByParentTime = Game1.timeOfDay;
                this.exits.TryAdd(CastEnterDirToExitDir(this.enterDir), new DeepWoodsExit(this.enterLocation));
            }

            foreach (var exit in this.exits)
            {
                if (exit.Value.deepWoods == null)
                {
                    // Use Task, so level generation doesn't block the game
                    Task.Run(() => {
                        try
                        {
                            DeepWoods child = new DeepWoods(this, this.level + 1, ExitDirToEnterDir(exit.Key));
                            queuedExitChildren.Enqueue(new ExitChildData(this, exit.Key, child));
                        }
                        catch (Exception e)
                        {
                            ModEntry.QueueErrorMessage("Child level spawn failed: " + e);
                        }
                    });
                }
            }
        }

        private void RandomizeExits()
        {
            this.warps.Clear();

            if (this.level > 1 && !this.exits.ContainsKey(CastEnterDirToExitDir(this.enterDir)))
            {
                this.abandonedByParentTime = Game1.timeOfDay;
                this.parent = null;
                this.exits.TryAdd(CastEnterDirToExitDir(this.enterDir), new DeepWoodsExit(this.enterLocation));
            }

            foreach (var exit in this.exits)
            {
                exit.Value.deepWoods = null;
            }

            this.validateAndIfNecessaryCreateExitChildrenHasRunSinceLastExitRandomization = false;

            if (this.playerCount > 0)
            {
                ValidateAndIfNecessaryCreateExitChildren();
            }
        }

        private bool TryRemove()
        {
            if (this.level == 1)
                return false;

            if (this.playerCount > 0)
                return false;

            if ((this.parent?.playerCount ?? 0) > 0 && Game1.timeOfDay <= (this.abandonedByParentTime + TIME_BEFORE_DELETION_ALLOWED_IF_OBELISK_SPAWNED))
                return false;

            if (this.spawnedFromObelisk && Game1.timeOfDay <= (this.spawnTime + TIME_BEFORE_DELETION_ALLOWED_IF_OBELISK_SPAWNED))
                return false;

            this.parent?.NotifyExitChildRemoved(EnterDirToExitDir(this.enterDir), this);

            foreach (var exit in this.exits)
            {
                if (exit.Value.deepWoods != null)
                {
                    exit.Value.deepWoods.parent = null;
                }
            }

            this.exits.Clear();
            this.parent = null;

            if (Game1.IsMasterGame)
            {
                this.warps.Clear();
                this.characters.Clear();
                this.terrainFeatures.Clear();
                this.largeTerrainFeatures.Clear();
                this.resourceClumps.Clear();
            }

            Game1.locations.Remove(this);
            return true;
        }

        private void NotifyExitChildRemoved(ExitDirection exitDir, DeepWoods child)
        {
            this.exits[exitDir].deepWoods = null;
        }

        private void DetermineExits()
        {
            this.exits.Clear();
            List<ExitDirection> possibleExitDirs = AllExitDirsBut(CastEnterDirToExitDir(this.enterDir));
            int numExitDirs = this.random.GetRandomValue(1, 4);
            if (numExitDirs < 3)
            {
                possibleExitDirs.RemoveAt(this.random.GetRandomValue(0, possibleExitDirs.Count));
                if (numExitDirs < 2)
                {
                    possibleExitDirs.RemoveAt(this.random.GetRandomValue(0, possibleExitDirs.Count));
                }
            }
            foreach (ExitDirection exitDir in possibleExitDirs)
            {
                this.exits.TryAdd(exitDir, new DeepWoodsExit(this.spaceManager.GetRandomExitLocation(exitDir, random)));
            }
        }

        private Location GetExitLocation(ExitDirection exitDir)
        {
            return this.exits[exitDir].location;
        }

        private DeepWoods GetExitDeepWoods(ExitDirection exitDir)
        {
            return this.exits[exitDir].deepWoods;
        }

        public Location GetEnterLocation()
        {
            return enterLocation;
        }

        public int GetSeed()
        {
            return this.random.GetSeed();
        }

        public int GetLevel()
        {
            return this.level;
        }

        private void CreateSpace()
        {
            this.isLichtung = this.level >= Settings.Level.MinLevelForClearing && this.parent != null && !this.parent.isLichtung && this.random.CheckChance(Settings.Luck.Clearings.ChanceForClearing);

            // Generate random size
            int mapWidth, mapHeight;
            if (this.isLichtung)
            {
                mapWidth = this.random.GetRandomValue(Settings.Map.MinMapWidth, Settings.Map.MaxMapWidthForClearing);
                mapHeight = this.random.GetRandomValue(Settings.Map.MinMapWidth, Settings.Map.MaxMapWidthForClearing);
            }
            else
            {
                mapWidth = this.random.GetRandomValue(Settings.Map.MinMapWidth, Settings.Map.MaxMapWidth);
                mapHeight = this.random.GetRandomValue(Settings.Map.MinMapHeight, Settings.Map.MaxMapHeight);
            }

            this.spaceManager = new DeepWoodsSpaceManager(mapWidth, mapHeight);
            this.enterLocation = this.level == 1 ? Settings.Map.RootLevelEnterLocation : this.spaceManager.GetRandomEnterLocation(this.enterDir, this.random);
        }

        private void GenerateMap()
        {
            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            // Create new map
            this.map = new Map("DeepWoods");

            // Add outdoor tilesheet
            this.map.AddTileSheet(new TileSheet(DEFAULT_OUTDOOR_TILESHEET_ID, this.map, "Maps\\" + Game1.currentSeason.ToLower() + "_outdoorsTileSheet", new Size(25, 79), new Size(16, 16)));
            this.map.AddTileSheet(new TileSheet(LAKE_TILESHEET_ID, this.map, "Maps\\deepWoodsLakeTilesheet", new Size(8, 5), new Size(16, 16)));
            this.map.LoadTileSheets(Game1.mapDisplayDevice);

            // Add default layers
            this.map.AddLayer(new Layer("Back", this.map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));
            this.map.AddLayer(new Layer("Buildings", this.map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));
            this.map.AddLayer(new Layer("Front", this.map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));
            this.map.AddLayer(new Layer("Paths", this.map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));
            this.map.AddLayer(new Layer("AlwaysFront", this.map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));

            this.deepWoodsBuilder = DeepWoodsBuilder.Build(this, this.random, this.spaceManager, this.map, DeepWoodsEnterExit.CreateExitDictionary(this.enterDir, this.enterLocation, new Dictionary<ExitDirection, DeepWoodsExit>(this.exits)));
        }

        public override void updateMap()
        {
            return;
        }

        // This is the default day update method of GameLocation, called only on the server
        public override void DayUpdate(int dayOfMonth)
        {
            // DeepWoodsStuffCreator.AddStuff(this, this.random, this.spaceManager);
            // DeepWoodsMonsters.AddMonsters(this, this.random, this.spaceManager);

            base.DayUpdate(dayOfMonth);
        }

        public int GetMapWidth()
        {
            return this.spaceManager.GetMapWidth();
        }

        public int GetMapHeight()
        {
            return this.spaceManager.GetMapHeight();
        }

        private string GetParentLocationName()
        {
            if (level == 1)
            {
                return "Woods";
            }
            else
            {
                return this.parent?.Name;
            }
        }

        private Location GetParentWarpLocation()
        {
            if (level == 1)
            {
                return WOODS_WARP_LOCATION;
            }
            else
            {
                return this.parent?.GetExitLocation(EnterDirToExitDir(this.enterDir)) ?? new Location();
            }
        }

        private void AddWarp(int x, int y, string locationName, Location warpLocation)
        {
            if (!Game1.IsMasterGame)
                return;

            lock (warpMutex)
            {
                foreach (Warp warp in new List<Warp>(this.warps))
                {
                    if (warp.X == x && warp.Y == y)
                    {
                        this.warps.Remove(warp);
                    }
                }

                this.warps.Add(new Warp(x, y, locationName, warpLocation.X, warpLocation.Y, false));
            }
        }

        private void AddExitWarps(ExitDirection exitDir, Location location, string targetLocationName, Location targetLocation)
        {
            if (!Game1.IsMasterGame)
                return;

            this.random.EnterMasterMode();

            switch (exitDir)
            {
                case ExitDirection.TOP:
                    AddWarp(location.X, -1, targetLocationName, targetLocation);
                    for (int i = 1; i <= Settings.Map.ExitRadius; i++)
                    {
                        AddWarp(location.X - i, -1, targetLocationName, targetLocation - new Location(i, 0));
                        AddWarp(location.X + i, -1, targetLocationName, targetLocation + new Location(i, 0));
                    }
                    break;
                case ExitDirection.BOTTOM:
                    {
                        // When warping into the map from the bottom, we want to end up one tile "too far" in, so the character is completely visible.
                        Location displacedTargetLocation = new Location(targetLocation.X, targetLocation.Y + 1);
                        AddWarp(location.X, this.spaceManager.GetMapHeight(), targetLocationName, displacedTargetLocation);
                        for (int i = 1; i <= Settings.Map.ExitRadius; i++)
                        {
                            AddWarp(location.X - i, this.spaceManager.GetMapHeight(), targetLocationName, displacedTargetLocation - new Location(i, 0));
                            AddWarp(location.X + i, this.spaceManager.GetMapHeight(), targetLocationName, displacedTargetLocation + new Location(i, 0));
                        }
                    }
                    break;
                case ExitDirection.LEFT:
                    {
                        // For some reason when warping into the map from the right, we always end up one tile too far left.
                        // We correct this here.
                        Location weirdBugfixLocation = new Location(targetLocation.X + 1, targetLocation.Y);
                        AddWarp(-1, location.Y, targetLocationName, weirdBugfixLocation);
                        for (int i = 1; i <= Settings.Map.ExitRadius; i++)
                        {
                            AddWarp(-1, location.Y - i, targetLocationName, weirdBugfixLocation - new Location(0, i));
                            AddWarp(-1, location.Y + i, targetLocationName, weirdBugfixLocation + new Location(0, i));
                        }
                    }
                    break;
                case ExitDirection.RIGHT:
                    AddWarp(this.spaceManager.GetMapWidth(), location.Y, targetLocationName, targetLocation);
                    for (int i = 1; i <= Settings.Map.ExitRadius; i++)
                    {
                        AddWarp(this.spaceManager.GetMapWidth(), location.Y - i, targetLocationName, targetLocation - new Location(0, i));
                        AddWarp(this.spaceManager.GetMapWidth(), location.Y + i, targetLocationName, targetLocation + new Location(0, i));
                    }
                    break;
            }

            this.random.LeaveMasterMode();
        }

        private void AddParentWarps()
        {
            if (!Game1.IsMasterGame)
                return;

            this.random.EnterMasterMode();

            if (this.level == 1 || this.parent != null)
            {
                string parentLocationName = GetParentLocationName();
                Location parentWarpLocation = GetParentWarpLocation();
                AddExitWarps(CastEnterDirToExitDir(this.enterDir), this.enterLocation, parentLocationName, parentWarpLocation);
            }
            
            this.random.LeaveMasterMode();
        }

        public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
        {
            if (!glider)
            {
                foreach (ResourceClump resourceClump in this.resourceClumps)
                {
                    if (resourceClump.getBoundingBox(resourceClump.tile).Intersects(position))
                        return true;
                }
            }
            return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
        }

        public override bool performToolAction(Tool t, int tileX, int tileY)
        {
            foreach (ResourceClump resourceClump in this.resourceClumps)
            {
                if (resourceClump.occupiesTile(tileX, tileY))
                {
                    if (resourceClump.performToolAction(t, 1, resourceClump.tile, this))
                    {
                        this.resourceClumps.Remove(resourceClump);
                    }
                    return true;
                }
            }
            return false;
        }

        public bool IsLocationOnBorderOrExit(Vector2 v)
        {
            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            // No placements on border tiles.
            if (v.X <= 0 || v.Y <= 0 || v.X >= (mapWidth - 2) || v.Y >= (mapHeight - 2))
                return true;

            // No placements on exits.
            foreach (var exit in this.exits)
            {
                Microsoft.Xna.Framework.Rectangle exitRectangle = new Microsoft.Xna.Framework.Rectangle(exit.Value.location.X - Settings.Map.ExitRadius, exit.Value.location.Y - Settings.Map.ExitRadius, Settings.Map.ExitRadius * 2 + 1, Settings.Map.ExitRadius * 2 + 1);
                if (exitRectangle.Contains((int)v.X, (int)v.Y))
                {
                    return true;
                }
            }

            // No placements on enter location as well.
            Microsoft.Xna.Framework.Rectangle enterRectangle = new Microsoft.Xna.Framework.Rectangle(enterLocation.X - Settings.Map.ExitRadius, enterLocation.Y - Settings.Map.ExitRadius, Settings.Map.ExitRadius * 2 + 1, Settings.Map.ExitRadius * 2 + 1);
            if (enterRectangle.Contains((int)v.X, (int)v.Y))
            {
                return true;
            }

            return false;
        }

        public override bool isTileLocationTotallyClearAndPlaceable(Vector2 v)
        {
            // No placements on tiles that are covered in forest.
            if (this.map.GetLayer("Buildings").Tiles[(int)v.X, (int)v.Y] != null)
                return false;

            // No placements on borders, exits and enter locations.
            if (IsLocationOnBorderOrExit(v))
                return false;

            // No placements if something is placed here already.
            foreach (ResourceClump resourceClump in this.resourceClumps)
            {
                if (resourceClump.occupiesTile((int)v.X, (int)v.Y))
                    return false;
            }

            // No placements if something is placed here already.
            foreach (LargeTerrainFeature largeTerrainFeature in this.largeTerrainFeatures)
            {
                if (largeTerrainFeature.getBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)v.X * 64, (int)v.Y * 64, 64, 64)))
                    return false;
            }

            // Call parent method for further checks.
            return base.isTileLocationTotallyClearAndPlaceable(v);
        }

        public override bool isTileOccupied(Vector2 tileLocation, string characterToIgnore = "")
        {
            // Check resourceClumps.
            foreach (ResourceClump resourceClump in this.resourceClumps)
            {
                if (resourceClump.occupiesTile((int)tileLocation.X, (int)tileLocation.Y))
                    return true;
            }

            // Call parent method for further checks.
            return base.isTileOccupied(tileLocation, characterToIgnore);
        }

        public virtual bool CanPlaceMonsterHere(int x, int y, Monster monster)
        {
            Vector2 v = new Vector2(x, y);
            if (monster.isGlider)
            {
                if (IsLocationOnBorderOrExit(v))
                    return false;

                Microsoft.Xna.Framework.Rectangle rectangle = monster.GetBoundingBox();
                rectangle.X = x;
                rectangle.Y = y;
                foreach (NPC npc in this.characters)
                {
                    if (npc.GetBoundingBox().Intersects(rectangle))
                        return false;
                }

                return true;
            }
            else
            {
                if (isTileLocationTotallyClearAndPlaceable(v))
                    return true;

                if (this.terrainFeatures.ContainsKey(v) && this.terrainFeatures[v] is Grass)
                    return true;

                return false;
            }
        }

        public bool AddMonsterAtRandomLocation(Monster monster)
        {
            int x = Game1.random.Next(0, this.spaceManager.GetMapWidth());
            int y = Game1.random.Next(0, this.spaceManager.GetMapHeight());

            int numTries = 0;
            for (; numTries < NUM_MONSTER_SPAWN_TRIES && !this.CanPlaceMonsterHere(x, y, monster); numTries++)
            {
                x = Game1.random.Next(0, this.spaceManager.GetMapWidth());
                y = Game1.random.Next(0, this.spaceManager.GetMapHeight());
            }

            if (numTries < NUM_MONSTER_SPAWN_TRIES)
            {
                monster.Position = new Vector2(x * 64f, y * 64f) - new Vector2(0, monster.Sprite.SpriteHeight - 64);
                this.addCharacter(monster);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void resetSharedState()
        {
            if (this.wasConstructedOverNetwork)
            {
                DeepWoods.InitializeMeAndReplaceLocalInstanceWithMe(this);
                this.wasConstructedOverNetwork = false;
            }

            base.resetSharedState();
        }

        protected override void resetLocalState()
        {
            if (this.wasConstructedOverNetwork)
            {
                DeepWoods.InitializeMeAndReplaceLocalInstanceWithMe(this);
                this.wasConstructedOverNetwork = false;
            }

            base.resetLocalState();

            // TODO: Better critter spawning in forest
            this.tryToAddCritters(false);

            DeepWoodsDebris.Initialize(this);

            foreach (Vector2 lightSource in this.lightSources)
            {
                Game1.currentLightSources.Add(new LightSource(LightSource.indoorWindowLight, lightSource * 64f, 1.0f));
            }
            DeepWoods.FixLighting();
        }

        public override void performTenMinuteUpdate(int timeOfDay)
        {
            base.performTenMinuteUpdate(timeOfDay);
            // TODO: Better critter spawning in forest
            this.tryToAddCritters(true);
        }

        public override void checkForMusic(GameTime time)
        {
            if (Game1.currentSong != null && Game1.currentSong.IsPlaying || Game1.nextMusicTrack != null && Game1.nextMusicTrack.Length != 0)
                return;

            if (Game1.isRaining)
            {
                Game1.changeMusicTrack("rain");
            }
            else
            {
                if (Game1.timeOfDay < 2500)
                {
                    if (Game1.random.NextDouble() < 0.75)
                    {
                        Game1.changeMusicTrack("woodsTheme");
                    }
                    else
                    {
                        if (Game1.isDarkOut())
                        {
                            if (Game1.currentSeason != "winter")
                            {
                                Game1.changeMusicTrack("spring_night_ambient");
                            }
                        }
                        else
                        {
                            Game1.changeMusicTrack(Game1.currentSeason + "_day_ambient");
                        }
                    }
                }
            }
        }

        public override void cleanupBeforePlayerExit()
        {
            base.cleanupBeforePlayerExit();
            DeepWoodsDebris.Clear(this);
            Game1.changeMusicTrack("");
        }

        public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
        {
            base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);
            DeepWoodsDebris.Update(this, time);
            foreach (ResourceClump resourceClump in this.resourceClumps)
            {
                resourceClump.tickUpdate(time, resourceClump.tile, this);
            }
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            foreach (ResourceClump resourceClump in this.resourceClumps)
            {
                resourceClump.draw(b, resourceClump.tile);
            }
        }

        public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
        {
            base.drawAboveAlwaysFrontLayer(b);
            foreach (var character in this.characters)
            {
                (character as Monster)?.drawAboveAllLayers(b);
            }
            DeepWoodsDebris.Draw(this, b);
            DrawLevelDisplay(b);
        }

        private void DrawLevelDisplay(SpriteBatch b)
        {
            string currentLevelAsString = string.Concat(this.level);
            Location titleSafeTopLeftCorner = this.spaceManager.GetActualTitleSafeTopleftCorner();
            SpriteText.drawString(
                b,
                currentLevelAsString,
                titleSafeTopLeftCorner.X + 16, titleSafeTopLeftCorner.Y + 16, /*x,y*/
                999999, -1, 999999, /*charPos,width,height*/
                1f, 1f, /*alpha,depth*/
                false, /*junimoText*/
                SpriteText.scrollStyle_darkMetal,
                "", /*placeHolderScrollWidthText*/
                SpriteText.color_Green);
        }

        public override StardewValley.Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency)
        {
            return this.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, (string)null);
        }

        public override StardewValley.Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, string locationName = null)
        {
            if ((locationName != null && locationName != this.Name) || !CanHazAwesomeFish())
            {
                return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, locationName);
            }
            return new StardewValley.Object(GetRandomAwesomeFish(), 1, false, -1, 0);
        }

        private bool CanHazAwesomeFish()
        {
            this.random.EnterMasterMode();
            bool result = this.random.CheckChance(Settings.Luck.Fishies.ChanceForAwesomeFish);
            this.random.LeaveMasterMode();
            return result;
        }

        private int GetRandomAwesomeFish()
        {
            this.random.EnterMasterMode();
            int result = this.random.GetRandomValue(Settings.Luck.Fishies.AwesomeFishies);
            this.random.LeaveMasterMode();
            return result;
        }

        public int GetCombatLevel()
        {
            int parentCombatLevel = this.parent?.GetCombatLevel() ?? 0;
            int totalCombatLevel = 0;
            int totalCombatLevelCount = 0;
            foreach (Farmer farmer in this.farmers)
            {
                totalCombatLevel += farmer.CombatLevel;
                totalCombatLevelCount++;
            }
            if (totalCombatLevelCount > 0)
            {
                return parentCombatLevel + totalCombatLevel / totalCombatLevelCount;
            }
            else
            {
                return parentCombatLevel;
            }
        }

        public int GetLuckLevel()
        {
            int totalLuckLevel = 0;
            int totalLuckLevelCount = 0;
            foreach (Farmer farmer in Game1.getOnlineFarmers())
            {
                if (farmer.currentLocation == this || farmer.currentLocation == this.parent)
                {
                    totalLuckLevel += farmer.LuckLevel;
                    totalLuckLevelCount++;
                }
            }
            if (totalLuckLevelCount > 0)
            {
                return totalLuckLevel / totalLuckLevelCount;
            }
            else
            {
                return 0;
            }
        }
    }
}
