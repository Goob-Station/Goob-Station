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
    public float MinimumAmoun = -1;
    [DataField]
    public float MaximumAmount = -1;
    [DataField]
    public float Threshold = -1;
    [DataField]
    public float Probability = 1f;
}
