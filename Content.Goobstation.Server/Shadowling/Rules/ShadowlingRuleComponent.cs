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

    public readonly List<EntityUid> ShadowlingMinds = new();
}

public enum ShadowlingWinCondition : byte
{
    Draw,
    Win,
    Failure
}
