// SPDX-FileCopyrightText: 2024 Firewatch <54725557+musicmanvr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <koolthunder019@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

public sealed partial class PointsCostLoadoutEffect : LoadoutEffect
{
    [DataField(required: true)]
    public int Cost = 1;

    public override bool Validate(
        HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        ICommonSession? session,
        IDependencyCollection collection,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;
        var protoManager = collection.Resolve<IPrototypeManager>();

        if (!protoManager.TryIndex(loadout.Role, out var roleProto) || roleProto.Points == null)
        {
            return true;
        }

        if (loadout.Points <= Cost)
        {
            reason = FormattedMessage.FromUnformatted("loadout-group-points-insufficient");
            return false;
        }

        return true;
    }

    public override void Apply(RoleLoadout loadout)
    {
        loadout.Points -= Cost;
    }
}