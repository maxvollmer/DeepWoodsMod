using StardewValley.Tools;
using static DeepWoodsMod.DeepWoodsSettings;

namespace DeepWoodsMod
{
    class Excalibur
    {
        public static MeleeWeapon GetOne()
        {
            // 4: "Galaxy Sword/It's unlike anything you've ever seen./60/80/1/8/0/0/0/-1/-1/0/.02/3" #!String
            MeleeWeapon excalibur = new MeleeWeapon(EXCALIBUR_TILE_INDEX)
            {
                BaseName = EXCALIBUR_BASE_NAME,                           // "Galaxy Sword"
                description = EXCALIBUR_DESCRIPTION,                      // "It's unlike anything you've ever seen."
                DisplayName = EXCALIBUR_DISPLAY_NAME                     // "Galaxy Sword"
            };
            excalibur.minDamage.Value = EXCALIBUR_MIN_DAMAGE;                   // 60
            excalibur.maxDamage.Value = EXCALIBUR_MAX_DAMAGE;                   // 80
            excalibur.knockback.Value = EXCALIBUR_KNOCKBACK;                    // 1
            excalibur.speed.Value = EXCALIBUR_SPEED;                            // 8
            excalibur.addedPrecision.Value = EXCALIBUR_ADDED_PRECISION;         // 0
            excalibur.addedDefense.Value = EXCALIBUR_ADDED_DEFENSE;             // 0
            // type                                         // 0
            excalibur.addedAreaOfEffect.Value = EXCALIBUR_ADDED_AREA_OF_EFFECT; // 0
            excalibur.critChance.Value = EXCALIBUR_CRITICAL_CHANCE;             // .02
            excalibur.critMultiplier.Value = EXCALIBUR_CRITICAL_MULTIPLIER;     // 3
            // excalibur.InitialParentTileIndex = TILE_INDEX;
            // excalibur.CurrentParentTileIndex = TILE_INDEX;
            // excalibur.IndexOfMenuItemView = TILE_INDEX;
            return excalibur;
        }
    }
}
