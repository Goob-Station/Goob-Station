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
    /// Was there a CC announcement after 10 mins
    /// </summary>
    [DataField]
    public bool Notified = false;

    /// <summary>
    /// After how many telecrystals produced to make an announcement.
    /// </summary>
    [DataField]
    public int Required = 60;

    /// <summary>
    /// How many telecrystals have we produced so far.
    /// </summary>
    [DataField]
    public int Accumulated = 0;

    [DataField]
    public LocId Announcement = "telecrystal-miner-announcement";
}
