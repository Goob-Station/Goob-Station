// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.Explosion;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Item;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Physics;
using Content.Shared.Polymorph;
using Content.Shared.Random;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Goobstation.Wizard;

public sealed partial class CluwneCurseEvent : EntityTargetActionEvent
{
    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan StutterDuration = TimeSpan.FromSeconds(30);
}

public sealed partial class BindSoulEvent : InstantActionEvent
{
    [DataField]
    public EntityWhitelist Blacklist;

    [DataField]
    public EntProtoId Entity = "MobSkeletonPerson";

    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public Dictionary<string, EntProtoId> Gear = new()
    {
        {"head", "ClothingHeadHatBlackwizardReal"},
        {"outerClothing", "ClothingOuterWizardBlackReal"},
    };

    [DataField]
    public ProtoId<ItemSizePrototype> PhylacterySize = "Ginormous";
}

public sealed partial class PolymorphSpellEvent : InstantActionEvent
{
    [DataField]
    public ProtoId<PolymorphPrototype>? ProtoId;

    [DataField]
    public bool MakeWizard = true;

    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public bool LoadActions;
}

public sealed partial class MutateSpellEvent : InstantActionEvent
{
    [DataField]
    public float Duration = 30f;
}

public sealed partial class TeslaBlastEvent : InstantActionEvent
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(10);

    [DataField]
    public float Range = 7f;

    [DataField]
    public int BoltCount = 1;

    [DataField]
    public int ArcDepth = 5;

    [DataField]
    public Vector2 MinMaxDamage = new(15f, 60f);

    [DataField]
    public Vector2 MinMaxStunTime = new(2f, 8f);

    [DataField]
    public EntProtoId LightningPrototype = "SuperchargedLightning";

    [DataField]
    public EntProtoId EffectPrototype = "EffectElectricity";

    [DataField]
    public SoundSpecifier? Sound;
}

public sealed partial class LightningBoltEvent : EntityTargetActionEvent
{
    [DataField]
    public float Damage = 50f;

    [DataField]
    public EntProtoId Proto = "ChargedLightning";
}

public sealed partial class SpellCardsEvent : WorldTargetActionEvent
{
    [DataField]
    public EntProtoId RedProto = "ProjectileSpellCardRed";

    [DataField]
    public EntProtoId PurpleProto = "ProjectileSpellCardPurple";

    [DataField]
    public float ProjectileSpeed = 20f;

    [DataField]
    public int ProjectilesAmount = 7;

    [DataField]
    public Angle Spread = Angle.FromDegrees(30);

    [DataField]
    public float MaxAngularVelocity = MathF.PI / 3f;

    [DataField]
    public Vector2 MinMaxLinearDamping = new(3f, 7f);
}

public sealed partial class BarnyardCurseEvent : EntityTargetActionEvent
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, SoundSpecifier?> Masks = new();

    [DataField]
    public ProtoId<TagPrototype> CursedMaskTag = "CursedAnimalMask";
}

public sealed partial class InstantSummonsEvent : InstantActionEvent
{
    [DataField]
    public SoundSpecifier? SummonSound;
}

public sealed partial class TrapsSpellEvent : InstantActionEvent
{
    [DataField]
    public List<EntProtoId> Traps = new()
    {
        "TrapShock",
        "TrapFlame",
        "TrapDamage",
        "TrapChill",
        "TrapBlind",
    };

    [DataField]
    public float Range = 3f;

    [DataField]
    public int Amount = 5;
}

public sealed partial class SummonMobsEvent : InstantActionEvent
{
    [DataField]
    public List<EntProtoId> Mobs = new();

    [DataField]
    public float Range = 1f;

    [DataField]
    public int Amount = 9;

    [DataField]
    public Angle SpawnAngle = Angle.FromDegrees(160);

    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionMask>))]
    public int CollisionMask = (int) CollisionGroup.MobMask;

    [DataField]
    public bool FactionIgnoreSummoner;
}

public sealed partial class SummonSimiansEvent : InstantActionEvent
{
    [DataField(required: true)]
    public ProtoId<WeightedRandomEntityPrototype> Mobs;

    [DataField(required: true)]
    public ProtoId<WeightedRandomEntityPrototype> Weapons;

    [DataField]
    public float Range = 1f;

    [DataField]
    public int Amount = 4;

    [DataField]
    public Angle SpawnAngle = Angle.FromDegrees(40);
}

public sealed partial class ExsanguinatingStrikeEvent : InstantActionEvent
{
}

public sealed partial class SwapSpellEvent : EntityTargetActionEvent
{
    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public float Range = 15f;

    [DataField]
    public EntProtoId Effect = "SwapSpellEffect";

    [DataField]
    public bool ThroughWalls = true;
}

public sealed partial class SoulTapEvent : InstantActionEvent
{
    [DataField]
    public FixedPoint2 MaxHealthReduction = 20;

    [DataField]
    public ProtoId<DamageTypePrototype> KillDamage = "Cellular";

    [DataField]
    public ProtoId<TagPrototype> DeadTag = "SoulTapped";
}

public sealed partial class ChargeMagicEvent : InstantActionEvent
{
    [DataField]
    public ProtoId<TagPrototype> WandTag = "WizardWand";

    [DataField]
    public float WandChargeRate = 1000f;

    [DataField]
    public float MinWandDegradeCharge = 1000f;

    [DataField]
    public float WandDegradePercentagePerCharge = 0.5f;

    [DataField]
    public List<ProtoId<TagPrototype>> RechargeTags = new()
    {
        "WizardWand",
        "WizardStaff",
    };
}

[DataDefinition]
public sealed partial class SummonSimiansMaxedOutEvent : EntityEventArgs
{
    [DataField]
    public EntProtoId Action = "ActionGorillaForm";

    [DataField]
    public ProtoId<TagPrototype> MaxLevelTag = "SummonSimiansMaxLevelAction";

    [DataField]
    public ProtoId<TagPrototype> GorillaFormTag = "GorillaFormAction";

    [DataField]
    public Color MessageColor = Color.FromHex("#EDC349");
}

[DataDefinition]
public sealed partial class GrantFactionsEvent : EntityEventArgs
{
    [DataField(required: true)]
    public HashSet<ProtoId<NpcFactionPrototype>> Factions = new();
}

[DataDefinition]
public sealed partial class RandomizeSpellsEvent : EntityEventArgs
{
    [DataField]
    public float TotalBalance = 10;

    [DataField(required: true)]
    public Dictionary<ProtoId<WeightedRandomEntityPrototype>, int?> SpellsDict;
}
