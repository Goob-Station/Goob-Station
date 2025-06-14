// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 widgetbeck <beckparrott@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Impstation.Replicator;

[RegisterComponent, NetworkedComponent]
public sealed partial class ReplicatorNestPointsStorageComponent : Component
{
    public int TotalPoints;

    public int TotalReplicators;

    public int Level;
}
