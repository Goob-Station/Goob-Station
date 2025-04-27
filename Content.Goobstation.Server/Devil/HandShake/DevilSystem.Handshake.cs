// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.Devil.HandShake;
using Content.Goobstation.Shared.CheatDeath;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Contract;
using Content.Shared.Mobs.Components;
using Content.Shared.Verbs;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Devil;

public sealed partial class DevilSystem
{
    private void InitializeHandshakeSystem()
    {
        SubscribeLocalEvent<DevilComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbs);
        SubscribeLocalEvent<PendingHandshakeComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbsPending);
    }
    private void OnGetVerbs(EntityUid uid, DevilComponent comp, GetVerbsEvent<InnateVerb> args)
    {
        // Can't shake your own hand, and you can't shake from a distance
        if (!args.CanAccess
        || !args.CanInteract
        || _state.IsIncapacitated(args.Target)
        || !HasComp<MobStateComponent>(args.Target)
        || args.Target == args.User)
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

    private void OnGetVerbsPending(EntityUid uid, PendingHandshakeComponent comp, GetVerbsEvent<InnateVerb> args)
    {
        if (!args.CanAccess
            || !args.CanInteract
            || _state.IsIncapacitated(args.Target)
            || !HasComp<MobStateComponent>(args.Target)
            || args.Target != comp.Offerer)
            return;

        InnateVerb handshakeVerb = new()
        {
            Act = () => HandleHandshake(args.Target, args.User),
            Text = Loc.GetString("hand-shake-accept-verb", ("target", args.Target)),
            Icon = new SpriteSpecifier.Rsi(new("_Goobstation/Actions/devil.rsi"), "summon-contract"),
            Priority = 1 // Higher priority than default verbs
        };
        args.Verbs.Add(handshakeVerb);
    }

    private void OfferHandshake(EntityUid user, EntityUid target)
    {
        if (HasComp<DevilComponent>(target) || HasComp<PendingHandshakeComponent>(target))
            return;

        var pending = AddComp<PendingHandshakeComponent>(target);
        pending.Offerer = user;
        pending.ExpiryTime = _timing.CurTime + TimeSpan.FromSeconds(15);

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

        AddRandomNegativeClause(target);
    }

    private void AddRandomNegativeClause(EntityUid target)
    {
        var negativeClauses = _prototype.EnumeratePrototypes<DevilClausePrototype>()
            .Where(c => c.ClauseWeight >= 0)
            .ToList();

        if (negativeClauses.Count == 0)
            return;

        var selectedClause = _random.Pick(negativeClauses);
        _contract.ApplyEffectToTarget(target, selectedClause, null);
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
