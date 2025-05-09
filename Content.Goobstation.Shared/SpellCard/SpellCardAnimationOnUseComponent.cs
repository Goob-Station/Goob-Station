using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.SpellCard;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpellCardAnimationOnUseComponent : Component
{
    /// <summary>
    /// Animation to play when this entity is triggered.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SpellCardAnimationData AnimationData;

    [DataField, AutoNetworkedField]
    public SpellCardBroadcastType BroadcastType = SpellCardBroadcastType.Pvs;
}

public enum SpellCardBroadcastType
{
    Local,
    Pvs,
    Grid,
    Map,
    Global,
}
