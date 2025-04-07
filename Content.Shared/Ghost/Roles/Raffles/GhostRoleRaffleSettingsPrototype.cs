// SPDX-FileCopyrightText: 2024 no <165581243+pissdemon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Ghost.Roles.Raffles;
using Robust.Shared.Prototypes;

namespace Content.Shared.Ghost.Roles.Raffles;

/// <summary>
/// Allows specifying the settings for a ghost role raffle as a prototype.
/// </summary>
[Prototype("ghostRoleRaffleSettings")]
public sealed partial class GhostRoleRaffleSettingsPrototype : IPrototype
{
    /// <inheritdoc />
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The settings for a ghost role raffle.
    /// </summary>
    /// <seealso cref="GhostRoleRaffleSettings"/>
    [DataField(required: true)]
    public GhostRoleRaffleSettings Settings { get; private set; } = new();
}