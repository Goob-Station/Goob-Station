// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Storage;
using Robust.Shared.Network;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Holiday.Christmas;

/// <summary>
/// This is used for granting items to lucky souls, exactly once.
/// </summary>
[RegisterComponent, Access(typeof(LimitedItemGiverSystem))]
public sealed partial class LimitedItemGiverComponent : Component
{
    /// <summary>
    /// Santa knows who you are behind the screen, only one gift per player per round!
    /// </summary>
    public readonly HashSet<NetUserId> GrantedPlayers = new();

    /// <summary>
    /// Selects what entities can be given out by the giver.
    /// </summary>
    [DataField("spawnEntries", required: true)]
    public List<EntitySpawnEntry> SpawnEntries = default!;

    /// <summary>
    /// The (localized) message shown upon receiving something.
    /// </summary>
    [DataField("receivedPopup", required: true)]
    public string ReceivedPopup = default!;

    /// <summary>
    /// The (localized) message shown upon being denied.
    /// </summary>
    [DataField("deniedPopup", required: true)]
    public string DeniedPopup = default!;

    /// <summary>
    /// The holiday required for this giver to work, if any.
    /// </summary>
    [DataField("requiredHoliday", customTypeSerializer: typeof(PrototypeIdSerializer<HolidayPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public string? RequiredHoliday = null;
}