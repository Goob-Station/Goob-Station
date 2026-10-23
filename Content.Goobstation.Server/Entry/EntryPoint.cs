
using Content.Goobstation.Server.IoC;
using Content.Goobstation.Server.Slasher;
using Content.Goobstation.Server.VoiceChat;
using Content.Goobstation.Common.JoinQueue;
using Content.Goobstation.Common.ServerCurrency;
using Robust.Shared.ContentPack;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Entry;

public sealed class EntryPoint : GameServer
{
    private VoiceChatManager _voiceManager = default!;
    private VoiceLogManager _voiceLogs = default!;
    private ICommonCurrencyManager _curr = default!;
    private IJoinQueueManager _joinQueue = default!;
    private SlasherPrestigeManager _prestige = default!;

    public override void Init()
    {
        base.Init();

        ServerGoobContentIoC.Register();

        IoCManager.BuildGraph();

        _voiceManager = IoCManager.Resolve<VoiceChatManager>();
        _voiceManager.Initialize();

        _voiceLogs = IoCManager.Resolve<VoiceLogManager>();
        _voiceLogs.Initialize();

        _joinQueue = IoCManager.Resolve<IJoinQueueManager>();
        _joinQueue.Initialize();

        _curr = IoCManager.Resolve<ICommonCurrencyManager>();
        _curr.Initialize();

        _prestige = IoCManager.Resolve<SlasherPrestigeManager>();
    }

    public override void Update(ModUpdateLevel level, FrameEventArgs frameEventArgs)
    {
        base.Update(level, frameEventArgs);

        switch (level)
        {
            case ModUpdateLevel.PreEngine:
                _joinQueue.Update(frameEventArgs.DeltaSeconds);
                break;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _curr.Shutdown();
        _prestige.Shutdown();
        _voiceManager.Shutdown();
        _voiceLogs.Shutdown();
    }
}
