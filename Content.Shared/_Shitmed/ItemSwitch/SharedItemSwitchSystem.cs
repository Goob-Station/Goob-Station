// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
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
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Content.Shared.Weapons.Melee;

namespace Content.Shared._Shitmed.ItemSwitch;
public abstract class SharedItemSwitchSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;

    private EntityQuery<ItemSwitchComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<ItemSwitchComponent>();

        //SubscribeLocalEvent<ItemSwitchComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ItemSwitchComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ItemSwitchComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<ItemSwitchComponent, GetVerbsEvent<ActivationVerb>>(OnActivateVerb);
        SubscribeLocalEvent<ItemSwitchComponent, ActivateInWorldEvent>(OnActivate);

        SubscribeLocalEvent<ClothingComponent, ItemSwitchedEvent>(UpdateClothingLayer);
    }


    /*private void OnStartup(Entity<ItemSwitchComponent> ent, ref ComponentStartup args)
    {
        var state = ent.Comp.State;
        state ??= ent.Comp.States.Keys.FirstOrDefault();
        if (state != null)
            Switch((ent, ent.Comp), state, predicted: ent.Comp.Predictable);
    }*/

    private void OnMapInit(Entity<ItemSwitchComponent> ent, ref MapInitEvent args)
    {
        var state = ent.Comp.State;
        state ??= ent.Comp.States.Keys.FirstOrDefault();
        if (state != null)
            Switch((ent, ent.Comp), state, predicted: ent.Comp.Predictable);
    }

    private void OnUseInHand(Entity<ItemSwitchComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled || !ent.Comp.OnUse || ent.Comp.States.Count == 0) return;
        args.Handled = true;

        if (ent.Comp.States.TryGetValue(Next(ent), out var state) && state.Hidden)
            return;

        Switch((ent, ent.Comp), Next(ent), args.User, predicted: ent.Comp.Predictable);
    }

    private void OnActivateVerb(Entity<ItemSwitchComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !ent.Comp.OnActivate || ent.Comp.States.Count == 0) return;

        var user = args.User;
        int addedVerbs = 0;

        foreach (var state in ent.Comp.States)
        {
            if (state.Value.Hidden)
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
        if (args.Handled || !ent.Comp.OnActivate)
            return;

        args.Handled = true;

        if (ent.Comp.States.TryGetValue(Next(ent), out var state) && state.Hidden)
            return;

        Switch((ent.Owner, ent.Comp), Next(ent), args.User, predicted: ent.Comp.Predictable);
    }

    private static string Next(Entity<ItemSwitchComponent> ent)
    {
        var foundCurrent = false;
        string firstState = null!;

        foreach (var state in ent.Comp.States.Keys)
        {
            firstState ??= state;

            if (foundCurrent)
                return state;

            if (state == ent.Comp.State)
                foundCurrent = true;
        }
        return firstState;
    }

    /// <summary>
    /// Used when an item is attempted to be toggled.
    /// Sets its state to the opposite of what it is.
    /// </summary>
    /// <returns>Same as <see cref="TrySetActive"/></returns>
    public bool Switch(Entity<ItemSwitchComponent?> ent, string key, EntityUid? user = null, bool predicted = true)
    {
        if (!_query.Resolve(ent, ref ent.Comp, false) || !ent.Comp.States.TryGetValue(key, out var state))
            return false;

        var uid = ent.Owner;
        var comp = ent.Comp;

        if (!comp.Predictable && _netManager.IsClient)
            return true;

        var attempt = new ItemSwitchAttemptEvent
        {
            User = user,
            State = key
        };
        RaiseLocalEvent(uid, ref attempt);

        TimeSpan nextAttack = new TimeSpan(0);
        if (TryComp<MeleeWeaponComponent>(ent, out var meleeComp))
            nextAttack = meleeComp.NextAttack;

        if (ent.Comp.States.TryGetValue(ent.Comp.State, out var prevState)
            && prevState.RemoveComponents
            && prevState.Components is not null)
            EntityManager.RemoveComponents(ent, prevState.Components);

        if (state.Components is not null)
            EntityManager.AddComponents(ent, state.Components);

        if (TryComp<MeleeWeaponComponent>(ent, out meleeComp) && nextAttack.Ticks != 0)
            meleeComp.NextAttack = nextAttack;

        if (!comp.Predictable) predicted = false;

        if (attempt.Cancelled)
        {
            if (predicted)
                _audio.PlayPredicted(state.SoundFailToActivate, uid, user);
            else
                _audio.PlayPvs(state.SoundFailToActivate, uid);

            if (attempt.Popup != null && user != null)
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
        => _clothing.SetEquippedPrefix(ent, args.State, ent.Comp);
}