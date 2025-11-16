// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
//
// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.MisandryBox.Hair;

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

        var mark = IoCManager.Resolve<MarkingManager>();

        // Check if person is bald or has a hairstyle to begin with
        if (profile.Appearance.HairStyleId == Bald || !mark.HasHair(profile.Species))
            return true;

        var saturation = Color.ToHsv(profile.Appearance.HairColor).Y;
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
