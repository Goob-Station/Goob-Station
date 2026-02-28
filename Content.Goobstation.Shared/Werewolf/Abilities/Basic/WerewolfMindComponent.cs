using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Werewolf.Abilities.Basic;

// fucking KILL YOURSELF!!!!
[RegisterComponent]
public sealed partial class WerewolfMindComponent : Component
{
    [DataField]
    public List<EntityUid> BittenPeople = new();
}
