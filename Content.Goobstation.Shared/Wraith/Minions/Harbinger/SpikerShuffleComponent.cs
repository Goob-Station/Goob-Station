using Content.Shared.StatusEffect;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpikerShuffleComponent : Component
{
    [DataField]
    public List<ProtoId<StatusEffectPrototype>> StatusEffectsToRemove = new();

    [ViewVariables]
    public EntProtoId StatusEffect = "StatusEffectShuffle";

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);
}
