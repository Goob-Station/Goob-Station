// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using Content.Goobstation.Shared.IoC;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Entry;

public sealed class EntryPoint : GameShared
{
    public override void PreInit()
    {
        IoCManager.InjectDependencies(this);
        SharedGoobContentIoC.Register();
    }
}