// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Xenobiology.Extract;

/// <summary>
/// Ideally, the extract component will hold an event that is fired on use.
/// </summary>
[RegisterComponent]
public sealed partial class SlimeExtractComponent : Component
{
    /// <summary>
    /// This is deprecated but I'm too lazy to remove it.
    /// </summary>
    [DataField]
    public SlimeExtractType ExtractType = SlimeExtractType.Grey;
}

public enum SlimeExtractType : byte
{
    Grey,
    Orange,
    Purple,
    Blue,
    Metal,
}
