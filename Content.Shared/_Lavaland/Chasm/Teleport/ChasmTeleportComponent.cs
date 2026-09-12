using Robust.Shared.Utility;

namespace Content.Shared._Lavaland.Chasm.Teleport;

/// <summary>
/// If a player steps onto an entity holding this component and also ChasmComponent, they will fall into the chasm,
/// but will teleport to a new map once the animation of falling finishes, instead of being deleted.
/// TL DR if you want to use this:
/// 1 - Make a chasm tile entity with ChasmComponent, this component and StepTrigger, plus the usual stuff that goes on chasms. (Just copy my rift portal)
/// 2 - Create a map file so that ChasmTeleport can load it in.
/// 3 - Place ChasmTeleportBeaconComponent entity inside this map on where you want the player to appear in.
/// 4 - Place (Map name)ExitBeaconComponent entity outside the chasm, or wherever you want them to exit at, doesn't matter.
/// 5 - Create a exit portal YAML entity with (Map name)ExitComponent.
/// </summary>

[RegisterComponent]
public sealed partial class ChasmTeleportComponent : Component
{
    /// <summary>
    /// The map to load.
    /// </summary>
    [DataField]
    public ResPath MapPath;

    /// <summary>
    /// Gets set on runtime. If null means not loaded.
    /// </summary>
    public EntityUid? LoadedMap;
}
