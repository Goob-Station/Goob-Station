using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Werewolf.Abilities.Basic;

/// <summary>
/// fucking KILL YOURSELF!!!!
/// </summary>
[RegisterComponent]
public sealed partial class WerewolfMindComponent : Component
{
    [DataField]
    public List<EntityUid> BittenPeople = new();
}
