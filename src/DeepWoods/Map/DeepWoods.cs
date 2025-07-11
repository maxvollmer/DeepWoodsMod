﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeepWoodsMod.API;
using DeepWoodsMod.Stuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Monsters;
using StardewValley.Network;
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
    public class DeepWoods : GameLocation, IDeepWoodsLocation
    {
        public readonly NetString parentName = new NetString();
        public readonly NetPoint parentExitLocation = new NetPoint(Point.Zero);

        public readonly NetBool hasReceivedNetworkData = new NetBool(false);

        public readonly NetInt enterDir = new NetInt(0);
        public readonly NetPoint enterLocation = new NetPoint(Point.Zero);
        public readonly NetObjectList<DeepWoodsExit> exits = new NetObjectList<DeepWoodsExit>();

        public readonly NetLong uniqueMultiplayerID = new NetLong(0);

        public readonly NetInt level = new NetInt(0);
        public readonly NetInt mapWidth = new NetInt(0);
        public readonly NetInt mapHeight = new NetInt(0);

        public readonly NetBool isLichtung = new NetBool(false);
        public readonly NetPoint lichtungCenter = new NetPoint(Point.Zero);

        public readonly NetString clearingType = new NetString(LichtungType.Default);

        public readonly NetBool spawnedFromObelisk = new NetBool(false);

        public readonly NetInt spawnTime = new NetInt(0);
        public readonly NetInt abandonedByParentTime = new NetInt(2600);
        public readonly NetBool hasEverBeenVisited = new NetBool(false);

        public readonly NetInt playerCount = new NetInt(0);

        public readonly NetBool isLichtungSetByAPI = new NetBool(false);
        public readonly NetBool isMapSizeSetByAPI = new NetBool(false);
        public readonly NetBool canGetLost = new NetBool(true);

        public readonly NetVector2Dictionary<int, NetInt> additionalExitLocations = new NetVector2Dictionary<int, NetInt>();

        public readonly NetBool isOverrideMap = new NetBool(false);

        // Local only
        public List<LightSource> lightSources = new List<LightSource>();
        public List<Vector2> baubles = new List<Vector2>();
        public List<WeatherDebris> weatherDebris = new List<WeatherDebris>();

        // Getters for underlying net fields
        public EnterDirection EnterDir { get { return (EnterDirection)enterDir.Value; } set { enterDir.Value = (int)value; } }
        public Location EnterLocation { get { return new Location(enterLocation.Value.X, enterLocation.Value.Y); } set { enterLocation.Value = new Point(value.X, value.Y); } }
        public DeepWoods Parent { get { return Game1.getLocationFromName(parentName.Value) as DeepWoods; } }
        public Location ParentExitLocation { get { return new Location(parentExitLocation.Value.X, parentExitLocation.Value.Y); } set { parentExitLocation.Value = new Point(value.X, value.Y); } }
        public bool HasReceivedNetworkData { get { return Game1.IsMasterGame || hasReceivedNetworkData.Value; } }


        // API
        public IDeepWoodsLocation ParentDeepWoods { get { return Parent; } }
        public bool IsCustomMap { get { return isOverrideMap.Value; } }
        public bool IsClearing
        {
            get
            {
                return isLichtung.Value;
            }
            set
            {
                isLichtung.Value = value;
                isLichtungSetByAPI.Value = true;
            }
        }
        public bool IsInfested => IsClearing && this.clearingType.Value == LichtungType.Infested;
        public bool IsLake => IsClearing && this.clearingType.Value == LichtungType.Lake;

        public Tuple<int, int> MapSize
        {
            get
            {
                return Tuple.Create<int, int>(mapWidth.Value, mapHeight.Value);
            }
            set
            {
                mapWidth.Value = value.Item1;
                mapHeight.Value = value.Item2;
                isMapSizeSetByAPI.Value = true;
            }
        }
        public bool CanGetLost
        {
            get
            {
                return canGetLost.Value;
            }
            set
            {
                canGetLost.Value = value;
                if (!value)
                {
                    Parent?.canGetLost?.Set(value);
                }
            }
        }
        public int Level { get { return this.level.Value; } }
        public int EnterSide { get { return (int)this.EnterDir; } }
        public bool IsLost
        {
            get
            {
                if (level.Value == 1)
                    return false;

                return Parent?.IsLost ?? true;
            }
        }
        public double LuckLevel
        {
            get
            {
                return Math.Max(0.0, Math.Max(1.0, this.GetLuckLevel() / 10.0));
            }
        }
        public int CombatLevel { get { return this.GetCombatLevel(); } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidImplicitNetFieldCast")]
        public IEnumerable<IDeepWoodsExit> Exits
        {
            get
            {
                return this.exits;
            }
        }
        public ICollection<ResourceClump> ResourceClumps { get { return base.resourceClumps; } }
        public ICollection<Vector2> Baubles { get { return this.baubles; } }
        public ICollection<WeatherDebris> WeatherDebris { get { return this.weatherDebris; } }


        private bool IsNullOrEmpty(string s)
        {
            return s == null || s.Length == 0;
        }

        private int seed = 0;
        public int Seed
        {
            get
            {
                if (seed == 0 && !IsNullOrEmpty(Name))
                {
                    if (Name == "DeepWoods")
                        seed = DeepWoodsRandom.CalculateSeed(1, EnterDirection.FROM_TOP, null);
                    else
                        seed = Int32.Parse(Name.Substring(10));
                }
                return seed;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        public DeepWoods()
            : base()
        {
            base.locationContextId = "Default";
            base.mapPath.Value = null;
            base.loadedMapPath = null;
            base._mapPathDirty = false;
            //typeof(GameLocation).GetField("seasonOverride").SetValue(this, new Lazy<Season>(null));
            base.name.Value = string.Empty;
            base.critters = new List<Critter>();
            this.updateMap();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        public DeepWoods(string name)
            : this()
        {
            base.name.Value = name;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        public DeepWoods(DeepWoods parent, int level, EnterDirection enterDir, bool spawnedFromObelisk)
            : this()
        {
            this.spawnedFromObelisk.Value = spawnedFromObelisk;

            base.isOutdoors.Value = true;
            base.ignoreDebrisWeather.Value = true;
            base.ignoreOutdoorLighting.Value = true;

            this.hasReceivedNetworkData.Value = true;

            this.uniqueMultiplayerID.Value = Game1.MasterPlayer.UniqueMultiplayerID;
            this.seed = DeepWoodsRandom.CalculateSeed(level, enterDir, parent?.Seed);
            if (level == 1)
            {
                base.name.Value = "DeepWoods";
            }
            else
            {
                base.name.Value = "DeepWoods_" + this.seed;
            }
            this.parentName.Value = parent?.Name;
            this.ParentExitLocation = parent?.GetExit(EnterDirToExitDir(enterDir))?.Location ?? new Location();
            this.level.Value = level;
            DeepWoodsState.LowestLevelReached = Math.Max(DeepWoodsState.LowestLevelReached, this.level.Value);
            this.EnterDir = enterDir;
            this.spawnTime.Value = Game1.timeOfDay;

            ModEntry.GetAPI().CallOnCreate(this);

            CreateSpace();
            DetermineExits();
            updateMap();

            FillLevel();

            if (parent == null && level > 1 && !this.HasExit(CastEnterDirToExitDir(this.EnterDir)))
            {
                this.exits.Add(new DeepWoodsExit(this, CastEnterDirToExitDir(this.EnterDir), this.EnterLocation));
            }

            if (parent != null)
            {
                ModEntry.Log($"Child spawned, time: {Game1.timeOfDay}, name: {this.Name}, level: {this.level}, parent: {this.parentName}, enterDir: {this.EnterDir}, enterLocation: {this.EnterLocation.X}, {this.EnterLocation.Y}", LogLevel.Trace);
            }
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddField(parentName).AddField(parentExitLocation).AddField(hasReceivedNetworkData).AddField(clearingType).AddField(enterDir).AddField(enterLocation).AddField(exits).AddField(uniqueMultiplayerID).AddField(level).AddField(mapWidth).AddField(mapHeight).AddField(isLichtung).AddField(lichtungCenter).AddField(spawnedFromObelisk).AddField(hasEverBeenVisited).AddField(spawnTime).AddField(abandonedByParentTime).AddField(playerCount).AddField(isLichtungSetByAPI).AddField(isMapSizeSetByAPI).AddField(canGetLost).AddField(additionalExitLocations).AddField(isOverrideMap);
        }

        private void FillLevel()
        {
            HashSet<Location> blockedLocations = new();

            if (this.level.Value == 1)
            {
                DeepWoodsStuffCreator.FillFirstLevel(this, new DeepWoodsRandom(this, this.seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()), blockedLocations);
                return;
            }

            if (IsInfested)
            {
                ModEntry.GetAPI().CallBeforeInfest(this);
                if (!ModEntry.GetAPI().CallOverrideInfest(this))
                {
                    DeepWoodsStuffCreator.Infest(this, new DeepWoodsRandom(this, this.seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()), blockedLocations);
                }
                ModEntry.GetAPI().CallAfterInfest(this);
            }
            else
            {
                ModEntry.GetAPI().CallBeforeFill(this);
                if (IsLake || !ModEntry.GetAPI().CallOverrideFill(this))
                {
                    DeepWoodsStuffCreator.AddStuff(this, new DeepWoodsRandom(this, this.seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()), blockedLocations);
                }
                ModEntry.GetAPI().CallAfterFill(this);
            }

            ModEntry.GetAPI().CallBeforeMonsterGeneration(this);
            if (!ModEntry.GetAPI().CallOverrideMonsterGeneration(this))
            {
                DeepWoodsMonsters.AddMonsters(this, new DeepWoodsRandom(this, this.seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()), blockedLocations);
            }
            ModEntry.GetAPI().CallAfterMonsterGeneration(this);
        }

        private void DetermineExits()
        {
            if (!Game1.IsMasterGame)
                throw new ApplicationException("Illegal call to DeepWoods.DetermineExits() in client.");

            this.exits.Clear();

            List<ExitDirection> possibleExitDirs = AllExitDirsBut(CastEnterDirToExitDir(this.EnterDir));

            // first level always exits left and right
            if (this.level.Value == 1)
            {
                possibleExitDirs.Remove(ExitDirection.BOTTOM);
            }
            else
            {
                int numExitDirs = Game1.random.Next(1, 4);
                if (numExitDirs < 3)
                {
                    possibleExitDirs.RemoveAt(Game1.random.Next(0, possibleExitDirs.Count));
                    if (numExitDirs < 2)
                    {
                        possibleExitDirs.RemoveAt(Game1.random.Next(0, possibleExitDirs.Count));
                    }
                }
            }

            foreach (ExitDirection exitDir in possibleExitDirs)
            {
                this.exits.Add(
                    new DeepWoodsExit(
                        this,
                        exitDir,
                        new DeepWoodsSpaceManager(this.mapWidth.Value, this.mapHeight.Value).GetRandomExitLocation(exitDir, new DeepWoodsRandom(this, this.seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()))
                    )
                    {
                        TargetLocationName = "DeepWoods_" + DeepWoodsRandom.CalculateSeed(level.Value + 1, ExitDirToEnterDir(exitDir), Seed)
                    }
                );
            }
        }

        private void CreateSpace()
        {
            if (!Game1.IsMasterGame)
                throw new ApplicationException("Illegal call to DeepWoods.CreateSpace in client.");

            // first level is special
            if (this.level.Value == 1)
            {
                this.isLichtung.Value = true;
                this.EnterLocation = Settings.Map.RootLevelEnterLocation;
                this.mapWidth.Value = 30;
                this.mapHeight.Value = 30;
                return;
            }


            var random = new DeepWoodsRandom(this, this.Seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next());

            if (!this.isLichtungSetByAPI.Value)
            {
                if (this.level.Value >= Settings.Level.MinLevelForClearing)
                {
                    if (random.CheckChance(Settings.Luck.Clearings.ChanceForClearing))
                    {
                        this.isLichtung.Value = true;
                    }
                    else if (Settings.Level.EnableGuaranteedClearings)
                    {
                        if (this.level.Value == Settings.Level.MinLevelForClearing || this.level.Value % Settings.Level.GuaranteedClearingsFrequency == 0)
                            this.isLichtung.Value = true;
                    }
                }
            }

            if (!this.isMapSizeSetByAPI.Value)
            {
                if (this.isLichtung.Value)
                {
                    this.mapWidth.Value = Game1.random.Next(Settings.Map.MinMapWidth, Settings.Map.MaxMapWidthForClearing);
                    this.mapHeight.Value = Game1.random.Next(Settings.Map.MinMapWidth, Settings.Map.MaxMapWidthForClearing);

                    if (!this.spawnedFromObelisk.Value)
                    {
                        clearingType.Value = random.GetRandomValue(Settings.Luck.Clearings.ClearingType);
                    }
                }
                else
                {
                    this.mapWidth.Value = Game1.random.Next(Settings.Map.MinMapWidth, Settings.Map.MaxMapWidth);
                    this.mapHeight.Value = Game1.random.Next(Settings.Map.MinMapHeight, Settings.Map.MaxMapHeight);
                }
            }

            this.EnterLocation = new DeepWoodsSpaceManager(this.mapWidth.Value, this.mapHeight.Value).GetRandomEnterLocation(this.EnterDir, random);
        }

        public void RemovePlayer(Farmer who)
        {
            ModEntry.Log($"RemovePlayer({who.UniqueMultiplayerID}): {this.Name}", LogLevel.Trace);

            if (who == Game1.player)
            {
                if (DeepWoodsManager.currentDeepWoods == this)
                    DeepWoodsManager.currentDeepWoods = null;
            }

            if (!Game1.IsMasterGame)
                return;

            this.playerCount.Value = this.playerCount.Value - 1;
        }

        public void FixPlayerPosAfterWarp(Farmer who)
        {
            // Only fix position for local player
            if (who != Game1.player)
                return;

            // Check if level is properly initialized
            if (this.map == null
                || this.map.Id != this.Name
                || this.Seed == 0
                || !this.HasReceivedNetworkData
                || mapWidth.Value == 0
                || mapHeight.Value == 0)
                return;

            ModEntry.Log($"FixPlayerPosAfterWarp: {this.Name}, mapWidth: {mapWidth}, pos: {who.Position.X}, {who.Position.Y}", LogLevel.Trace);

            // First check for current warp request (stored globally for local player):
            if (DeepWoodsManager.currentWarpRequestName == this.Name
                && DeepWoodsManager.currentWarpRequestLocation.HasValue)
            {
                who.Position = DeepWoodsManager.currentWarpRequestLocation.Value;
                DeepWoodsManager.currentWarpRequestName = null;
                DeepWoodsManager.currentWarpRequestLocation = null;
            }
            // then check if we spawned at the minecart (no action needed):
            else if (Level == 1 && who.Position.X == DeepWoodsMineCart.MineCartLocation.X && who.Position.Y == (DeepWoodsMineCart.MineCartLocation.Y + 1))
            {
                // noop
            }
            // Otherwise we will heuristically determine the nearest valid location:
            else
            {
                Vector2 nearestEnterLocation = new Vector2(EnterLocation.X * 64, EnterLocation.Y * 64);
                float nearestEnterLocationDistance = (nearestEnterLocation - who.Position).Length();
                int faceDirection = EnterDirToFacingDirection(this.EnterDir);
                foreach (var exit in this.exits)
                {
                    Vector2 exitLocation = new Vector2(exit.Location.X * 64, exit.Location.Y * 64);
                    float exitDistance = (exitLocation - who.Position).Length();
                    if (exitDistance < nearestEnterLocationDistance)
                    {
                        nearestEnterLocation = exitLocation;
                        nearestEnterLocationDistance = exitDistance;
                        faceDirection = EnterDirToFacingDirection(CastExitDirToEnterDir(exit.ExitDir));
                    }
                }
                who.Position = nearestEnterLocation;
            }

            // Finally fix any errors on the border (this still happens according to some bug reports)
            who.Position = new Vector2(
                Math.Max(0, Math.Min((mapWidth.Value - 1) * 64, who.Position.X)),
                Math.Max(0, Math.Min((mapHeight.Value - 1) * 64, who.Position.Y))
                );
        }

        public void AddPlayer(Farmer who)
        {
            ModEntry.Log($"AddPlayer({who.UniqueMultiplayerID}): {this.Name}", LogLevel.Trace);

            if (who == Game1.player)
            {
                // Fix enter position (some bug I haven't figured out yet spawns network clients outside the map delimiter...)
                FixPlayerPosAfterWarp(who);
                DeepWoodsManager.currentDeepWoods = this;
            }

            if (!Game1.IsMasterGame)
                return;

            this.hasEverBeenVisited.Value = true;
            this.playerCount.Value = this.playerCount.Value + 1;
            ValidateAndIfNecessaryCreateExitChildren();
        }

        public bool infestedTreeIsDrawing = false;

        public override bool SeedsIgnoreSeasonsHere()
        {
            return !infestedTreeIsDrawing;
        }

        public void ValidateAndIfNecessaryCreateExitChildren()
        {
            if (!Game1.IsMasterGame)
                return;

            if (this.playerCount.Value <= 0)
                return;

            if (this.level.Value > 1 && this.Parent == null && !this.HasExit(CastEnterDirToExitDir(this.EnterDir)))
            {
                // this.abandonedByParentTime = Game1.timeOfDay;
                this.exits.Add(new DeepWoodsExit(this, CastEnterDirToExitDir(this.EnterDir), this.EnterLocation));
            }

            foreach (var exit in this.exits)
            {
                DeepWoods exitDeepWoods = Game1.getLocationFromName(exit.TargetLocationName) as DeepWoods;
                if (exitDeepWoods == null)
                {
                    exitDeepWoods = new DeepWoods(this, this.level.Value + 1, ExitDirToEnterDir(exit.ExitDir), false);
                    DeepWoodsManager.AddDeepWoodsToGameLocations(exitDeepWoods);
                }
                exit.TargetLocationName = exitDeepWoods.Name;
                exit.TargetLocation = exitDeepWoods.EnterLocation;
            }
        }


        private DeepWoodsExit GetExit(ExitDirection exitDir)
        {
            foreach (var exit in this.exits)
            {
                if (exit.ExitDir == exitDir)
                {
                    return exit;
                }
            }
            return null;
        }

        private bool HasExit(ExitDirection exitDir)
        {
            return GetExit(exitDir) != null;
        }

        public void RandomizeExits()
        {
            if (!Game1.IsMasterGame)
                return;

            if (!this.hasEverBeenVisited.Value)
                return;

            if (!CanGetLost)
                return;

            if (this.level.Value > 2
                && !this.HasExit(CastEnterDirToExitDir(this.EnterDir))
                && (Parent?.CanGetLost ?? true))
            {
                // this.abandonedByParentTime = Game1.timeOfDay;
                this.parentName.Value = null;
                this.parentExitLocation.Value = Point.Zero;
                this.exits.Add(new DeepWoodsExit(this, CastEnterDirToExitDir(this.EnterDir), this.EnterLocation));
            }

            foreach (var exit in this.exits)
            {
                // Randomize exit if child level exists and has been visited
                if (exit.TargetLocationName != null
                    && Game1.getLocationFromName(exit.TargetLocationName) is DeepWoods exitDeepWoods
                    && exitDeepWoods.hasEverBeenVisited.Value
                    && exitDeepWoods.CanGetLost)
                {
                    // Don't randomize if there's a player here AND in the child level (don't separate multiplayer groups)
                    if (this.farmers.Count == 0 || exitDeepWoods.farmers.Count == 0)
                    {
                        exit.TargetLocationName = null;
                    }
                }
            }

            ValidateAndIfNecessaryCreateExitChildren();
        }

        public bool TryRemove()
        {
            if (!Game1.IsMasterGame)
                throw new ApplicationException("Illegal call to DeepWoods.TryRemove() in client.");

            if (this.level.Value == 1)
                return false;

            if (this.playerCount.Value > 0)
                return false;

            if ((this.Parent?.playerCount?.Value ?? 0) > 0 && Game1.timeOfDay <= (this.abandonedByParentTime.Value + TIME_BEFORE_DELETION_ALLOWED))
                return false;

            if (Game1.timeOfDay <= (this.spawnTime.Value + TIME_BEFORE_DELETION_ALLOWED))
                return false;

            foreach (var exit in this.exits)
            {
                if (Game1.getLocationFromName(exit.TargetLocationName) is DeepWoods exitDeepWoods)
                {
                    exitDeepWoods.parentName.Value = null;
                    exitDeepWoods.parentExitLocation.Value = Point.Zero;
                }
            }

            this.parentName.Value = null;
            this.parentExitLocation.Value = Point.Zero;

            this.exits.Clear();
            this.characters.Clear();
            this.terrainFeatures.Clear();
            this.largeTerrainFeatures.Clear();
            this.resourceClumps.Clear();

            DeepWoodsManager.RemoveDeepWoodsFromGameLocations(this);
            return true;
        }

        public override void updateSeasonalTileSheets(Map map = null)
        {
        }

        private Map CreateEmptyMap(string name, int mapWidth, int mapHeight)
        {
            // Create new map
            Map map = new Map(name);

            // Add outdoor tilesheet

            map.AddTileSheet(new TileSheet(DEFAULT_OUTDOOR_TILESHEET_ID, map, "Maps\\" + Game1.currentSeason.ToLower() + "_outdoorsTileSheet", new Size(25, 79), new Size(16, 16)));
            map.AddTileSheet(new TileSheet(INFESTED_OUTDOOR_TILESHEET_ID, map, "Maps\\deepWoodsInfestedOutdoorsTileSheet", new Size(25, 79), new Size(16, 16)));
            map.AddTileSheet(new TileSheet(LAKE_TILESHEET_ID, map, "Maps\\deepWoodsLakeTilesheet", new Size(8, 5), new Size(16, 16)));

            for (int i = 0; i < map.TileSheets.Count; i++)
            {
                Game1.mapDisplayDevice.LoadTileSheet(map.TileSheets[i]);
            }
            map.LoadTileSheets(Game1.mapDisplayDevice);

            // Add default layers
            map.AddLayer(new Layer("Back", map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));
            map.AddLayer(new Layer("Buildings", map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));
            map.AddLayer(new Layer("Front", map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));
            map.AddLayer(new Layer("Paths", map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));
            map.AddLayer(new Layer("AlwaysFront", map, new xTile.Dimensions.Size(mapWidth, mapHeight), new xTile.Dimensions.Size(64, 64)));

            return map;
        }

        public override string GetLocationContextId()
        {
            if (map == null)
            {
                updateMap();
            }
            return base.GetLocationContextId();
        }

        public override void updateMap()
        {
            // Always create an empty map, to avoid crashes
            // (give it maximum size, so game doesn't mess with warp locations on network)
            if (this.map == null)
                this.map = CreateEmptyMap("DEEPWOODSEMPTY", Settings.Map.MaxMapWidth, Settings.Map.MaxMapHeight);

            // Check if level is properly initialized
            if (this.Seed == 0)
                return;

            // Check that network data has been sent and initialized by server
            if (!this.HasReceivedNetworkData)
                return;

            // Check if map is already created
            if (this.map != null && this.map.Id == this.Name)
            {
                return;
            }

            // Check that mapWidth and mapHeight are set
            if (mapWidth.Value == 0 || mapHeight.Value == 0)
                return;

            // Create map with proper size
            this.map = CreateEmptyMap(this.Name, mapWidth.Value, mapHeight.Value);

            // Build the map!
            ModEntry.GetAPI().CallBeforeMapGeneration(this);
            if (ModEntry.GetAPI().CallOverrideMapGeneration(this))
            {
                this.isOverrideMap.Value = true;
                // Make sure map id is our name, otherwise game will reload the map every frame crashing the game
                this.map.Id = this.Name;
            }
            else
            {
                DeepWoodsBuilder.Build(this, this.map, DeepWoodsEnterExit.CreateExitDictionary(this.EnterDir, this.EnterLocation, this.exits));
            }
            ModEntry.GetAPI().CallAfterMapGeneration(this);

            SortLayers();
        }

        // This is the default day update method of GameLocation, called only on the server
        public override void DayUpdate(int dayOfMonth)
        {
            if (largeTerrainFeatures != null)
            {
                foreach (var largeTerrainFeature in largeTerrainFeatures.ToArray())
                {
                    if (largeTerrainFeature is Tent tent)
                    {
                        tent.dayUpdate();
                    }
                }
            }
        }

        public void AddExitLocation(Location tile, DeepWoodsExit exit)
        {
            additionalExitLocations[new Vector2(tile.X, tile.Y)] = exit != null ? (int)exit.ExitDir : -1;
        }

        public void RemoveExitLocation(Location tile)
        {
            additionalExitLocations.Remove(new Vector2(tile.X, tile.Y));
        }

        public void CheckWarp()
        {
            if (Game1.player.currentLocation == this && Game1.currentLocation == this && Game1.locationRequest == null)
            {
                if (Game1.player.Position.X + 48 < 0)
                    Warp(ExitDirection.LEFT);
                else if (Game1.player.Position.Y + 48 < 0)
                    Warp(ExitDirection.TOP);
                else if (Game1.player.Position.X + 16 > this.mapWidth.Value * 64)
                    Warp(ExitDirection.RIGHT);
                else if (Game1.player.Position.Y + 16 > this.mapHeight.Value * 64)
                    Warp(ExitDirection.BOTTOM);
                else
                {
                    var playerRectangle = new Microsoft.Xna.Framework.Rectangle((int)(Game1.player.Position.X + 8), (int)(Game1.player.Position.Y + 8), 64 - 16, 64 - 16);
                    foreach (var additionalExitLocation in additionalExitLocations.Keys)
                    {
                        var additionalExitLocationRectangle = new Microsoft.Xna.Framework.Rectangle((int)(additionalExitLocation.X * 64 + 8), (int)(additionalExitLocation.Y * 64 + 8), 64 - 16, 64 - 16);
                        if (playerRectangle.Intersects(additionalExitLocationRectangle))
                        {
                            if (additionalExitLocations[additionalExitLocation] == -1)
                                Warp(CastEnterDirToExitDir(EnterDir));
                            else
                                Warp((ExitDirection)additionalExitLocations[additionalExitLocation]);
                        }
                    }
                }
            }
        }

        private void Warp(ExitDirection exitDir)
        {
            if (Game1.locationRequest == null)
            {
                bool warped = false;

                string targetDeepWoodsName = null;
                Location? targetLocationWrapper = null;

                if (level.Value == 1 && exitDir == ExitDirection.TOP)
                {
                    targetDeepWoodsName = "Woods";
                    targetLocationWrapper = new Location(DeepWoodsSettings.Settings.WoodsPassage.WoodsWarpLocation.X, DeepWoodsSettings.Settings.WoodsPassage.WoodsWarpLocation.Y);
                }
                else if (GetExit(exitDir) is DeepWoodsExit exit)
                {
                    targetDeepWoodsName = exit.TargetLocationName;
                    if (exit.TargetLocation.X == 0 && exit.TargetLocation.Y == 0)
                    {
                        if (Game1.getLocationFromName(targetDeepWoodsName) is DeepWoods exitDeepWoods)
                            exit.TargetLocation = new Location(exitDeepWoods.enterLocation.X, exitDeepWoods.enterLocation.Y);
                    }
                    targetLocationWrapper = exit.TargetLocation;
                }
                else if (CastEnterDirToExitDir(EnterDir) == exitDir)
                {
                    targetDeepWoodsName = parentName.Value;
                    if (ParentExitLocation.X == 0 && ParentExitLocation.Y == 0)
                    {
                        if (Game1.getLocationFromName(targetDeepWoodsName) is DeepWoods parentDeepWoods)
                            ParentExitLocation = parentDeepWoods.GetExit(EnterDirToExitDir(EnterDir)).Location;
                    }
                    targetLocationWrapper = ParentExitLocation;
                }

                ModEntry.Log($"Trying to warp from {this.Name}: (ExitDir: {exitDir}, Position: {Game1.player.Position.X}, {Game1.player.Position.Y}, targetDeepWoodsName: {targetDeepWoodsName}, targetLocation: {(targetLocationWrapper?.X ?? -1)}, {(targetLocationWrapper?.Y ?? -1)})", LogLevel.Trace);

                if (targetLocationWrapper.HasValue && targetDeepWoodsName != null)
                {
                    Location targetLocation = targetLocationWrapper.Value;

                    if (!(targetLocation.X == 0 && targetLocation.Y == 0))
                    {
                        if (exitDir == ExitDirection.LEFT)
                            targetLocation.X += 1;
                        else if (exitDir == ExitDirection.BOTTOM)
                            targetLocation.Y += 1;

                        if (targetDeepWoodsName != "Woods")
                        {
                            DeepWoodsManager.currentWarpRequestName = targetDeepWoodsName;
                            DeepWoodsManager.currentWarpRequestLocation = new Vector2(targetLocation.X * 64, targetLocation.Y * 64);
                            if (!Game1.IsMasterGame)
                                DeepWoodsManager.AddBlankDeepWoodsToGameLocations(targetDeepWoodsName);
                        }

                        Game1.warpFarmer(targetDeepWoodsName, targetLocation.X, targetLocation.Y, false);
                        warped = true;
                    }
                }

                if (!warped)
                {
                    ModEntry.Log("Warp from " + this.Name + " failed. (ExitDir: " + exitDir + ")", LogLevel.Warn);
                }
            }
        }

        public override bool performToolAction(Tool t, int tileX, int tileY)
        {
            foreach (ResourceClump resourceClump in this.resourceClumps)
            {
                if (resourceClump.occupiesTile(tileX, tileY))
                {
                    if (resourceClump.performToolAction(t, 1, resourceClump.Tile))
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
            // No placements on border tiles.
            if (v.X <= 0 || v.Y <= 0 || v.X >= (mapWidth.Value - 2) || v.Y >= (mapHeight.Value - 2))
                return true;

            // No placements on exits.
            foreach (var exit in this.exits)
            {
                Microsoft.Xna.Framework.Rectangle exitRectangle = new Microsoft.Xna.Framework.Rectangle(exit.Location.X - Settings.Map.ExitRadius, exit.Location.Y - Settings.Map.ExitRadius, Settings.Map.ExitRadius * 2 + 1, Settings.Map.ExitRadius * 2 + 1);
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

        public override bool CanItemBePlacedHere(Vector2 v, bool itemIsPassable = false, CollisionMask collisionMask = CollisionMask.All, CollisionMask ignorePassables = CollisionMask.None, bool useFarmerTile = false, bool ignorePassablesExactly = false)
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
            return base.CanItemBePlacedHere(v, itemIsPassable, collisionMask, ignorePassables, useFarmerTile, ignorePassablesExactly);
        }

        protected override void resetSharedState()
        {
            base.resetSharedState();
        }

        public override void tryToAddCritters(bool onlyIfOnScreen = false)
        {
            if (IsInfested)
                return;

            // TODO: Better critter spawning in forest
            base.tryToAddCritters(onlyIfOnScreen);
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();

            this.tryToAddCritters(false);

            ModEntry.GetAPI().CallBeforeDebrisCreation(this);
            if (!ModEntry.GetAPI().CallOverrideDebrisCreation(this))
            {
                DeepWoodsDebris.Initialize(this);
            }
            ModEntry.GetAPI().CallAfterDebrisCreation(this);

            foreach (var lightSource in this.lightSources)
            {
                Game1.currentLightSources.TryAdd(lightSource.Id, lightSource);
            }

            DeepWoodsManager.FixLighting();
        }

        public override void performTenMinuteUpdate(int timeOfDay)
        {
            base.performTenMinuteUpdate(timeOfDay);

            // TODO: Better critter spawning in forest
            if (this.map != null)
                this.tryToAddCritters(true);
        }

        public override void checkForMusic(GameTime time)
        {
            if (!Game1.isRaining && IsInfested)
            {
                Game1.changeMusicTrack("tribal");
                return;
            }

            if (Game1.currentSong != null && Game1.currentSong.IsPlaying)
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
                        if (Game1.isDarkOut(this))
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
            // Intercept exploding bombs
            base.temporarySprites
                .Where(t => t.bombRadius > 0)
                .ToList()
                .ForEach(t => t.endFunction = new TemporaryAnimatedSprite.endBehavior(delegate (int extraInfo) {
                    HandleExplosion(t.position / 64, t.bombRadius);
                })
            );
            base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
        }

        private void HandleExplosion(Vector2 tile, int radius)
        {
            if (radius <= 0)
                return;

            List<ResourceClump> resourceClumpsCopy = new List<ResourceClump>(resourceClumps);
            List<LargeTerrainFeature> largeTerrainFeaturesCopy = new List<LargeTerrainFeature>(largeTerrainFeatures);

            bool[,] circleOutlineGrid = Game1.getCircleOutlineGrid(radius);
            for (int x = 0; x < radius * 2 + 1; x++)
            {
                bool isInBombRadius = false;
                for (int y = 0; y < radius * 2 + 1; y++)
                {
                    if (circleOutlineGrid[x, y])
                        isInBombRadius = !isInBombRadius;

                    if (isInBombRadius)
                    {
                        Vector2 location = new Vector2(tile.X + x - radius, tile.Y + y - radius);
                        resourceClumpsCopy.RemoveAll(r =>
                        {
                            if (r.getBoundingBox().Contains((int)location.X * 64, (int)location.Y * 64))
                            {
                                if (r.performToolAction(null, radius, location))
                                {
                                    resourceClumps.Remove(r);
                                }
                                return true;
                            }
                            return false;
                        });
                        largeTerrainFeaturesCopy.RemoveAll(lt =>
                        {
                            if (lt.getBoundingBox().Contains((int)location.X * 64, (int)location.Y * 64))
                            {
                                if (lt.performToolAction(null, radius, location))
                                {
                                    largeTerrainFeatures.Remove(lt);
                                }
                                return true;
                            }
                            return false;
                        });
                        if (this.terrainFeatures.ContainsKey(location) && this.terrainFeatures[location] is Flower)
                        {
                            this.terrainFeatures.Remove(location);
                        }
                    }
                }
            }
        }

        public override bool isActionableTile(int xTile, int yTile, Farmer who)
        {
            if (Level == 1 && (largeTerrainFeatures.Where(l => l is MaxHut).Select(l => l as MaxHut).FirstOrDefault()?.isActionableTile(new Vector2(xTile, yTile), who) ?? false))
            {
                return true;
            }

            return base.isActionableTile(xTile, yTile, who);
        }

        public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            if (Level == 1 && (largeTerrainFeatures.Where(l => l is MaxHut).Select(l => l as MaxHut).FirstOrDefault()?.doAction(new Vector2(tileLocation.X, tileLocation.Y), who) ?? false))
            {
                return true;
            }

            return base.checkAction(tileLocation, viewport, who);
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);
            DeepWoodsDebris.Update(this, time);

            if (this.Level == 1)
            {
                var maxHutLocation = new Vector2(EnterLocation.X + Settings.Map.ExitRadius + 2, EnterLocation.Y);

                temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(((int)maxHutLocation.X + 4) * 64 + -20, ((int)maxHutLocation.Y + 3) * 64 - 420), flipped: false, 0.002f, Color.Gray)
                {
                    alpha = 0.75f,
                    motion = new Vector2(0f, -0.5f),
                    acceleration = new Vector2(0.002f, 0f),
                    interval = 99999f,
                    layerDepth = 1f,
                    scale = 2f,
                    scaleChange = 0.02f,
                    rotationChange = (float)Game1.random.Next(-5, 6) * MathF.PI / 256f
                });
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
        }

        public void DrawLevelDisplay()
        {
            // Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);

            string currentLevelAsString = string.Concat(this.level);
            Location titleSafeTopLeftCorner = new DeepWoodsSpaceManager(this.mapWidth.Value, this.mapHeight.Value).GetActualTitleSafeTopleftCorner();

            SpriteText.drawString(
                Game1.spriteBatch,
                currentLevelAsString,
                titleSafeTopLeftCorner.X + 16, titleSafeTopLeftCorner.Y + 16, /*x,y*/
                999999, -1, 999999, /*charPos,width,height*/
                1f, 1f, /*alpha,depth*/
                false, /*junimoText*/
                SpriteText.scrollStyle_darkMetal,
                "", /*placeHolderScrollWidthText*/
                SpriteText.color_Green);

            // Game1.spriteBatch.End();
        }

        public override StardewValley.Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
        {
            if ((locationName != null && locationName != this.Name) || !CanHazAwesomeFish())
            {
                return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
            }
            return new StardewValley.Object(GetRandomAwesomeFish(), 1, false, -1, 0);
        }

        private bool CanHazAwesomeFish()
        {
            return new DeepWoodsRandom(this, this.Seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()).CheckChance(Settings.Luck.Fishies.ChanceForAwesomeFish);
        }

        private string GetRandomAwesomeFish()
        {
            return new DeepWoodsRandom(this, this.Seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()).GetRandomValue(Settings.Luck.Fishies.AwesomeFishies).ToString();
        }

        public int GetCombatLevel()
        {
            int parentCombatLevel = this.Parent?.GetCombatLevel() ?? 0;
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
                if (farmer.currentLocation == this
                    || (farmer.currentLocation is DeepWoods && farmer.currentLocation == this.Parent))
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

        protected override void updateCharacters(GameTime time)
        {
            base.updateCharacters(time);

            if (Context.IsMainPlayer && IsInfested && !this.characters.Any(c => c is Monster))
            {
                DeInfest();
            }
        }

        public void DeInfest()
        {
            if (Context.IsMainPlayer)
            {
                foreach (Farmer who in Game1.otherFarmers.Values)
                {
                    if (who != Game1.player)
                    {
                        ModEntry.SendMessage(Name, MessageId.DeInfest, who.UniqueMultiplayerID);
                    }
                }
            }

            // not infested anymore!
            this.clearingType.Value = LichtungType.Default;

            if (Game1.player.currentLocation == this)
            {
                // audible feedback
                Game1.playSound(Sounds.YOBA);

                // start woods theme
                if (!Game1.isRaining)
                {
                    Game1.changeMusicTrack("woodsTheme");
                }
            }

            // restore good debris
            ModEntry.GetAPI().CallBeforeDebrisCreation(this);
            if (!ModEntry.GetAPI().CallOverrideDebrisCreation(this))
            {
                DeepWoodsDebris.Initialize(this);
            }
            ModEntry.GetAPI().CallAfterDebrisCreation(this);

            // remove infested look
            DeepWoodsBuilder.RemoveInfested(this, this.map);

            // spawn a gift
            DeepWoodsStuffCreator.ClearAndGiftInfestedLevel(this, new DeepWoodsRandom(this, this.seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()));

            // spawn critters
            tryToAddCritters(false);
        }

        internal void AddLightSource(Vector2 pos)
        {
            string id = $"DeepWoodsLight_{level}_{lightSources.Count}";
            lightSources.Add(new LightSource(id, LightSource.indoorWindowLight, pos * 64f, 1.0f));
        }
    }
}
