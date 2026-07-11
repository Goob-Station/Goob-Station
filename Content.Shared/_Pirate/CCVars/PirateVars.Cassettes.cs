using Robust.Shared.Configuration;

namespace Content.Shared._Pirate.CCVars;

public sealed partial class PirateVars
{
    public static readonly CVarDef<float> VolumeGainCassettes =
        CVarDef.Create("pirate.volume_gain_cassettes", 0.33f, CVar.REPLICATED | CVar.CLIENT | CVar.ARCHIVE);
}
