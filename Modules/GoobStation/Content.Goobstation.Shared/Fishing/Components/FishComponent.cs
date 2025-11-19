// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Fishing.Components;

/// <summary>
/// The fish itself!
/// </summary>
[RegisterComponent]
public sealed partial class FishComponent : Component
{
    public const float DefaultDifficulty = 0.021f;

    [DataField("difficulty")]
    public float FishDifficulty = DefaultDifficulty;
}
