using System.Text;
using Content.Goobstation.Common.Speech;
using Content.Goobstation.Shared.Voidwalker.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Speech;

public sealed partial class VoidAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VoidAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    private void OnAccentGet(Entity<VoidAccentComponent> entity, ref AccentGetEvent args)
    {
        var message = args.Message;
        message = _replacement.ApplyReplacements(message, "voidwalker");
        message = ApplyLegallyDistinctVoidSpeechPattern(message);
        message = message.ToUpperInvariant();
        args.Message = message;
    }

    /// <summary>
    /// the stringbuilder sobs when it sees me coming
    /// </summary>
    public string ApplyLegallyDistinctVoidSpeechPattern(string input)
    {
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new StringBuilder();

        for (var i = 0; i < words.Length; i++)
        {
            var word = words[i];
            if (string.IsNullOrWhiteSpace(word))
                continue;
            var processedWord = ProcessWord(word);
            result.Append(processedWord);
            if (i < words.Length - 1)
                result.Append(' ');
        }
        return result.ToString();
    }

    private string ProcessWord(string word)
    {
        var result = new StringBuilder();
        var chars = word.ToCharArray();
        var length = chars.Length;
        if (length <= 2)
        {
            // For very short words, add question marks around them
            if (_random.Prob(0.7f))
                result.Append('?');
            result.Append(word);
            if (_random.Prob(0.7f))
                result.Append('?');
            if (_random.Prob(0.3f))
                result.Append('?');
            return result.ToString();
        }
        for (var i = 0; i < length; i++)
        {
            var currentChar = chars[i];
            // Sometimes add question marks before a character
            if (i == 0 || i == length - 1)
            {
                if (_random.Prob(0.4f))
                {
                    result.Append('?');
                    if (_random.Prob(0.3f))
                        result.Append('?');
                }
            }
            else
            {
                if (_random.Prob(0.15f))
                    result.Append('?');
            }
            // Add the actual character
            result.Append(currentChar);
            // Sometimes add question marks after a character
            if (i >= length - 1)
                continue;
            switch (i)
            {
                case 0 when _random.Prob(0.3f):
                {
                    // Double question mark after first character is common
                    result.Append('?');
                    if (_random.Prob(0.5f))
                        result.Append('?');
                    break;
                }
                case > 0 when i < length - 2 && _random.Prob(0.2f):
                    result.Append('?');
                    break;
            }
        }
        // Sometimes add trailing question marks
        if (_random.Prob(0.3f))
        {
            result.Append('?');
            if (_random.Prob(0.4f))
                result.Append('?');
        }
        return result.ToString();
    }
}
