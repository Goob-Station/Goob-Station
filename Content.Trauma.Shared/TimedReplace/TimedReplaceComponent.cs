using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.TimedReplace;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class TimedReplaceComponent : Component
{
    [DataField]
    public EntProtoId Entity;

    [DataField]
    public float MinTime = 40f;

    [DataField]
    public float MaxTime = 60f;

    [DataField, AutoPausedField]
    public TimeSpan SpawnTime = TimeSpan.Zero;
}
