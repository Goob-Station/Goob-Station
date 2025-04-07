// SPDX-FileCopyrightText: 2022 Francesco <frafonia@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Standing;
using Robust.Shared.Containers;

namespace Content.Shared.Medical.Cryogenics;

public abstract partial class SharedCryoPodSystem
{
    public virtual void InitializeInsideCryoPod()
    {
        SubscribeLocalEvent<InsideCryoPodComponent, DownAttemptEvent>(HandleDown);
        SubscribeLocalEvent<InsideCryoPodComponent, EntGotRemovedFromContainerMessage>(OnEntGotRemovedFromContainer);
    }

    // Must stand in the cryo pod
    private void HandleDown(EntityUid uid, InsideCryoPodComponent component, DownAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnEntGotRemovedFromContainer(EntityUid uid, InsideCryoPodComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (Terminating(uid))
        {
            return;
        }

        RemComp<InsideCryoPodComponent>(uid);
    }
}