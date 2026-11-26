
namespace Content.Goobstation.Common.BluespaceStorm;

[RegisterComponent]
public sealed partial class BluespaceStormPortalMobComponent : Component
{
    /// <summary>
    /// Portal the mob spawned from
    /// </summary>
    [DataField]
    public EntityUid LinkedPortal;
}
