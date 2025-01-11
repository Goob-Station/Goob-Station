namespace Content.Server._Lavaland.Aggression;

[RegisterComponent]
public sealed partial class AggressorComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)] public List<EntityUid> Aggressives = new();
}
