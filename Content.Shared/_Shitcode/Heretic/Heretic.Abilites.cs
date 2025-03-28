using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Heretic;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticActionComponent : Component
{
    /// <summary>
    ///     Indicates that a user should wear a heretic amulet, a hood or something else.
    /// </summary>
    [DataField] public bool RequireMagicItem = true;

    [DataField] public string? MessageLoc = null;
}

#region DoAfters

[Serializable, NetSerializable] public sealed partial class EldritchInfluenceDoAfterEvent : SimpleDoAfterEvent
{
}
[Serializable, NetSerializable] public sealed partial class DrawRitualRuneDoAfterEvent : SimpleDoAfterEvent
{
    [NonSerialized] public EntityCoordinates Coords;
    [NonSerialized] public EntityUid RitualRune;

    public DrawRitualRuneDoAfterEvent(EntityUid ritualRune, EntityCoordinates coords)
    {
        RitualRune = ritualRune;
        Coords = coords;
    }
}
[Serializable, NetSerializable] public sealed partial class HereticMansusLinkDoAfter : SimpleDoAfterEvent
{
    [NonSerialized] public EntityUid Target;

    public HereticMansusLinkDoAfter(EntityUid target)
    {
        Target = target;
    }
}
[Serializable, NetSerializable] public sealed partial class EventHereticFleshSurgeryDoAfter : SimpleDoAfterEvent
{
    [NonSerialized] public EntityUid? Target;

    public EventHereticFleshSurgeryDoAfter(EntityUid target)
    {
        Target = target;
    }
}

#endregion

#region Abilities

/// <summary>
///     Raised whenever we need to check for a magic item before casting a spell that requires one to be worn.
/// </summary>
public sealed partial class CheckMagicItemEvent : HandledEntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}

// basic
public sealed partial class EventHereticOpenStore : InstantActionEvent { }
public sealed partial class EventHereticMansusGrasp : InstantActionEvent { }
public sealed partial class EventHereticLivingHeart : InstantActionEvent { } // opens ui

// living heart
[Serializable, NetSerializable] public sealed partial class EventHereticLivingHeartActivate : BoundUserInterfaceMessage // triggers the logic
{
    public NetEntity? Target { get; set; }
}
[Serializable, NetSerializable] public enum HereticLivingHeartKey : byte
{
    Key
}

// ghoul specific
public sealed partial class EventHereticMansusLink : EntityTargetActionEvent { }

// ash
public sealed partial class EventHereticAshenShift : InstantActionEvent { }
public sealed partial class EventHereticVolcanoBlast : InstantActionEvent { }
public sealed partial class EventHereticNightwatcherRebirth : InstantActionEvent { }
public sealed partial class EventHereticFlames : InstantActionEvent { }
public sealed partial class EventHereticCascade : InstantActionEvent { }

// flesh
public sealed partial class EventHereticFleshSurgery : EntityTargetActionEvent { }

// void (+ upgrades)
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticAristocratWayEvent : EntityEventArgs { }
public sealed partial class HereticVoidBlastEvent : InstantActionEvent { }
public sealed partial class HereticVoidBlinkEvent : WorldTargetActionEvent { }
public sealed partial class HereticVoidPullEvent : InstantActionEvent { }

// blade (+ upgrades)
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticCuttingEdgeEvent : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticDanceOfTheBrandEvent : EntityEventArgs { }
public sealed partial class EventHereticRealignment : InstantActionEvent { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticChampionStanceEvent : EntityEventArgs { }
public sealed partial class EventHereticFuriousSteel : InstantActionEvent { }

// lock
public sealed partial class EventHereticBulglarFinesse : InstantActionEvent { }
public sealed partial class EventHereticLastRefugee : InstantActionEvent { }

// rust
[Serializable, NetSerializable, DataDefinition]
public sealed partial class HereticLeechingWalkEvent : EntityEventArgs;

public sealed partial class EventHereticRustConstruction : WorldTargetActionEvent
{
    [DataField]
    public EntProtoId RustedWall = "WallSolidRust";

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/constructform.ogg");

    [DataField]
    public float ObstacleCheckRange = 0.05f;

    [DataField]
    public float MobCheckRange = 0.6f;

    [DataField]
    public float ThrowSpeed = 15f;

    [DataField]
    public float ThrowRange = 5f;

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(5f);

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            { "Blunt", 20 },
        },
    };
}

public sealed partial class EventHereticEntropicPlume : InstantActionEvent
{
    [DataField]
    public EntProtoId Proto = "EntropicPlume";

    [DataField]
    public float Offset = 2.5f;

    [DataField]
    public float Speed = 0.1f;

    [DataField]
    public float Radius = 2.5f;

    [DataField]
    public float LookupRange = 0.1f;

    [DataField]
    public EntProtoId TileRune = "TileHereticRustRune";
}

public sealed partial class EventHereticAggressiveSpread : InstantActionEvent
{
    [DataField]
    public float AoeRadius = 2f;

    [DataField]
    public float Range = 4f;

    [DataField]
    public float LookupRange = 0.1f;

    [DataField]
    public EntProtoId TileRune = "TileHereticRustRune";
}

// side
public sealed partial class EventHereticCleave : WorldTargetActionEvent
{
    [DataField]
    public float Range = 1f;

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            {"Heat", 20f},
            {"Bloodloss", 10f},
        },
    };

    [DataField]
    public FixedPoint2 BloodModifyAmount = -50f;

    [DataField]
    public EntProtoId Effect = "EffectCleave";

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/blood3.ogg");
}

// ascensions
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticAscensionAshEvent : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticAscensionVoidEvent : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticAscensionFleshEvent : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticAscensionLockEvent : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticAscensionBladeEvent : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticAscensionRustEvent : EntityEventArgs { }
#endregion
