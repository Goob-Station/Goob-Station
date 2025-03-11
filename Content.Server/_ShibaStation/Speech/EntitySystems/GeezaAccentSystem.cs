using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class GeezaAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    // Dictionary for common South-East London slang replacements
    private static readonly Dictionary<string, string> GeezaReplacements = new()
    {
        { "friend", "mate" },
        { "money", "dosh" },
        { "food", "grub" },
        { "security", "the old bill" },
        { "scientist", "boffin" },
        { "scientists", "boffins" },
        { "secoff", "coppa" },
        { "secoffs", "rozzas" },
        { "officer", "coppa" },
        { "cops", "filth" },
        { "brig", "nick" },
        { "jail", "nick" },
        { "arrested", "banged up" },
        { "arrest", "nick" },
        { "woman", "bird" },
        { "man", "bloke" },
        { "girl", "lass" },
        { "boy", "lad" },
        { "house", "gaff" },
        { "drink", "bevvy" },
        { "bar", "pub" },
        { "very", "propa" },
        { "smart", "clevva" },
        { "rich", "loaded" },
        { "cool", "minted" },
        { "idiot", "cunt" },
        { "fight", "scrap" },
        { "tired", "knackered" },
        { "shoes", "creps" },
        { "shuttle", "motor" },
        { "work", "graft" },
        { "me", "us" },
        { "mine", "ours" },
        { "my", "me" },
        { "hey", "oi" },
        { "love", "luv" },
        { "stairs", "apples and pears" },
        { "a look", "a gander" },
        { "look at", "check out" },
        { "tea", "rosie lee" },
        { "lies", "porky pies" },
        { "drunk", "pissed" },
        { "really drunk", "smashed" },
        { "crazy", "barmy" },
        { "suspicious", "dodgy" },
        { "sus", "dodgy" },
        { "shocked", "gobsmacked" },
        { "lucky", "jammy" },
        { "steal", "nick" },
        { "stolen", "nicked" },
        { "eyes", "peepers" },
        { "complain", "whinge" },
        { "thank you", "ta" },
        { "thanks", "ta" },
        { "go away", "sod off" },
        { "laugh", "laff" },
        { "moth", "moff" },
        { "alright", "owite" },
        { "gun", "tool" },
        { "sword", "choppa" },
        { "cleaver", "choppa" },
        { "bro", "bruv" },
        { "dead", "brown bread" },
        { "engineer", "bodger" },
        { "engineers", "bodgers" },
        { "engie", "bodger" },
        { "engies", "bodgers" },
        { "would you like", "fancy" },
        { "would you", "would'ya" },
        { "could you", "could'ya" },
        { "should you", "should'ya" },
        { "should have", "shoulda" },
        { "would have", "woulda" },
        { "could have", "coulda" },
        { "have to", "hafta" },
        { "have got to", "hafta" },
        { "have got", "got" },
        { "have not", "ain't" },
        { "some", "a bit" },
        { "really good", "bang tidy" },
        { "really bad", "total shite" },
        { "of", "o'" },
        { "shit", "shite" },
        { "cock", "nob" },
        { "moron", "wanka" },
        { "ass", "arse" },
        { "asshole", "arsehole" },
        { "damn it", "bollocks" },
        { "darn it", "bollocks" },
        { "lying", "talking bollocks" },
        { "kill him", "do 'im in" },
        { "kill her", "do 'er in" },
        { "kill it", "do it in" },
        { "warden", "guv'na" },
        { "bitch", "slag" },
        { "whore", "slag" }
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GeezaAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public string Accentuate(string message, GeezaAccentComponent component)
    {
        // Step 1: Direct word/phrase replacements with South-East London slang
        foreach (var (first, replace) in GeezaReplacements)
        {
            var regex = new Regex($@"(?<!\w){first}(?!\w)", RegexOptions.IgnoreCase);
            message = regex.Replace(message, match => PreserveCase(match.Value, replace));
        }

        // Step 2: Letter manipulations according to the 'geeza' accent rules

        // 2a: Drop initial 'H'
        message = Regex.Replace(message, @"\b[Hh]", "'");

        // 2b: Replace 'UTH' with 'uv'
        message = Regex.Replace(message, @"[Uu][Tt][Hh]", match => PreserveCase(match.Value, "uv"));

        // 2c: Replace words ending in 'ER' with 'a', but ignore words starting with ' like "'er"
        message = Regex.Replace(message, @"(?<!\b')(?:[Ee][Rr])\b", match => PreserveCase(match.Value, "a"));

        // 2d: Replace words ending in 'ING' with "in'"
        message = Regex.Replace(message, @"[Ii][Nn][Gg]\b", match => PreserveCase(match.Value, "in'"));

        // 2e: Replace 'TT' in the middle of a word with a glottal stop (')
        message = Regex.Replace(message, @"(?<=\w)[Tt][Tt](?=\w)", "'");

        // Step 3: Random phrase insertion - check for punctuation and insert a random phrase before it
        if (_random.Prob(component.innitChance))
        {
            // List of possible phrases to insert
            var phrases = new List<string> { "innit", "yeh", "owite", "mate", "bruv" };

            // Pick a random phrase from the list
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

    private void OnAccentGet(EntityUid uid, GeezaAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
