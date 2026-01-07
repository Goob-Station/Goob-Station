using Content.Shared.Whitelist;

namespace Content.Server.Objectives.Components;

[RegisterComponent]
public sealed partial class CosmicEffigyConditionComponent : Component
{
    [DataField]
    public EntityUid? EffigyTarget;

    /// <summary>
    /// Tags that should be used to exclude Warp Points
    /// from the list of valid effigy targets
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;
}
