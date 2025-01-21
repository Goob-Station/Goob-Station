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
    private static readonly Regex FirstPersonPronounRegex = new(@"\b(I|me|my|myself)\b", RegexOptions.IgnoreCase);
    private static readonly Regex PossessiveMineRegex = new(@"\b(mine)\b(?!\s+(?:for|some|into|through|out|up|down|diamond|diamonds|ore|ores|minerals|resources|bluespace|steel|gold|silver|crystal|crystals))", RegexOptions.IgnoreCase);
    private static readonly Regex IVerbPattern = new(@"\bI\s+(\w+)(?:\s+(.*))?", RegexOptions.IgnoreCase);
    private static readonly Regex ICommaPattern = new(@"\bI\s*,", RegexOptions.IgnoreCase);
    private static readonly Regex ImPattern = new(@"\bI'm\b", RegexOptions.IgnoreCase);
    private static readonly Regex IDontPattern = new(@"\bI don't\b", RegexOptions.IgnoreCase);
    private static readonly Regex IWithContractionPattern = new(@"\bI\s+(\w+'t)\b", RegexOptions.IgnoreCase);
    private static readonly Regex SentenceSplitRegex = new(@"(?<=[.!?])\s+");
    private static readonly Regex PrepositionPattern = new(@"\b(?:to|for|with|by|at|from|in|on|under|over|of)\s+(me)\b", RegexOptions.IgnoreCase);
    private static readonly Regex VerbMinePattern = new(@"\b(?:can|will|shall|may|might|must|would|could|should|to)\s+(mine)\b", RegexOptions.IgnoreCase);

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
        {"going to", "going to"},
        // Past tense and participles (no change needed)
        {"got", "got"},
        {"had", "had"},
        {"said", "said"},
        {"made", "made"},
        {"went", "went"},
        {"took", "took"},
        {"came", "came"},
        {"saw", "saw"},
        {"knew", "knew"},
        {"got bitten", "got bitten"},
        {"got hit", "got hit"},
        {"got hurt", "got hurt"}
    };

    // Add regex for identifying past tense and participles
    private static readonly Regex PastTenseRegex = new(@"\b\w+(?:ed|en|t)\b", RegexOptions.IgnoreCase);

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
                return $"{subjectPronoun} {match.Value.Substring(2)}"; // Skip the "I "

            hasReplacedFirstPronoun = true;
            var verb = match.Groups[1].Value.ToLower();
            var rest = match.Groups[2].Success ? " " + match.Groups[2].Value : "";

            // Check for special verb conjugations first
            if (SpecialVerbConjugations.TryGetValue(verb, out var specialConjugation))
                return $"{name} {specialConjugation}{rest}";

            // Check if it's a past tense or participle
            if (PastTenseRegex.IsMatch(verb))
                return $"{name} {verb}{rest}";

            // Regular verb conjugation - add 's' unless it's a special case
            if (verb.EndsWith("s") || verb.EndsWith("sh") || verb.EndsWith("ch") || verb.EndsWith("x") || verb.EndsWith("z"))
                return $"{name} {verb}es{rest}";
            if (verb.EndsWith("y") && !verb.EndsWith("ay") && !verb.EndsWith("ey") && !verb.EndsWith("oy") && !verb.EndsWith("uy"))
                return $"{name} {verb[..^1]}ies{rest}";

            return $"{name} {verb}s{rest}";
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
                    "me" => name,
                    _ => name
                };
            }

            return match.Value.ToLower() switch
            {
                "i" => subjectPronoun,
                "me" => objectPronoun,
                "my" => possessivePronoun,
                "myself" => reflexivePronoun,
                _ => match.Value
            };
        });

        // Handle possessive 'mine' separately from verb 'mine'
        sentence = PossessiveMineRegex.Replace(sentence, match =>
        {
            if (!hasReplacedFirstPronoun)
            {
                hasReplacedFirstPronoun = true;
                return name + "'s";
            }
            return possessivePronoun;
        });

        // Preserve verb form of 'mine'
        sentence = VerbMinePattern.Replace(sentence, "$1 mine");

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
