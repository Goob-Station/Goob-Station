// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Goobstation.Shared.OfficeChair;

[RegisterComponent]
public sealed partial class RocketChairComponent : Component
{
    /// <summary>
    /// Name of the solution container on this entity that holds fuel.
    /// </summary>
    [DataField] public string FuelSolution = "rocket";

    /// <summary>
    /// Reagent prototype ID consumed as fuel.
    /// </summary>
    [DataField] public string FuelReagent = "Water";

    /// <summary>
    /// Maximum capacity of the fuel solution container.
    /// </summary>
    [DataField] public float FuelCapacity = 100f;

    /// <summary>
    /// Fuel amount to add the the vehicle upon load.
    /// </summary>
    [DataField] public float StartFuel = 100f;

    /// <summary>
    /// Fuel consumption rate while boosting (units per second).
    /// </summary>
    [DataField] public float FuelPerSecond = 15f;

    /// <summary>
    /// Peak acceleration applied during boost
    /// </summary>
    [DataField] public float ThrustAcceleration = 30f;

    /// <summary>
    /// Duration in seconds of a single boost activation.
    /// </summary>
    [DataField] public float BoostDuration = 0.6f;

    /// <summary>
    /// Action prototype granted to a strapped pilot to trigger the boost.
    /// </summary>
    [DataField] public EntProtoId ActionProto = "ActionRocketChairBoost";

    /// <summary>
    /// Vapor entity prototype used to visualize exhaust plumes.
    /// </summary>
    [DataField] public string VaporPrototype = "ExtinguisherSpray";

    /// <summary>
    /// Reagent amount transferred into each spawned vapor entity.
    /// </summary>
    [DataField] public float VaporReagentPerPuff = 0.05f;

    /// <summary>
    /// Initial speed of emitted vapor particles.
    /// </summary>
    [DataField] public float VaporVelocity = 4.0f;

    /// <summary>
    /// Lifetime of each vapor particle in seconds.
    /// </summary>
    [DataField] public float VaporLifetime = 0.8f;

    /// <summary>
    /// Number of vapor particles spawned per emission burst.
    /// </summary>
    [DataField] public int VaporCountPerEmit = 1;

    /// <summary>
    /// Angular spread in degrees across which vapor particles are emitted.
    /// </summary>
    [DataField] public float VaporSpread = 30f;

    /// <summary>
    /// Time in seconds between vapor emission bursts while boosting.
    /// </summary>
    [DataField] public float EmitInterval = 0.4f;

    /// <summary>
    /// Maximum number of emission bursts processed in a single update tick.
    /// </summary>
    [DataField] public int EmitMaxPerTick = 2;

    /// <summary>
    /// Offset from the vehicle along the exhaust direction where vapor spawns.
    /// </summary>
    [DataField] public float NozzleOffset = 0.1f;

    /// <summary>
    /// Sound played when vapor/exhaust is emitted.
    /// </summary>
    [DataField] public SoundSpecifier SpraySound = new SoundPathSpecifier("/Audio/Effects/extinguish.ogg");

    /// <summary>
    /// When true, VehicleHitAndRunComponent is only enabled while a boost is active
    /// </summary>
    [DataField] public bool LockHitAndRunComponent = true;

    // Internal shit
    public TimeSpan EmitElapsed;
    public TimeSpan BoostStart;
    public TimeSpan BoostEnd;
    public Vector2 BoostDir;
    public EntityUid? LastPilot;
    public EntityUid? BoostAction;
}

public sealed partial class RocketChairBoostActionEvent : WorldTargetActionEvent
{
}
