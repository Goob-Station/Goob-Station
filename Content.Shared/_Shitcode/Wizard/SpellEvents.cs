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

public sealed partial class InstantSummonsEvent : InstantActionEvent
{
    [DataField]
    public SoundSpecifier? SummonSound;
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
