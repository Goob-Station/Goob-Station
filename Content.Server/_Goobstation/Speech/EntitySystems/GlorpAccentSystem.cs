using Content.Server.Speech.Components;
using System.Text.RegularExpressions;

namespace Content.Server.Speech.EntitySystems;

public sealed class GlorpAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;
    private static readonly string[] StartingLetters = { "n", "x", "z", "v", "g" };
    private static readonly string[] Suffixes = { "narp", "lorp", "leeb", "orp", "orple", "ip", "op", "eegle" };
    private static readonly string[] RandomInserts = { "Glupshitto", "Glorpshit" };
    private static readonly HashSet<string> WhitelistedWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "discrimination", "inferior", "surgery", "probing", "neanderthal", "animal",
        "tool", "heart", "zoo", "subject", "organ", "skill", "issue", "extract", "remove", "eyes",
        "sleep", "bruh", "skibidi", "ohio", "brazil", "shitsec"
    };
    private static readonly Regex WordRegex = new(@"\b\w+\b", RegexOptions.IgnoreCase);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GlorpAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    private string GenerateRandomAlienWord()
    {
        var start = StartingLetters[Random.Shared.Next(StartingLetters.Length)];
        var suffix = Suffixes[Random.Shared.Next(Suffixes.Length)];
        return start + suffix;
    }

    private string AdjustCapitalization(string word, bool allCaps)
    {
        if (allCaps)
            return word.ToUpper();
        if (string.IsNullOrEmpty(word))
            return word;
        return char.ToUpper(word[0]) + word.Substring(1).ToLower();
    }

    private bool IsWhitelisted(string word)
    {
        // whitelist check
        if (WhitelistedWords.Contains(word.ToLower()))
            return true;

        // plurality check
        if (word.Length > 1 && word.EndsWith("s", StringComparison.OrdinalIgnoreCase))
        {
            var singular = word.Substring(0, word.Length - 1);
            if (WhitelistedWords.Contains(singular.ToLower()))
                return true;
        }

        return false;
    }

    private string ProcessWord(string originalWord, bool allCaps)
    {
        // whitelist plus plurality
        if (IsWhitelisted(originalWord))
        {
            return $"\"{AdjustCapitalization(originalWord, allCaps)}\"";  // apply quotes and caps
        }

        // if note whitelisted replace with some real glorp shit
        var alienWord = GenerateRandomAlienWord();
        return AdjustCapitalization(alienWord, allCaps);
    }

    private string ReplaceWithRandomAlienWords(string message, bool allCaps)
    {
        var words = new List<string>();
        var previousWord = string.Empty;

        foreach (Match match in WordRegex.Matches(message))
        {
            var currentWord = match.Value;
            var processedWord = ProcessWord(currentWord, allCaps);

            // checks if two whitelisted words are next to eachother
            if (IsWhitelisted(previousWord) && IsWhitelisted(currentWord))
            {
                // combine under single quotes
                var combined = $"{AdjustCapitalization(previousWord, allCaps)} {AdjustCapitalization(currentWord, allCaps)}";
                words[words.Count - 1] = $"\"{combined}\"";
            }
            else
            {
                words.Add(processedWord);
            }

            previousWord = currentWord;
        }

        // adds glupshitto and glorpshit randomly
        if (Random.Shared.NextDouble() < 0.25) // percent chance
        {
            var randomInsert = RandomInserts[Random.Shared.Next(RandomInserts.Length)];
            var randomPosition = Random.Shared.Next(words.Count + 1);
            words.Insert(randomPosition, AdjustCapitalization(randomInsert, allCaps));
        }

        return string.Join(" ", words);
    }

    public string Accentuate(string message, GlorpAccentComponent component)
    {
        var msg = message;

        // all caps check
        var allCaps = IsAllCaps(msg);
        msg = _replacement.ApplyReplacements(msg, "glorp_accent");
        msg = ReplaceWithRandomAlienWords(msg, allCaps);

        return msg;
    }

    private bool IsAllCaps(string message)
    {
        var hasLetters = false;
        foreach (var c in message)
        {
            if (char.IsLetter(c))
            {
                hasLetters = true;
                if (char.IsLower(c))
                    return false;
            }
        }
        return hasLetters;
    }

    private void OnAccentGet(EntityUid uid, GlorpAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
