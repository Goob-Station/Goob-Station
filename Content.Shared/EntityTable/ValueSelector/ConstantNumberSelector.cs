// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityTable.ValueSelector;

/// <summary>
/// Gives a constant value.
/// </summary>
public sealed partial class ConstantNumberSelector : NumberSelector
{
    [DataField]
    public float Value = 1;

    public ConstantNumberSelector(float value)
    {
        Value = value;
    }

    public override float Get(System.Random rand, IEntityManager entMan, IPrototypeManager proto)
    {
        return Value;
    }
}