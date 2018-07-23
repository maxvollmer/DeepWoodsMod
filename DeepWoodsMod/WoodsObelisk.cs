using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Reflection;

namespace DeepWoodsMod
{
    class WoodsObelisk
    {
        // TODO: Add description and displayname strings, add materials and fix money value.

        private const int MONEY_REQUIRED_FOR_WOODS_OBELISK = 10;

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
                    && building.buildingType == "Woods Obelisk")
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
                        name = "Woods Obelisk",
                        displayName = "Woods Obelisk Displayname",
                        description = "Woods Obelisk Description",
                        moneyRequired = MONEY_REQUIRED_FOR_WOODS_OBELISK
                    };
                    woodsObeliskBluePrint.itemsRequired.Clear();
                    SetBluePrintField(woodsObeliskBluePrint, "textureName", "Buildings\\Woods Obelisk");
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
            return GetBluePrints(carpenterMenu).Exists(bluePrint => bluePrint.name == "Woods Obelisk");
        }
    }
}
