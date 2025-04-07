// SPDX-FileCopyrightText: 2020 L.E.D <10257081+unusualcrow@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Remie Richards <remierichards@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 unusualcrow <unusualcrow@protonmail.com>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Saphire <lattice@saphi.re>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

using Content.Shared.Physics;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.GameStates;

namespace Content.Shared.Singularity.Components;

[RegisterComponent, AutoGenerateComponentPause, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SingularityGeneratorComponent : Component
{
    /// <summary>
    /// The amount of power this generator has accumulated.
    /// If you want to set this use <see  cref="SingularityGeneratorSystem.SetPower"/>
    /// </summary>
    [DataField]
    public float Power = 0;

    /// <summary>
    /// The power threshold at which this generator will spawn a singularity.
    /// If you want to set this use <see  cref="SingularityGeneratorSystem.SetThreshold"/>
    /// </summary>
    [DataField]
    public float Threshold = 16;

    /// <summary>
    /// Allows the generator to ignore all the failsafe stuff, e.g. when emagged
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool FailsafeDisabled = false;

    /// <summary>
    /// Maximum distance at which the generator will check for a field at
    /// </summary>
    [DataField]
    public float FailsafeDistance = 16;

    /// <summary>
    ///     The prototype ID used to spawn a singularity.
    /// </summary>
    [DataField("spawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? SpawnPrototype = "Singularity";

    /// <summary>
    /// The masks the raycast should not go through
    /// </summary>
    [DataField]
    public int CollisionMask = (int)CollisionGroup.FullTileMask;

    /// <summary>
    /// Message to use when there's no containment field on cardinal directions
    /// </summary>
    [DataField]
    public LocId ContainmentFailsafeMessage = "comp-generator-failsafe";

    /// <summary>
    /// For how long the failsafe will cause the generator to stop working and not issue a failsafe warning
    /// </summary>
    [DataField]
    public TimeSpan FailsafeCooldown = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How long until the generator can issue a failsafe warning again
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextFailsafe = TimeSpan.Zero;
}