// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Shared._Lavaland.Damage;

[RegisterComponent]
public sealed partial class DamageSquareComponent : Component
{
    /// <summary>
    /// Entity that caused this damaging square to spawn.
    /// It will be ignored by this square.
    /// </summary>
    [DataField]
    public EntityUid OwnerEntity;

    [DataField(required: true)]
    public DamageSpecifier Damage = new();

    [DataField]
    public SoundPathSpecifier? Sound;

    /// <summary>
    /// After how many seconds we should deal the damage to all entities above.
    /// 0.1 by default because ping will make it unfair
    /// </summary>
    [DataField]
    public float DamageDelay = 0.2f;
}