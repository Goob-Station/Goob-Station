// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Effects;
using Content.Server._Goobstation.Wizard.Components;
using Content.Shared._EinsteinEngines.TelescopicBaton;
using Content.Shared.Timing;
using Robust.Server.Audio;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class UseDelayBlockKnockdownSystem : EntitySystem
{
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SparksSystem _sparks = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UseDelayBlockKnockdownComponent, KnockdownOnHitAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<UseDelayBlockKnockdownComponent, KnockdownOnHitSuccessEvent>(OnSuccess);
    }

    private void OnSuccess(Entity<UseDelayBlockKnockdownComponent> ent, ref KnockdownOnHitSuccessEvent args)
    {
        var (uid, comp) = ent;

        if (comp.ResetDelayOnSuccess)
            _delay.TryResetDelay(uid, id: comp.Delay);

        _audio.PlayPvs(comp.KnockdownSound, Transform(uid).Coordinates);

        if (!comp.DoSparks)
            return;
        foreach (var coords in args.KnockedDown.Select(knocked => Transform(knocked).Coordinates))
        {
            if (comp.DoCustom)
            {
                Spawn(comp.CustomEffect, coords);
                return;
            }

            _sparks.DoSparks(coords, playSound: false);
        }
    }

    private void OnAttempt(Entity<UseDelayBlockKnockdownComponent> ent, ref KnockdownOnHitAttemptEvent args)
    {
        var (uid, comp) = ent;

        if (args.Cancelled || !TryComp(uid, out UseDelayComponent? delay))
            return;

        if (_delay.IsDelayed((uid, delay), comp.Delay))
            args.Cancelled = true;
    }
}
