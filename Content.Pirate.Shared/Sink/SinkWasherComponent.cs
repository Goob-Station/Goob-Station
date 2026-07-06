using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.Sink;

/// <summary>
/// Lets sinks wash stained held items or gloves/bare hands.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SinkWasherComponent : Component
{
    [DataField]
    public float WashDuration = 6f;

    [DataField]
    public SoundSpecifier WashSound = new SoundPathSpecifier("/Audio/_Pirate/Machines/sink_faucet.ogg");

    /// <summary>Wetness added by a sink wash.</summary>
    [DataField]
    public Content.Goobstation.Maths.FixedPoint.FixedPoint2 WetnessAdded =
        Content.Goobstation.Maths.FixedPoint.FixedPoint2.New(8);
}

[Serializable, NetSerializable]
public sealed partial class SinkWashDoAfterEvent : SimpleDoAfterEvent;
