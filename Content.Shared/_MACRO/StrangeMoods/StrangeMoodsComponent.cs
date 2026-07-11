using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MACRO.StrangeMoods;

/// <summary>
///     Allows an entity to use the 'moods' system, which contains a number of
///     roleplay prompts that a player can engage with. Basically a more freeform
///     version of silicon laws.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedStrangeMoodsSystem))]
public sealed partial class StrangeMoodsComponent : Component
{
    /// <summary>
    /// The strange mood definition that this entity follows.
    /// </summary>
    [DataField("mood", readOnly: true)]
    public ProtoId<StrangeMoodDefinitionPrototype>? StrangeMoodPrototype;

    /// <summary>
    ///     The strange mood definition that this entity follows.
    ///     Is automatically assigned on component initialization.
    /// </summary>
    [DataField, AutoNetworkedField]
    public StrangeMoodDefinition StrangeMood = new();

    /// <summary>
    ///     A mood shared between all entities with the component.
    ///     Null if this entity does not have the round's shared mood.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SharedMood? SharedMood;

    /// <summary>
    ///     Action granted to the entity to view moods
    /// </summary>
    [DataField(serverOnly: true)]
    public EntityUid? Action;
}

/// <summary>
///     Signals when the moods screen is toggled open or closed.
/// </summary>
public sealed partial class ToggleMoodsScreenEvent : InstantActionEvent;

[NetSerializable, Serializable]
public enum StrangeMoodsUiKey : byte
{
    Key
}

/// <summary>
/// BUI state to tell the client what the shared moods are.
/// </summary>
[Serializable, NetSerializable]
public sealed class StrangeMoodsBuiState(List<StrangeMood> sharedMoods, List<StrangeMood> moods) : BoundUserInterfaceState
{
    public readonly List<StrangeMood> SharedMoods = sharedMoods;
    public readonly List<StrangeMood> Moods = moods;
}