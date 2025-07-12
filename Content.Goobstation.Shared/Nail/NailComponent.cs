// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Nail;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NailComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new ();

    [DataField]
    public DamageSpecifier DamageToWhitelisted = new ();

    [DataField]
    public EntityWhitelist Whitelist = new ();

    [DataField]
    public bool AutoHammerIntoNonWhitelisted;

    [DataField]
    public TargetBodyPart? ForceBodyPart = TargetBodyPart.Chest;

    [DataField, AutoNetworkedField]
    public bool ShotFromNailgun;
}
