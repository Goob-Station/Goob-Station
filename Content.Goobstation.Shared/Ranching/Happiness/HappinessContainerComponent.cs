using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Ranching.Happiness;

/// <summary>
/// Used for entities to determine their happiness
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(HappinessContainerSystem))]
[AutoGenerateComponentState]
public sealed partial class HappinessContainerComponent : Component
{
    /// <summary>
    /// The current happiness we have
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Happiness;

    /// <summary>
    /// The maximum happiness this entity can have. Set to -1 to disable
    /// </summary>
    [DataField]
    public int MaxHappiness;
}
