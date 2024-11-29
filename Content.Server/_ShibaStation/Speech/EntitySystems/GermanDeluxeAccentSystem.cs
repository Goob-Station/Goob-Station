using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class GermanDeluxeAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [GeneratedRegex(@"(?<!\bthat)\bth", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ThRegex();

    [GeneratedRegex(@"(?<!o)w", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex WRegex();

    private static readonly Dictionary<string, string> DirectReplacements = new()
    {
        { "and", "und" },
        { "yes", "ja" },
        { "no", "nein" },
        { "is", "ist" },
        { "that", "das" },
        { "good", "gut" },
        { "a", "ein" },
        { "my", "mein" },
        { "god", "gott" },
        { "bless you", "gesundheit" },
        { "german", "deutsch" },
        { "english", "englisch" },
        { "please", "bitte" },
        { "thank you", "danke" },
        { "thanks", "danke" },
        { "hello", "hallo" },
        { "goodbye", "auf wiedersehen" },
        { "bye", "tschüss" },
        { "friend", "freund" },
        { "beer", "bier" },
        { "beers", "biere" },
        { "cheese", "käse" },
        { "doctor", "doktor" },
        { "house", "haus" },
        { "school", "schule" },
        { "security", "polizei" },
        { "sec", "polizei" },
        { "security officer", "polizist" },
        { "scientist", "wissenschaftler" },
        { "cargo", "kargo" },
        { "atmosphere", "atmosphäre" },
        { "atmos", "atmosphäre" },
        { "engineering", "technik" },
        { "engineer", "techniker" },
        { "chaplain", "kaplan" },
        { "captain", "kapitän" },
        { "passenger", "passagier" },
        { "shit", "scheiße" },
        { "fuck", "fick" },
        { "fucker", "ficker" },
        { "damn", "verdammt" },
        { "ass", "arsch" },
        { "arse", "arsch" },
        { "asshole", "arschloch" },
        { "arsehole", "arschloch" },
        { "bitch", "schlampe" },
        { "whore", "schlampe" },
        { "shut up", "halt die Fresse" },
        { "pig", "schwein" },
        { "cat", "katze" },
        { "dog", "hund" },
        { "man", "mann" },
        { "sir", "herr" },
        { "mister", "herr" },
        { "mr", "herr" },
        { "woman", "frau" },
        { "ma'am", "frau" },
        { "lady", "frau" },
        { "miss", "frau" },
        { "mrs", "frau" },
        { "money", "geld" },
        { "water", "wasser" },
        { "milk", "milch" },
        { "bread", "brot" },
        { "meat", "fleisch" },
        { "fish", "fisch" },
        { "car", "auto" },
        { "book", "buch" },
        { "paper", "papier" },
        { "phone", "telefon" },
        { "fax", "telefax" },
        { "machine", "maschine" },
        { "chair", "stuhl" },
        { "bed", "bett" },
        { "garden", "garten" },
        { "city", "stadt" },
        { "village", "dorf" },
        { "country", "land" },
        { "world", "welt" },
        { "love", "liebe" },
        { "hate", "hass" },
        { "wonderful", "wunderbar" },
        { "naked", "nackt" },
        { "cow", "kuh" },
        { "blue", "blau" },
        { "green", "grün" },
        { "new", "neu" },
        { "morning", "morgen" }
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GermanDeluxeAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public string Accentuate(string message, GermanDeluxeAccentComponent component)
    {
        // Step 1: Direct word/phrase replacements
        foreach (var (first, replace) in DirectReplacements)
        {
            var regex = new Regex($@"(?<!\w){first}(?!\w)", RegexOptions.IgnoreCase);
            message = regex.Replace(message, match => PreserveCase(match.Value, replace));
        }

        // Step 2: Character manipulations
        // Replace all 'th' with 'z'
        message = ThRegex().Replace(message, match => PreserveCase(match.Value, "z"));

        // Capitalize the first character if the message starts with 'z' or 'v' due to replacement
        if (!string.IsNullOrEmpty(message) && (message.StartsWith('z') || message.StartsWith('v')))
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

    private void OnAccentGet(EntityUid uid, GermanDeluxeAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
