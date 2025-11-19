// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Nuke;

/// <summary>
///     This generates a label for a nuclear bomb.
/// </summary>
/// <remarks>
///     This is a separate component because the fake nuclear bomb keg exists.
/// </remarks>
[RegisterComponent]
public sealed partial class NukeLabelComponent : Component
{
    [DataField] public LocId Prefix = "nuke-label-nanotrasen";
    [DataField] public int SerialLength = 6;
}
