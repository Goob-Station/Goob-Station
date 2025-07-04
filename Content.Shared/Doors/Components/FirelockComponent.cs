// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Git-Nivrak <59925169+Git-Nivrak@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 tmtmtl30 <53132901+tmtmtl30@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 vulppine <vulppine@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Guidebook;
using Robust.Shared.GameStates;

namespace Content.Shared.Doors.Components
{
    /// <summary>
    /// Companion component to <see cref="DoorComponent"/> that handles firelock-specific behavior, including
    /// auto-closing on depressurization, air/fire alarm interactions, and preventing normal door functions when
    /// retaining pressure..
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class FirelockComponent : Component
    {
        #region Settings

        /// <summary>
        /// Pry time modifier to be used when the firelock is currently closed due to fire or pressure.
        /// </summary>
        /// <returns></returns>
        [DataField("lockedPryTimeModifier"), ViewVariables(VVAccess.ReadWrite)]
        public float LockedPryTimeModifier = 1.5f;

        /// <summary>
        /// Maximum pressure difference before the firelock will refuse to open, in kPa.
        /// </summary>
        [DataField("pressureThreshold"), ViewVariables(VVAccess.ReadWrite)]
        [GuidebookData]
        public float PressureThreshold = 20;

        /// <summary>
        /// Maximum temperature difference before the firelock will refuse to open, in k.
        /// </summary>
        [DataField("temperatureThreshold"), ViewVariables(VVAccess.ReadWrite)]
        [GuidebookData]
        public float TemperatureThreshold = 330;
        // this used to check for hot-spots, but because accessing that data is a a mess this now just checks
        // temperature. This does mean a cold room will trigger hot-air pop-ups

        /// <summary>
        /// If true, and if this door has an <see cref="AtmosAlarmableComponent"/>, then it will only auto-close if the
        /// alarm is set to danger.
        /// </summary>
        [DataField("alarmAutoClose"), ViewVariables(VVAccess.ReadWrite)]
        public bool AlarmAutoClose = true;

        /// <summary>
        /// The cooldown duration before a firelock can automatically close due to a hazardous environment after it has
        /// been pried open. Measured in seconds.
        /// </summary>
        [DataField]
        public TimeSpan EmergencyCloseCooldownDuration = TimeSpan.FromSeconds(2);

        #endregion

        #region Set by system

        /// <summary>
        /// When the firelock will be allowed to automatically close again due to a hazardous environment.
        /// </summary>
        [DataField]
        public TimeSpan? EmergencyCloseCooldown;

        /// <summary>
        /// Whether the firelock can open, or is locked due to its environment.
        /// </summary>
        public bool IsLocked => Pressure || Temperature;

        /// <summary>
        /// Whether the firelock is holding back a hazardous pressure.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool Pressure;

        /// <summary>
        /// Whether the firelock is holding back extreme temperatures.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool Temperature;

        /// <summary>
        /// Whether the airlock is powered.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool Powered;

        #endregion
    }
}