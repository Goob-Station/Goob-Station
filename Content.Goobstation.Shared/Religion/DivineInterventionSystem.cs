// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Religion;
using Content.Goobstation.Common.Religion.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Religion;

/// <summary>
/// Handles "Spell Denial", these methods are largely targeted towards TargetActionEvents, however,
/// may also have other edge-cases.
/// </summary>
public sealed class DivineInterventionSystem : EntitySystem
{

    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BeforeCastTouchSpellEvent>(OnTouchSpellAttempt);

        SubscribeLocalEvent<DivineInterventionComponent, TouchSpellDenialRelayEvent>(OnTouchSpellDenied);
    }

    //Handles denial flavour (VFX/SFX/POPUPS)
    private void DenialEffects(EntityUid uid, EntityUid? ent)
    {
        if (ent != null
            && TryComp<DivineInterventionComponent>(uid, out var comp)
            && !_net.IsClient)
        {
            _popupSystem.PopupEntity(Loc.GetString("nullrod-spelldenial-popup"),
                ent.Value,
                type: PopupType.MediumCaution);
            _audio.PlayPvs(comp.DenialSound, ent.Value);
            Spawn(comp.EffectProto, Transform(ent.Value).Coordinates);
        }
    }

    #region Touch Spells (Wizard)

    private void OnTouchSpellAttempt(ref BeforeCastTouchSpellEvent args)
    {
        if (!HasComp<HandsComponent>(args.Target))
            return;

        if (!HasComp<InventoryComponent>(args.Target))
            return;

        var contains = _inventory.GetHandOrInventoryEntities(args.Target.Value);
        foreach (var item in contains)
        {
            if (!HasComp<DivineInterventionComponent>(item))
                continue;

            args.Cancelled = true;
            DenialEffects(item, args.Target.Value);
            break;
        }
    }

    //Relays whether denial took place so Spells in Core can be cancelled.
    private void OnTouchSpellDenied(Entity<DivineInterventionComponent> uid, ref TouchSpellDenialRelayEvent args)
    {
        var beforeTouchSpellEvent = new BeforeCastTouchSpellEvent(uid);
        RaiseLocalEvent(uid, ref beforeTouchSpellEvent, true);

        args.Cancelled = beforeTouchSpellEvent.Cancelled;
    }

    //Redundant for wizard but saving for later (heretic will use similar bool)
   // public bool TouchSpellDenied(EntityUid target)
    //{
        //var beforeTouchSpellEvent = new BeforeCastTouchSpellEvent(target);
        //RaiseLocalEvent(target, ref beforeTouchSpellEvent, true);

        //return beforeTouchSpellEvent.Cancelled;
    //}

    #endregion



}
