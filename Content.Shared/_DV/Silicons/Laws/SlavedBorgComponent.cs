// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Silicons.Laws;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Silicons.Laws;

/// <summary>
/// Adds a law no matter the default lawset.
/// Switching borg chassis type keeps this law.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSlavedBorgSystem))]
public sealed partial class SlavedBorgComponent : Component
{
    /// <summary>
    /// The law to add after loading the default laws or switching chassis.
    /// This is assumed to be law 0 so gets inserted to the top of the laws.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<SiliconLawPrototype> Law;

    /// <summary>
    /// Prevents adding the same law twice.
    /// </summary>
    [DataField]
    public bool Added;
}
