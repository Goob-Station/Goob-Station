// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Weapons.Multihit;


[ByRefEvent]
public struct MultihitGetWeaponsEvent(EntityUid user, EntityUid weapon, float damageMultiplier, TimeSpan delay)
{
    public readonly EntityUid User = user;
    public readonly EntityUid Weapon = weapon;
    public float DamageMultiplier = damageMultiplier;
    public TimeSpan Delay = delay;
    public readonly List<EntityUid> Weapons = new();
}

[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class BaseMultihitUserConditionEvent : HandledEntityEventArgs
{
    public EntityUid User = EntityUid.Invalid;
}

public sealed partial class MultihitUserWhitelistEvent : BaseMultihitUserConditionEvent
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist;

    [DataField]
    public bool Blacklist;
}

public sealed partial class MultihitUserHereticEvent : BaseMultihitUserConditionEvent
{
    [DataField]
    public int MinPathStage;

    [DataField]
    public string? RequiredPath;
}
