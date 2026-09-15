using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Actions;
using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Shared._White.Xenomorphs.Construction;

/// <summary>
/// RMC-style resin construction: choose a structure, then secrete it with a separate action.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphConstructionComponent : Component
{
    [DataField]
    public List<XenomorphConstructionOption> Options = new()
    {
        new()
        {
            Id = "WallResin",
            Entity = "WallResin",
            PlasmaCost = 50,
            BuildLength = 2f,
            Name = "xenomorphs-construction-wall",
            Icon = new SpriteSpecifier.Rsi(new ResPath("_RMC14/Structures/Xenos/xeno_resin_wall.rsi"), "resin"),
            BlockedCollisionMask = (int) CollisionGroup.FullTileMask,
            BlockedCollisionLayer = (int) CollisionGroup.WallLayer,
        },
        new()
        {
            Id = "ResinMembrane",
            Entity = "ResinMembrane",
            PlasmaCost = 50,
            BuildLength = 0.5f,
            Name = "xenomorphs-construction-membrane",
            Icon = new SpriteSpecifier.Rsi(new ResPath("_RMC14/Structures/Xenos/xeno_resin_wall.rsi"), "membrane"),
            BlockedCollisionMask = (int) CollisionGroup.FullTileMask,
            BlockedCollisionLayer = (int) CollisionGroup.GlassLayer,
        },
        new()
        {
            Id = "ResinDoor",
            Entity = "ResinDoor",
            PlasmaCost = 75,
            BuildLength = 2f,
            Name = "xenomorphs-construction-door",
            Icon = new SpriteSpecifier.Rsi(new ResPath("_RMC14/Structures/Xenos/xeno_resin_door.rsi"), "resin"),
            BlockedCollisionMask = (int) CollisionGroup.TableMask,
            BlockedCollisionLayer = (int) (CollisionGroup.TableLayer | CollisionGroup.BulletImpassable),
        },
        new()
        {
            Id = "ResinNest",
            Entity = "ResinNest",
            PlasmaCost = 50,
            BuildLength = 2f,
            Name = "xenomorphs-construction-nest",
            Icon = new SpriteSpecifier.Rsi(new ResPath("_White/Structures/Windows/resin_membrane.rsi"), "full"),
            BlockedCollisionMask = (int) CollisionGroup.TableMask,
            BlockedCollisionLayer = (int) (CollisionGroup.TableLayer | CollisionGroup.BulletImpassable),
        },
        new()
        {
            Id = "XenoNest",
            Entity = "XenoNest",
            PlasmaCost = 50,
            BuildLength = 2f,
            Name = "xenomorphs-construction-wall-nest",
            Icon = new SpriteSpecifier.Rsi(new ResPath("_RMC14/Structures/Xenos/xeno_weeds.rsi"), "nest_overlay"),
            BlockedCollisionMask = (int) CollisionGroup.TableMask,
            BlockedCollisionLayer = (int) (CollisionGroup.TableLayer | CollisionGroup.BulletImpassable),
        },
        new()
        {
            Id = "FloorXenoHive",
            TileId = "FloorXenoHive",
            PlasmaCost = 50,
            BuildLength = 1f,
            Name = "xenomorphs-construction-hive-floor",
            Icon = new SpriteSpecifier.Rsi(new ResPath("_White/Structures/Furniture/resin_weed.rsi"), "full"),
        },
    };

    [DataField, AutoNetworkedField]
    public string? SelectedId;

    [DataField]
    public EntProtoId ChooseActionId = "ActionXenomorphChooseStructure";

    [DataField]
    public EntProtoId SecreteActionId = "ActionXenomorphSecrete";

    [DataField]
    public SoundSpecifier? BuildAudio;

    [ViewVariables]
    public EntityUid? ChooseAction;

    [ViewVariables]
    public EntityUid? SecreteAction;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class XenomorphConstructionOption
{
    [DataField(required: true)]
    public string Id = string.Empty;

    [DataField]
    public EntProtoId? Entity;

    [DataField]
    public string? TileId;

    [DataField]
    public FixedPoint2 PlasmaCost = 50;

    [DataField]
    public float BuildLength = 2f;

    [DataField]
    public LocId Name = "xenomorphs-construction-unknown";

    [DataField]
    public SpriteSpecifier? Icon;

    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionMask>))]
    public int BlockedCollisionMask;

    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionLayer>))]
    public int BlockedCollisionLayer;
}

public sealed partial class XenomorphChooseStructureEvent : InstantActionEvent;

public sealed partial class XenomorphSecreteEvent : WorldTargetActionEvent;
