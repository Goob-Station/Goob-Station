// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Religion;
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
        var modifier = 1f - ent.Comp.Stacks * 0.14f;
        if (TryComp(ent, out TemperatureSpeedComponent? tempSpeed) &&
            tempSpeed.CurrentSpeedModifier != null && tempSpeed.CurrentSpeedModifier != 0f)
            modifier /= 1.2f * tempSpeed.CurrentSpeedModifier.Value;

        modifier = Math.Clamp(modifier, 0f, 1f);

        args.ModifySpeed(modifier, modifier, true);
    }

    protected void RefreshLifetime(VoidCurseComponent comp)
    {
        comp.Lifetime = comp.MaxLifetime + comp.LifetimeIncreasePerLevel * comp.Stacks;
    }

    public bool DoCurse(EntityUid uid, int stacks = 1, int max = 0)
    {
        if (stacks < 1)
            return false;

        if (!HasComp<MobStateComponent>(uid))
            return false; // ignore non mobs because holy shit

        if (TryComp<HereticComponent>(uid, out var h) && h.CurrentPath == "Void" || HasComp<GhoulComponent>(uid))
            return false;

        var ev = new BeforeCastTouchSpellEvent(uid, false);
        RaiseLocalEvent(uid, ev, true);
        if (ev.Cancelled)
            return false;

        var curse = EnsureComp<VoidCurseComponent>(uid);

        if (max > 0 && curse.Stacks > max)
            return false;

        if (max > 0 && curse.Stacks + stacks > max)
            stacks = Math.Max(0, max - (int) curse.Stacks);

        curse.Stacks = Math.Clamp(curse.Stacks + stacks, 0, curse.MaxStacks);
        RefreshLifetime(curse);
        Dirty(uid, curse);

        _modifier.RefreshMovementSpeedModifiers(uid);
        return true;
    }
}
