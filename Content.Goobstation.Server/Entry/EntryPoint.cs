// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Sara Aldrete's Top Guy <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Voice;
using Content.Goobstation.Common.MisandryBox;
using Content.Goobstation.Common.ServerCurrency;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Entry;

public sealed class EntryPoint : GameServer
{
    // private IVoiceChatServerManager _voiceManager = default!; // deleted by CorvaxGoob
    // private ICommonCurrencyManager _curr = default!; // deleted by CorvaxGoob

    public override void Init()
    {
        base.Init();

        // ServerGoobContentIoC.Register(); // deleted by CorvaxGoob

        IoCManager.BuildGraph();

        /* deleted by CorvaxGoob
        _voiceManager = IoCManager.Resolve<IVoiceChatServerManager>();

        IoCManager.Resolve<IJoinQueueManager>().Initialize();
        IoCManager.Resolve<ISpiderManager>().Initialize();

        _curr = IoCManager.Resolve<ICommonCurrencyManager>(); // Goobstation
        _curr.Initialize(); // Goobstation
        */
    }
}
