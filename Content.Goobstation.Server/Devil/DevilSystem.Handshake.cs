// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.CheatDeath;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Condemned;
using Content.Shared.Body.Part;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Devil;

public sealed partial class DevilSystem
{
    private static readonly SpriteSpecifier HandshakeIcon =
        new SpriteSpecifier.Rsi(new("_Goobstation/Actions/devil.rsi"), "summon-contract");

    private const int HandshakeVerbPriority = 1;
    private static readonly TimeSpan HandshakeOfferDuration = TimeSpan.FromSeconds(15);

    private void InitializeHandshakeSystem()
    {
        SubscribeLocalEvent<DevilComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbs);
        SubscribeLocalEvent<PendingHandshakeComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbsPending);
    }

    private bool CanOfferHandshake(EntityUid user, EntityUid target)
    {
        return user != target
               && !HasComp<CondemnedComponent>(target)
               && HasComp<MobStateComponent>(target)
               && !_state.IsIncapacitated(target)
               && _body.BodyHasPartType(user, BodyPartType.Hand)
               && _body.BodyHasPartType(target, BodyPartType.Hand)
               && _contract.IsUserValid(target, out _);
    }

    private void OnGetVerbs(EntityUid uid, DevilComponent comp, GetVerbsEvent<InnateVerb> args)
    {
        // Can't shake your own hand, and you can't shake from a distance
        if (!args.CanAccess ||
            !args.CanInteract ||
            !CanOfferHandshake(args.User, args.Target))
        {
            return;
        }

        args.Verbs.Add(new InnateVerb
        {
            Act = () => OfferHandshake(args.User, args.Target),
            Text = Loc.GetString("hand-shake-prompt-verb", ("target", args.Target)),
            Icon = HandshakeIcon,
            Priority = HandshakeVerbPriority
        });
    }

    private void OnGetVerbsPending(EntityUid uid, PendingHandshakeComponent comp, GetVerbsEvent<InnateVerb> args)
    {
        if (!args.CanAccess ||
            !args.CanInteract ||
            _state.IsIncapacitated(args.Target) ||
            !HasComp<MobStateComponent>(args.Target) ||
            args.Target != comp.Offerer)
        {
            return;
        }

        args.Verbs.Add(new InnateVerb
        {
            Act = () => HandleHandshake(args.Target, args.User),
            Text = Loc.GetString("hand-shake-accept-verb", ("target", args.Target)),
            Icon = HandshakeIcon,
            Priority = HandshakeVerbPriority
        });
    }

    private void OfferHandshake(EntityUid user, EntityUid target)
    {
        if (HasComp<DevilComponent>(target) ||
            HasComp<PendingHandshakeComponent>(target) ||
            !_contract.IsUserValid(target, out _))
        {
            return;
        }

        var pending = AddComp<PendingHandshakeComponent>(target);
        pending.Offerer = user;
        pending.ExpiryTime = _timing.CurTime + HandshakeOfferDuration;

        var popupMessage = Loc.GetString("handshake-offer-popup", ("user", user));
        _popup.PopupEntity(popupMessage, target, target);

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

        var handshakeSuccess = Loc.GetString("handshake-success", ("user", user));
        _popup.PopupEntity(handshakeSuccess, target, target);

        _rejuvenate.PerformRejuvenate(target);

        var cheatdeath = EnsureComp<CheatDeathComponent>(target);
        cheatdeath.ReviveAmount = 1;

        _contract.AddRandomNegativeClause(target);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PendingHandshakeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.ExpiryTime > _timing.CurTime)
                continue;

            RemCompDeferred(uid, comp);
        }
    }
}
