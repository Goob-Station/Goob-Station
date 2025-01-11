namespace Content.Server._Lavaland.Aggression;

/// <summary>
///     Keeps track of whoever attacked our mob, so that it could prioritize or randomize targets.
/// </summary>
[RegisterComponent]
public sealed partial class AggressiveComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)] public List<EntityUid> Aggressors = new();

    [DataField] public float ForgiveTime = 10f;

    [DataField] public float ForgiveRange = 10f;
}
