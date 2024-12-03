namespace Content.Shared._Lavaland.Mobs;

[RegisterComponent]
public sealed partial class AggressorComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)] public List<EntityUid> Aggrods = new();
}
