// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.OnHit;

[RegisterComponent]
public sealed partial class CuffsOnHitComponent : Component
{
    [DataField("proto")]
    public EntProtoId? HandcuffPrototype;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1);

    [DataField("sound")]
    public SoundSpecifier? Sound;
}

[ByRefEvent]
public record struct CuffsOnHitAttemptEvent(bool Cancelled);

[Serializable, NetSerializable]
public sealed partial class CuffsOnHitDoAfter : SimpleDoAfterEvent { }