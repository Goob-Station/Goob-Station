using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.Physics.Cramming;

[RegisterComponent]
public sealed partial class CrammingPressureComponent : Component
{
    [DataField] public float BuildupAccumulator;
    [DataField] public bool BuildupComplete;
}
