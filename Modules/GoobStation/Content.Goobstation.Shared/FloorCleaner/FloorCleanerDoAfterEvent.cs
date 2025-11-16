// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Decals;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.FloorCleaner;

[Serializable, NetSerializable]
public sealed partial class FloorCleanerDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public List<NetEntity> Entities = default!;

    [DataField]
    public HashSet<(uint Index, Decal Decal)> Decals = default!;

    public FloorCleanerDoAfterEvent(List<NetEntity> entities, HashSet<(uint Index, Decal Decal)> decals)
    {
        Entities = entities;
        Decals = decals;
    }

    public override DoAfterEvent Clone() => this;
}
