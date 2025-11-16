// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.PlayerListener;

[RegisterComponent]
public sealed partial class DormNotifierComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public HashSet<Condemnation> Potential = [];

    /// <summary>
    /// Stores sessions that have been found to be engaging in dorm activity
    /// </summary>
    public HashSet<Condemnation> Condemned = [];
}

public sealed class Condemnation(EntityUid marker, HashSet<EntityUid> condemned)
{
    public EntityUid Marker = marker;
    public HashSet<EntityUid> Condemned = condemned;
}