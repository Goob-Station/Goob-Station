using Content.Goobstation.Shared.Capo.Components;
using Content.Shared.Inventory;
using Content.Shared.Body.Systems;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Weapons.Reflect;
using Content.Shared.Item;
using Content.Shared.Wieldable;
using Robust.Shared.Log;
using Robust.Shared.Timing;
using System.Collections.Generic;

namespace Content.Goobstation.Shared.CaposOutfit;

public sealed class CaposFullSetEffectSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    private readonly Dictionary<EntityUid, int> _capoPieceCounts = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CaposoutfitComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<CaposoutfitComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<CaposoutfitComponent, InventoryRelayedEvent<CaposOutfitCountEvent>>(OnCount);
        SubscribeLocalEvent<TigersClawComponent, ItemWieldedEvent>(OnWield);
        SubscribeLocalEvent<TigersClawComponent, ItemUnwieldedEvent>(OnUnwield);
    }

    private void OnEquipped(Entity<CaposoutfitComponent> ent, ref GotEquippedEvent args)
    {
        if (args.Equipee != EntityUid.Invalid)
            RecountCapoPieces(args.Equipee);
    }

    private void OnUnequipped(Entity<CaposoutfitComponent> ent, ref GotUnequippedEvent args)
    {
        if (args.Equipee != EntityUid.Invalid)
            RecountCapoPieces(args.Equipee);
    }

    private void RecountCapoPieces(EntityUid player)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (!TryComp<InventoryComponent>(player, out var inv))
            return;

        var countEvent = new CaposOutfitCountEvent();
        _inventory.RelayEvent((player, inv), countEvent);

        _capoPieceCounts[player] = countEvent.Count;

        if (countEvent.Count >= 5)
            AddFullSetEffect(player);
        else
            RemoveFullSetEffect(player);
    }

    private void AddFullSetEffect(EntityUid player)
    {
        _movementSpeedModifierSystem.ChangeBaseSpeed(player, 25, 25, 25);
        foreach (var held in _hands.EnumerateHeld(player))
        {
            if (TryComp<TigersClawComponent>(held, out _) && TryComp<ReflectComponent>(held, out var reflect))
            {
                reflect.ReflectProb = 0.7f;
                Dirty(held, reflect);
            }
        }
    }

    private void RemoveFullSetEffect(EntityUid player)
    {
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(player);
        _bodySystem.UpdateMovementSpeed(player);

        foreach (var held in _hands.EnumerateHeld(player))
        {
            if (TryComp<TigersClawComponent>(held, out _) && TryComp<ReflectComponent>(held, out var reflect))
            {
                reflect.ReflectProb = 0f;
                Dirty(held, reflect);
            }
        }
    }

    private void OnUnwield(Entity<TigersClawComponent> ent, ref ItemUnwieldedEvent args)
    {
        if (TryComp<ReflectComponent>(ent, out var reflect))
        {
            reflect.ReflectProb = 0f;
            Dirty(ent, reflect);
        }
    }

    private void OnWield(Entity<TigersClawComponent> ent, ref ItemWieldedEvent args)
    {
        var user = args.User;
        if (user == EntityUid.Invalid)
            return;

        if (_capoPieceCounts.TryGetValue(user, out var count) && count >= 5)
        {
            if (TryComp<ReflectComponent>(ent, out var reflect))
            {
                reflect.ReflectProb = 0.7f;
                Dirty(ent, reflect);
            }
        }
    }

    private void OnCount(Entity<CaposoutfitComponent> ent, ref InventoryRelayedEvent<CaposOutfitCountEvent> args)
    {
        args.Args.Count++;
    }
}

public sealed class CaposOutfitCountEvent : IInventoryRelayEvent
{
    public int Count = 0;
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}
