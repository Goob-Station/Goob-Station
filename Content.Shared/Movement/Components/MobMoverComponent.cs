// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared.Movement.Components
{
    /// <summary>
    /// Has additional movement data such as footsteps and weightless grab range for an entity.
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class MobMoverComponent : Component
    {
        private float _stepSoundDistance;
        [DataField] public float GrabRange = 1.0f;

        [DataField] public float PushStrength = 600f;

        [DataField, AutoNetworkedField]
        public float StepSoundMoveDistanceRunning = 2;

        [DataField, AutoNetworkedField]
        public float StepSoundMoveDistanceWalking = 1.5f;

        [DataField, AutoNetworkedField]
        public float FootstepVariation;

        [ViewVariables(VVAccess.ReadWrite)]
        public EntityCoordinates LastPosition { get; set; }

        /// <summary>
        ///     Used to keep track of how far we have moved before playing a step sound
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public float StepSoundDistance
        {
            get => _stepSoundDistance;
            set
            {
                if (MathHelper.CloseToPercent(_stepSoundDistance, value)) return;
                _stepSoundDistance = value;
            }
        }

        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float GrabRangeVV
        {
            get => GrabRange;
            set
            {
                if (MathHelper.CloseToPercent(GrabRange, value)) return;
                GrabRange = value;
                Dirty();
            }
        }

        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float PushStrengthVV
        {
            get => PushStrength;
            set
            {
                if (MathHelper.CloseToPercent(PushStrength, value)) return;
                PushStrength = value;
                Dirty();
            }
        }
    }
}