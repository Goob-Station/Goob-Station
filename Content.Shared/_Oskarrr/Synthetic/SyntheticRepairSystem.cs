// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.Tag;
using Robust.Shared.Serialization;

namespace Content.Shared._Oskarrr.Synthetic;

/// <summary>
/// Lets synthetics be repaired with cable coils (welders use <see cref="Content.Shared.Repairable.RepairableComponent"/>).
/// </summary>
public sealed class SyntheticRepairSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SyntheticComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<SyntheticComponent, SyntheticCableRepairDoAfterEvent>(OnCableDoAfter);
    }

    private void OnInteractUsing(EntityUid uid, SyntheticComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!_tag.HasTag(args.Used, "CableCoil"))
            return;

        if (!TryComp(uid, out DamageableComponent? damageable))
            return;

        var hasBrute = false;
        foreach (var type in new[] { "Blunt", "Slash", "Piercing" })
        {
            if (damageable.Damage.DamageDict.TryGetValue(type, out var amount) && amount > FixedPoint2.Zero)
            {
                hasBrute = true;
                break;
            }
        }

        if (!hasBrute)
        {
            _popup.PopupClient(Loc.GetString("synthetic-repair-not-needed"), uid, args.User);
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, args.User, component.CableDelay,
            new SyntheticCableRepairDoAfterEvent(), uid, target: uid, used: args.Used)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnCableDoAfter(EntityUid uid, SyntheticComponent component, SyntheticCableRepairDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Used is not { } used)
            return;

        if (!_tag.HasTag(used, "CableCoil"))
            return;

        if (TryComp(used, out StackComponent? stack) && !_stack.TryUse((used, stack), 1))
        {
            _popup.PopupClient(Loc.GetString("synthetic-repair-no-cable"), uid, args.User);
            return;
        }

        var heal = new DamageSpecifier();
        heal.DamageDict["Blunt"] = -component.CableHealAmount;
        heal.DamageDict["Slash"] = -component.CableHealAmount;
        heal.DamageDict["Piercing"] = -component.CableHealAmount;
        _damageable.TryChangeDamage(uid, heal, true, false, origin: args.User);

        _popup.PopupEntity(Loc.GetString("synthetic-repair-cable-success"), uid, args.User);
        args.Handled = true;
    }
}

[Serializable, NetSerializable]
public sealed partial class SyntheticCableRepairDoAfterEvent : SimpleDoAfterEvent;
