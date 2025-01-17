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
    private static readonly Regex IVerbPattern = new(@"\bI\s+(\w+(?:\s+\w+)?)\b", RegexOptions.IgnoreCase);
    private static readonly Regex ICommaPattern = new(@"\bI\s*,", RegexOptions.IgnoreCase);
    private static readonly Regex ImPattern = new(@"\bI'm\b", RegexOptions.IgnoreCase);
    private static readonly Regex IDontPattern = new(@"\bI don't\b", RegexOptions.IgnoreCase);
    private static readonly Regex IWithContractionPattern = new(@"\bI\s+(\w+'t)\b", RegexOptions.IgnoreCase);
    private static readonly Regex SentenceSplitRegex = new(@"(?<=[.!?])\s+");
    private static readonly Regex PrepositionPattern = new(@"\b(?:to|for|with|by|at|from|in|on|under|over|of)\s+(me)\b", RegexOptions.IgnoreCase);

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
        string subjectPronoun = "they";    // for "I" cases
        string objectPronoun = "them";      // for "me" cases after prepositions
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
                    subjectPronoun = "he";
                    objectPronoun = "him";
                    possessivePronoun = "his";
                    reflexivePronoun = "himself";
                    break;
                case Gender.Female:
                    subjectPronoun = "she";
                    objectPronoun = "her";
                    possessivePronoun = "her";
                    reflexivePronoun = "herself";
                    break;
                case Gender.Neuter:
                    subjectPronoun = "it";
                    objectPronoun = "it";
                    possessivePronoun = "its";
                    reflexivePronoun = "itself";
                    break;
                case Gender.Epicene:
                default:
                    subjectPronoun = "they";
                    objectPronoun = "them";
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

            var processedSentence = ProcessSentence(sentence, name, subjectPronoun, objectPronoun, possessivePronoun, reflexivePronoun);
            processedSentences.Add(processedSentence);
        }

        return string.Join(" ", processedSentences);
    }

    private string ProcessSentence(string sentence, string name, string subjectPronoun, string objectPronoun, string possessivePronoun, string reflexivePronoun)
    {
        bool hasReplacedFirstPronoun = false;

        // Handle preposition + "me" cases first
        sentence = PrepositionPattern.Replace(sentence, match =>
        {
            var preposition = match.Value.Substring(0, match.Value.Length - 2).TrimEnd(); // Remove "me" and get preposition
            return $"{preposition} {(hasReplacedFirstPronoun ? objectPronoun : name)}";
        });

        // Handle common contractions
        sentence = ImPattern.Replace(sentence, match =>
        {
            if (hasReplacedFirstPronoun)
                return $"{subjectPronoun} is";

            hasReplacedFirstPronoun = true;
            return $"{name} is";
        });

        // Handle "don't" as a special case
        sentence = IDontPattern.Replace(sentence, match =>
        {
            if (hasReplacedFirstPronoun)
                return $"{subjectPronoun} doesn't";

            hasReplacedFirstPronoun = true;
            return $"{name} doesn't";
        });

        // Handle all other "'t" contractions (won't, can't, etc.) - these don't change form
        sentence = IWithContractionPattern.Replace(sentence, match =>
        {
            // Skip if it's "don't" since we handled it above
            if (match.Groups[1].Value.ToLower() == "don't")
                return match.Value;

            if (hasReplacedFirstPronoun)
                return $"{subjectPronoun} {match.Groups[1].Value}";

            hasReplacedFirstPronoun = true;
            return $"{name} {match.Groups[1].Value}";
        });

        // Handle "I, ..." cases
        sentence = ICommaPattern.Replace(sentence, match =>
        {
            if (hasReplacedFirstPronoun)
                return $"{subjectPronoun},";

            hasReplacedFirstPronoun = true;
            return $"{name},";
        });

        // Handle "I verb" patterns
        sentence = IVerbPattern.Replace(sentence, match =>
        {
            if (hasReplacedFirstPronoun)
                return $"{subjectPronoun} {match.Groups[1].Value}";

            hasReplacedFirstPronoun = true;
            var verbPhrase = match.Groups[1].Value.ToLower();

            // Check for special verb conjugations first
            if (SpecialVerbConjugations.TryGetValue(verbPhrase, out var specialConjugation))
                return $"{name} {specialConjugation}";

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
                    "me" => name,
                    _ => name
                };
            }

            return match.Value.ToLower() switch
            {
                "i" => subjectPronoun,
                "me" => objectPronoun,
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
