using System.Diagnostics.CodeAnalysis;
using Content.Shared._CorvaxGoob.Players;
using Content.Shared.Localizations;
using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._CorvaxGoob.Roles.JobRequirement;

[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class GhostTimeRequirement : Shared.Roles.JobRequirement
{
    [DataField(required: true)]
    public TimeSpan Time;

    public override bool Check(IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        var ghostTime = playTimes.GetValueOrDefault(GhostTimeTrackingShared.GhostTime);
        var overallDiffSpan = Time - ghostTime;
        var ghostDiff = overallDiffSpan.TotalMinutes;
        var formattedOverallDiff = ContentLocalizationManager.FormatPlaytime(overallDiffSpan);

        if (!Inverted)
        {
            if (ghostDiff <= 0 || ghostTime >= Time)
                return true;

            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString(
                "ghost-timer-insufficient",
                ("time", formattedOverallDiff)));
            return false;
        }

        if (ghostDiff <= 0 || ghostTime >= Time)
        {
            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString("ghost-timer-too-high",
                ("time", formattedOverallDiff)));
            return false;
        }

        return true;
    }
}
