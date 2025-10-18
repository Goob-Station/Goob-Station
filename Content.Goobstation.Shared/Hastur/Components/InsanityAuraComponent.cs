using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hastur.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class InsanityAuraComponent : Component
{
    /// <summary>
    /// Dictionary of reagents and their quantities to be injected.
    /// Key: Reagent ID, Value: Quantity to inject.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> Reagents = new();


    [DataField]
    public SoundSpecifier? VoidSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/Hastur/Void_Song.ogg");

    /// <summary>
    /// The maximum distance for visibility/LOS check.
    /// </summary>
    [DataField]
    public float Range;
}
