// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Religion;
using Content.Goobstation.Shared.Religion.Nullrod;
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
    private void DenialEffects(EntityUid uid, EntityUid? entNullable)
    {
        if (entNullable is not { } ent  || !TryComp<DivineInterventionComponent>(uid, out var comp) || _net.IsClient)
            return;

        _popupSystem.PopupEntity(Loc.GetString("nullrod-spelldenial-popup"), ent, PopupType.MediumCaution);
        _audio.PlayPvs(comp.DenialSound, ent);
        Spawn(comp.EffectProto, Transform(ent).Coordinates);
    }

    #region Touch Spells (Wizard)

    private void OnTouchSpellAttempt(BeforeCastTouchSpellEvent args)
    {
        if (args.Target is not { } target
            || !HasComp<HandsComponent>(target)
            || !HasComp<InventoryComponent>(target))
            return;

        var contained = _inventory.GetHandOrInventoryEntities(target);
        foreach (var item in contained)
        {
            if (!HasComp<DivineInterventionComponent>(item))
                continue;

            args.Cancel();
            DenialEffects(item, args.Target.Value);
            break;
        }
    }

    //Relays whether denial took place so Spells in Core can be cancelled.
    private void OnTouchSpellDenied(EntityUid uid, DivineInterventionComponent comp, TouchSpellDenialRelayEvent args)
    {
        var ev = new BeforeCastTouchSpellEvent(uid);
        RaiseLocalEvent(uid, ev, true);

        if (ev.Cancelled)
            args.Cancel();
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
