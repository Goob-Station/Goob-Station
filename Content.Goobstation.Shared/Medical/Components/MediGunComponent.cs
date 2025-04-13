// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Medical.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MediGunComponent : Component
{
    /// <summary>
    /// Game time for the next tick of healing.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan? NextTick;

    /// <summary>
    /// Time when uber should end if it's activated.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan? UberEndTime;

    /// <summary>
    /// All entities that we're currently healing.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> HealedEntities = new();

    /// <summary>
    /// Player that activated this gun. If parent is changed, and
    /// new parent is not equal to this, then we disable this gun.
    /// Equals to null when it's not active.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? ParentEntity;

    [ViewVariables]
    public bool IsActive = false;

    [ViewVariables]
    public bool UberActivated = false;

    /// <summary>
    /// How much damage did we heal in total.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public float UberPoints;

    /// <summary>
    /// UberPoints must be higher than this in order to be possible to activate uber.
    /// </summary>
    [DataField]
    public float PointsToUber = 150;

    /// <summary>
    /// How long uber should be.
    /// </summary>
    [DataField]
    public float UberDefaultLenght = 10f;

    /// <summary>
    /// This gun will work only with entities that are in this whitelist.
    /// </summary>
    [DataField]
    public EntityWhitelist HealAbleWhitelist = new()
    {
        RequireAll = true,
        Components = new[]
        {
            "Damageable",
            "MobState",
        }
    };

    /// <summary>
    /// Damage applied every med beam tick.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Healing = new();

    /// <summary>
    /// Damage applied on uber activation.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier UberHealing = new();

    /// <summary>
    /// How many bleeding we stop every tick
    /// </summary>
    [DataField]
    public float BleedingModifier = -1.0f;

    /// <summary>
    /// Tick frequency in seconds
    /// </summary>
    [DataField]
    public float Frequency = 1f;

    /// <summary>
    /// How many entities we can heal at once
    /// </summary>
    [DataField]
    public int MaxLinksAmount = 1;

    /// <summary>
    /// Max range for this beam to work
    /// </summary>
    [DataField]
    public float MaxRange = 6f;

    /// <summary>
    /// Charge that is removed from the battery every healing tick for one entity.
    /// </summary>
    [DataField]
    public float BatteryWithdraw = 10f;

    /// <summary>
    /// Charge that is removed from the battery every
    /// healing tick for one entity While uber mode is active.
    /// </summary>
    [DataField]
    public float UberBatteryWithdraw = 15f;

    [DataField, ViewVariables]
    public SpriteSpecifier BeamSprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Objects/Specific/Medical/medigun.rsi"), "beam");

    [DataField, ViewVariables]
    public SpriteSpecifier UberBeamSprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Objects/Specific/Medical/medigun.rsi"), "beam_uber");

    [DataField, AutoNetworkedField]
    public EntProtoId UberActionId = "ActionActivateMedigunUber";

    [DataField, AutoNetworkedField]
    public EntityUid? UberAction;

    [DataField]
    public Color DefaultLineColor = Color.Aqua;

    [DataField]
    public Color UberLineColor = Color.OrangeRed;
}
