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
        {"they", "mor'ka"}
    };
}
