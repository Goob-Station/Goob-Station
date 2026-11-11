using Robust.Shared.Configuration;

namespace Content.Goobstation.Common.CCVar;

public sealed partial class GoobCVars
{
    public static readonly CVarDef<float> NullspaceSensitivity =
        CVarDef.Create("nullspace.sensitivity", 0.0007f, CVar.CLIENTONLY | CVar.ARCHIVE);
}
