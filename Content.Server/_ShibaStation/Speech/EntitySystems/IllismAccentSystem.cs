using System.Text.RegularExpressions;
using Content.Server._ShibaStation.Speech.Components;
using Content.Server.Speech;
using Content.Shared.Humanoid;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Server._ShibaStation.Speech.EntitySystems;

public sealed class IlleismAccentSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private static readonly Regex FirstPersonPronounRegex = new(@"\b(I|me|my|mine|myself)\b", RegexOptions.IgnoreCase);
    private static readonly Regex FirstPersonVerbRegex = new(@"\b(am|'m|have|'ve|was)\b", RegexOptions.IgnoreCase);
    private static readonly Regex SentenceSplitRegex = new(@"(?<=[.!?])\s+");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IlleismAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, IlleismAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(uid, args.Message);
    }

    public string Accentuate(EntityUid uid, string message)
    {
        // Get the character's name and gender
        string name = "Unknown";
        string pronoun = "they";
        string possessivePronoun = "their";
        string reflexivePronoun = "themselves";
        string verbSuffix = "";

        if (_entityManager.TryGetComponent<HumanoidAppearanceComponent>(uid, out var humanoid))
        {
            name = _entityManager.GetComponent<MetaDataComponent>(uid).EntityName;

            // Get only the first part of hyphenated names
            if (name.Contains('-'))
            {
                name = name.Split('-')[0];
            }

            // Set pronouns based on gender
            switch (humanoid.Sex)
            {
                case Sex.Male:
                    pronoun = "he";
                    possessivePronoun = "his";
                    reflexivePronoun = "himself";
                    verbSuffix = "s";
                    break;
                case Sex.Female:
                    pronoun = "she";
                    possessivePronoun = "her";
                    reflexivePronoun = "herself";
                    verbSuffix = "s";
                    break;
                default:
                    pronoun = "they";
                    possessivePronoun = "their";
                    reflexivePronoun = "themselves";
                    verbSuffix = "";
                    break;
            }
        }

        // Split into sentences and process each one
        var sentences = SentenceSplitRegex.Split(message);
        var processedSentences = new List<string>();

        foreach (var sentence in sentences)
        {
            if (string.IsNullOrWhiteSpace(sentence))
                continue;

            var processedSentence = ProcessSentence(sentence, name, pronoun, possessivePronoun, reflexivePronoun, verbSuffix);
            processedSentences.Add(processedSentence);
        }

        return string.Join(" ", processedSentences);
    }

    private string ProcessSentence(string sentence, string name, string pronoun, string possessivePronoun, string reflexivePronoun, string verbSuffix)
    {
        bool hasUsedName = false;

        // Replace first person pronouns
        sentence = FirstPersonPronounRegex.Replace(sentence, match =>
        {
            var replacement = match.Value.ToLower() switch
            {
                "i" => hasUsedName ? pronoun : name,
                "me" => pronoun,
                "my" => possessivePronoun,
                "mine" => possessivePronoun,
                "myself" => reflexivePronoun,
                _ => match.Value
            };

            if (replacement == name)
                hasUsedName = true;

            return replacement;
        });

        // Replace first person verbs, ensuring proper spacing
        sentence = FirstPersonVerbRegex.Replace(sentence, match =>
        {
            return match.Value.ToLower() switch
            {
                "am" => " is",
                "'m" => " is",
                "have" => $" ha{verbSuffix}",
                "'ve" => $" ha{verbSuffix}",
                "was" => " was",
                _ => match.Value
            };
        });

        // Clean up any double spaces that might have been created
        sentence = Regex.Replace(sentence, @"\s+", " ").Trim();

        // Ensure the first character is uppercase
        if (sentence.Length > 0)
        {
            sentence = char.ToUpper(sentence[0]) + sentence.Substring(1);
        }

        return sentence;
    }
}
