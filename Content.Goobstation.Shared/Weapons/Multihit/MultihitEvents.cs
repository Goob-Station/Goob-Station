// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Weapons.Multihit;


[ByRefEvent]
public record struct MultihitGetWeaponsEvent(EntityUid User,
    EntityUid Weapon,
    float DamageMultiplier,
    TimeSpan Delay,
    List<EntityUid> Weapons);

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
