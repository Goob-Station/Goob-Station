using Content.Goobstation.Common.Ranching;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Ranching.Happiness;

/// <summary>
/// Used on foods to mark them as having a preference for the chickens
/// I'm gonna put it in ediblecomponent once we get upstream merge
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HappinessPreferenceComponent : Component
{
    [DataField(required: true)]
    public ProtoId<HappinessPreferencePrototype> Preference;
}
