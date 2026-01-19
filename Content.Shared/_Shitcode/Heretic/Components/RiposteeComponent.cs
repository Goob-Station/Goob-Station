// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class RiposteeComponent : Component
{
    public override bool SessionSpecific => true;

    [DataField]
    public Dictionary<string, RiposteData> Data = new();
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class RiposteData(
    float cooldown,
    bool requiresWeapon,
    EntityWhitelist? whitelist,
    bool canRiposte,
    SoundSpecifier? riposteSound,
    TimeSpan knockdownTime,
    TimeSpan stunTime,
    bool canRiposteWhileProne,
    float riposteChance,
    LocId? riposteUsedMessage,
    LocId? riposteAvailableMessage,
    BaseRiposteCheckEvent? canRiposteEvent)
{
    // Default values for blade heretic
    public RiposteData() : this(20f,
        true,
        new() { Tags = new() { "HereticBlade", }, },
        true,
        new SoundPathSpecifier("/Audio/_Goobstation/Heretic/parry.ogg"),
        TimeSpan.FromSeconds(1),
        TimeSpan.Zero,
        true,
        1f,
        "heretic-riposte-used",
        "heretic-riposte-available",
        null)
    {
    }

    [DataField]
    public float Cooldown = cooldown;

    [DataField]
    public bool RequiresWeapon = requiresWeapon;

    [DataField]
    public EntityWhitelist? WeaponWhitelist = whitelist;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Timer = cooldown;

    [DataField]
    public bool CanRiposte = canRiposte;

    [DataField]
    public SoundSpecifier? RiposteSound = riposteSound;

    [DataField]
    public TimeSpan StunTime = stunTime;

    [DataField]
    public TimeSpan KnockdownTime = knockdownTime;

    [DataField]
    public bool CanRiposteWhileProne = canRiposteWhileProne;

    [DataField]
    public float RiposteChance = riposteChance;

    [DataField]
    public LocId? RiposteUsedMessage = riposteUsedMessage;

    [DataField]
    public LocId? RiposteAvailableMessage = riposteAvailableMessage;

    [DataField, NonSerialized]
    public BaseRiposteCheckEvent? CanRiposteEvent = canRiposteEvent;
}
