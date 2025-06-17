// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Pirate.Server.IoC;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;

namespace Content.Pirate.Server.Entry;

public sealed class EntryPoint : GameServer
{
    public override void Init()
    {
        base.Init();

        ContentPirateServerIoC.Register();

        IoCManager.BuildGraph();
        IoCManager.InjectDependencies(this);
    }
}
