using Content.Goobstation.Common.Ranching;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Ranching.Happiness;

/// <summary>
/// Requires HappinessContainer component.
/// Holds things that can increase happiness for our entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class HappinessPreferencesComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<HappinessPreferencePrototype>> Likes;

    [DataField, AutoNetworkedField]
    public List<ProtoId<HappinessPreferencePrototype>> Dislikes;
}
