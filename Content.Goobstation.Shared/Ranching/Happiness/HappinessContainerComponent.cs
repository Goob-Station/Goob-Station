using Content.Goobstation.Common.Ranching;
using Content.Goobstation.Maths.FixedPoint;
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
    public FixedPoint2 Happiness;

    /// <summary>
    /// The maximum happiness for this life. Set to -1 to disable
    /// </summary>
    [DataField]
    public FixedPoint2 MaxHappiness;

    /// <summary>
    /// Maps the preference to the corresponding value that it gives once consumed
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<HappinessPreferencePrototype>, FixedPoint2> Preferences = new();
}
