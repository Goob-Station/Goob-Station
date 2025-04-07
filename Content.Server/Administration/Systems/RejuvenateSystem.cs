// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
//
// SPDX-License-Identifier: MIT
using Content.Shared.Rejuvenate;

namespace Content.Server.Administration.Systems;

public sealed class RejuvenateSystem : EntitySystem
{
    public void PerformRejuvenate(EntityUid target)
    {
        RaiseLocalEvent(target, new RejuvenateEvent());
    }
}