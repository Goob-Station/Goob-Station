// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared._White.FootPrint;

[RegisterComponent]
public sealed partial class PuddleFootPrintsComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float SizeRatio = 0.2f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float OffPercent = 80f;
}