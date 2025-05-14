// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.OverlaysAnimation.Animations;

[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class ScaleOverlayAnimation : OverlayAnimation
{
    [DataField]
    public float StartScale = 1.0f;

    [DataField]
    public float EndScale = 1.0f;
}
