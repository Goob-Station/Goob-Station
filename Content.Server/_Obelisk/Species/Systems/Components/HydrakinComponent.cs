// SPDX-FileCopyrightText: 2025 sneb
//
// SPDX-License-Identifier: MPL-2.0

using Content.Shared.Atmos;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Species.Systems.Components;

[RegisterComponent]
public sealed partial class HydrakinComponent : Component
{
    [DataField]
    public float MinTemperature = Atmospherics.T20C;

    [DataField]
    public float MaxTemperature = 340f;

    [DataField]
    public float Buildup = 0f;

    [DataField]
    public bool HeatBuildupEnabled = true;

    [DataField]
    public float TemperatureProcessingCooldown = 0.5f;

    public TimeSpan CurrentTemperatureCooldown = TimeSpan.Zero;

    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? CoolOffActionId = "ActionHydrakinCoolOff";

    [DataField]
    public SoundSpecifier? CoolOffSound = new SoundCollectionSpecifier("HydrakinFlap");

    [DataField]
    public EntityUid? CoolOffAction;

    [DataField]
    public float CoolOffCoefficient = 0.9f;
}
