using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class ItalianDeluxeAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly Dictionary<string, string> DirectReplacements = new()
    {
        { "and", "e" },
        { "yes", "sì" },
        { "no", "no" },
        { "is", "è" },
        { "good", "buono" },
        { "a", "un" },
        { "my", "mio" },
        { "god", "dio" },
        { "bless you", "salute" },
        { "italian", "italiano" },
        { "english", "inglese" },
        { "please", "per favore" },
        { "thank you", "grazie" },
        { "thanks", "grazie" },
        { "hello", "ciao" },
        { "goodbye", "arrivederci" },
        { "bye", "ciao" },
        { "friend", "amico" },
        { "beer", "birra" },
        { "beers", "birre" },
        { "cheese", "formaggio" },
        { "doctor", "dottore" },
        { "house", "casa" },
        { "school", "scuola" },
        { "security", "sicurezza" },
        { "sec", "sicurezza" },
        { "security officer", "ufficiale di sicurezza" },
        { "scientist", "scienziato" },
        { "cargo", "carico" },
        { "atmosphere", "atmosfera" },
        { "atmos", "atmosfera" },
        { "engineering", "ingegneria" },
        { "engineer", "ingegnere" },
        { "chaplain", "cappellano" },
        { "captain", "capitano" },
        { "passenger", "passeggero" },
        { "shit", "merda" },
        { "fuck", "cazzo" },
        { "fucker", "bastardo" },
        { "damn", "dannazione" },
        { "ass", "culo" },
        { "arse", "culo" },
        { "asshole", "stronzo" },
        { "arsehole", "stronzo" },
        { "bitch", "stronza" },
        { "whore", "puttana" },
        { "shut up", "stai zitto" },
        { "cat", "gatto" },
        { "dog", "cane" },
        { "sir", "signore" },
        { "mister", "signore" },
        { "mr", "signore" },
        { "ma'am", "signora" },
        { "lady", "signora" },
        { "miss", "signorina" },
        { "mrs", "signora" },
        { "water", "acqua" },
        { "milk", "latte" },
        { "bread", "pane" },
        { "phone", "telefono" },
        { "fax", "telefax" },
        { "machine", "macchina" },
        { "garden", "giardino" },
        { "city", "città" },
        { "village", "villaggio" },
        { "love", "amore" },
        { "beautiful", "bellissimo" },
        { "wonderful", "bellissimo" },
        { "naked", "nudo" },
        { "cow", "mucca" },
        { "new", "nuovo" },
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItalianDeluxeAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public string Accentuate(string message, ItalianDeluxeAccentComponent component)
    {
        // Step 1: Direct word/phrase replacements
        foreach (var (first, replace) in DirectReplacements)
        {
            var regex = new Regex($@"(?<!\w){first}(?!\w)", RegexOptions.IgnoreCase);
            message = regex.Replace(message, match => PreserveCase(match.Value, replace));
        }

        // Step 2: Character manipulations

        // Replace 'h' at the beginning of words with nothing (apostrophe)
        message = Regex.Replace(message, @"\bh", "'", RegexOptions.IgnoreCase);

        // Capitalize the first character if the message starts with certain letters due to replacement
        if (!string.IsNullOrEmpty(message))
        {
            message = char.ToUpper(message[0]) + message.Substring(1);
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

    private void OnAccentGet(EntityUid uid, ItalianDeluxeAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
