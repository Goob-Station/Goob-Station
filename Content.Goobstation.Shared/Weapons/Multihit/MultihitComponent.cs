// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.Multihit;

[RegisterComponent, NetworkedComponent]
public sealed partial class MultihitComponent : Component
{
    [DataField]
    public float DamageMultiplier = 0.67f;

    [DataField]
    public TimeSpan MultihitDelay = TimeSpan.FromSeconds(0.25);

    [DataField]
    public EntityWhitelist? MultihitWhitelist;

    [DataField]
    public List<BaseMultihitUserConditionEvent> Conditions = new();

    [DataField]
    public bool RequireAllConditions;
}
