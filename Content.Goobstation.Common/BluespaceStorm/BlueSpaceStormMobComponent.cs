
namespace Content.Goobstation.Common.BlueSpaceStorm;

[RegisterComponent]
public sealed partial class BlueSpaceStormPortalMobComponent : Component
{
    /// <summary>
    /// Portal the mob spawned from
    /// </summary>
    [DataField]
    public EntityUid LinkedPortal;

}
