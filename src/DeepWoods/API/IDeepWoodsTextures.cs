﻿using Microsoft.Xna.Framework.Graphics;

namespace DeepWoodsMod.API
{
    public interface IDeepWoodsTextures
    {
        Texture2D WoodsObelisk { get; set; }
        Texture2D HealingFountain { get; set; }
        Texture2D IridiumTree { get; set; }
        Texture2D GingerbreadHouse { get; set; }
        Texture2D BushThorns { get; set; }
        Texture2D Unicorn { get; set; }
        Texture2D ExcaliburStone { get; set; }
        Texture2D LakeTilesheet { get; set; }
        Texture2D Festivals { get; set; }
        Texture2D CuteSign { get; set; }
        Texture2D BigWoodenSign { get; set; }
        Texture2D InfestedOutdoorsTilesheet { get; set; }
        Texture2D InfestedBushes { get; set; }
        Texture2D MaxHut { get; set; }
        Texture2D DeepWoodsMaxHousePuzzleColumn { get; set; }
        Texture2D OrbStone { get; set; }
        Texture2D OrbStoneOrb { get; set; }

        //Texture2D MaxCharacter { get; set; }
        //Texture2D MaxPortrait { get; set; }
    }
}
