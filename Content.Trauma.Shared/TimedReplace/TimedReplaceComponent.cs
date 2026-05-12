namespace Content.Trauma.Shared.TimedReplace;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class TimedReplaceComponent : Component
{
    [DataField]
    public EntProtoId Entity;

    [DataField]
    public float MinTime = 50f;

    [DataField]
    public float MaxTime = 60f;

    [DataField]
    public bool Active;

    [DataField, AutoPausedField]
    public TimeSpan SpawnTime = TimeSpan.Zero;
}
