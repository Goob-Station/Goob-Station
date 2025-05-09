// SPDX-FileCopyrightText: 2024 Firewatch <54725557+musicmanvr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <koolthunder019@gmail.com>
// SPDX-FileCopyrightText: 2024 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

/// <summary>
/// Uses a <see cref="LoadoutEffectGroupPrototype"/> prototype as a singular effect that can be re-used.
/// </summary>
public sealed partial class GroupLoadoutEffect : LoadoutEffect
{
    [DataField(required: true)]
    public ProtoId<LoadoutEffectGroupPrototype> Proto;

    public override bool Validate(HumanoidCharacterProfile profile, RoleLoadout loadout, ICommonSession? session, IDependencyCollection collection, [NotNullWhen(false)] out FormattedMessage? reason)
    {
        var effectsProto = collection.Resolve<IPrototypeManager>().Index(Proto);

        var reasons = new List<string>();
        foreach (var effect in effectsProto.Effects)
        {
            if (effect.Validate(profile, loadout, session, collection, out reason))
                continue;

            reasons.Add(reason.ToMarkup());
        }

        reason = reasons.Count == 0 ? null : FormattedMessage.FromMarkupOrThrow(string.Join('\n', reasons));
        return reason == null;
    }
}