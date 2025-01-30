using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Security.Cryptography;
using Content.Server._ShibaStation.Speech.Components;
using Content.Server.Speech;
using Robust.Shared.Random;
using Robust.Shared.IoC;

namespace Content.Server._ShibaStation.Speech.EntitySystems;

public sealed class AlieneseSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly Regex WordSplitRegex = new(@"(\""[^\""]*\""|[\w]+)", RegexOptions.Compiled);

    // Alien vowel substitutions - each vowel has two possible replacements for variety
    private static readonly Dictionary<char, char[]> AlienVowels = new()
    {
        {'a', new[] {'æ', 'α'}},
        {'e', new[] {'ε', 'э'}},
        {'i', new[] {'ï', 'í'}},
        {'o', new[] {'ø', 'δ'}},
        {'u', new[] {'ü', 'μ'}}
    };

    // Reduced set of alien syllables for occasional use
    private static readonly string[] AlienSyllables = {
        "zx", "kth", "rx", "vx", "xr", "kr"
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

            var word = match.Value;

            // Check if the word is wrapped in quotes
            if (word.StartsWith("\"") && word.EndsWith("\""))
            {
                // Keep quoted text as-is
                result.Append(word);
            }
            else
            {
                var lowerWord = word.ToLower();
                string alienWord;

                // Check if it's a common word with a predefined translation
                if (component.CommonTranslations.TryGetValue(lowerWord, out var translation))
                {
                    // Apply vowel substitution to the dictionary translation
                    alienWord = SubstituteVowels(translation, translation.GetHashCode());
                }
                else
                {
                    // Generate an alien word based on the original
                    alienWord = AlienizeWord(word);
                }

                // Preserve original capitalization
                if (char.IsUpper(word[0]))
                    alienWord = char.ToUpper(alienWord[0]) + alienWord.Substring(1);

                result.Append(alienWord);
            }

            lastIndex = match.Index + match.Length;
        }

        // Add any remaining text after the last word
        result.Append(message.Substring(lastIndex));

        return result.ToString();
    }

    private string AlienizeWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return word;

        // For very short words, use a simple vowel substitution
        if (word.Length <= 2)
        {
            return SubstituteVowels(word, word.GetHashCode());
        }

        // Generate a deterministic hash for this word
        using var md5 = MD5.Create();
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(word);
        var hashBytes = md5.ComputeHash(inputBytes);

        // Convert word to char array for manipulation
        var chars = word.ToCharArray();

        // Deterministically scramble the middle letters of the word
        // Keep first and last letters in place for some readability
        for (int i = 1; i < chars.Length - 1; i++)
        {
            var j = 1 + (Math.Abs(hashBytes[i % hashBytes.Length]) % (chars.Length - 2));
            if (i != j)
            {
                var temp = chars[i];
                chars[i] = chars[j];
                chars[j] = temp;
            }
        }

        var alienWord = new StringBuilder();

        // 25% chance to start with an alien syllable based on hash
        if (hashBytes[0] % 4 == 0)
        {
            var syllableIndex = hashBytes[0] % AlienSyllables.Length;
            alienWord.Append(AlienSyllables[syllableIndex]);
        }

        // Build the word with vowel substitutions and occasional punctuation
        for (int i = 0; i < chars.Length; i++)
        {
            var currentChar = chars[i];

            // Substitute vowels
            if (char.IsLetter(currentChar))
            {
                var lowerChar = char.ToLower(currentChar);
                if (AlienVowels.TryGetValue(lowerChar, out var alienVowels))
                {
                    var vowelIndex = hashBytes[(i * 2) % hashBytes.Length] % 2;
                    currentChar = alienVowels[vowelIndex];
                }
            }

            alienWord.Append(currentChar);

            // Add occasional punctuation or syllable (but less frequently than before)
            if (i < chars.Length - 1 && i > 0)
            {
                var hashByte = hashBytes[i % hashBytes.Length];
                if (hashByte % 8 == 0) // Reduced frequency (was 4)
                {
                    alienWord.Append(hashByte % 2 == 0 ? '-' : '\'');
                }
                else if (hashByte % 12 == 0) // Even more reduced frequency for syllables
                {
                    var syllableIndex = hashByte % AlienSyllables.Length;
                    alienWord.Append(AlienSyllables[syllableIndex]);
                }
            }
        }

        return alienWord.ToString();
    }

    private string SubstituteVowels(string word, int seed)
    {
        var result = new StringBuilder();
        foreach (var c in word)
        {
            if (AlienVowels.TryGetValue(char.ToLower(c), out var alienVowels))
            {
                var vowelIndex = Math.Abs(seed) % 2;
                result.Append(alienVowels[vowelIndex]);
            }
            else
            {
                result.Append(c);
            }
        }
        return result.ToString();
    }
}
