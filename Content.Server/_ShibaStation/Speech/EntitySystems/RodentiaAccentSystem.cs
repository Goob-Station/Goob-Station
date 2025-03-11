using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class RodentiaAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly Dictionary<string, string> DirectReplacements = new()
    {
        { "please", "cheese" },
        { "pleased", "cheesed" },
        { "pissed off", "cheesed off" },
        { "piss off", "squeak off" },
        { "eat", "nibble" },
        { "ate", "nibbled" },
        { "hungry", "munchy" },
        { "full", "stomached" },
        { "food", "nibbles" },
        { "drink", "sip" },
        { "drank", "sipped" },
        { "money", "cheddar" },
        { "cash", "cheddar" },
        { "mysterious", "mousterious" },
        { "mysteriousness", "mousteriousness" },
        { "mysteriousnesses", "mousteriousnesses" },
        { "hurry", "scurry" },
        { "hurrying", "scurrying" },
        { "captain", "Big Cheese" },
        { "speak", "squeak" },
        { "speaking", "squeaking" },
        { "speaks", "squeaks" },
        { "speaker", "squeaker" },
        { "speakers", "squeakers" }
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RodentiaAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public string Accentuate(string message, RodentiaAccentComponent component)
    {
        // Step 1: Direct word/phrase replacements
        foreach (var (first, replace) in DirectReplacements)
        {
            var regex = new Regex($@"(?<!\w){first}(?!\w)", RegexOptions.IgnoreCase);
            message = regex.Replace(message, match => PreserveCase(match.Value, replace));
        }

        // Modified random phrase insertion for squeaks
        if (_random.Prob(component.squeakChance))
        {
            var phrases = new List<string> { "squeak", "pip", "eek", "chu", "peep", "squeee" };
            var randomPhrase = _random.Pick(phrases);

            var punctuationRegex = new Regex(@"[.!?]$", RegexOptions.IgnoreCase);
            if (punctuationRegex.IsMatch(message))
            {
                message = punctuationRegex.Replace(message, $", {randomPhrase}$0");
            }
            else
            {
                message += $", {randomPhrase}";
            }
        }

        return message;
    }

    private static string PreserveCase(string original, string replacement)
    {
        if (original.All(char.IsUpper))
        {
            return replacement.ToUpper();
        }
        return char.IsUpper(original[0]) ? char.ToUpper(replacement[0]) + replacement.Substring(1) : replacement;
    }

    private void OnAccentGet(EntityUid uid, RodentiaAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
