using Content.Goobstation.Common.Weapons.MeleeDash;
using Content.Goobstation.Shared.Capo.Components;
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Body.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Reflect;
using Content.Shared.Wieldable;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Capo.Systems;

public sealed class CaposFullSetEffectSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

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

        var tracker = EnsureComp<CaposFullSetTrackerComponent>(player);
        tracker.Count = countEvent.Count;

        if (tracker.Count >= 5)
            AddFullSetEffect(player);
        else
            RemoveFullSetEffect(player);
    }

    private void OnCount(Entity<CaposoutfitComponent> ent, ref InventoryRelayedEvent<CaposOutfitCountEvent> args)
    {
        args.Args.Count++;
    }

    private void AddFullSetEffect(EntityUid player)
    {
        _movementSpeedModifierSystem.ChangeBaseSpeed(player, 5, 5, 20);

        var thermal = EnsureComp<ThermalVisionComponent>(player);
        thermal.LightRadius = 15;
        thermal.Color = Color.FromHex("#ffffff");

        foreach (var held in _hands.EnumerateHeld(player))
        {
            if (TryComp<TigersClawComponent>(held, out _) && TryComp<ReflectComponent>(held, out var reflect))
            {
                reflect.ReflectProb = 0f;
                Dirty(held, reflect);
            }
        }
    }

    private void RemoveFullSetEffect(EntityUid player)
    {
        RemComp<ThermalVisionComponent>(player);
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

        if (TryComp<MeleeWeaponComponent>(ent, out var melee))
        {
            melee.Animation = "WeaponArcThrust";
            Dirty(ent, melee);
        }

        if (HasComp<MeleeDashComponent>(ent))
            RemComp<MeleeDashComponent>(ent);
    }

    private void OnWield(Entity<TigersClawComponent> ent, ref ItemWieldedEvent args)
    {
        var user = args.User;
        if (user == EntityUid.Invalid)
            return;

        if (TryComp<MeleeWeaponComponent>(ent, out var melee))
        {
            melee.Animation = "WeaponTigersClawHit";
            Dirty(ent, melee);
        }

        if (!TryComp<MeleeDashComponent>(ent, out var dash))
        {
            dash = AddComp<MeleeDashComponent>(ent);
            dash.DoAfter = 0.6f;
            dash.DashSprite = "ability-icon";
            Dirty(ent, dash);
        }

        if (TryComp<CaposFullSetTrackerComponent>(user, out var tracker) && tracker.Count >= 5 && TryComp<ReflectComponent>(ent, out var reflect))
        {
            reflect.ReflectProb = 0.4f;
            Dirty(ent, reflect);
        }
    }
}

public sealed class CaposOutfitCountEvent : IInventoryRelayEvent
{
    public int Count = 0;
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}
