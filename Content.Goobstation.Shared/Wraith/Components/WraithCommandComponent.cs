using Content.Shared.StatusEffect;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class WraithCommandComponent : Component
{
    /// <summary>
    /// The search range of nearby objects
    /// </summary>
    [DataField(required: true)]
    public float SearchRange = 5f;

    /// <summary>
    ///  What objects are allowed
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Blacklist = new();

    /// <summary>
    /// The throw speed in which to throw the objects
    /// </summary>
    [DataField]
    public float ThrowSpeed = 30f;

    [DataField(required: true)]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(5);
}
