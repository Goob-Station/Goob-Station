// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Random.Rules;

/// <summary>
/// Always returns true. Used for fallbacks.
/// </summary>
public sealed partial class AlwaysTrueRule : RulesRule
{
    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        return !Inverted;
    }
}