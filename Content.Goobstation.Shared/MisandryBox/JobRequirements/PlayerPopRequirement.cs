// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Goob Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Shared.GameTicking;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.MisandryBox.JobRequirements;

[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class PlayerPopRequirement : JobRequirement
{
    [DataField(required: true)]
    public int Players;

    public override bool Check(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        if (!entManager.TrySystem<SharedGameTicker>(out var gameTicker))
            return true;

        var playerCount = gameTicker.GetActivePlayerCount();

        if (!Inverted)
        {
            if (playerCount >= Players)
                return true;

            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString(
                "role-timer-players-insufficient",
                ("players", Players)));
            return false;
        }

        if (playerCount < Players)
            return true;

        reason = FormattedMessage.FromMarkupPermissive(Loc.GetString(
            "role-timer-players-too-high",
            ("players", Players)));
        return false;
    }
}
