// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Goobstation.Shared.Bible;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.RandomizeMovementSpeed;

public sealed class RandomizeMovementSpeedSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = null!;

    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromSeconds(3);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMovementSpeedModifiers);
    }

    #region Helper Functions
    private void OnGotEquippedHand(Entity<RandomizeMovementspeedComponent> ent, ref GotEquippedHandEvent args)
    {
        // Refresh the movement speed modifiers.
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.User);
        // Get the Uid of the entity who picked up the item.
        GetEntityUid(ent, ref args);
        ent.Comp.NextExecutionTime = _timing.CurTime;
    }

    private void OnGotUnequippedHand(Entity<RandomizeMovementspeedComponent> ent, ref GotUnequippedHandEvent args)
    {
        // Refresh the movement speed modifiers.
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.User);
        // Reset the user Uid.
        ent.Comp.EntityUid = null;
    }

    private void GetEntityUid(Entity<RandomizeMovementspeedComponent> ent, ref GotEquippedHandEvent args)
    {
        // Set the entity Uid field of the Component equal to the entity who picked up the item.
        ent.Comp.EntityUid = args.User;
    }

    private float GetMovementSpeedModifiers(RandomizeMovementspeedComponent comp)
    {
        // Generate a modifier, which is a float between the minimum and maxiumum defined by the component.
        var modifier = _random.NextFloat(comp.Min, comp.Max);
        // Return that modifier.
        return modifier;

    }
    private void OnRefreshMovementSpeedModifiers(EntityUid uid, RandomizeMovementspeedComponent  comp, ref HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        args.Args.ModifySpeed(comp.CurrentModifier);
        Dirty(uid, comp);
    }

    #endregion

    #region Update Loop
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RandomizeMovementspeedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Whitelist == null || !_whitelist.IsValid(comp.Whitelist, comp.EntityUid))
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

