using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Content.Server._ShibaStation.Speech.Components;
using Content.Server.Speech;
using Robust.Shared.Random;
using Robust.Shared.IoC;

namespace Content.Server._ShibaStation.Speech.EntitySystems;

public sealed class AlieneseSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly Regex WordSplitRegex = new(@"\b\w+\b", RegexOptions.Compiled);
    private static readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u' };
    private static readonly char[] Consonants = {
        'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm',
        'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z'
    };

    // Common alien syllables to make words feel more alien
    private static readonly string[] AlienSyllables = {
        "zx", "kth", "rx", "vx", "xr", "kr", "zh", "kh",
        "th", "rx", "vk", "xk", "sk", "tk", "nx", "px",
        "xt", "zk", "kz", "xz", "vz", "gz", "mx", "tx"
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AlieneseAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, AlieneseAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }

    private string Accentuate(string message, AlieneseAccentComponent component)
    {
        var result = new StringBuilder();
        var lastIndex = 0;

        foreach (Match match in WordSplitRegex.Matches(message))
        {
            // Add any non-word characters before this word
            result.Append(message.Substring(lastIndex, match.Index - lastIndex));

            var word = match.Value.ToLower();
            string alienWord;

            // Check if it's a common word with a predefined translation
            if (component.CommonTranslations.TryGetValue(word, out var translation))
            {
                alienWord = translation;
            }
            else
            {
                // Generate a random alien word based on the original
                alienWord = AlienizeWord(word);
            }

            // Preserve original capitalization
            if (char.IsUpper(match.Value[0]))
                alienWord = char.ToUpper(alienWord[0]) + alienWord.Substring(1);

            result.Append(alienWord);
            lastIndex = match.Index + match.Length;
        }

        // Add any remaining text after the last word
        result.Append(message.Substring(lastIndex));

        return result.ToString();
    }

    private string AlienizeWord(string word)
    {
        if (word.Length <= 2)
        {
            // For very short words, replace with a random alien syllable
            return AlienSyllables[_random.Next(AlienSyllables.Length)];
        }

        var alienWord = new StringBuilder();

        // 50% chance to start with an alien syllable instead of the original word
        if (_random.Prob(0.5f))
        {
            alienWord.Append(AlienSyllables[_random.Next(AlienSyllables.Length)]);
        }

        // Convert the word to a char array and completely shuffle it
        var chars = word.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            var j = _random.Next(chars.Length);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        // Build the alien word with random insertions
        for (int i = 0; i < chars.Length; i++)
        {
            alienWord.Append(chars[i]);

            // Don't add punctuation or syllables at the end of the word
            if (i < chars.Length - 1)
            {
                // 40% chance to insert an alien syllable
                if (_random.Prob(0.4f))
                {
                    alienWord.Append(AlienSyllables[_random.Next(AlienSyllables.Length)]);
                }
                // 30% chance to add punctuation after any character if we're not at the start or end
                else if (i > 0 && _random.Prob(0.3f))
                {
                    alienWord.Append(_random.Prob(0.5f) ? '-' : '\'');
                }
            }
        }

        // 30% chance to append an alien syllable at the end
        if (_random.Prob(0.3f))
        {
            alienWord.Append(AlienSyllables[_random.Next(AlienSyllables.Length)]);
        }

        return alienWord.ToString();
    }
}
