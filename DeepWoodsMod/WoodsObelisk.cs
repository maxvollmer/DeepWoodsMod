using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Reflection;
using static DeepWoodsMod.DeepWoodsSettings;

namespace DeepWoodsMod
{
    class WoodsObelisk
    {
        private static void ObeliskWarpForRealOverride()
        {
            Game1.activeClickableMenu = new WoodsObeliskMenu();
        }

        public static void InjectWoodsObeliskIntoGame()
        {
            foreach (var a in Game1.delayedActions)
            {
                if (a.behavior == a.doGlobalFade && a.afterFadeBehavior != null
                    && a.afterFadeBehavior.GetMethodInfo() == typeof(Building).GetMethod("obeliskWarpForReal", BindingFlags.Instance | BindingFlags.NonPublic)
                    && a.afterFadeBehavior.Target is Building building
                    && building.buildingType == WOODS_OBELISK_BUILDING_NAME)
                {
                    a.afterFadeBehavior = new Game1.afterFadeFunction(ObeliskWarpForRealOverride);
                }
            }

            if (Game1.activeClickableMenu is CarpenterMenu carpenterMenu)
            {
                if (IsMagical(carpenterMenu) && !HasBluePrint(carpenterMenu))
                {
                    // Create a new BluePrint based on "Earth Obelisk", override name, texture and resources.
                    BluePrint woodsObeliskBluePrint = new BluePrint("Earth Obelisk")
                    {
                        name = WOODS_OBELISK_BUILDING_NAME,
                        displayName = WOODS_OBELISK_DISPLAY_NAME,
                        description = WOODS_OBELISK_DESCRIPTION,
                        moneyRequired = WOODS_OBELISK_MONEY_REQUIRED
                    };
                    woodsObeliskBluePrint.itemsRequired.Clear();
                    foreach (var item in WOODS_OBELISK_ITEMS_REQUIRED)
                    {
                        woodsObeliskBluePrint.itemsRequired.Add(item.Key, item.Value);
                    }
                    SetBluePrintField(woodsObeliskBluePrint, "textureName", "Buildings\\" + WOODS_OBELISK_BUILDING_NAME);
                    SetBluePrintField(woodsObeliskBluePrint, "texture", Game1.content.Load<Texture2D>(woodsObeliskBluePrint.textureName));

                    // Add Woods Obelisk directly after the other obelisks
                    int lastObeliskIndex = GetBluePrints(carpenterMenu).FindLastIndex(bluePrint => bluePrint.name.Contains("Obelisk"));
                    GetBluePrints(carpenterMenu).Insert(lastObeliskIndex + 1, woodsObeliskBluePrint);
                }
            }
        }

        private static bool IsMagical(CarpenterMenu carpenterMenu)
        {
            return (bool)typeof(CarpenterMenu).GetField("magicalConstruction", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(carpenterMenu);
        }

        private static List<BluePrint> GetBluePrints(CarpenterMenu carpenterMenu)
        {
            return (List<BluePrint>)typeof(CarpenterMenu).GetField("blueprints", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(carpenterMenu);
        }

        private static void SetBluePrintField(BluePrint bluePrint, string fieldName, object value)
        {
            typeof(BluePrint).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(bluePrint, value);
        }

        private static bool HasBluePrint(CarpenterMenu carpenterMenu)
        {
            return GetBluePrints(carpenterMenu).Exists(bluePrint => bluePrint.name == WOODS_OBELISK_BUILDING_NAME);
        }
    }
}
