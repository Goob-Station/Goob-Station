// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.MantisBlades;
using Content.Server.Emp;
using Content.Shared.Actions;
using Content.Shared.Body.Part;
using Content.Shared.Emp;
using Content.Shared.Hands.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.PairedExtendable.Systems;

public sealed class MantisBladesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PairedExtendableSystem _pairedExtendable = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MantisBladeArmComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MantisBladeArmComponent, BodyPartAddedEvent>(OnAttach);
        SubscribeLocalEvent<MantisBladeArmComponent, ToggleMantisBladeEvent>(OnToggle);
        SubscribeLocalEvent<MantisBladeArmComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MantisBladeArmComponent, BodyPartRemovedEvent>(OnDetach);
        SubscribeLocalEvent<MantisBladeArmComponent, EmpPulseEvent>(OnEmpPulse);
    }

    private void OnInit(Entity<MantisBladeArmComponent> ent, ref ComponentInit args) => AddAction(ent);

    private void OnAttach(Entity<MantisBladeArmComponent> ent, ref BodyPartAddedEvent args) => AddAction(ent);

    private void AddAction(Entity<MantisBladeArmComponent> ent)
    {
        if (!TryComp<BodyPartComponent>(ent, out var part)
            || part.Body == null)
            return;

        ent.Comp.ActionUid = _actions.AddAction(part.Body.Value, ent.Comp.ActionProto, ent);
    }

    private void OnToggle(Entity<MantisBladeArmComponent> ent, ref ToggleMantisBladeEvent args)
    {
        if (!TryComp<BodyPartComponent>(ent, out var part)
        || part.Body == null)
            return;

        if (HasComp<EmpDisabledComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("mantis-blade-disabled-emp"), ent, part.Body.Value);
            return;
        }

        var handLocation = part.Symmetry switch
        {
            BodyPartSymmetry.Left => HandLocation.Left,
            BodyPartSymmetry.Right => HandLocation.Right,
            BodyPartSymmetry.None => HandLocation.Middle,
            _ => throw new ArgumentOutOfRangeException(),
        };

        args.Handled = _pairedExtendable.ToggleExtendable(part.Body.Value,
            ent.Comp.BladeProto,
            handLocation,
            out ent.Comp.BladeUid,
            ent.Comp.BladeUid);

        if (args.Handled)
            _audio.PlayPvs(ent.Comp.BladeUid == null ? ent.Comp.RetractSound : ent.Comp.ExtendSound, ent);
    }

    private void OnShutdown(Entity<MantisBladeArmComponent> ent, ref ComponentShutdown args)
    {
        Del(ent.Comp.BladeUid);
        Del(ent.Comp.ActionUid);
    }

    private void OnDetach(Entity<MantisBladeArmComponent> ent, ref BodyPartRemovedEvent args)
    {
        Del(ent.Comp.BladeUid);
        Del(ent.Comp.ActionUid);
    }

    private void OnEmpPulse(EntityUid uid, MantisBladeArmComponent comp, ref EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;
    }

}
