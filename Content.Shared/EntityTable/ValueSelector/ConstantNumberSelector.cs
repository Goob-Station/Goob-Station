// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.EntityTable.ValueSelector;

/// <summary>
/// Gives a constant value.
/// </summary>
public sealed partial class ConstantNumberSelector : NumberSelector
{
    [DataField]
    public int Value = 1;

    public ConstantNumberSelector(int value)
    {
        Value = value;
    }

    public override int Get(System.Random rand)
    {
        return Value;
    }
}