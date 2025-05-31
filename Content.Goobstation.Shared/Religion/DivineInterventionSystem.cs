// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Religion;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Utility;

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

    /// <summary>
    /// The bare minimum, no flavor -
    /// used for spells that do not necessarily require players to be notified of an immunity event
    /// </summary>
    private bool ShouldDeny(EntityUid target, out EntityUid? denyingItem)
    {
        denyingItem = _inventory.GetHandOrInventoryEntities(target, SlotFlags.WITHOUT_POCKET)
            .FirstOrNull(HasComp<DivineInterventionComponent>);

        return denyingItem != null;
    }
    //Overload Method
    public bool ShouldDeny(EntityUid target) => ShouldDeny(target, out _);

    #region Flavour
    /// <summary>
    /// Handles denial flavour (VFX/SFX/POPUPS)
    /// </summary>
    private void DenialEffects(EntityUid uid, EntityUid? entNullable, DivineInterventionComponent? comp = null)
    {
        if (_net.IsClient
            || entNullable is not { } ent
            || !Resolve(uid, ref comp))
            return;

        _popupSystem.PopupEntity(Loc.GetString(comp.DenialString), ent, PopupType.MediumCaution);
        _audio.PlayPvs(comp.DenialSound, ent);
        Spawn(comp.EffectProto, Transform(ent).Coordinates);
    }
    #endregion

    #region EntityTargetActionEvent Spells
    /// <summary>
    /// Handles EntityTargetActionEvent spells.
    /// </summary>
    private void OnTouchSpellAttempt(BeforeCastTouchSpellEvent args)
    {
        if (args.Target is not { } target)
            return;

        if (ShouldDeny(target, out var denyingItem)
            && denyingItem != null
            && Exists(denyingItem.Value))
        {
            args.Cancel();
            DenialEffects(denyingItem.Value, target);
        }
    }

    /// <summary>
    /// Relays whether a spell denial took place - especially useful for working between Core & GoobMod
    /// </summary>
    private void OnTouchSpellDenied(EntityUid uid, DivineInterventionComponent comp, TouchSpellDenialRelayEvent args)
    {
        var ev = new BeforeCastTouchSpellEvent(uid);
        RaiseLocalEvent(uid, ev, true);

        if (ev.Cancelled)
            args.Cancel();
    }

    /// <summary>
    /// Used where dependency is possible i.e. GoobMod Magic.
    /// </summary>
    public bool TouchSpellDenied(EntityUid uid)
    {
        var ev = new BeforeCastTouchSpellEvent(uid);
        RaiseLocalEvent(uid, ev, true);

        return ev.Cancelled;
    }

    #endregion



}
