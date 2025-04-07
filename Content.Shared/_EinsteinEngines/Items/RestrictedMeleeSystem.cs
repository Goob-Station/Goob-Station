// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._EinsteinEngines.Items;

public sealed class RestrictedMeleeSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RestrictedMeleeComponent, AttemptMeleeEvent>(OnMeleeAttempt);
    }

    private bool CanUse(EntityUid uid, RestrictedMeleeComponent comp) => comp.Whitelist != null && _entityWhitelist.IsValid(comp.Whitelist, uid);

    private void OnMeleeAttempt(EntityUid uid, RestrictedMeleeComponent comp, ref AttemptMeleeEvent args)
    {
        // Specism.
        if (CanUse(args.User, comp))
            return;

        args.Message = Loc.GetString(comp.FailText, ("item", uid));



        if (comp.DoKnockdown)
            _stun.TryKnockdown(args.User, comp.KnockdownDuration, true);

        if (comp.ForceDrop)
            _hands.TryDrop(args.User);

        if (!_statusEffects.HasStatusEffect(args.User, "KnockedDown"))
            _audioSystem.PlayPredicted(comp.FallSound, args.User, args.User);

        // Display the message to the player and cancel the melee attempt.
        _popupSystem.PopupClient(args.Message, uid, PopupType.Large);
        args.Cancelled = true;
    }
}