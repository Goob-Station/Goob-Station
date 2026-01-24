using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._Pirate.Contractors.Prototypes;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Preferences;
using Content.Shared.Prototypes;
using Content.Shared.Roles;
using Content.Shared.Traits;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Localization;
using Robust.Shared.Utility; 

namespace Content.Shared.Customization.Systems;

/// <summary>
///     Requires the profile to have one of a list of nationalities
/// </summary>
[UsedImplicitly, Serializable, NetSerializable]
public sealed partial class CharacterNationalityRequirement : JobRequirement
{
    [DataField(required: true)]
    public HashSet<ProtoId<NationalityPrototype>> Nationalities;

    public override bool Check(
        IEntityManager entMan,
        IPrototypeManager protMan,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        if (profile == null)
        {
            reason = FormattedMessage.FromUnformatted(Loc.GetString("requirement-character-profile-not-found"));
            return Inverted; 
        }

        var localeString = "character-nationality-requirement";
        const string color = "green";

        var reasonString = Loc.GetString(
            localeString,
            ("inverted", Inverted),
            ("nationality", $"[color={color}]{string.Join($"[/color], [color={color}]",
                Nationalities.Select(s => Loc.GetString(protMan.Index(s).NameKey)))}[/color]"));

        reason = FormattedMessage.FromMarkup(reasonString);

        var isValid = profile.Nationality != null && Nationalities.Any(o => o == profile.Nationality);
        return Inverted ? !isValid : isValid;
    }
}

/// <summary>
///     Requires the profile to have one of a list of employers
/// </summary>
[UsedImplicitly, Serializable, NetSerializable]
public sealed partial class CharacterEmployerRequirement : JobRequirement
{
    [DataField(required: true)]
    public HashSet<ProtoId<EmployerPrototype>> Employers;

    public override bool Check(
        IEntityManager entMan,
        IPrototypeManager protMan,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        if (profile == null)
        {
            reason = FormattedMessage.FromUnformatted(Loc.GetString("requirement-character-profile-not-found"));
            return Inverted; 
        }

        var localeString = "character-employer-requirement";
        const string color = "green";
        var reasonString = Loc.GetString(
            localeString,
            ("inverted", Inverted),
            ("employers", $"[color={color}]{string.Join($"[/color], [color={color}]",
                Employers.Select(s => Loc.GetString(protMan.Index(s).NameKey)))}[/color]"));

        reason = FormattedMessage.FromMarkup(reasonString);

        var isValid = profile.Employer != null && Employers.Any(o => o == profile.Employer);
        return Inverted ? !isValid : isValid;
    }
}