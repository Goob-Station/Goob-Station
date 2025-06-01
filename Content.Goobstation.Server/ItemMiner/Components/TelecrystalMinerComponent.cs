// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.ItemMiner;

[RegisterComponent]
public sealed partial class TelecrystalMinerComponent : Component
{
    /// <summary>
    /// 0 - no announcements made, 1 - normal announcement made, 2 - location announcement made
    /// </summary>
    [DataField]
    public int NotifiedStage = 0;

    /// <summary>
    /// After how many telecrystals produced to make an announcement.
    /// </summary>
    [DataField]
    public int AnnounceAt = 40;

    [DataField]
    public LocId Announcement = "telecrystal-miner-announcement";

    /// <summary>
    /// After how many telecrystals produced to make an announcement with the miner's location.
    /// </summary>
    [DataField]
    public int LocationAt = 100;

    [DataField]
    public LocId LocationAnnouncement = "telecrystal-miner-announcement2";

    /// <summary>
    /// How many telecrystals have we produced so far.
    /// </summary>
    [DataField]
    public int Accumulated = 0;
}
