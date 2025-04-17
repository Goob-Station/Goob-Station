using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.MissChance;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MissChanceComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Chance = 0.65f; // 65%

    [DataField, AutoNetworkedField]
    public bool MissNext;
}
