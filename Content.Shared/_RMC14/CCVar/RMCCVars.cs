using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._RMC14.CCVar;

[CVarDefs]
public sealed partial class RMCCVars : CVars
{
    public static readonly CVarDef<bool> RMCDeadChatEnabled =
        CVarDef.Create("rmc.dead_chat_enabled", true, CVar.SERVER | CVar.NOTIFY | CVar.REPLICATED);
}
