// SPDX-FileCopyrightText: 2023 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Forensics;

/// <summary>
/// This controls residues left on items
/// which the forensics system uses.
/// </summary>
[RegisterComponent]
public sealed partial class ResidueComponent : Component
{
    [DataField]
    public LocId ResidueAdjective = "residue-unknown";

    [DataField]
    public string? ResidueColor;
}