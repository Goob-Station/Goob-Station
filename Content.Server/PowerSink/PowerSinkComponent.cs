// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Skye <22365940+Skyedra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.PowerSink
{
    /// <summary>
    /// Absorbs power up to its capacity when anchored then explodes.
    /// </summary>
    [RegisterComponent, AutoGenerateComponentPause]
    public sealed partial class PowerSinkComponent : Component
    {
        /// <summary>
        /// When the power sink is nearing its explosion, warn the crew so they can look for it
        /// (if they're not already).
        /// </summary>
        [DataField("sentImminentExplosionWarning")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool SentImminentExplosionWarningMessage = false;

        /// <summary>
        /// If explosion has been triggered, time at which to explode.
        /// </summary>
        [DataField("explosionTime", customTypeSerializer:typeof(TimeOffsetSerializer))]
        [AutoPausedField]
        public System.TimeSpan? ExplosionTime = null;

        /// <summary>
        /// The highest sound warning threshold that has been hit (plays sfx occasionally as explosion nears)
        /// </summary>
        [DataField("highestWarningSoundThreshold")]
        [ViewVariables(VVAccess.ReadWrite)]
        public float HighestWarningSoundThreshold = 0f;

        [DataField("chargeFireSound")]
        public SoundSpecifier ChargeFireSound = new SoundPathSpecifier("/Audio/Effects/PowerSink/charge_fire.ogg");

        [DataField("electricSound")] public SoundSpecifier ElectricSound =
            new SoundPathSpecifier("/Audio/Effects/PowerSink/electric.ogg")
            {
                Params = AudioParams.Default
                    .WithVolume(15f) // audible even behind walls
                    .WithRolloffFactor(10)
            };
    }
}