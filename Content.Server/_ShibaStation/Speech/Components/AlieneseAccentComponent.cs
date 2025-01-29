using Robust.Shared.GameObjects;

namespace Content.Server._ShibaStation.Speech.Components;

/// <summary>
/// A component that transforms speech into an alien language with consistent patterns
/// </summary>
[RegisterComponent]
public sealed partial class AlieneseAccentComponent : Component
{
    /// <summary>
    /// Dictionary of common words and their consistent alien translations
    /// </summary>
    [DataField("commonTranslations")]
    public Dictionary<string, string> CommonTranslations { get; set; } = new()
    {
        // Common verbs
        {"hello", "kree'ta"},
        {"help", "vor'kath"},
        {"stop", "nak'tul"},
        {"run", "shi'ra"},
        {"kill", "kro'vak"},
        {"die", "nak'tor"},

        // Common nouns
        {"human", "ter'ran"},
        {"alien", "xen'mor"},
        {"station", "vor'tex"},
        {"captain", "pri'mus"},
        {"security", "guar'dax"},
        {"medical", "hea'lix"},
        {"engineer", "tek'nix"},

        // Common adjectives
        {"good", "zar'ka"},
        {"bad", "nak'ra"},
        {"big", "mor'tu"},
        {"small", "min'ra"},

        // Common pronouns
        {"i", "zar"},
        {"you", "kor"},
        {"we", "zar'ka"},
        {"they", "mor'ka"},

        // Abductor-specific medical terms
        {"heart", "zx'kar"},
        {"gland", "vex'nar"},
        {"organ", "kth'rix"},
        {"blood", "nex'ar"},
        {"brain", "psy'kth"},
        {"surgery", "kir'thex"},
        {"implant", "zx'thor"},
        {"specimen", "ter'kax"},
        {"experiment", "vex'thor"},
        {"sample", "kth'mar"},
        {"harvest", "zx'krul"},
        {"extract", "vex'trak"},
        {"analyze", "psy'thex"},

        // Abduction-related terms
        {"ship", "vex'tel"},
        {"beam", "zx'por"},
        {"take", "kth'nar"},
        {"grab", "vor'tak"},
        {"scan", "psy'nar"},
        {"probe", "zx'thex"},
        {"study", "vex'mar"},
        {"observe", "kth'zar"},
        {"success", "zar'thex"},
        {"failure", "nak'thex"},
        {"ready", "kir'ta"},
        {"begin", "vor'ka"},
        {"complete", "zx'tha"},

        // Scientific terms
        {"research", "vex'sci"},
        {"data", "psy'dat"},
        {"result", "kth'zex"},
        {"test", "vor'sci"},
        {"subject", "ter'spec"},
        {"control", "kir'trol"},
        {"progress", "zx'prog"},
        {"perfect", "vex'tha"},
        {"improve", "kth'pro"},
        {"enhance", "zx'han"}
    };
}
