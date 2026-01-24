using Content.Shared.Preferences;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Localization;
using Content.Shared._Pirate.Contractors.Prototypes;
using Content.Shared.Humanoid.Prototypes;

namespace Content.Shared.Preferences.Loadouts.Effects;

// --- 1. CharacterNationalityRequirement ---
[DataDefinition]
public sealed partial class CharacterNationalityRequirement : LoadoutEffect
{
    [DataField(required: true)]
    public List<string> Nationalities = new();

    [DataField]
    public bool Inverted { get; private set; } = false;

    public override bool Validate(
        HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        ICommonSession? session,
        IDependencyCollection collection,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        var protoManager = collection.Resolve<IPrototypeManager>();
        const string localeString = "character-nationality-requirement";
        const string color = "green";

        var isValid = Nationalities.Contains(profile.Nationality);
        var shouldFail = Inverted ? isValid : (profile.Nationality == string.Empty || !isValid);

        if (shouldFail)
        {
            var reasonString = Loc.GetString(
                localeString,
                ("inverted", Inverted),
                ("nationality", $"[color={color}]{string.Join($"[/color], [color={color}]",
                    Nationalities.Select(id =>
                        protoManager.TryIndex<NationalityPrototype>(id, out var proto)
                            ? Loc.GetString(proto.NameKey)
                            : id))}[/color]"));

            reason = FormattedMessage.FromMarkup(reasonString);
            return false;
        }

        return true;
    }

    public override void Apply(RoleLoadout loadout) {}
}

// --- 2. CharacterEmployerRequirement ---
[DataDefinition]
public sealed partial class CharacterEmployerRequirement : LoadoutEffect
{
    [DataField(required: true)]
    public List<string> Employers = new();

    [DataField]
    public bool Inverted { get; private set; } = false;

    public override bool Validate(
        HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        ICommonSession? session,
        IDependencyCollection collection,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        var protoManager = collection.Resolve<IPrototypeManager>();
        const string localeString = "character-employer-requirement";
        const string color = "green";

        var isValid = Employers.Contains(profile.Employer);
        var shouldFail = Inverted ? isValid : (profile.Employer == string.Empty || !isValid);

        if (shouldFail)
        {
            var reasonString = Loc.GetString(
                localeString,
                ("inverted", Inverted),
                ("employers", $"[color={color}]{string.Join($"[/color], [color={color}]",
                    Employers.Select(id =>
                        protoManager.TryIndex<EmployerPrototype>(id, out var proto)
                            ? Loc.GetString(proto.NameKey)
                            : id))}[/color]"));

            reason = FormattedMessage.FromMarkup(reasonString);
            return false;
        }

        return true;
    }

    public override void Apply(RoleLoadout loadout) {}
}