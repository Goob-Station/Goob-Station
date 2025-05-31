// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lincoln McQueen <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared.Mobs.Components;

namespace Content.Goobstation.Shared.MartialArts;

/// <summary>
/// This handles determining if a combo was performed.
/// </summary>
public partial class SharedMartialArtsSystem
{
    private void InitializeCanPerformCombo()
    {
        SubscribeLocalEvent<CanPerformComboComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CanPerformComboComponent, ComboAttackPerformedEvent>(OnAttackPerformed);
        SubscribeLocalEvent<CanPerformComboComponent, ComboBeingPerformedEvent>(OnComboBeingPerformed);
    }

    private void OnMapInit(EntityUid uid, CanPerformComboComponent component, MapInitEvent args)
    {
        foreach (var item in component.RoundstartCombos)
        {
            component.AllowedCombos.Add(_proto.Index(item));
        }
    }

    private void OnAttackPerformed(EntityUid uid, CanPerformComboComponent component, ComboAttackPerformedEvent args)
    {
        if (!HasComp<MobStateComponent>(args.Target))
            return;

        if (component.CurrentTarget != null && args.Target != component.CurrentTarget.Value)
        {
            component.LastAttacks.Clear();
        }

        if (args.Weapon != uid)
        {
            component.LastAttacks.Clear();
            return;
        }

        component.CurrentTarget = args.Target;
        component.ResetTime = _timing.CurTime + TimeSpan.FromSeconds(4);
        component.LastAttacks.Add(args.Type);
        CheckCombo(uid, component);
    }

    private void CheckCombo(EntityUid uid, CanPerformComboComponent comp)
    {
        var success = false;

        foreach (var proto in comp.AllowedCombos)
        {
            if (success)
                break;

            var sum = comp.LastAttacks.Count - proto.AttackTypes.Count;
            if (sum < 0)
                continue;

            var list = comp.LastAttacks.GetRange(sum, proto.AttackTypes.Count).AsEnumerable();
            var attackList = proto.AttackTypes.AsEnumerable();

            if (!list.SequenceEqual(attackList) || proto.ResultEvent == null)
                continue;
            var beingPerformedEv = new ComboBeingPerformedEvent(proto.ID);
            var ev = proto.ResultEvent;

            RaiseLocalEvent(uid, beingPerformedEv);
            RaiseLocalEvent(uid, ev);
            comp.LastAttacks.Clear();
        }
    }
    private void OnComboBeingPerformed(Entity<CanPerformComboComponent> ent, ref ComboBeingPerformedEvent args)
    {
        ent.Comp.BeingPerformed = args.ProtoId;
        Dirty(ent, ent.Comp);
    }
}
