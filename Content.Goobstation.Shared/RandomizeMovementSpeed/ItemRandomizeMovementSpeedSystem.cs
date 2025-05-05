// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Movement.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.RandomizeMovementSpeed;

public sealed class ItemRandomizeMovementSpeedSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItemRandomizeMovementspeedComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<ItemRandomizeMovementspeedComponent, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent>>(
            OnRefreshMovementSpeedModifiers);
    }

    private void OnGotEquippedHand(EntityUid uid, ItemRandomizeMovementspeedComponent comp, GotEquippedHandEvent args)
    {
        comp.EntityUid = args.User;
    }

    private void OnRefreshMovementSpeedModifiers(EntityUid uid, ItemRandomizeMovementspeedComponent comp, ref HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        args.Args.ModifySpeed(comp.CurrentModifier);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ItemRandomizeMovementspeedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.CurrentModifier = MathHelper.Lerp(comp.CurrentModifier, comp.TargetModifier, frameTime / comp.SmoothingTime);

            if (comp.Whitelist is not { } whitelist
                || comp.EntityUid is not { } user
                || !_whitelist.IsValid(whitelist, user)
                || _timing.CurTime < comp.NextExecutionTime)
                continue;

            if (!_hands.IsHolding(user, uid))
            {
                _movementSpeedModifier.RefreshMovementSpeedModifiers(user);
                comp.EntityUid = null;
            }

            _movementSpeedModifier.RefreshMovementSpeedModifiers(user);
            comp.TargetModifier = _random.NextFloat(comp.Min, comp.Max);

            Dirty(uid, comp);
            comp.NextExecutionTime = _timing.CurTime + comp.ExecutionInterval;
        }

    }
}
