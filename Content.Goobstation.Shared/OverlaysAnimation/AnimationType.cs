// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.OverlaysAnimation;

public enum AnimationType
{
    /// <summary>
    /// Do this animation instantly when it should start.
    /// After that will wait until the next animation
    /// </summary>
    Instant,

    /// <summary>
    ///
    /// </summary>
    Linear,

    /// <summary>
    /// Slow at the start, and accelerates on the end.
    /// </summary>
    Exponential,

    /// <summary>
    /// Faster in the middle, slower at the start and the end.
    /// </summary>
    Sinus,

    /// <summary>
    /// Faster at the start and the end, slower in the middle.
    /// </summary>
    Cosinus,
}
