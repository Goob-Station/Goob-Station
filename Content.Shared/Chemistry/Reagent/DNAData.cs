// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry.Reagent;

[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
public sealed partial class DnaData : ReagentData
{
    [DataField]
    public string DNA = string.Empty;

    public override ReagentData Clone()
    {
        return new DnaData
        {
            DNA = DNA,
        };
    }
    [DataField] // Goobstation
    public TimeSpan Freshness = TimeSpan.Zero; // Goobstation

    // Goobstation start - fix solution shallow copy
    public DnaData(DnaData other)
    {
        DNA = other.DNA;
        Freshness = other.Freshness;
    }
    // Goobstation End

    public override bool Equals(ReagentData? other)
    {
        if (other == null)
        {
            return false;
        }

        return ((DnaData) other).DNA == DNA;
    }

    public override int GetHashCode()
    {
        return DNA.GetHashCode();
    }
}
