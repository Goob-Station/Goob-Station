// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Goobstation.Common.TheManWhoSoldTheWorld;

/// <summary>
/// This is used to identify a Holo Cigar User
/// </summary>
[RegisterComponent]
public sealed partial class TheManWhoSoldTheWorldComponent : Component
{
    [ViewVariables]
    public EntityUid? HoloCigarEntity = null;

    [ViewVariables]
    public SoundSpecifier DeathAudio = new SoundPathSpecifier("/Audio/_Goobstation/Items/TheManWhoSoldTheWorld/whouuuHOOAAAAAAAAAAAAH.ogg");

    [DataField]
    public bool AddedNoWieldNeeded;
}
