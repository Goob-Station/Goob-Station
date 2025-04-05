using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.MeleeDash;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DashingComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> HitEntities = new();

    [DataField, AutoNetworkedField]
    public List<string> ChangedFixtures = new();

    [DataField, AutoNetworkedField]
    public EntityUid? Weapon;
}
