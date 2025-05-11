// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Shared.MisandryBox.Helpers;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.MisandryBox.Saturation;

/// <summary>
/// Requires the character to have hair color of less saturation than required for this job
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class SaturationRequirement : JobRequirement
{
    public const string Bald = "HairBald";

    [DataField(required: true)]
    public double HairColorSaturation { get; set; }

    public override bool Check(IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        if (profile is null)
            return true;

        if (profile.Appearance.HairStyleId == Bald)
            return true;

        var saturation = profile.Appearance.HairColor.ToHSV().S;
        var failed = Inverted
            ? HairColorSaturation > saturation
            : HairColorSaturation < saturation;

        if (!failed)
            return true;

        var messageKey = Inverted
            ? "role-timer-hair-not-neon"
            : "role-timer-hair-too-neon";

        reason = FormattedMessage.FromMarkupPermissive(
            Loc.GetString(messageKey, ("threshold", HairColorSaturation * 100)));

        return false;
    }
}
