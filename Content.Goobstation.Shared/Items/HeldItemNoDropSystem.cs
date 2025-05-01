// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Items;

namespace Content.Goobstation.Shared.Items;

public sealed class HeldItemNoDropSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeldItemNoDropComponent, AttemptDropHeldItemEvent>(OnAttempt);
    }

    private void OnAttempt(Entity<HeldItemNoDropComponent> ent, ref AttemptDropHeldItemEvent args)
    {
        args.Cancel();
    }
}
