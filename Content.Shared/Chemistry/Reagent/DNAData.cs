// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry.Reagent;

[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
public sealed partial class DnaData : ReagentData
{
    [DataField]
    public string DNA = String.Empty;

    public override ReagentData Clone() => this;

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