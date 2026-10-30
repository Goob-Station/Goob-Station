using Content.Goobstation.Client.VoiceChat;
using Content.Goobstation.Common.CCVar;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Client.News.Conditions;

public sealed partial class VoiceChatAvailable : NewsCondition
{
    public override bool Check(IDependencyCollection deps)
    {
        return deps.Resolve<IConfigurationManager>().GetCVar(GoobCVars.VoiceChatEnabled);
    }
}

public sealed partial class VoiceMicDisconnected : NewsCondition
{
    public override bool Check(IDependencyCollection deps)
    {
        return !deps.Resolve<VoiceChatManager>().WebConnected;
    }
}
