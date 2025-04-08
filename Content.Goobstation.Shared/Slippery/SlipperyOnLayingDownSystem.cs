// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BeeRobynn <166929042+BeeRobynn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Slippery;
using Content.Shared.Standing;
using Content.Shared.StepTrigger.Components;

namespace Content.Goobstation.Shared.Slippery;

/// <summary>
/// Causes the person given this to inherit
/// Slippery and StepTrigger when they're laying down.
/// </summary>

public sealed class SlipperyOnLayingDownSystem : EntitySystem
{

    [Dependency] private readonly StandingStateSystem _standing = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SlipperyOnLayingDownComponent,DownedEvent>(OnLayingDown);
        SubscribeLocalEvent<SlipperyOnLayingDownComponent,StoodEvent>(OnGetUp);
    }

    private void OnLayingDown(Entity<SlipperyOnLayingDownComponent> uid, ref DownedEvent args)
    {
            EnsureComp<SlipperyComponent>(uid);
            EnsureComp<StepTriggerComponent>(uid);
    }

    private void OnGetUp(Entity<SlipperyOnLayingDownComponent> uid, ref StoodEvent args)
    {
        RemComp<SlipperyComponent>(uid);
        RemComp<StepTriggerComponent>(uid);
    }
}