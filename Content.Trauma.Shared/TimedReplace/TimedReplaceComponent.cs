namespace Content.Trauma.Shared.TimedReplace;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class TimedReplaceComponent : Component
{
    [DataField]
    public EntProtoId Entity;

    [DataField]
    public float MinTime = 240f;

    [DataField]
    public float MaxTime = 300f;

    [DataField]
    public bool Active;

    [DataField, AutoPausedField]
    public TimeSpan SpawnTime = TimeSpan.Zero;
}
