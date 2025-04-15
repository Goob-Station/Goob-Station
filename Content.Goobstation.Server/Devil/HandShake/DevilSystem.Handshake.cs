// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Devil.HandShake;
using Content.Goobstation.Shared.CheatDeath;
using Content.Goobstation.Shared.Devil;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Devil;

public sealed partial class DevilSystem
{
    private void InitializeHandshakeSystem()
    {
        SubscribeLocalEvent<DevilComponent, InteractHandEvent>(OnDevilHandInteract);
        SubscribeLocalEvent<DevilComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbs);
    }
    private void OnGetVerbs(EntityUid uid, DevilComponent comp, GetVerbsEvent<InnateVerb> args)
    {
        // Can't shake your own hand, and you can't shake from a distance
        if (!args.CanAccess || !args.CanInteract || args.User == args.Target || _state.IsIncapacitated(args.Target))
            return;

        // Only allow offering to living entities that can interact
        if (!HasComp<MobStateComponent>(args.Target))
            return;

        InnateVerb handshakeVerb = new()
        {
            Act = () => OfferHandshake(args.User, args.Target),
            Text = Loc.GetString("hand-shake-prompt-verb", ("target", args.Target)),
            Icon = new SpriteSpecifier.Rsi(new("_Goobstation/Actions/devil.rsi"), "summon-contract"),
            Priority = 1 // Higher priority than default verbs
        };

        args.Verbs.Add(handshakeVerb);
    }

    private void OnDevilHandInteract(EntityUid uid, DevilComponent component, InteractHandEvent args)
    {
        if (args.Handled || !TryComp<PendingHandshakeComponent>(args.User, out var pending))
            return;

        // Validate this is our offerer and check expiry
        if (pending.Offerer != uid)
            return;

        args.Handled = true;

        if (_timing.CurTime > pending.ExpiryTime)
        {
            var expiredMessage = Loc.GetString("handshake-expired");
            _popup.PopupEntity(expiredMessage, args.User, args.User);
            RemComp<PendingHandshakeComponent>(args.User);
            return;
        }

        // Accept the handshake
        HandleHandshake(uid, args.User);
        RemComp<PendingHandshakeComponent>(args.User);
    }

    private void OfferHandshake(EntityUid user, EntityUid target)
    {
        if (HasComp<DevilComponent>(target) || HasComp<PendingHandshakeComponent>(target))
            return;

        var pending = AddComp<PendingHandshakeComponent>(target);
        pending.Offerer = user;
        pending.ExpiryTime = _timing.CurTime + TimeSpan.FromSeconds(10); // 10-second window

        // Notify target
        var popupMessage = Loc.GetString("handshake-offer-popup", ("user", user));
        _popup.PopupEntity(popupMessage, target, target);

        // Notify self
        var selfPopup = Loc.GetString("handshake-offer-popup-self", ("target", target));
        _popup.PopupEntity(selfPopup, user, user);
    }
    private void HandleHandshake(EntityUid user, EntityUid target)
    {
        if (!_contract.TryTransferSouls(user, target, 1))
        {
            var handshakeFail = Loc.GetString("handshake-fail", ("user", user));
            _popup.PopupEntity(handshakeFail, user, user);
            return;
        }

        var handshakeSucess = Loc.GetString("handshake-success", ("user", user));
        _popup.PopupEntity(handshakeSucess, target, target);
        _rejuvenate.PerformRejuvenate(target);
        var cheatdeath = EnsureComp<CheatDeathComponent>(target);
        cheatdeath.ReviveAmount = 1;
    }
}
