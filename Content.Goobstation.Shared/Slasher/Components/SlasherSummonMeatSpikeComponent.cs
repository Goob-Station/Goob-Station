using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Grants the Slasher the ability to instantly spawn a meat spike.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherSummonMeatSpikeComponent : Component
{
    [ViewVariables]
    public EntityUid? ActionEnt;

    [DataField]
    public EntProtoId ActionId = "ActionSlasherSummonMeatSpike";

    /// <summary>
    /// Prototype id of the spike to spawn.
    /// </summary>
    [DataField]
    public EntProtoId MeatSpikePrototype = "SlasherMeatSpike";

    /// <summary>
    /// Sound to play when the spike is summoned.
    /// </summary>
    [DataField]
    public SoundSpecifier SummonSound
        = new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/SlasherSummonMeatspike.ogg")
        {
            Params = AudioParams.Default
                       .WithMaxDistance(4f)
        };
}
