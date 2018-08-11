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
    public class DeepWoods : GameLocation
    {
        public readonly NetInt enterDir = new NetInt(0);
        public readonly NetPoint enterLocation = new NetPoint(Point.Zero);
        public readonly NetObjectList<DeepWoodsExit> exits = new NetObjectList<DeepWoodsExit>();

        public readonly NetLong uniqueMultiplayerID = new NetLong(0);

        public readonly NetInt level = new NetInt(0);
        public readonly NetInt mapWidth = new NetInt(0);
        public readonly NetInt mapHeight = new NetInt(0);

        public readonly NetBool isLichtung = new NetBool(false);
        public readonly NetBool lichtungHasLake = new NetBool(false);
        public readonly NetPoint lichtungCenter = new NetPoint(Point.Zero);

        public readonly NetBool spawnedFromObelisk = new NetBool(false);

        public readonly NetInt spawnTime = new NetInt(0);
        public readonly NetInt abandonedByParentTime = new NetInt(2600);
        public readonly NetBool hasEverBeenVisited = new NetBool(false);

        public readonly NetInt playerCount = new NetInt(0);

        public readonly NetObjectList<ResourceClump> resourceClumps = new NetObjectList<ResourceClump>();

        // Local only
        public DeepWoods parent = null;

        public List<Vector2> lightSources = new List<Vector2>();
        public List<Vector2> baubles = new List<Vector2>();
        public List<WeatherDebris> weatherDebris = new List<WeatherDebris>();
        public bool validateAndIfNecessaryCreateExitChildrenHasRunSinceLastExitRandomization = false;

        public EnterDirection EnterDir { get { return (EnterDirection)enterDir.Value; } set { enterDir.Value = (int)value; } }
        public Location EnterLocation { get { return new Location(enterLocation.Value.X, enterLocation.Value.Y); } set { enterLocation.Value = new Point(value.X, value.Y); } }

        private int seed = 0;
        public int Seed
        {
            get
            {
                if (seed == 0 && Name.Length > 0)
                {
                    if (Name == "DeepWoods")
                        seed = DeepWoodsRandom.CalculateSeed(1, EnterDirection.FROM_TOP, null);
                    else
                        seed = Int32.Parse(Name.Substring(10));
                }
                return seed;
            }
        }

        public DeepWoods()
            : base()
        {
            base.critters = new List<Critter>();
        }

        public DeepWoods(string name)
            : base()
        {
            base.critters = new List<Critter>();
            base.name.Value = name;
        }

        public DeepWoods(DeepWoods parent, int level, EnterDirection enterDir)
            : this()
        {
            base.isOutdoors.Value = true;
            base.ignoreDebrisWeather.Value = true;
            base.ignoreOutdoorLighting.Value = true;

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
            this.parent = parent;
            this.level.Value = level;
            DeepWoodsState.LowestLevelReached = Math.Max(DeepWoodsState.LowestLevelReached, this.level.Value - 1);
            this.EnterDir = enterDir;
            this.spawnTime.Value = Game1.timeOfDay;

            this.spawnedFromObelisk.Value = parent?.spawnedFromObelisk?.Value ?? false;

            CreateSpace();
            DetermineExits();
            updateMap();

            DeepWoodsStuffCreator.AddStuff(this, new DeepWoodsRandom(this, this.seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()));
            DeepWoodsMonsters.AddMonsters(this, new DeepWoodsRandom(this, this.seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()));

            if (parent == null && level > 1 && !this.HasExit(CastEnterDirToExitDir(this.EnterDir)))
            {
                this.exits.Add(new DeepWoodsExit(CastEnterDirToExitDir(this.EnterDir), this.EnterLocation));
            }

            AddParentWarps();

            if (parent != null)
            {
                // TODO: Remove this
                ModEntry.Log("Child spawned, time: " + Game1.timeOfDay + " this: " + this.Name + ", level: " + this.level + ", parent: " + this.parent.Name + ", enterDir: " + this.enterDir);
            }
        }

        public DeepWoods(int level)
            : this(null, level, EnterDirection.FROM_TOP)
        {
            this.spawnedFromObelisk.Value = true;
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(enterDir, enterLocation, exits, uniqueMultiplayerID, level, mapWidth, mapHeight, isLichtung, lichtungHasLake, lichtungCenter, spawnedFromObelisk, hasEverBeenVisited, spawnTime, abandonedByParentTime, playerCount, resourceClumps);
        }

        private void DetermineExits()
        {
            if (!Game1.IsMasterGame)
                throw new ApplicationException("Illegal call to DeepWoods.DetermineExits() in client.");

            this.exits.Clear();
            List<ExitDirection> possibleExitDirs = AllExitDirsBut(CastEnterDirToExitDir(this.EnterDir));
            int numExitDirs = Game1.random.Next(1, 4);
            if (numExitDirs < 3)
            {
                possibleExitDirs.RemoveAt(Game1.random.Next(0, possibleExitDirs.Count));
                if (numExitDirs < 2)
                {
                    possibleExitDirs.RemoveAt(Game1.random.Next(0, possibleExitDirs.Count));
                }
            }
            foreach (ExitDirection exitDir in possibleExitDirs)
            {
                this.exits.Add(
                    new DeepWoodsExit(
                        exitDir,
                        new DeepWoodsSpaceManager(this.mapWidth.Value, this.mapHeight.Value).GetRandomExitLocation(exitDir, new DeepWoodsRandom(this, this.seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()))
                    )
                );
            }
        }

        private void CreateSpace()
        {
            if (!Game1.IsMasterGame)
                throw new ApplicationException("Illegal call to DeepWoods.CreateSpace in client.");

            var random = new DeepWoodsRandom(this, this.Seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next());

            this.isLichtung.Value = this.level.Value >= Settings.Level.MinLevelForClearing && this.parent != null && !this.parent.isLichtung && random.CheckChance(Settings.Luck.Clearings.ChanceForClearing);

            if (this.isLichtung)
            {
                this.mapWidth.Value = Game1.random.Next(Settings.Map.MinMapWidth, Settings.Map.MaxMapWidthForClearing);
                this.mapHeight.Value = Game1.random.Next(Settings.Map.MinMapWidth, Settings.Map.MaxMapWidthForClearing);
                this.lichtungHasLake.Value = random.GetRandomValue(Settings.Luck.Clearings.Perks) == LichtungStuff.Lake;
            }
            else
            {
                this.mapWidth.Value = Game1.random.Next(Settings.Map.MinMapWidth, Settings.Map.MaxMapWidth);
                this.mapHeight.Value = Game1.random.Next(Settings.Map.MinMapHeight, Settings.Map.MaxMapHeight);
            }

            this.EnterLocation = this.level == 1 ? Settings.Map.RootLevelEnterLocation : new DeepWoodsSpaceManager(this.mapWidth.Value, this.mapHeight.Value).GetRandomEnterLocation(this.EnterDir, random);
        }

        public void RemovePlayer(Farmer who)
        {
            if (!Game1.IsMasterGame)
                return;
            this.playerCount.Value = this.playerCount.Value - 1;
        }

        public void AddPlayer(Farmer who)
        {
            if (!Game1.IsMasterGame)
                return;
            this.hasEverBeenVisited.Value = true;
            this.playerCount.Value = this.playerCount.Value + 1;
            if (this.playerCount.Value == 1)
            {
                ValidateAndIfNecessaryCreateExitChildren();
            }
        }

        private void ValidateAndIfNecessaryCreateExitChildren()
        {
            if (!Game1.IsMasterGame)
                return;

            if (this.playerCount <= 0)
                return;

            if (this.validateAndIfNecessaryCreateExitChildrenHasRunSinceLastExitRandomization)
                return;

            this.validateAndIfNecessaryCreateExitChildrenHasRunSinceLastExitRandomization = true;

            if (this.level.Value > 1 && this.parent == null && !this.HasExit(CastEnterDirToExitDir(this.EnterDir)))
            {
                // this.abandonedByParentTime = Game1.timeOfDay;
                this.exits.Add(new DeepWoodsExit(CastEnterDirToExitDir(this.EnterDir), this.EnterLocation));
            }

            foreach (var exit in this.exits)
            {
                if (exit.deepWoodsName.Value == null)
                    exit.deepWoodsName.Value = "DeepWoods_" + DeepWoodsRandom.CalculateSeed(level + 1, ExitDirToEnterDir(CastEnterDirToExitDir(this.EnterDir)), Seed);
                if (Game1.getLocationFromName(exit.deepWoodsName.Value) == null)
                {
                    DeepWoods exitDeepWoods = new DeepWoods(this, this.level + 1, ExitDirToEnterDir(exit.ExitDir));
                    exit.deepWoodsName.Value = exitDeepWoods.Name;
                    DeepWoodsManager.AddDeepWoodsToGameLocations(exitDeepWoods);
                    AddExitWarps(exit.ExitDir, exit.Location, exitDeepWoods.Name, exitDeepWoods.EnterLocation);
                }
            }
        }


        private DeepWoodsExit GetExit(ExitDirection exitDir)
        {
            foreach (var exit in this.exits)
                if (exit.ExitDir == exitDir)
                    return exit;
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

            if (!this.hasEverBeenVisited)
                return;

            this.warps.Clear();

            if (this.level > 1 && !this.HasExit(CastEnterDirToExitDir(this.EnterDir)))
            {
                // this.abandonedByParentTime = Game1.timeOfDay;
                this.parent = null;
                this.exits.Add(new DeepWoodsExit(CastEnterDirToExitDir(this.EnterDir), this.EnterLocation));
            }

            foreach (var exit in this.exits)
            {
                // Randomize exit if child level exists and has been visited
                if (exit.deepWoodsName.Value != null
                    && Game1.getLocationFromName(exit.deepWoodsName.Value) is DeepWoods exitDeepWoods
                    && exitDeepWoods.hasEverBeenVisited)
                {
                    exit.deepWoodsName.Value = null;
                }
            }

            this.validateAndIfNecessaryCreateExitChildrenHasRunSinceLastExitRandomization = false;

            if (this.playerCount > 0)
            {
                ValidateAndIfNecessaryCreateExitChildren();
            }
        }

        public bool TryRemove()
        {
            if (!Game1.IsMasterGame)
                throw new ApplicationException("Illegal call to DeepWoods.TryRemove() in client.");

            if (this.level == 1)
                return false;

            if (this.playerCount > 0)
                return false;

            if ((this.parent?.playerCount ?? 0) > 0 && Game1.timeOfDay <= (this.abandonedByParentTime + TIME_BEFORE_DELETION_ALLOWED))
                return false;

            if (Game1.timeOfDay <= (this.spawnTime + TIME_BEFORE_DELETION_ALLOWED))
                return false;

            if (this.parent != null)
                this.parent.validateAndIfNecessaryCreateExitChildrenHasRunSinceLastExitRandomization = false;

            foreach (var exit in this.exits)
            {
                if (exit.deepWoodsName.Value != null
                    && Game1.getLocationFromName(exit.deepWoodsName.Value) is DeepWoods exitDeepWoods
                    && exitDeepWoods.parent != null)
                {
                    exitDeepWoods.parent = null;
                    exitDeepWoods.validateAndIfNecessaryCreateExitChildrenHasRunSinceLastExitRandomization = false;
                }
            }

            this.parent = null;

            this.exits.Clear();
            this.warps.Clear();
            this.characters.Clear();
            this.terrainFeatures.Clear();
            this.largeTerrainFeatures.Clear();
            this.resourceClumps.Clear();

            DeepWoodsManager.RemoveDeepWoodsFromGameLocations(this);
            return true;
        }


        public override void updateMap()
        {
            // Check if level is properly initialized
            if (this.Seed == 0)
                return;

            // Check if map is already created
            if (this.map != null)
                return;

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

            DeepWoodsBuilder.Build(this, this.map, DeepWoodsEnterExit.CreateExitDictionary(this.EnterDir, this.EnterLocation, this.exits));
        }

        // This is the default day update method of GameLocation, called only on the server
        public override void DayUpdate(int dayOfMonth)
        {
            base.DayUpdate(dayOfMonth);

            if (this.level < Settings.Level.MinLevelForFruits)
            {
                foreach (TerrainFeature terrainFeature in this.terrainFeatures.Values)
                {
                    if (terrainFeature is FruitTree fruitTree)
                        fruitTree.fruitsOnTree.Value = 0;
                }
            }
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
                return this.parent?.GetExit(EnterDirToExitDir(this.EnterDir))?.Location ?? new Location();
            }
        }

        private void AddWarp(int x, int y, string locationName, Location warpLocation)
        {
            if (!Game1.IsMasterGame)
                return;

            foreach (Warp warp in new List<Warp>(this.warps))
            {
                if (warp.X == x && warp.Y == y)
                {
                    this.warps.Remove(warp);
                }
            }

            this.warps.Add(new Warp(x, y, locationName, warpLocation.X, warpLocation.Y, false));
        }

        private void AddExitWarps(ExitDirection exitDir, Location location, string targetLocationName, Location targetLocation)
        {
            if (!Game1.IsMasterGame)
                return;

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
                        AddWarp(location.X, this.mapHeight.Value, targetLocationName, displacedTargetLocation);
                        for (int i = 1; i <= Settings.Map.ExitRadius; i++)
                        {
                            AddWarp(location.X - i, this.mapHeight.Value, targetLocationName, displacedTargetLocation - new Location(i, 0));
                            AddWarp(location.X + i, this.mapHeight.Value, targetLocationName, displacedTargetLocation + new Location(i, 0));
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
                    AddWarp(this.mapWidth.Value, location.Y, targetLocationName, targetLocation);
                    for (int i = 1; i <= Settings.Map.ExitRadius; i++)
                    {
                        AddWarp(this.mapWidth.Value, location.Y - i, targetLocationName, targetLocation - new Location(0, i));
                        AddWarp(this.mapWidth.Value, location.Y + i, targetLocationName, targetLocation + new Location(0, i));
                    }
                    break;
            }
        }

        private void AddParentWarps()
        {
            if (!Game1.IsMasterGame)
                return;

            if (this.level == 1 || this.parent != null)
            {
                AddExitWarps(CastEnterDirToExitDir(this.EnterDir), this.EnterLocation, GetParentLocationName(), GetParentWarpLocation());
            }
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
            Microsoft.Xna.Framework.Rectangle rectangle = monster.GetBoundingBox();
            rectangle.X = x;
            rectangle.Y = y;

            foreach (NPC npc in this.characters)
            {
                if (npc.GetBoundingBox().Intersects(rectangle))
                    return false;
            }

            for (int i = 0; i < rectangle.Width; i++)
            {
                for (int j = 0; j < rectangle.Height; j++)
                {
                    Vector2 v = new Vector2(x + i, y + j);

                    if (IsLocationOnBorderOrExit(v))
                        return false;

                    if (!monster.isGlider
                        && !isTileLocationTotallyClearAndPlaceable(v)
                        && !(this.terrainFeatures.ContainsKey(v) && this.terrainFeatures[v] is Grass))
                        return false;
                }
            }

            return true;
        }

        protected override void resetSharedState()
        {
            base.resetSharedState();
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();

            // TODO: Better critter spawning in forest
            this.tryToAddCritters(false);

            DeepWoodsDebris.Initialize(this);

            foreach (Vector2 lightSource in this.lightSources)
            {
                Game1.currentLightSources.Add(new LightSource(LightSource.indoorWindowLight, lightSource * 64f, 1.0f));
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
            Location titleSafeTopLeftCorner = new DeepWoodsSpaceManager(this.mapWidth.Value, this.mapHeight.Value).GetActualTitleSafeTopleftCorner();
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
            return new DeepWoodsRandom(this, this.Seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()).CheckChance(Settings.Luck.Fishies.ChanceForAwesomeFish);
        }

        private int GetRandomAwesomeFish()
        {
            return new DeepWoodsRandom(this, this.Seed ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ Game1.random.Next()).GetRandomValue(Settings.Luck.Fishies.AwesomeFishies);
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
