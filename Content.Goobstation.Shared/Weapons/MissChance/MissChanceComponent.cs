using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.MissChance;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MissChanceComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Chance = 0.35f; // 65% to hit the target
}
