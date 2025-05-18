// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.MisandryBox;

/// <summary>
/// Hook for marking-related things
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class MarkingSpecial
{
    public abstract void AfterEquip(EntityUid mob);
}
