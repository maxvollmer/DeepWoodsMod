using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.IO;

namespace DeepWoodsMod.API.Impl
{
    /// <summary>
    /// Credit to https://www.reddit.com/user/sinceriouslyyy for their pixel art fountain: https://www.reddit.com/r/PixelArt/comments/6h8wld/oc_pixel_art_fountain/
    /// Credit to https://forums.nexusmods.com/index.php?/user/21105194-mostlyreal/ for their ginger bread house.
    /// Credit to https://forums.nexusmods.com/index.php?/user/5497250-zhuria/ for their unicorn: https://www.nexusmods.com/stardewvalley/mods/8
    /// </summary>
    public class DeepWoodsTextures : IDeepWoodsTextures
    {
        public Texture2D WoodsObelisk { get; set; }
        public Texture2D HealingFountain { get; set; }
        public Texture2D IridiumTree { get; set; }
        public Texture2D GingerbreadHouseWinter { get; set; }
        public Texture2D GingerbreadHouse { get; set; }
        public Texture2D BushThorns { get; set; }
        public Texture2D Unicorn { get; set; }
        public Texture2D ExcaliburStone { get; set; }
        public Texture2D LakeTilesheet { get; set; }
        public Texture2D Festivals { get; set; }

        public static DeepWoodsTextures Textures { get; } = new DeepWoodsTextures();

        public void LoadAll()
        {
            WoodsObelisk ??= ModEntry.GetHelper().ModContent.Load<Texture2D>(Path.Combine("assets", "woods_obelisk_mostlyreal.png"));
            HealingFountain ??= ModEntry.GetHelper().ModContent.Load<Texture2D>(Path.Combine("assets", "sinceriouslyyy_fountain.png"));
            IridiumTree ??= ModEntry.GetHelper().ModContent.Load<Texture2D>(Path.Combine("assets", "iridium_tree.png"));
            GingerbreadHouse ??= ModEntry.GetHelper().ModContent.Load<Texture2D>(Path.Combine("assets", "gingerbread_house_technopoptart98.png"));
            GingerbreadHouseWinter ??= ModEntry.GetHelper().ModContent.Load<Texture2D>(Path.Combine("assets", "gingerbread_house_mostlyreal.png"));
            BushThorns ??= ModEntry.GetHelper().ModContent.Load<Texture2D>(Path.Combine("assets", "bush_thorns.png"));
            Unicorn ??= ModEntry.GetHelper().ModContent.Load<Texture2D>(Path.Combine("assets", "unicorn_zhuria.png"));
            ExcaliburStone ??= ModEntry.GetHelper().ModContent.Load<Texture2D>(Path.Combine("assets", "excalibur_stone.png"));
            LakeTilesheet ??= ModEntry.GetHelper().ModContent.Load<Texture2D>(Path.Combine("assets", "lake_tilesheet.png"));
            Festivals ??= Game1.content.Load<Texture2D>("Maps\\Festivals");
        }
    }
}
