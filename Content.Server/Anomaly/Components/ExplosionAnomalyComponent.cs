// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ThunderBear2006 <100388962+ThunderBear2006@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Explosion;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Anomaly.Components;

[RegisterComponent]
public sealed partial class ExplosionAnomalyComponent : Component
{
    /// <summary>
    /// The explosion prototype to spawn
    /// </summary>
    [DataField("supercriticalExplosion", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<ExplosionPrototype>))]
    public string ExplosionPrototype = default!;

    /// <summary>
    /// The total amount of intensity an explosion can achieve
    /// </summary>
    [DataField("explosionTotalIntensity")]
    public float TotalIntensity = 100f;

    /// <summary>
    /// How quickly does the explosion's power slope? Higher = smaller area and more concentrated damage, lower = larger area and more spread out damage
    /// </summary>
    [DataField("explosionDropoff")]
    public float Dropoff = 10f;

    /// <summary>
    /// How much intensity can be applied per tile?
    /// </summary>
    [DataField("explosionMaxTileIntensity")]
    public float MaxTileIntensity = 10f;
}