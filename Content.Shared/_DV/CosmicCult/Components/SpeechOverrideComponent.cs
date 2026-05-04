using Content.Shared.Chat.Prototypes;
using Content.Shared.Humanoid;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for overriding an entity's vocal sounds through equipment.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpeechOverrideComponent : Component
{
    /// <summary>
    /// Sounds to assign to the entity equipping this item.
    /// </summary>
    [AutoNetworkedField]
    public Dictionary<Sex, ProtoId<EmoteSoundsPrototype>>? OverrideIDs = null;

    /// <summary>
    /// Entity's original sounds to use when the item is unequipped.
    /// </summary>
    [AutoNetworkedField]
    public Dictionary<Sex, ProtoId<EmoteSoundsPrototype>>? StoredIDs = null;
}
