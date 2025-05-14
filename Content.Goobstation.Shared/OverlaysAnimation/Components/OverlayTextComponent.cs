// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.OverlaysAnimation.Components;

/// <summary>
/// Draws text on AnimationsOverlay.
/// Also requires OverlayAnimationComponent to work
/// </summary>
[RegisterComponent]
public sealed partial class OverlayTextComponent : OverlayObjectComponent
{
    [DataField]
    public string Text = string.Empty;

    /// <summary>
    /// Font for the text to use.
    /// </summary>
    [DataField]
    public string TextFontPath = "/Fonts/NotoSans/NotoSans-Regular.ttf";

    /// <summary>
    /// Text font size.
    /// </summary>
    [DataField]
    public int TextFontSize = 26;
}
