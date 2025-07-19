// SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Stunnable.Components
{
    /// <summary>
    /// Adds stun when it collides with an entity
    /// </summary>
    [RegisterComponent, Access(typeof(StunOnCollideSystem))]
    public sealed partial class StunOnCollideComponent : Component
    {
        // TODO: Can probably predict this.

        /// <summary>
        /// How long we are stunned for
        /// </summary>
        [DataField]
        public TimeSpan StunAmount;

        /// <summary>
        /// How long we are knocked down for
        /// </summary>
        [DataField]
        public TimeSpan KnockdownAmount;

        /// <summary>
        /// How long we are slowed down for
        /// </summary>
        [DataField]
        public TimeSpan SlowdownAmount;

        /// <summary>
        /// Multiplier for a mob's walking speed
        /// </summary>
        [DataField]
        public float WalkSpeedModifier = 1f;

        /// <summary>
        /// Multiplier for a mob's sprinting speed
        /// </summary>
        [DataField]
        public float SprintSpeedModifier = 1f;

        /// <summary>
        /// Refresh Stun or Slowdown on hit
        /// </summary>
        [DataField]
        public bool Refresh = true;

        /// <summary>
        /// Should the entity try and stand automatically after being knocked down?
        /// </summary>
        [DataField]
        public bool AutoStand = true;

        /// <summary>
        /// Fixture we track for the collision.
        /// </summary>
        [DataField("fixture")] public string FixtureID = "projectile";
    }
}