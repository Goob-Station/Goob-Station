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
    [DataField]
    public float SearchRange = 5f;

    /// <summary>
    ///  What objects are allowed
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Blacklist = new();

    /// <summary>
    /// Max objects to pick
    /// </summary>
    [DataField(required: true)]
    public int MaxObjects;

    /// <summary>
    /// The throw speed in which to throw the objects
    /// </summary>
    [DataField]
    public float ThrowSpeed = 30f;

    /// <summary>
    ///  Status effect to apply to the user
    /// </summary>
    [DataField(required: true)]
    public ProtoId<StatusEffectPrototype> StatusEffect = "Stun";

    [DataField(required: true)]
    public string StatusEffectComponent = string.Empty;

    [DataField(required: true)]
    public TimeSpan StatusEffectDuration = TimeSpan.Zero;
}
