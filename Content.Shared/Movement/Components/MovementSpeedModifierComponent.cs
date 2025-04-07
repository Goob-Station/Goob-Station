// SPDX-FileCopyrightText: 2020 4dplanner <3combined@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 KISS <59531932+YuriyKiss@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components
{
    /// <summary>
    /// Applies basic movement speed and movement modifiers for an entity.
    /// If this is not present on the entity then they will use defaults for movement.
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    [Access(typeof(MovementSpeedModifierSystem))]
    public sealed partial class MovementSpeedModifierComponent : Component
    {
        // Weightless
        public const float DefaultMinimumFrictionSpeed = 0.005f;
        public const float DefaultWeightlessFriction = 1f;
        public const float DefaultWeightlessFrictionNoInput = 0f;
        public const float DefaultOffGridFriction = 0f;
        public const float DefaultWeightlessModifier = 0.7f;
        public const float DefaultWeightlessAcceleration = 1f;

        public const float DefaultAcceleration = 20f;
        public const float DefaultFriction = 20f;
        public const float DefaultFrictionNoInput = 20f;

        public const float DefaultBaseWalkSpeed = 2.5f;
        public const float DefaultBaseSprintSpeed = 4.5f;

        [AutoNetworkedField, ViewVariables]
        public float WalkSpeedModifier = 1.0f;

        [AutoNetworkedField, ViewVariables]
        public float SprintSpeedModifier = 1.0f;

        [ViewVariables(VVAccess.ReadWrite)]
        private float _baseWalkSpeedVV
        {
            get => BaseWalkSpeed;
            set
            {
                BaseWalkSpeed = value;
                Dirty();
            }
        }

        [ViewVariables(VVAccess.ReadWrite)]
        private float _baseSprintSpeedVV
        {
            get => BaseSprintSpeed;
            set
            {
                BaseSprintSpeed = value;
                Dirty();
            }
        }

        /// <summary>
        /// Minimum speed a mob has to be moving before applying movement friction.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField]
        public float MinimumFrictionSpeed = DefaultMinimumFrictionSpeed;

        /// <summary>
        /// The negative velocity applied for friction when weightless and providing inputs.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField]
        public float WeightlessFriction = DefaultWeightlessFriction;

        /// <summary>
        /// The negative velocity applied for friction when weightless and not providing inputs.
        /// This is essentially how much their speed decreases per second.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField]
        public float WeightlessFrictionNoInput = DefaultWeightlessFrictionNoInput;

        /// <summary>
        /// The negative velocity applied for friction when weightless and not standing on a grid or mapgrid
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField]
        public float OffGridFriction = DefaultOffGridFriction;

        /// <summary>
        /// The movement speed modifier applied to a mob's total input velocity when weightless.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField]
        public float WeightlessModifier = DefaultWeightlessModifier;

        /// <summary>
        /// The acceleration applied to mobs when moving and weightless.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField]
        public float WeightlessAcceleration = DefaultWeightlessAcceleration;

        /// <summary>
        /// The acceleration applied to mobs when moving.
        /// </summary>
        [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite), DataField]
        public float Acceleration = DefaultAcceleration;

        /// <summary>
        /// The negative velocity applied for friction.
        /// </summary>
        [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite), DataField]
        public float Friction = DefaultFriction;

        /// <summary>
        /// The negative velocity applied for friction.
        /// </summary>
        [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite), DataField]
        public float? FrictionNoInput;

        [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
        public float BaseWalkSpeed { get; set; } = DefaultBaseWalkSpeed;

        [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
        public float BaseSprintSpeed { get; set; } = DefaultBaseSprintSpeed;

        [ViewVariables]
        public float CurrentWalkSpeed => WalkSpeedModifier * BaseWalkSpeed;
        [ViewVariables]
        public float CurrentSprintSpeed => SprintSpeedModifier * BaseSprintSpeed;
    }
}