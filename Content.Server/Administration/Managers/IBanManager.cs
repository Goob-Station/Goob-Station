// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Net;
using System.Threading.Tasks;
using Content.Shared.Database;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Administration.Managers;

public interface IBanManager
{
    public void Initialize();
    public void Restart();

    /// <summary>
    /// Bans the specified target, address range and / or HWID. One of them must be non-null
    /// </summary>
    /// <param name="target">Target user, username or GUID, null for none</param>
    /// <param name="banningAdmin">The person who banned our target</param>
    /// <param name="addressRange">Address range, null for none</param>
    /// <param name="hwid">H</param>
    /// <param name="minutes">Number of minutes to ban for. 0 and null mean permanent</param>
    /// <param name="severity">Severity of the resulting ban note</param>
    /// <param name="reason">Reason for the ban</param>
    public void CreateServerBan(NetUserId? target, string? targetUsername, NetUserId? banningAdmin, (IPAddress, int)? addressRange, ImmutableTypedHwid? hwid, uint? minutes, NoteSeverity severity, string reason);
    public HashSet<string>? GetRoleBans(NetUserId playerUserId);
    public HashSet<ProtoId<JobPrototype>>? GetJobBans(NetUserId playerUserId);

    /// <summary>
    /// Creates a job ban for the specified target, username or GUID
    /// </summary>
    /// <param name="target">Target user, username or GUID, null for none</param>
    /// <param name="role">Role to be banned from</param>
    /// <param name="severity">Severity of the resulting ban note</param>
    /// <param name="reason">Reason for the ban</param>
    /// <param name="minutes">Number of minutes to ban for. 0 and null mean permanent</param>
    /// <param name="timeOfBan">Time when the ban was applied, used for grouping role bans</param>
    public void CreateRoleBan(NetUserId? target, string? targetUsername, NetUserId? banningAdmin, (IPAddress, int)? addressRange, ImmutableTypedHwid? hwid, string role, uint? minutes, NoteSeverity severity, string reason, DateTimeOffset timeOfBan);

    /// <summary>
    /// Pardons a role ban for the specified target, username or GUID
    /// </summary>
    /// <param name="banId">The id of the role ban to pardon.</param>
    /// <param name="unbanningAdmin">The admin, if any, that pardoned the role ban.</param>
    /// <param name="unbanTime">The time at which this role ban was pardoned.</param>
    public Task<string> PardonRoleBan(int banId, NetUserId? unbanningAdmin, DateTimeOffset unbanTime);

    /// <summary>
    /// Sends role bans to the target
    /// </summary>
    /// <param name="pSession">Player's session</param>
    public void SendRoleBans(ICommonSession pSession);
}