// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Sara Aldrete's Top Guy <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Client.IoC;
using Content.Goobstation.Client.Voice;
using Content.Goobstation.Client.JoinQueue;
using Content.Goobstation.Common.MisandryBox;
using Content.Goobstation.Common.ServerCurrency;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Entry;

public sealed class EntryPoint : GameClient
{
    [Dependency] private readonly IVoiceChatManager _voiceManager = default!;
    [Dependency] private readonly JoinQueueManager _joinQueue = default!;
    [Dependency] private readonly ISpiderManager _spider = default!;
    [Dependency] private readonly ICommonCurrencyManager _currMan = default!;

    public override void PreInit()
    {
        base.PreInit();
    }

    public override void Init()
    {
        ContentGoobClientIoC.Register();

        IoCManager.BuildGraph();
        IoCManager.InjectDependencies(this);
    }

    public override void PostInit()
    {
        base.PostInit();

        _voiceManager.Initalize();
        _joinQueue.Initialize();
        _spider.Initialize();
        _currMan.Initialize();
    }

    public override void Update(ModUpdateLevel level, FrameEventArgs frameEventArgs)
    {
        base.Update(level, frameEventArgs);

        switch (level)
        {
            case ModUpdateLevel.FramePreEngine:
                _voiceManager.Update();
                break;
        }
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _currMan.Shutdown();
    }
}
