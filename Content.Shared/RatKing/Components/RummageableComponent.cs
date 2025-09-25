// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 portfiend <109661617+portfiend@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 OnsenCapy <101037138+OnsenCapy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Random;
using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.RatKing.Components;

/// <summary>
/// This is used for entities that can be
/// rummaged through by the rat king to get loot.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class RummageableComponent : Component
{
    /// <summary>
    /// Whether or not this entity has been rummaged through already.
    /// </summary>
    [DataField("looted"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public bool Looted;

    /// <summary>
    /// DeltaV: Last time the object was looted, used to check if cooldown has expired
    /// </summary>
    [ViewVariables]
    public TimeSpan? LastLooted;

    /// <summary>
    /// DeltaV: Minimum time between rummage attempts
    /// </summary>
    [DataField("rummageCooldown"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public TimeSpan RummageCooldown = TimeSpan.FromMinutes(1);

    /// <summary>
    /// How long it takes to rummage through a rummageable container.
    /// </summary>
    [DataField("rummageDuration"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float RummageDuration = 2f;

    /// <summary>
    /// The entity table to select loot from.
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector Table = default!;

    /// <summary>
    /// Sound played on rummage completion.
    /// </summary>
    [DataField("sound")]
    public SoundSpecifier? Sound = new SoundCollectionSpecifier("storageRustle");
}
