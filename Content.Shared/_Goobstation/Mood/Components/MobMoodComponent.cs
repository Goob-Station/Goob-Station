using Content.Shared._Goobstation.Mood.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Mood.Components;

[RegisterComponent, NetworkedComponent(), AutoGenerateComponentState]
public sealed partial class MobMoodComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<MobMoodletPrototype> MobMoods { get; set; } = new();
}
