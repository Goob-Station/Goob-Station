namespace Content.Shared._Lavaland.Aggression;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class AggressorComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)] public HashSet<EntityUid> Aggressives = new();
}
