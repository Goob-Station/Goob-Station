// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Stalen <33173619+stalengd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Localizations;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Preferences;
using Content.Shared.Roles.Jobs;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Roles;

[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class RoleTimeRequirement : JobRequirement
{
    /// <summary>
    /// What particular role they need the time requirement with.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<PlayTimeTrackerPrototype> Role;

    /// <inheritdoc cref="DepartmentTimeRequirement.Time"/>
    [DataField(required: true)]
    public TimeSpan Time;

    public override bool Check(IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        string proto = Role;

        playTimes.TryGetValue(proto, out var roleTime);
        var roleDiffSpan = Time - roleTime;
        var roleDiff = roleDiffSpan.TotalMinutes;
        var formattedRoleDiff = ContentLocalizationManager.FormatPlaytime(roleDiffSpan);
        var departmentColor = Color.Yellow;

        if (!entManager.EntitySysManager.TryGetEntitySystem(out SharedJobSystem? jobSystem))
            return false;

        var jobProto = jobSystem.GetJobPrototype(proto);

        if (jobSystem.TryGetDepartment(jobProto, out var departmentProto))
            departmentColor = departmentProto.Color;

        if (!protoManager.TryIndex<JobPrototype>(jobProto, out var indexedJob))
            return false;

        if (!Inverted)
        {
            if (roleDiff <= 0)
                return true;

            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString(
                "role-timer-role-insufficient",
                ("time", formattedRoleDiff),
                ("job", indexedJob.LocalizedName),
                ("departmentColor", departmentColor.ToHex())));
            return false;
        }

        if (roleDiff <= 0)
        {
            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString(
                "role-timer-role-too-high",
                ("time", formattedRoleDiff),
                ("job", indexedJob.LocalizedName),
                ("departmentColor", departmentColor.ToHex())));
            return false;
        }

        return true;
    }
}