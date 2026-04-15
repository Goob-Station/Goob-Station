// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Emag.Systems; // Goobstation - Jestographic
using Content.Shared.Mobs;
using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.Bots;

/// <summary>
/// Replaces the medibot's meds with these when emagged. Could be poison, could be fun.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(MedibotSystem))]
public sealed partial class EmaggableMedibotComponent : Component
{
    /// <summary>
    /// Original treatments before it was emagged.
    /// </summary>
    [ViewVariables]
    public Dictionary<MobState, MedibotTreatment> OriginalTreatments = new();

    /// <summary>
    /// Treatments to replace from the original set.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<EmagType, Dictionary<MobState, MedibotTreatment>> Replacements = new(); // Goobstation - Jestographic, added EmagType
}
