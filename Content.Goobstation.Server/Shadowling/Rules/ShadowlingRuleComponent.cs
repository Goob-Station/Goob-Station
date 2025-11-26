// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Shadowling.Rules;

[RegisterComponent, Access(typeof(ShadowlingRuleSystem))]
public sealed partial class ShadowlingRuleComponent : Component
{
    [DataField]
    public ShadowlingWinCondition WinCondition = ShadowlingWinCondition.Draw;
}

/// <summary>
/// The winning conditions of the shadowling.
/// Draw happens when both sides are alive. The default option.
/// Win happens when a Shadowling ascends.
/// Failure happens when all Shadowlings die.
/// </summary>
public enum ShadowlingWinCondition : byte
{
    Draw,
    Win,
    Failure
}
