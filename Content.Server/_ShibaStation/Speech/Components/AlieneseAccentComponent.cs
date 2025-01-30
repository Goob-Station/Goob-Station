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

        // Station Departments & Roles
        {"cargo", "tra'dex"},
        {"janitor", "kle'nex"},
        {"chemist", "mix'or"},
        {"scientist", "sci'thor"},
        {"botanist", "flo'rax"},
        {"clown", "joy'kax"},
        {"mime", "sil'nex"},
        {"chaplain", "spi'rex"},
        {"quartermaster", "tra'rex"},
        {"warden", "cel'dax"},
        {"detective", "see'kex"},
        {"lawyer", "law'rex"},
        {"geneticist", "gen'thor"},
        {"atmospherics", "air'nex"},
        {"roboticist", "tek'thor"},
        {"bartender", "mix'tar"},
        {"chef", "foo'dex"},
        {"librarian", "lor'kex"},

        // Station Areas
        {"bridge", "com'dex"},
        {"medbay", "hea'plex"},
        {"brig", "pri'zex"},
        {"engineering", "tek'plex"},
        {"science", "sci'plex"},
        {"hydroponics", "flo'plex"},
        {"kitchen", "foo'plex"},
        {"chapel", "spi'plex"},
        {"dormitory", "sle'plex"},
        {"armory", "arm'plex"},
        {"airlock", "vac'dor"},
        {"hallway", "pat'wex"},

        // Common Station Items
        {"toolbox", "tek'box"},
        {"gun", "kil'tol"},
        {"laser", "bea'mon"},
        {"plasma", "pla'zex"},
        {"oxygen", "bre'thex"},
        {"radio", "com'tol"},
        {"pda", "dat'tol"},
        {"id", "ide'kax"},
        {"battery", "pow'cel"},
        {"wire", "con'nex"},
        {"tool", "tek'tol"},
        {"suit", "vac'mor"},
        {"mask", "bre'mor"},
        {"tank", "gas'tor"},
        {"pill", "med'tab"},
        {"syringe", "med'jek"},
        {"bottle", "liq'tor"},

        // Station Systems
        {"power", "ene'sys"},
        {"engine", "cor'sys"},
        {"solar", "sol'sys"},
        {"disposal", "was'sys"},
        {"camera", "see'sys"},
        {"computer", "dat'sys"},
        {"console", "con'sys"},
        {"network", "net'sys"},
        {"reactor", "fus'sys"},
        {"gravity", "gra'sys"},
        {"radiation", "rad'sys"},
        {"teleporter", "tel'sys"},

        // Emergency Terms
        {"emergency", "kri'sis"},
        {"evacuation", "esc'pex"},
        {"breach", "hul'rip"},
        {"fire", "bur'nex"},
        {"meteor", "rok'fal"},
        {"singularity", "voi'dex"},
        {"supermatter", "ene'rex"},
        {"tesla", "lig'rex"},
        {"delamination", "cor'rip"},
        {"containment", "hol'dex"},
        {"shuttle", "esc'pod"},
        {"mayday", "hel'pex"},

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
        {"enhance", "zx'han"},

        // Common Station Phrases
        {"maints", "ser'vex"},
        {"robust", "pow'ful"},
        {"spacing", "voi'jek"},
        {"toolboxing", "tek'hit"},
        {"greytide", "cha'swarm"},
        {"honk", "joy'kek"},
        {"ahelp", "adm'help"},
        {"centcom", "hig'kom"},
        {"syndie", "reb'kax"},
        {"ling", "sha'mor"},
        {"nuke", "bom'rex"},
        {"traitor", "dec'tor"},
        {"wizard", "mag'kax"},
        {"cult", "blo'kul"},
        {"rev", "reb'vol"},
        {"blob", "mas'vor"},
        {"xeno", "ali'rex"}
    };
}
