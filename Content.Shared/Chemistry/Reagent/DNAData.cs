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

    [DataField]
    public bool VampireToxin = false; // Pirate

    // Goobstation start - fix solution shallow copy
    public DnaData(DnaData other)
    {
        DNA = other.DNA;
        Freshness = other.Freshness;
        VampireToxin = other.VampireToxin; // Pirate
    }
    // Goobstation End

    public override bool Equals(ReagentData? other)
    {
        if (other == null)
        {
            return false;
        }

        var otherData = (DnaData)other; // Pirate
        return otherData.DNA == DNA && otherData.VampireToxin == VampireToxin; // Pirate
    }

    public override int GetHashCode()
    {
        return DNA.GetHashCode();
    }
}
