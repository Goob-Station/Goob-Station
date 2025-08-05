// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Heretic.Components;

/// <summary>
///     Indicates that an entity can act as a protective blade.
/// </summary>
[RegisterComponent]
public sealed partial class ProtectiveBladeComponent : Component
{
    [DataField]
    public float Lifetime = 60f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Timer = 60f;

    [DataField]
    public EntProtoId BladeProjectilePrototype = "HereticProtectiveBladeProjectile";

    [DataField]
    public SoundSpecifier BladeAppearSound = new SoundPathSpecifier("/Audio/Items/unsheath.ogg");

    [DataField]
    public SoundSpecifier BladeBlockSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/parry.ogg");
}
