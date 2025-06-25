// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Megafauna;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Hierophant.Actions;

[MeansImplicitUse]
public abstract partial class BaseHierophantAction : MegafaunaAction
{
    [DataField]
    public EntProtoId DamageTile = "LavalandHierophantSquare";

    /// <summary>
    /// Controls the speed of consecutive hierophant attacks.
    /// </summary>
    [DataField]
    public float TileDamageDelay = 0.7f;
}
