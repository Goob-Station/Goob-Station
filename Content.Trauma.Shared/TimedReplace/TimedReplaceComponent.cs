using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.TimedReplace;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class TimedReplaceComponent : Component
{
    [DataField]
    public EntProtoId Entity;

    [DataField]
    public int MinTime = 40;

    [DataField]
    public int MaxTime = 60;

    [DataField, AutoPausedField]
    public TimeSpan SpawnTime = TimeSpan.Zero;
}
