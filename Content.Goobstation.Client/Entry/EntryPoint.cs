using Content.Goobstation.Client.IoC;
using Content.Goobstation.Client.Polls;
using Content.Goobstation.Client.VoiceChat;
using Content.Goobstation.Client.JoinQueue;
using Content.Goobstation.Common.ServerCurrency;
using Robust.Shared.ContentPack;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Entry;

public sealed class EntryPoint : GameClient
{
    [Dependency] private readonly VoiceChatManager _voiceManager = default!;
    [Dependency] private readonly JoinQueueManager _joinQueue = default!;
    [Dependency] private readonly PollManager _pollManager = default!;
    [Dependency] private readonly ICommonCurrencyManager _currMan = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Init()
    {
        ContentGoobClientIoC.Register();

        IoCManager.BuildGraph();
        IoCManager.InjectDependencies(this);

        _prototypeManager.RegisterIgnore("achievement");
    }

    public override void PostInit()
    {
        base.PostInit();

        _voiceManager.Initialize();
        _joinQueue.Initialize();
        _pollManager.Initialize();
        _currMan.Initialize();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _currMan.Shutdown();
        _voiceManager.Shutdown();
    }
}
