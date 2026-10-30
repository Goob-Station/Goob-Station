using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(GameDirector.GameDirectorSystem))]
public sealed partial class GameDirectorReinforcementComponent : Component
{
    [DataField(required: true)]
    public HashSet<ProtoId<DepartmentPrototype>> Departments = new();

    [DataField]
    public int UnderstaffedBelow = 2;

    [DataField]
    public int RequiredGhosts = 1;
}
