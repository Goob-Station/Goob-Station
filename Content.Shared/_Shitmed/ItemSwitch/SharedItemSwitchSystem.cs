// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared._Shitmed.ItemSwitch.Components;
using Content.Shared._Shitmed.Switchable;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Containers;

namespace Content.Shared._Shitmed.ItemSwitch;
public abstract class SharedItemSwitchSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private EntityQuery<ItemSwitchComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<ItemSwitchComponent>();

        SubscribeLocalEvent<ItemSwitchComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ItemSwitchComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<ItemSwitchComponent, GetVerbsEvent<ActivationVerb>>(OnActivateVerb);
        SubscribeLocalEvent<ItemSwitchComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<ItemSwitchComponent, ItemSwitchAttemptEvent>(OnSwitchAttempt);

        SubscribeLocalEvent<ClothingComponent, ItemSwitchedEvent>(UpdateClothingLayer);
    }

    private void OnInit(Entity<ItemSwitchComponent> ent, ref ComponentInit args)
    {
        Switch((ent, ent.Comp), ent.Comp.State, predicted: ent.Comp.Predictable);
    }

    private void OnSwitchAttempt(EntityUid uid, ItemSwitchComponent comp, ref ItemSwitchAttemptEvent args)
    {
        if (comp.IsPowered || !comp.NeedsPower || comp.State != comp.DefaultState)
            return;

        args.Popup = Loc.GetString("item-switch-failed-no-power");
        args.Cancelled = true;
        Dirty(uid, comp);
    }

    private void OnUseInHand(Entity<ItemSwitchComponent> ent, ref UseInHandEvent args)
    {
        var comp = ent.Comp;

        if (args.Handled || !comp.OnUse || comp.States.Count == 0)
            return;

        args.Handled = true;

        if (comp.States.TryGetValue(Next(ent), out var state) && state.Hidden)
            return;

        Switch((ent, comp), Next(ent), args.User, predicted: comp.Predictable);
    }

    private void OnActivateVerb(Entity<ItemSwitchComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !ent.Comp.OnActivate || ent.Comp.States.Count == 0)
            return;

        var user = args.User;
        var addedVerbs = 0;

        foreach (var state in ent.Comp.States.Where(state => !state.Value.Hidden)) // I'm linq-ing all over the place.
        {
            if (state.Value.Verb == null)
                continue;
            args.Verbs.Add(new ActivationVerb()
            {
                Text = Loc.TryGetString(state.Value.Verb, out var title) ? title : state.Value.Verb,
                Category = VerbCategory.Switch,
                Act = () => Switch((ent.Owner, ent.Comp), state.Key, user, ent.Comp.Predictable)
            });
            addedVerbs++;
        }

        if (addedVerbs > 0)
            args.ExtraCategories.Add(VerbCategory.Switch);
    }

    private void OnActivate(Entity<ItemSwitchComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !ent.Comp.OnActivate || ent.Comp is { IsPowered: false, NeedsPower: true })
            return;

        args.Handled = true;

        if (ent.Comp.States.TryGetValue(Next(ent), out var state) && state.Hidden)
            return;

        Switch((ent.Owner, ent.Comp), Next(ent), args.User, predicted: ent.Comp.Predictable);
    }

    private static string Next(Entity<ItemSwitchComponent> ent)
    {
        var foundCurrent = false;
        foreach (var state in ent.Comp.States.Keys)
        {
            if (foundCurrent)
                return state;

            if (state == ent.Comp.State)
                foundCurrent = true;
        }

        return ent.Comp.States.Keys.First();
    }

    /// <summary>
    /// Used when an item is attempted to be toggled.
    /// Sets its state to the opposite of what it is.
    /// </summary>
    /// <returns>false if the attempt fails for any reason</returns>
    public bool Switch(Entity<ItemSwitchComponent?> ent, string? key, EntityUid? user = null, bool predicted = true)
    {
        if (key == null
            || !_query.Resolve(ent, ref ent.Comp, false)
            || !ent.Comp.States.TryGetValue(key, out var state))
            return false;

        var uid = ent.Owner;
        var comp = ent.Comp;

        if (!comp.Predictable && _netManager.IsClient)
            return true;

        var attempt = new ItemSwitchAttemptEvent
        {
            User = user,
            State = key,
        };
        RaiseLocalEvent(uid, ref attempt);

        var nextAttack = new TimeSpan(0);
        if (TryComp<MeleeWeaponComponent>(ent, out var meleeComp))
            nextAttack = meleeComp.NextAttack;

        if (ent.Comp.States.TryGetValue(ent.Comp.State, out var prevState)
            && prevState is { RemoveComponents: true, Components: not null })
            EntityManager.RemoveComponents(ent, prevState.Components);

        if (state.Components is not null)
            EntityManager.AddComponents(ent, state.Components);

        if (TryComp(ent, out meleeComp)
            && nextAttack.Ticks != 0)
            meleeComp.NextAttack = nextAttack;

        if (!comp.Predictable)
            predicted = false;

        if (attempt.Cancelled)
        {
            if (predicted)
                _audio.PlayPredicted(state.SoundFailToActivate, uid, user);
            else
                _audio.PlayPvs(state.SoundFailToActivate, uid);

            if (attempt.Popup == null || user == null)
                return false;

            if (predicted)
                _popup.PopupClient(attempt.Popup, uid, user.Value);
            else
                _popup.PopupEntity(attempt.Popup, uid, user.Value);

            return false;
        }

        if (predicted)
            _audio.PlayPredicted(state.SoundStateActivate, uid, user);
        else
            _audio.PlayPvs(state.SoundStateActivate, uid);

        if (TryComp<ItemComponent>(uid, out var item) && _container.TryGetContainingContainer((uid, null, null), out var container))
        {
            if (TryComp(container.Owner, out StorageComponent? storage))
            {
                _transform.AttachToGridOrMap(uid);
                if (!_storage.Insert(container.Owner, uid, out _, null, storage, false))
                    _hands.PickupOrDrop(user, uid, animate: false);
            }
            else if (HasComp<InventoryComponent>(container.Owner) && _item.GetSizePrototype(item.Size) > _item.GetSizePrototype(InventorySystem.PocketableItemSize))
            {
                var enumerator = _inventory.GetSlotEnumerator(container.Owner, SlotFlags.POCKET);
                while (enumerator.NextItem(out var slotItem))
                {
                    if (slotItem != uid)
                        continue;

                    _transform.AttachToGridOrMap(uid);
                    _hands.PickupOrDrop(user, uid, animate: false);
                    break;
                }
            }
        }

        comp.State = key;
        UpdateVisuals((uid, comp), key);
        Dirty(uid, comp);

        var switched = new ItemSwitchedEvent { Predicted = predicted, State = key, User = user };
        RaiseLocalEvent(uid, ref switched);

        return true;
    }
    public virtual void VisualsChanged(Entity<ItemSwitchComponent> ent, string key)
    {
    }
    protected virtual void UpdateVisuals(Entity<ItemSwitchComponent> ent, string key)
    {
        if (TryComp(ent, out AppearanceComponent? appearance))
            _appearance.SetData(ent, SwitchableVisuals.Switched, key, appearance);
        _item.SetHeldPrefix(ent, key);

        VisualsChanged(ent, key);
    }
    private void UpdateClothingLayer(Entity<ClothingComponent> ent, ref ItemSwitchedEvent args)
    {
        _clothing.SetEquippedPrefix(ent, args.State, ent.Comp);
    }
}
