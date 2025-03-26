using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Damage;

[RegisterComponent]

public sealed partial class SpawnSolutionOnDamageComponent : Component
{
    [DataField]
    public EntProtoId Solution = "unknown";
    [DataField]
    public float MinimumAmount = 0;
    [DataField]
    public float MaximumAmount = 30;
    [DataField]
    public float Threshold = 5;
    [DataField]
    public float Probability = 0.5f;
}
