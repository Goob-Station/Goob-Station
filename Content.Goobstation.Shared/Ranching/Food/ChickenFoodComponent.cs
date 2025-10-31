using Content.Goobstation.Common.Ranching;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Ranching.Food;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ChickenFoodComponent : Component
{
    /// <summary>
    /// How much food can you place before it gets deleted
    /// </summary>
    [DataField, AutoNetworkedField]
    public int PlacementsLeft = 5;

    /// <summary>
    /// Holds the preferences this chicken food has
    /// </summary>
    [DataField]
    public List<ProtoId<HappinessPreferencePrototype>> Preferences = new();
}
