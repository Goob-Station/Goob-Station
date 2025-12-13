// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 amogus <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Goobstation.Common.Pirates;

[RegisterComponent]
public sealed partial class ResourceSiphonComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)] public EntityUid? BoundGamerule;
    [ViewVariables(VVAccess.ReadOnly)] public bool Active = false;

    [DataField] public float CreditsThreshold = 1000000f; // Millianare

    [ViewVariables(VVAccess.ReadWrite)] public float Credits = 0f;


    /// <summary>
    ///  % of total station budget to drain number 0 - 1
    /// </summary>
    [DataField] public float DrainPercent = .05f;

    /// <summary>
    /// If the calculated DrainPercent is lower then this drains this amount instead
    /// </summary>
    [DataField] public float DrainRate = 10f;

    [ViewVariables(VVAccess.ReadOnly)] public int ActivationPhase = 0;

    public TimeSpan NextUpdateTime = TimeSpan.Zero;
    public TimeSpan NextUpdateInterval = TimeSpan.FromSeconds(1);

    [DataField] public float MaxSignalRange = 250f;
}
