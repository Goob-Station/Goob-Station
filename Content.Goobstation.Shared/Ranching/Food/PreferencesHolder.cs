using Content.Goobstation.Common.Ranching;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Ranching.Food;

/// <summary>
/// Use this on entities that hold preferences
/// Currently used on feed sack and chicken food.
/// Feed sack passes down its Preferences to chicken food Preferences
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PreferencesHolderComponent : Component
{
    /// <summary>
    /// Holds the preferences this entity has
    /// </summary>
    [DataField]
    public List<ProtoId<HappinessPreferencePrototype>> Preferences = new();
}
