// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Vehicles.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DamageOnCollisionComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public float MinImpactSpeed = 5f;

    [DataField]
    public float DamageCooldown = 0.5f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? LastHit;
}
