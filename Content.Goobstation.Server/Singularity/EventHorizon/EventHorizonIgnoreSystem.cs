// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Singularity.Events;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.Singularity.EventHorizon;

public sealed class EventHorizonIgnoreSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EventHorizonIgnoreComponent, EventHorizonAttemptConsumeEntityEvent>(OnAttemptConsume);
    }

    private void OnAttemptConsume(Entity<EventHorizonIgnoreComponent> ent, ref EventHorizonAttemptConsumeEntityEvent args)
    {
        args.Cancelled = args.Cancelled || _whitelist.IsWhitelistPassOrNull(ent.Comp.HorizonWhitelist, args.EventHorizonUid);
    }
}
