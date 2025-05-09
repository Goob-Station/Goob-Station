// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ray <vigersray@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 no <165581243+pissdemon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Eui;
using Content.Shared.Roles;
using Robust.Shared.Serialization;

namespace Content.Shared.Ghost.Roles
{
    [NetSerializable, Serializable]
    public struct GhostRoleInfo
    {
        public uint Identifier { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Rules { get; set; }

        // TODO ROLE TIMERS
        // Actually make use of / enforce this requirement?
        // Why is this even here.
        // Move to ghost role prototype & respect CCvars.GameRoleTimerOverride
        public HashSet<JobRequirement>? Requirements { get; set; }

        /// <inheritdoc cref="GhostRoleKind"/>
        public GhostRoleKind Kind { get; set; }

        /// <summary>
        /// if <see cref="Kind"/> is <see cref="GhostRoleKind.RaffleInProgress"/>, specifies how many players are currently
        /// in the raffle for this role.
        /// </summary>
        public uint RafflePlayerCount { get; set; }

        /// <summary>
        /// if <see cref="Kind"/> is <see cref="GhostRoleKind.RaffleInProgress"/>, specifies when raffle finishes.
        /// </summary>
        public TimeSpan RaffleEndTime { get; set; }

    }

    [NetSerializable, Serializable]
    public sealed class GhostRolesEuiState : EuiStateBase
    {
        public GhostRoleInfo[] GhostRoles { get; }

        public GhostRolesEuiState(GhostRoleInfo[] ghostRoles)
        {
            GhostRoles = ghostRoles;
        }
    }

    [NetSerializable, Serializable]
    public sealed class RequestGhostRoleMessage : EuiMessageBase
    {
        public uint Identifier { get; }

        public RequestGhostRoleMessage(uint identifier)
        {
            Identifier = identifier;
        }
    }

    [NetSerializable, Serializable]
    public sealed class FollowGhostRoleMessage : EuiMessageBase
    {
        public uint Identifier { get; }

        public FollowGhostRoleMessage(uint identifier)
        {
            Identifier = identifier;
        }
    }

    [NetSerializable, Serializable]
    public sealed class LeaveGhostRoleRaffleMessage : EuiMessageBase
    {
        public uint Identifier { get; }

        public LeaveGhostRoleRaffleMessage(uint identifier)
        {
            Identifier = identifier;
        }
    }

    /// <summary>
    /// Determines whether a ghost role is a raffle role, and if it is, whether it's running.
    /// </summary>
    [NetSerializable, Serializable]
    public enum GhostRoleKind
    {
        /// <summary>
        /// Role is not a raffle role and can be taken immediately.
        /// </summary>
        FirstComeFirstServe,

        /// <summary>
        /// Role is a raffle role, but raffle hasn't started yet.
        /// </summary>
        RaffleReady,

        /// <summary>
        ///  Role is raffle role and currently being raffled, but player hasn't joined raffle.
        /// </summary>
        RaffleInProgress,

        /// <summary>
        /// Role is raffle role and currently being raffled, and player joined raffle.
        /// </summary>
        RaffleJoined
    }
}