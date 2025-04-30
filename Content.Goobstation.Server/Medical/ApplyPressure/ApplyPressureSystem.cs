// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Medical;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Medical.ApplyPressure;

public sealed partial class ApplyPressureSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CanApplyPressureComponent, GetVerbsEvent<InnateVerb>>(AddVerb);
        SubscribeLocalEvent<CanApplyPressureComponent, ApplyPressureDoAfterEvent>(OnDoAfter);
    }
    private void AddVerb(Entity<CanApplyPressureComponent> performer, ref GetVerbsEvent<InnateVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var target = args.Target;
        InnateVerb verb = new()
        {
            Act = () => { StartApplyPressure(performer, target); },
            Text = Loc.GetString("apply-pressure-verb"),
            Icon = new SpriteSpecifier.Rsi(new("Objects/Specific/Medical/medical.rsi"), "bloodpack"),
            Priority = 1,
        };

        args.Verbs.Add(verb);
    }

    private void StartApplyPressure(Entity<CanApplyPressureComponent> performer, EntityUid target)
    {
        if (!TryComp<BloodstreamComponent>(target, out var bloodstream) || bloodstream.BleedAmount <= 0)
        {
            var cantBreathePopup = Loc.GetString("apply-pressure-target-cant-bleed", ("target", target));
            _popupSystem.PopupEntity(cantBreathePopup, performer, performer);

            return;
        }

        if (_inventory.TryGetSlotEntity(target, "outerClothing", out var outer))
        {
            var mustRemovePopup = Loc.GetString("cpr-must-remove", ("clothing", outer)); // ehh no point changing this popup
            _popupSystem.PopupEntity(mustRemovePopup, performer, performer);

            return;
        }

        if (performer.Owner == target)
        {
            var pressureSelfMessage = Loc.GetString("apply-pressure-start-self");
            _popupSystem.PopupEntity(pressureSelfMessage, target, performer);
        }
        else
        {
            var pressurePerformerMessage = Loc.GetString("apply-pressure-start-second-person", ("target", target));
            var pressureTargetMessage = Loc.GetString("apply-pressure-start-second-person-patient", ("performer", performer));

            _popupSystem.PopupEntity(pressurePerformerMessage, target, performer);
            _popupSystem.PopupEntity(pressureTargetMessage, target, target);
        }

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            performer,
            performer.Comp.DoAfterDuration,
            new ApplyPressureDoAfterEvent(),
            performer,
            target,
            performer)
        {
            BreakOnMove = true,
            NeedHand = true,
            BlockDuplicate = true,
            BreakOnHandChange = true,
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(Entity<CanApplyPressureComponent> performer, ref ApplyPressureDoAfterEvent args)
    {
        if (args.Cancelled
        || args.Handled
        || !args.Target.HasValue
        || !TryComp<BloodstreamComponent>(args.Target, out var bloodstream))
        return;

        _bloodstreamSystem.TryModifyBleedAmount(args.Target.Value, performer.Comp.BleedModifier, bloodstream);

        args.Repeat = bloodstream.BleedAmount > 0;
    }
}
