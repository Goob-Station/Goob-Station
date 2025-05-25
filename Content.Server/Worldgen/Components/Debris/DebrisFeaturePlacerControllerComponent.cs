// SPDX-FileCopyrightText: 2023 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Server.Worldgen.Prototypes;
using Content.Server.Worldgen.Systems.Debris;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Worldgen.Components.Debris;

/// <summary>
///     This is used for controlling the debris feature placer.
/// </summary>
[RegisterComponent]
[Access(typeof(DebrisFeaturePlacerSystem))]
public sealed partial class DebrisFeaturePlacerControllerComponent : Component
{
    /// <summary>
    ///     Whether or not to clip debris that would spawn at a location that has a density of zero.
    /// </summary>
    [DataField("densityClip")] public bool DensityClip = true;

    /// <summary>
    ///     Whether or not entities are already spawned.
    /// </summary>
    public bool DoSpawns = true;

    [DataField("ownedDebris")] public Dictionary<Vector2, EntityUid?> OwnedDebris = new();

    /// <summary>
    ///     The chance spawning a piece of debris will just be cancelled randomly.
    /// </summary>
    [DataField("randomCancelChance")] public float RandomCancellationChance = 0.1f;

    /// <summary>
    ///     Radius in which there should be no objects for debris to spawn.
    /// </summary>
    [DataField("safetyZoneRadius")] public float SafetyZoneRadius = 16.0f;

    /// <summary>
    ///     The noise channel to use as a density controller.
    /// </summary>
    [DataField("densityNoiseChannel", customTypeSerializer: typeof(PrototypeIdSerializer<NoiseChannelPrototype>))]
    public string DensityNoiseChannel { get; private set; } = default!;
}
