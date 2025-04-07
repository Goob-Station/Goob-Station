// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.IoC;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Redial;

public abstract class SharedRedialManager : IPostInjectInit
{
    [Dependency] protected readonly INetManager _netManager = default!;

    public void PostInject()
    {
        Initialize();
    }

    public virtual void Initialize()
    {

    }
}