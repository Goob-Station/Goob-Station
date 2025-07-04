// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Pirate.Common.AlternativeJobs;
using Content.Pirate.Server.AlternativeJobs;
using Content.Pirate.Server.IoC;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;

namespace Content.Pirate.Server.Entry;

public sealed class EntryPoint : GameServer
{
    public override void Init()
    {
        base.Init();

        ServerPirateContentIoC.Register();

        IoCManager.BuildGraph();
    }
    public override void PreInit()
    {
        base.PreInit();
        IoCManager.Register<IAlternativeJob, AlternativeJobSystem>(true);
    }
}
