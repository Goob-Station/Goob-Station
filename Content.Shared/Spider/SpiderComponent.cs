// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Spider;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedSpiderSystem))]
public sealed partial class SpiderComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("webPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string WebPrototype = "SpiderWeb";

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("webAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string WebAction = "ActionSpiderWeb";

    [DataField] public EntityUid? Action;

    // Pirate: Starlight terror spider building action support.
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public bool HasBuilding;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("buildingActionProto", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string BuildingActionProto = "ActionSpiderBuildingPole";

    [DataField]
    public EntityUid? BuildingAction;

    /// <summary>
    /// Whether the spider will spawn webs when not controlled by a player.
    /// </summary>
    [DataField]
    public bool SpawnsWebsAsNonPlayer = true;

    /// <summary>
    /// The cooldown in seconds between web spawns when not controlled by a player.
    /// </summary>
    [DataField]
    public TimeSpan WebSpawnCooldown = TimeSpan.FromSeconds(45f);

    /// <summary>
    /// The next time the spider can spawn a web when not controlled by a player.
    /// </summary>
    [DataField]
    public TimeSpan? NextWebSpawn;
}

public sealed partial class SpiderWebActionEvent : InstantActionEvent { }

// Pirate: raised after a spider successfully creates a web tile.
public sealed class SpiderWebSpawnedEvent : EntityEventArgs;
