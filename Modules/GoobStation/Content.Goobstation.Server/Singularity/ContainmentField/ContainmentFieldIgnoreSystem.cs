// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Singularity;

namespace Content.Goobstation.Server.Singularity.ContainmentField;

public sealed class ContainmentFieldIgnoreSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContainmentFieldIgnoreComponent, ContainmentFieldThrowEvent>(OnThrow);
    }

    private void OnThrow(Entity<ContainmentFieldIgnoreComponent> ent, ref ContainmentFieldThrowEvent args)
    {
        args.Cancelled = true;
    }
}
