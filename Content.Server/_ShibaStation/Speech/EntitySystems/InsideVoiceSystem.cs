using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Content.Server._ShibaStation.Speech.Components;
using Content.Server.Speech;
using Robust.Shared.Random;
using Robust.Shared.IoC;

namespace Content.Server._ShibaStation.Speech.EntitySystems;

public sealed class InsideVoiceSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly Regex WordSplitRegex = new(@"\b\w+\b", RegexOptions.Compiled);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InsideVoiceComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, InsideVoiceComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }

    private string Accentuate(string message)
    {
        var words = WordSplitRegex.Matches(message).Cast<Match>().ToList();
        var consecutiveUppercaseCount = 0;
        var needsReformatting = false;

        // First pass: check if we need to reformat
        foreach (var match in words)
        {
            var word = match.Value;

            // Skip single-letter words and "I"
            if (word.Length <= 1 || word == "I")
                continue;

            if (word.ToUpper() == word)
                consecutiveUppercaseCount++;
            else
                consecutiveUppercaseCount = 0;

            if (consecutiveUppercaseCount >= 2)
            {
                needsReformatting = true;
                break;
            }
        }

        if (!needsReformatting)
            return message;

        // Second pass: reformat the text
        var result = new StringBuilder();
        var lastIndex = 0;
        var startOfSentence = true;

        foreach (Match match in WordSplitRegex.Matches(message))
        {
            // Add any non-word characters before this word
            result.Append(message.Substring(lastIndex, match.Index - lastIndex));

            var word = match.Value;

            // Special case for "I"
            if (word == "I" || word.Length <= 1)
            {
                result.Append(word);
            }
            else
            {
                // Capitalize first letter if it's start of sentence, lowercase the rest
                var formattedWord = startOfSentence
                    ? char.ToUpper(word[0]) + word.Substring(1).ToLower()
                    : word.ToLower();

                result.Append(formattedWord);
            }

            lastIndex = match.Index + match.Length;

            // Check if the next character starts a new sentence
            if (lastIndex < message.Length)
            {
                var nextChar = message[lastIndex];
                startOfSentence = nextChar == '.' || nextChar == '!' || nextChar == '?';
            }
        }

        // Add any remaining text after the last word
        result.Append(message.Substring(lastIndex));

        return result.ToString();
    }
}
