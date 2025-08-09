// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.MisandryBox.Hair;

public sealed partial class StyleRequirement : JobRequirement
{
    private MarkingManager? _mark;

    [DataField(required: true)]
    public HashSet<ProtoId<MarkingPrototype>> Styles;

    public override bool Check(IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        if (profile is null)
            return true;

        _mark ??= IoCManager.Resolve<MarkingManager>();

        if (!_mark.HasHair(profile.Species))
            return true;

        var sb = new StringBuilder();

        var haircolor = profile.Appearance.HairColor.ToHex();

        sb.Append($"[color={haircolor}]");

        foreach (var s in Styles)
        {
            sb.Append("\n" + Loc.GetString($"marking-{s.Id}"));
        }

        var style = profile.Appearance.HairStyleId;

        if (!Inverted)
        {
            reason = FormattedMessage.FromMarkupPermissive($"{Loc.GetString("role-timer-blacklisted-hair")}{sb}");

            if (Styles.Any(e => e.Id == style))
                return false;
        }
        else
        {
            reason = FormattedMessage.FromMarkupPermissive($"{Loc.GetString("role-timer-whitelisted-hair")}{sb}");

            if (Styles.All(e => e.Id != style))
                return false;
        }

        return true;
    }
}
