// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client.Holiday;

/// <summary>
/// This is used for a component that swaps an entity's RSI based on HolidayVisuals
/// </summary>
[RegisterComponent]
public sealed partial class HolidayRsiSwapComponent : Component
{
    /// <summary>
    /// A dictionary of arbitrary visual keys to an rsi to swap the sprite to.
    /// </summary>
    [DataField]
    public Dictionary<string, string> Sprite = new();
}