// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.CloneProjector.Clone;
using Content.Server.Emp;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Systems;
using Content.Shared.Examine;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;

namespace Content.Goobstation.Server.CloneProjector;

public partial class CloneProjectorSystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    public void InitializeClone()
    {
        SubscribeLocalEvent<HolographicCloneComponent, MapInitEvent>(OnInit);

        SubscribeLocalEvent<HolographicCloneComponent, MobStateChangedEvent>(OnCloneStateChanged);
        SubscribeLocalEvent<HolographicCloneComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<HolographicCloneComponent, EmpPulseEvent>(OnEmpPulse);
    }

    private void OnInit(Entity<HolographicCloneComponent> clone, ref MapInitEvent args)
    {
        foreach (var part in _body.GetBodyChildren(clone))
        {
            if (!TryComp(part.Id, out WoundableComponent? woundable))
                continue;

            woundable.CanRemove = false;
            woundable.CanBleed = false;
            woundable.AllowWounds = false;

            Dirty(part.Id, woundable);
        }
    }
    private void OnCloneStateChanged(Entity<HolographicCloneComponent> clone, ref MobStateChangedEvent args)
    {
        if (!_mobState.IsIncapacitated(clone)
            || clone.Comp.HostProjector is not { } projector)
            return;

        TryInsertClone(projector, true);
        RaiseLocalEvent(clone, new RejuvenateEvent(true, false));

        if (clone.Comp.HostEntity is not { } host)
            return;

        var destroyedPopup = Loc.GetString("gemini-projector-clone-destroyed");
        _popup.PopupEntity(destroyedPopup, host, host, PopupType.LargeCaution);

        if (!projector.Comp.StunOnDestroyed)
            return;

        _stun.TryParalyze(host, projector.Comp.StunDuration, true);
        _damageable.TryChangeDamage(host, projector.Comp.DamageOnDestroyed, true, targetPart: TargetBodyPart.Groin);
    }
    private void OnExamined(Entity<HolographicCloneComponent> clone, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange
            || clone.Comp.HostProjector is not { } projector)
            return;

        var flavor = Loc.GetString(projector.Comp.FlavorText);
        args.PushMarkup(flavor);
    }

    private void OnEmpPulse(Entity<HolographicCloneComponent> clone, ref EmpPulseEvent args)
    {
        if (clone.Comp.HostProjector is not { } projector
            || clone.Comp.HostEntity is not { } host)
            return;

        args.Disabled = true;
        args.Affected = true;

        var duration = args.Duration;
        if (duration > projector.Comp.StunDuration)
            duration = projector.Comp.StunDuration;

        TryInsertClone(projector, true);
        if (projector.Comp.StunOnDestroyed)
            _stun.TryParalyze(host, duration, true);

        var destroyedPopup = Loc.GetString("gemini-projector-clone-destroyed");
        _popup.PopupEntity(destroyedPopup, host, host, PopupType.LargeCaution);
    }
}
