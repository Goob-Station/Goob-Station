using System.Text.RegularExpressions;
using Content.Server._ShibaStation.Speech.Components;
using Content.Server.Speech;
using Content.Shared.Humanoid;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Enums;

namespace Content.Server._ShibaStation.Speech.EntitySystems;

public sealed class IlleismAccentSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    // Updated regex patterns to better handle verb conjugation and contractions
    private static readonly Regex FirstPersonPronounRegex = new(@"\b(I|me|my|mine|myself)\b", RegexOptions.IgnoreCase);
    private static readonly Regex IVerbPattern = new(@"\bI\s+(\w+(?:n't)?(?:\s+\w+)?)\b", RegexOptions.IgnoreCase);
    private static readonly Regex ICommaPattern = new(@"\bI\s*,", RegexOptions.IgnoreCase);
    private static readonly Regex ImPattern = new(@"\bI'm\b", RegexOptions.IgnoreCase);
    private static readonly Regex SentenceSplitRegex = new(@"(?<=[.!?])\s+");

    // Helper dictionary for special verb cases
    private static readonly Dictionary<string, string> SpecialVerbConjugations = new(StringComparer.OrdinalIgnoreCase)
    {
        // Modal verbs and auxiliaries (no change)
        {"am", "is"},
        {"can", "can"},
        {"will", "will"},
        {"shall", "shall"},
        {"may", "may"},
        {"might", "might"},
        {"must", "must"},
        {"would", "would"},
        {"could", "could"},
        {"should", "should"},
        // Special conjugations
        {"do", "does"},
        {"don't", "doesn't"},
        {"have", "has"},
        {"haven't", "hasn't"},
        {"was", "was"},
        {"wasn't", "wasn't"},
        // Common verb phrases
        {"want to", "wants to"},
        {"need to", "needs to"},
        {"have to", "has to"},
        {"got to", "got to"},
        {"going to", "going to"}
    };

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
        // Get the character's name and pronouns
        string name = "Unknown";
        string pronoun = "they";
        string possessivePronoun = "their";
        string reflexivePronoun = "themselves";

        if (_entityManager.TryGetComponent<HumanoidAppearanceComponent>(uid, out var humanoid))
        {
            var fullName = _entityManager.GetComponent<MetaDataComponent>(uid).EntityName;

            // Get the first part of the name, whether it's hyphenated or space-separated
            name = fullName.Split(new[] { '-', ' ' }, 2)[0];

            // Set pronouns based on chosen pronouns (Gender)
            switch (humanoid.Gender)
            {
                case Gender.Male:
                    pronoun = "he";
                    possessivePronoun = "his";
                    reflexivePronoun = "himself";
                    break;
                case Gender.Female:
                    pronoun = "she";
                    possessivePronoun = "her";
                    reflexivePronoun = "herself";
                    break;
                case Gender.Neuter:
                    pronoun = "it";
                    possessivePronoun = "its";
                    reflexivePronoun = "itself";
                    break;
                case Gender.Epicene:
                default:
                    pronoun = "they";
                    possessivePronoun = "their";
                    reflexivePronoun = "themselves";
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

            var processedSentence = ProcessSentence(sentence, name, pronoun, possessivePronoun, reflexivePronoun);
            processedSentences.Add(processedSentence);
        }

        return string.Join(" ", processedSentences);
    }

    private string ProcessSentence(string sentence, string name, string pronoun, string possessivePronoun, string reflexivePronoun)
    {
        bool hasReplacedFirstPronoun = false;

        // Handle "I'm" cases first
        sentence = ImPattern.Replace(sentence, match =>
        {
            if (hasReplacedFirstPronoun)
                return $"{pronoun} is";

            hasReplacedFirstPronoun = true;
            return $"{name} is";
        });

        // Handle "I, ..." cases
        sentence = ICommaPattern.Replace(sentence, match =>
        {
            if (hasReplacedFirstPronoun)
                return $"{pronoun},";

            hasReplacedFirstPronoun = true;
            return $"{name},";
        });

        // Handle "I verb" patterns
        sentence = IVerbPattern.Replace(sentence, match =>
        {
            if (hasReplacedFirstPronoun)
                return $"{pronoun} {match.Groups[1].Value}";

            hasReplacedFirstPronoun = true;
            var verbPhrase = match.Groups[1].Value.ToLower();

            // Check for special verb conjugations first
            if (SpecialVerbConjugations.TryGetValue(verbPhrase, out var specialConjugation))
                return $"{name} {specialConjugation}";

            // Handle negations and special endings
            if (verbPhrase.EndsWith("n't"))
            {
                var baseVerb = verbPhrase[..^3];
                if (SpecialVerbConjugations.TryGetValue(baseVerb, out var baseConjugation))
                    return $"{name} {baseConjugation}n't";
                return $"{name} {baseVerb}sn't";
            }

            // Regular verb conjugation - add 's' unless it's a special case
            if (verbPhrase.EndsWith("s") || verbPhrase.EndsWith("sh") || verbPhrase.EndsWith("ch") || verbPhrase.EndsWith("x") || verbPhrase.EndsWith("z"))
                return $"{name} {verbPhrase}es";
            if (verbPhrase.EndsWith("y") && !verbPhrase.EndsWith("ay") && !verbPhrase.EndsWith("ey") && !verbPhrase.EndsWith("oy") && !verbPhrase.EndsWith("uy"))
                return $"{name} {verbPhrase[..^1]}ies";

            return $"{name} {verbPhrase}s";
        });

        // Handle remaining pronouns
        sentence = FirstPersonPronounRegex.Replace(sentence, match =>
        {
            if (!hasReplacedFirstPronoun)
            {
                hasReplacedFirstPronoun = true;
                return match.Value.ToLower() switch
                {
                    "my" => name + "'s",
                    "mine" => name + "'s",
                    _ => name
                };
            }

            return match.Value.ToLower() switch
            {
                "i" => pronoun,
                "me" => pronoun,
                "my" => possessivePronoun,
                "mine" => possessivePronoun,
                "myself" => reflexivePronoun,
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
