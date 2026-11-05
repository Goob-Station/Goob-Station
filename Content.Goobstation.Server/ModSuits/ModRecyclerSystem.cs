using System.Numerics;
using Content.Goobstation.Shared.ModSuits;
using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Modules;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Server.Materials;
using Content.Shared.Actions;
using Content.Shared.Item;
using Content.Shared.Materials;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.ModSuits;

public sealed partial class ModRecyclerSystem : VirtualController
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private ActionContainerSystem _actionContainer = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private MaterialStorageSystem _materialStorage = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    private const int MaxProductsPerUnload = 30;

    private readonly HashSet<Entity<ItemComponent>> _items = new();

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<ModRecyclerModuleComponent> ent, ref MapInitEvent args)
    {
        if (!_actionContainer.EnsureAction(ent, ref ent.Comp.UnloadActionEntity, ent.Comp.UnloadAction))
            return;

        _actions.SetEntityIcon(ent.Comp.UnloadActionEntity.Value, ent.Owner);
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnWearerChanged(Entity<ModRecyclerModuleComponent> ent, ref ModSuitWearerChangedEvent args)
    {
        if (args.OldWearer is { } old)
            RemoveUnloadAction(ent, old);

        if (args.NewWearer is { } wearer)
            GrantUnloadAction(ent, wearer);
    }

    [SubscribeLocalEvent]
    private void OnInstalled(Entity<ModRecyclerModuleComponent> ent, ref ModModuleInstalledEvent args)
    {
        if (TryComp<ModSuitComponent>(args.Suit, out var suit) && suit.Wearer is { } wearer)
            GrantUnloadAction(ent, wearer);
    }

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<ModRecyclerModuleComponent> ent, ref ModModuleRemovedEvent args)
    {
        if (_actions.GetAction(ent.Comp.UnloadActionEntity, false) is { } action && action.Comp.AttachedEntity is { } user)
            RemoveUnloadAction(ent, user);
    }

    private void GrantUnloadAction(Entity<ModRecyclerModuleComponent> ent, EntityUid wearer)
    {
        if (_actions.GetAction(ent.Comp.UnloadActionEntity, false) is { } action && action.Comp.AttachedEntity != wearer)
            _actions.AddAction(wearer, action.AsNullable(), ent.Owner);
    }

    private void RemoveUnloadAction(Entity<ModRecyclerModuleComponent> ent, EntityUid user)
    {
        if (_actions.GetAction(ent.Comp.UnloadActionEntity, false) is { } action && action.Comp.AttachedEntity == user)
            _actions.RemoveProvidedAction(user, ent, action);
    }

    [SubscribeLocalEvent]
    private void OnUnload(Entity<ModRecyclerModuleComponent> ent, ref ModRecyclerUnloadActionEvent args)
    {
        if (args.Handled
            || !_modSuit.TryGetModuleSuit(ent, out var suit)
            || suit.Comp.Wearer != args.Performer)
            return;

        args.Handled = true;
        var coords = Transform(args.Performer).Coordinates;

        if (ent.Comp.Product is { } product)
        {
            var count = Math.Min(ent.Comp.Products, MaxProductsPerUnload);
            if (count <= 0)
            {
                _popup.PopupEntity(Loc.GetString("modsuit-recycler-empty"), args.Performer, args.Performer);
                return;
            }

            for (var i = 0; i < count; i++)
            {
                Spawn(product, coords);
            }

            ent.Comp.Products -= count;
            Dirty(ent);
            return;
        }

        if (ent.Comp.Materials.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("modsuit-recycler-empty"), args.Performer, args.Performer);
            return;
        }

        var leftover = new Dictionary<string, int>();
        foreach (var (material, amount) in ent.Comp.Materials)
        {
            _materialStorage.SpawnMultipleFromMaterial(amount, material, coords, out var overflow);
            if (overflow > 0)
                leftover[material] = overflow;
        }

        ent.Comp.Materials = leftover;
        Dirty(ent);
    }

    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<ModRecyclerModuleComponent, ModModuleComponent>();
        while (query.MoveNext(out var uid, out var recycler, out var module))
        {
            if (!module.Active
                || !_modSuit.TryGetModuleSuit(uid, out var suit)
                || suit.Comp.Wearer is not { } wearer
                || _container.IsEntityInContainer(wearer))
                continue;

            var collect = now >= recycler.NextCollect;
            if (collect)
                recycler.NextCollect = now + recycler.Interval;

            if (Pull((uid, recycler), wearer, collect))
                Dirty(uid, recycler);
        }
    }

    private bool Pull(Entity<ModRecyclerModuleComponent> ent, EntityUid wearer, bool collect)
    {
        var collected = false;
        var wearerPos = TransformSystem.GetMapCoordinates(wearer);
        _items.Clear();
        _lookup.GetEntitiesInRange(wearerPos, ent.Comp.Range, _items, LookupFlags.Dynamic | LookupFlags.Sundries);
        foreach (var item in _items)
        {
            if (_container.IsEntityInContainer(item)
                || Transform(item).Anchored
                || HasComp<MobStateComponent>(item)
                || !_whitelist.IsWhitelistPass(ent.Comp.Whitelist, item))
                continue;

            var offset = wearerPos.Position - TransformSystem.GetMapCoordinates(item).Position;
            var distance = offset.Length();
            if (distance > ent.Comp.CatchDistance)
            {
                if (TryComp<PhysicsComponent>(item, out var body))
                    PhysicsSystem.SetLinearVelocity(item, offset / distance * ent.Comp.PullSpeed, body: body);
                continue;
            }

            if (TryComp<PhysicsComponent>(item, out var caught))
                PhysicsSystem.SetLinearVelocity(item, Vector2.Zero, body: caught);

            if (!collect)
                continue;

            Recycle(ent, item);
            collected = true;
        }

        if (collected)
            _audio.PlayPvs(ent.Comp.Sound, wearer);

        return collected;
    }

    private void Recycle(Entity<ModRecyclerModuleComponent> ent, EntityUid item)
    {
        if (ent.Comp.Product != null)
        {
            ent.Comp.Products++;
        }
        else if (TryComp<PhysicalCompositionComponent>(item, out var composition))
        {
            foreach (var (material, amount) in composition.MaterialComposition)
            {
                ent.Comp.Materials[material] = ent.Comp.Materials.GetValueOrDefault(material) + amount;
            }
        }

        QueueDel(item);
    }
}
