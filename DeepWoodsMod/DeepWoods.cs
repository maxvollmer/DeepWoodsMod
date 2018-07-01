using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
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
using static DeepWoodsMod.DeepWoodsRandom;

namespace DeepWoodsMod
{
    public class DeepWoods : GameLocation
    {
        public const string DEFAULT_OUTDOOR_TILESHEET_ID = "DefaultOutdoor";
        public const string FESTIVAL_TILESHEET_ID = "Festivals";

        public static Location ENTER_LOCATION = new Location(DeepWoodsSpaceManager.MIN_MAP_WIDTH/2, 0);
        private static Location WOODS_WARP_LOCATION = new Location(26, 30);

        private static HashSet<DeepWoods> allDeepWoods = new HashSet<DeepWoods>();
        private static DeepWoods root;

        public static void Remove()
        {
            foreach (DeepWoods deepWood in DeepWoods.allDeepWoods)
            {
                Game1.locations.Remove(deepWood);
            }
        }

        private static void CheckValid()
        {
            if (!IsValidForThisGame())
            {
                Remove();
                allDeepWoods.Clear();
                root = new DeepWoods(null, 1, EnterDirection.FROM_TOP);
                allDeepWoods.Add(root);
            }
        }

        public static void Add()
        {
            CheckValid();
            foreach (DeepWoods deepWood in DeepWoods.allDeepWoods)
            {
                Game1.locations.Add(deepWood);
            }
        }

        public static void Save()
        {
            // TODO: Use SMAPI JSON API to store things like how deep players went, what secrets they already found etc.
        }

        public static void Load()
        {
            // TODO: Use SMAPI JSON API to load things like how deep players went, what secrets they already found etc.
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
            allDeepWoods.Add(root);
            Add();

            root.RandomizeExits();
        }

        // This is called by every client everytime the time of day changes (10 ingame minute intervals)
        public static void LocalTimeUpdate(int timeOfDay)
        {
            CheckValid();

            // Check if it's a new hour
            if (timeOfDay % 100 == 0)
            {
                // First randomize all warps
                List<DeepWoods> copy = new List<DeepWoods>(allDeepWoods);
                foreach (DeepWoods deepWoods in copy)
                {
                    deepWoods.RandomizeExits();
                }

                // Then check which DeepWoods can be removed
                allDeepWoods.RemoveWhere(deepWoods => deepWoods.TryRemove());

                ModEntry.Log("allDeepWoods.Count: " + allDeepWoods.Count);
            }
        }

        private static Color DAY_LIGHT = new Color(150, 120, 50, 255);
        private static Color NIGHT_LIGHT = new Color(255, 255, 50, 255);

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
        }

        // Called when a new DeepWoods instance was constructed over network, after the server sent it to us.
        private static void InitializeMeAndReplaceLocalInstanceWithMe(DeepWoods networkInstance)
        {
            // Make sure we have local instances ready
            foreach (DeepWoods deepWoods in allDeepWoods)
            {
                deepWoods.ValidateAndIfNecessaryCreateExitChildren();
            }

            // Get local instance for this DeepWoods level (by name)
            DeepWoods localInstance = new List<DeepWoods>(allDeepWoods).Find(deepWoods => deepWoods.Name == networkInstance.Name);
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
            networkInstance.ValidateAndIfNecessaryCreateExitChildren();
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
            allDeepWoods.Remove(localDeepWoods);
            allDeepWoods.Add(networkDeepWoods);

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


        private DeepWoodsRandom random;
        private DeepWoods parent;
        private int level;
        private int playerCount;
        private EnterDirection enterDir;
        private Location enterLocation;
        private Dictionary<ExitDirection, DeepWoodsExit> exits = new Dictionary<ExitDirection, DeepWoodsExit>();
        public List<Vector2> lightSources = new List<Vector2>();
        private DeepWoodsSpaceManager spaceManager;
        private long uniqueMultiplayerID;
        private bool wasConstructedOverNetwork;

        public NetObjectList<ResourceClump> resourceClumps = new NetObjectList<ResourceClump>();

        // We need a public default constructor for Stardew Valley's network code (it sends entire objects over the wire 🙄)
        // We don't initialize anything here, instead we set a flag and sort this out in resetLocalState() or resetSharedState() further down.
        // Stardew Valley will have initialized our netfields before calling resetLocalState() or resetSharedState(),
        // so we can use our name to copy everything we need from the local instance.
        public DeepWoods()
            : base()
        {
            ModEntry.Log("DeepWoods() over network: " + this.name.Value);
            this.wasConstructedOverNetwork = true;
        }

        private DeepWoods(DeepWoods parent, int level, EnterDirection enterDir)
            : base()
        {
            InternalInitialize(parent, level, enterDir);
        }

        private void InternalInitialize(DeepWoods parent, int level, EnterDirection enterDir)
        {
            this.uniqueMultiplayerID = Game1.MasterPlayer.UniqueMultiplayerID;
            this.random = new DeepWoodsRandom(level, enterDir, parent?.GetSeed());
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
            this.playerCount = 0;
            this.enterDir = enterDir;

            InitializeBaseFields();

            CreateSpace();
            DetermineExits();
            GenerateMap();

            // DeepWoodsStuffCreator.AddStuff(this, this.random, this.spaceManager);
            // DeepWoodsMonsters.AddMonsters(this, this.random, this.spaceManager);
            DeepWoods.allDeepWoods.Add(this);

            if (parent != null)
            {
                ModEntry.Log("Child spawned, this: " + this.Name + ", parent: " + this.parent.Name + ", enterDir: " + this.enterDir);
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
        }

        private void RemovePlayer(Farmer who)
        {
            this.playerCount--;
        }

        private void AddPlayer(Farmer who)
        {
            this.playerCount++;
            ValidateAndIfNecessaryCreateExitChildren();
        }

        private void ValidateAndIfNecessaryCreateExitChildren()
        {
            if (this.playerCount <= 0)
                return;

            bool addedExit = false;

            foreach (var exit in this.exits)
            {
                if (exit.Value.deepWoods == null)
                {
                    ModEntry.Log("Adding child, this: " + this.Name + ", exitDir: " + exit.Key);
                    exit.Value.deepWoods = new DeepWoods(this, this.level+1, ExitDirToEnterDir(exit.Key));
                    Game1.locations.Add(exit.Value.deepWoods);
                    addedExit = true;
                }
            }

            if (addedExit)
            {
                Game1.locations.Remove(this);
                AddWarps();
                Game1.locations.Add(this);
            }
        }

        private void RandomizeExits()
        {
            foreach (var exit in this.exits)
            {
                if (exit.Value.deepWoods != null)
                {
                    exit.Value.deepWoods.NotifyAbandonedByParent();
                }
                exit.Value.deepWoods = null;
            }

            ValidateAndIfNecessaryCreateExitChildren();
        }

        private bool TryRemove()
        {
            if (this.level == 1)
                return false;

            if (HasPlayerIncludingChildren())
                return false;

            if ((this.parent?.playerCount ?? 0) > 0)
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

        private bool HasPlayerIncludingChildren()
        {
            if (this.playerCount > 0)
                return true;

            foreach (var exit in this.exits)
            {
                if (exit.Value.deepWoods?.HasPlayerIncludingChildren() ?? false)
                    return true;
            }

            return false;
        }

        private void NotifyExitChildRemoved(ExitDirection exitDir, DeepWoods child)
        {
            this.exits[exitDir].deepWoods = null;
        }

        private void NotifyAbandonedByParent()
        {
            this.parent = null;
            ExitDirection exitDir = CastEnterDirToExitDir(this.enterDir);
            this.exits.Add(exitDir, new DeepWoodsExit(this.enterLocation));
            ValidateAndIfNecessaryCreateExitChildren();
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
                this.exits.Add(exitDir, new DeepWoodsExit(this.spaceManager.GetRandomExitLocation(exitDir, random)));
            }
        }

        private Location GetExitLocation(ExitDirection exitDir)
        {
            ModEntry.Log("GetExitLocation(), this: " + this.Name + ", exitDir: " + exitDir);
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

        private void CreateSpace()
        {
            // Generate random size
            int mapWidth = this.random.GetRandomValue(DeepWoodsSpaceManager.MIN_MAP_WIDTH, DeepWoodsSpaceManager.MAX_MAP_WIDTH);
            int mapHeight = this.random.GetRandomValue(DeepWoodsSpaceManager.MIN_MAP_HEIGHT, DeepWoodsSpaceManager.MAX_MAP_HEIGHT);
            this.spaceManager = new DeepWoodsSpaceManager(mapWidth, mapHeight);
            this.enterLocation = this.level == 1 ? ENTER_LOCATION : this.spaceManager.GetRandomEnterLocation(this.enterDir, this.random);
        }

        private void GenerateMap()
        {
            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            // Create new map
            this.map = new Map("DeepWoods");

            // Add outdoor tilesheet
            this.map.AddTileSheet(new TileSheet(DEFAULT_OUTDOOR_TILESHEET_ID, this.map, "Maps\\" + Game1.currentSeason.ToLower() + "_outdoorsTileSheet", new Size(25, 79), new Size(16, 16)));
            this.map.AddTileSheet(new TileSheet(FESTIVAL_TILESHEET_ID, this.map, "Maps\\Festivals", new Size(32, 32), new Size(16, 16)));
            this.map.LoadTileSheets(Game1.mapDisplayDevice);

            // Add default layers
            this.map.AddLayer(new Layer("Back", this.map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));
            this.map.AddLayer(new Layer("Buildings", this.map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));
            this.map.AddLayer(new Layer("Front", this.map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));
            this.map.AddLayer(new Layer("Paths", this.map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));
            this.map.AddLayer(new Layer("AlwaysFront", this.map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));

            DeepWoodsBuilder.Build(this, this.random, this.spaceManager, this.map, DeepWoodsEnterExit.CreateExitDictionary(this.enterDir, this.enterLocation, this.exits));
        }

        public override void updateMap()
        {
            return;
        }

        // This is the default day update method of GameLocation, called only on the server
        public override void DayUpdate(int dayOfMonth)
        {
            base.DayUpdate(dayOfMonth);
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
            ModEntry.Log("GetParentWarpLocation(), this: " + this.Name + ", level: " + this.level + ", parent: " + this.parent?.Name + ", this.enterDir: " + this.enterDir);
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
            this.warps.Add(new Warp(x, y, locationName, warpLocation.X, warpLocation.Y, false));
        }

        private void AddExitWarps(ExitDirection exitDir, Location location, string targetLocationName, Location targetLocation)
        {
            ModEntry.Log("AddExitWarps: " + exitDir + ", location: " + location + ", to: " + targetLocationName + ", targetLocation: " + targetLocation);
            switch (exitDir)
            {
                case ExitDirection.TOP:
                    AddWarp(location.X - 1, -1, targetLocationName, targetLocation);
                    AddWarp(location.X + 0, -1, targetLocationName, targetLocation);
                    AddWarp(location.X + 1, -1, targetLocationName, targetLocation);
                    break;
                case ExitDirection.BOTTOM:
                    AddWarp(location.X - 1, this.spaceManager.GetMapHeight(), targetLocationName, targetLocation);
                    AddWarp(location.X + 0, this.spaceManager.GetMapHeight(), targetLocationName, targetLocation);
                    AddWarp(location.X + 1, this.spaceManager.GetMapHeight(), targetLocationName, targetLocation);
                    break;
                case ExitDirection.LEFT:
                    {
                        // For some reason when warping into the map from the right, we always end up one tile too far left.
                        // We correct this here.
                        Location weirdBugfixLocation = new Location(targetLocation.X + 1, targetLocation.Y);
                        AddWarp(-1, location.Y - 1, targetLocationName, weirdBugfixLocation);
                        AddWarp(-1, location.Y + 0, targetLocationName, weirdBugfixLocation);
                        AddWarp(-1, location.Y + 1, targetLocationName, weirdBugfixLocation);
                    }
                    break;
                case ExitDirection.RIGHT:
                    AddWarp(this.spaceManager.GetMapWidth(), location.Y - 1, targetLocationName, targetLocation);
                    AddWarp(this.spaceManager.GetMapWidth(), location.Y + 0, targetLocationName, targetLocation);
                    AddWarp(this.spaceManager.GetMapWidth(), location.Y + 1, targetLocationName, targetLocation);
                    break;
            }
        }

        private void AddWarps()
        {
            if (!Game1.IsMasterGame)
                return;

            this.random.EnterMasterMode();

            this.warps.Clear();

            if (this.level == 1 || this.parent != null)
            {
                string parentLocationName = GetParentLocationName();
                Location parentWarpLocation = GetParentWarpLocation();
                AddExitWarps(CastEnterDirToExitDir(this.enterDir), this.enterLocation, parentLocationName, parentWarpLocation);
            }

            foreach (var exit in this.exits)
            {
                if (exit.Value.deepWoods != null)
                {
                    AddExitWarps(exit.Key, exit.Value.location, exit.Value.deepWoods.Name, exit.Value.deepWoods.enterLocation);
                }
            }

            this.random.LeaveMasterMode();
        }

        public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
        {
            foreach (ResourceClump resourceClump in this.resourceClumps)
            {
                if (resourceClump.getBoundingBox(resourceClump.tile).Intersects(position))
                    return true;
            }
            return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
        }

        public override bool performToolAction(Tool t, int tileX, int tileY)
        {
            foreach (ResourceClump resourceClump in this.resourceClumps)
            {
                if (resourceClump.getBoundingBox(resourceClump.tile).Contains(tileX * 64, tileY * 64))
                {
                    if (resourceClump.performToolAction(t, 1, resourceClump.tile, this))
                    {
                        this.resourceClumps.Remove(resourceClump);
                        this.terrainFeatures.Remove(resourceClump.tile);
                    }
                    return true;
                }
            }
            return false;
        }

        public override bool isTileLocationTotallyClearAndPlaceable(Vector2 v)
        {
            foreach (ResourceClump resourceClump in this.resourceClumps)
            {
                if (resourceClump.occupiesTile((int)v.X, (int)v.Y))
                    return false;
            }
            return base.isTileLocationTotallyClearAndPlaceable(v);
        }

        protected override void resetSharedState()
        {
            ModEntry.Log("DeepWoods.resetSharedState(): " + this.name.Value);

            if (this.wasConstructedOverNetwork)
            {
                DeepWoods.InitializeMeAndReplaceLocalInstanceWithMe(this);
                this.wasConstructedOverNetwork = false;
            }

            base.resetSharedState();
        }

        protected override void resetLocalState()
        {
            ModEntry.Log("DeepWoods.resetLocalState(): " + this.name.Value);

            if (this.wasConstructedOverNetwork)
            {
                DeepWoods.InitializeMeAndReplaceLocalInstanceWithMe(this);
                this.wasConstructedOverNetwork = false;
            }

            base.resetLocalState();
            foreach (Vector2 lightSource in this.lightSources)
            {
                Game1.currentLightSources.Add(new LightSource(LightSource.indoorWindowLight, lightSource * 64f, 1.0f));
            }
            DeepWoods.FixLighting();
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
                if (Game1.random.NextDouble() < 0.75)
                {
                    Game1.changeMusicTrack("woodsTheme");
                }
                else
                {
                    Game1.changeMusicTrack(Game1.currentSeason + "_day_ambient");
                }
            }
        }

        public override void cleanupBeforePlayerExit()
        {
            base.cleanupBeforePlayerExit();
            Game1.changeMusicTrack("");
        }

        public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
        {
            base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
        }

        public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
        {
            base.drawAboveAlwaysFrontLayer(b);
            foreach (var character in this.characters)
            {
                (character as Monster)?.drawAboveAllLayers(b);
            }
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
            if ((locationName != null && locationName != this.Name) || Game1.random.NextDouble() < 0.5) // Don't use this.random here!
            {
                return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, locationName);
            }
            StardewValley.Object @fish = new StardewValley.Object(800, 1, false, -1, 0);
            return fish;
        }
    }
}
