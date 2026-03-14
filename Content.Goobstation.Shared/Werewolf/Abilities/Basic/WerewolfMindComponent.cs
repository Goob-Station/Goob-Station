using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Werewolf.Abilities.Basic;

// fucking KILL YOURSELF!!!!
[RegisterComponent]
public sealed partial class WerewolfMindComponent : Component
{
    [DataField]
    public List<EntityUid> BittenPeople = new(); // would be used in the manifest

    [DataField]
    public List<string> UnlockedActions = new();

    [DataField]
    public int Currency; // needed becasue polymorph & store shitcode
}
