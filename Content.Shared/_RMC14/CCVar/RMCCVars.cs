using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._RMC14.CCVar;

[CVarDefs]
public sealed partial class RMCCVars : CVars
{
    public static readonly CVarDef<float> VolumeGainCassettes =
        CVarDef.Create("rmc.volume_gain_cassettes", 0.5f, CVar.REPLICATED | CVar.CLIENT | CVar.ARCHIVE);
}
