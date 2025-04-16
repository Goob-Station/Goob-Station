// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands;
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

    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromSeconds(3);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItemRandomizeMovementspeedComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ItemRandomizeMovementspeedComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<ItemRandomizeMovementspeedComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);
        SubscribeLocalEvent<ItemRandomizeMovementspeedComponent, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMovementSpeedModifiers);
    }

    #region Main Methods

    private void OnStartup(EntityUid uid, ItemRandomizeMovementspeedComponent comp, MapInitEvent args)
    {
        if (comp.Min < comp.Max)
            return;

        (comp.Max, comp.Min) = (comp.Min, comp.Max);
    }
    private void OnGotEquippedHand(EntityUid uid, ItemRandomizeMovementspeedComponent comp, GotEquippedHandEvent args)
    {
        // Refresh the movement speed modifiers.
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.User);

        // Track the UID of the entity who is holding the item so we can properly remove the effects.
        comp.EntityUid = args.User;
        comp.NextExecutionTime = _timing.CurTime;
    }

    private void OnGotUnequippedHand(EntityUid uid, ItemRandomizeMovementspeedComponent comp, GotUnequippedHandEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.User);
        comp.EntityUid = null;
    }

    private float GetMovementSpeedModifiers(ItemRandomizeMovementspeedComponent comp)
    {
        return _random.NextFloat(comp.Min, comp.Max);
    }
    private void OnRefreshMovementSpeedModifiers(EntityUid uid, ItemRandomizeMovementspeedComponent  comp, ref HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        args.Args.ModifySpeed(comp.CurrentModifier);
        Dirty(uid, comp);
    }

    #endregion

    #region Update Loop
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ItemRandomizeMovementspeedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Whitelist == null || TerminatingOrDeleted(comp.EntityUid) || !_whitelist.IsValid(comp.Whitelist, comp.EntityUid))
                continue;

            if (_timing.CurTime < comp.NextExecutionTime)
                continue;

            var modifier = GetMovementSpeedModifiers(comp);
            comp.CurrentModifier = modifier;

            _movementSpeedModifier.RefreshMovementSpeedModifiers((EntityUid)comp.EntityUid);
            Dirty(uid, comp);

            comp.NextExecutionTime = _timing.CurTime + ExecutionInterval;
        }

    }

    #endregion


}

