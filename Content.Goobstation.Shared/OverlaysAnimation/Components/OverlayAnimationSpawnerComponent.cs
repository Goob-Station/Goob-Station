// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityTable.EntitySelectors;

namespace Content.Goobstation.Shared.OverlaysAnimation.Components;

[RegisterComponent]
public sealed partial class OverlayAnimationSpawnerComponent : Component
{
    [DataField(required: true)]
    public EntityTableSelector AnimationsTable = default!;
}
