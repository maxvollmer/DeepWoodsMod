using xTile.Dimensions;

namespace DeepWoodsMod.API
{
    public interface IDeepWoodsExit
    {
        int ExitDirection { get; }
        Location Location { get; set; }
        string TargetLocationName { get; set; }
        Location TargetLocation { get; set; }
    }
}
