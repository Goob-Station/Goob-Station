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

    // Updated regex patterns to better handle verb conjugation
    private static readonly Regex FirstPersonPronounRegex = new(@"\b(I|me|my|mine|myself)\b", RegexOptions.IgnoreCase);
    private static readonly Regex IVerbPattern = new(@"\bI\s+(\w+)\b", RegexOptions.IgnoreCase);
    private static readonly Regex ICommaPattern = new(@"\bI\s*,", RegexOptions.IgnoreCase);
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
        {"have", "has"},
        {"was", "was"},
        {"'m", "is"},
        {"'ve", "has"}
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

        // Handle "I, ..." cases first
        sentence = ICommaPattern.Replace(sentence, $"{name},");

        // Handle "I verb" patterns
        sentence = IVerbPattern.Replace(sentence, match =>
        {
            if (hasReplacedFirstPronoun)
                return $"{pronoun} {match.Groups[1].Value}";

            hasReplacedFirstPronoun = true;
            var verb = match.Groups[1].Value.ToLower();

            // Check for special verb conjugations
            if (SpecialVerbConjugations.TryGetValue(verb, out var specialConjugation))
                return $"{name} {specialConjugation}";

            // Regular verb conjugation - add 's' unless it's a special case
            if (verb.EndsWith("s") || verb.EndsWith("sh") || verb.EndsWith("ch") || verb.EndsWith("x") || verb.EndsWith("z"))
                return $"{name} {verb}es";
            if (verb.EndsWith("y") && !verb.EndsWith("ay") && !verb.EndsWith("ey") && !verb.EndsWith("oy") && !verb.EndsWith("uy"))
                return $"{name} {verb[..^1]}ies";

            return $"{name} {verb}s";
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
