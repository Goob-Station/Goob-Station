// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Temperature;
using Content.Shared.Temperature.Components;

namespace Content.Shared._Goobstation.Heretic.Systems;

public abstract class SharedVoidCurseSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _modifier = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidCurseComponent, TemperatureChangeAttemptEvent>(OnTemperatureChangeAttempt);
        SubscribeLocalEvent<VoidCurseComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<VoidCurseComponent, ComponentRemove>(OnRemove);
    }

    private void OnRemove(Entity<VoidCurseComponent> ent, ref ComponentRemove args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        _modifier.RefreshMovementSpeedModifiers(ent);
    }

    private void OnTemperatureChangeAttempt(Entity<VoidCurseComponent> ent, ref TemperatureChangeAttemptEvent args)
    {
        if (!args.Cancelled && ent.Comp.Stacks >= ent.Comp.MaxStacks && args.CurrentTemperature > args.LastTemperature)
            args.Cancel();
    }

    private void OnRefreshMoveSpeed(Entity<VoidCurseComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        // If entity is not slowed down by temperature - slow them down even more
        var divisor = HasComp<TemperatureSpeedComponent>(ent) ? 15f : 10f;
        var modifier = 1f - Math.Clamp(ent.Comp.Stacks / divisor, 0f, 1f);
        args.ModifySpeed( modifier, modifier);
    }

    protected virtual void Cycle(Entity<VoidCurseComponent> ent)
    {

    }

    public void DoCurse(EntityUid uid, int stacks = 1)
    {
        if (stacks < 1)
            return;

        if (!HasComp<MobStateComponent>(uid))
            return; // ignore non mobs because holy shit

        if (TryComp<HereticComponent>(uid, out var h) && h.CurrentPath == "Void" || HasComp<GhoulComponent>(uid))
            return;

        var curse = EnsureComp<VoidCurseComponent>(uid);
        curse.Lifetime = curse.MaxLifetime;
        curse.Stacks = Math.Clamp(curse.Stacks + stacks, 0, curse.MaxStacks);
        Dirty(uid, curse);

        _modifier.RefreshMovementSpeedModifiers(uid);
    }
}
