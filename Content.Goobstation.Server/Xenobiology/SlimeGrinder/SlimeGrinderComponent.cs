// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Xenobiology.SlimeGrinder;

[RegisterComponent]
public sealed partial class SlimeGrinderComponent : Component
{
    /// <summary>
    /// This gets set for each mob it processes.
    /// When it hits 0, spit out extract.
    /// </summary>
    [ViewVariables]
    public float ProcessingTimer = default;

    /// <summary>
    /// The entity being ground.
    /// </summary>
    [ViewVariables]
    public Dictionary<EntProtoId, float> YieldQueue = new();

    /// <summary>
    /// The time it takes to process a mob, per mass.
    /// </summary>
    [DataField]
    public float ProcessingTimePerUnitMass = 0.1f;

    [DataField]
    public SoundSpecifier GrindSound = new SoundPathSpecifier("/Audio/Machines/reclaimer_startup.ogg");

}
