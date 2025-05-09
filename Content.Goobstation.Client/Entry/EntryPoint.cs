// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Client.IoC;
using Content.Goidastation.Client.JoinQueue;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;

namespace Content.Goidastation.Client.Entry;

public sealed class EntryPoint : GameClient
{
    [Dependency] private readonly JoinQueueManager _joinQueue = default!;

    public override void PreInit()
    {
        base.PreInit();
    }

    public override void Init()
    {
        ContentGoidaClientIoC.Register();

        IoCManager.BuildGraph();
        IoCManager.InjectDependencies(this);
    }

    public override void PostInit()
    {
        base.PostInit();

        _joinQueue.Initialize();
    }
}
