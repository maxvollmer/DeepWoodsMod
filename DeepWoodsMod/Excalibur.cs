using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWoodsMod
{
    class Excalibur
    {
        private static int TILE_INDEX = 3;
        private static string BASE_NAME = "Excalibur";
        private static string DESCRIPTION = "It feels hopeful to wield.";
        private static string DISPLAY_NAME = "Excalibur";
        private static int MIN_DAMAGE = 120;
        private static int MAX_DAMAGE = 180;
        private static float KNOCKBACK = 1.5f;
        private static int SPEED = 10;
        private static int ADDED_PRECISION = 5;
        private static int ADDED_DEFENSE = 5;
        private static int ADDED_AREA_OF_EFFECT = 5;
        private static float CRITICAL_CHANCE = .05f;
        private static float CRITICAL_MULTIPLIER = 5;

        public static MeleeWeapon GetOne()
        {
            // 4: "Galaxy Sword/It's unlike anything you've ever seen./60/80/1/8/0/0/0/-1/-1/0/.02/3" #!String
            MeleeWeapon excalibur = new MeleeWeapon(TILE_INDEX)
            {
                BaseName = BASE_NAME,                           // "Galaxy Sword"
                description = DESCRIPTION,                      // "It's unlike anything you've ever seen."
                DisplayName = DISPLAY_NAME                     // "Galaxy Sword"
            };
            excalibur.minDamage.Value = MIN_DAMAGE;                   // 60
            excalibur.maxDamage.Value = MAX_DAMAGE;                   // 80
            excalibur.knockback.Value = KNOCKBACK;                    // 1
            excalibur.speed.Value = SPEED;                            // 8
            excalibur.addedPrecision.Value = ADDED_PRECISION;         // 0
            excalibur.addedDefense.Value = ADDED_DEFENSE;             // 0
            // type                                         // 0
            excalibur.addedAreaOfEffect.Value = ADDED_AREA_OF_EFFECT; // 0
            excalibur.critChance.Value = CRITICAL_CHANCE;             // .02
            excalibur.critMultiplier.Value = CRITICAL_MULTIPLIER;     // 3
            // excalibur.InitialParentTileIndex = TILE_INDEX;
            // excalibur.CurrentParentTileIndex = TILE_INDEX;
            // excalibur.IndexOfMenuItemView = TILE_INDEX;
            return excalibur;
        }
    }
}
