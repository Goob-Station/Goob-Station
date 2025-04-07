// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Júlio César Ueti <52474532+Mirino97@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Willhelm53 <97707302+Willhelm53@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Vasilis The Pikachu <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Bed.Sleep;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.Interaction;

public sealed class InteractionPopupSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly INetManager _netMan = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InteractionPopupComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<InteractionPopupComponent, ActivateInWorldEvent>(OnActivateInWorld);
    }

    private void OnActivateInWorld(EntityUid uid, InteractionPopupComponent component, ActivateInWorldEvent args)
    {
        if (!args.Complex)
            return;

        if (!component.OnActivate)
            return;

        SharedInteract(uid, component, args, args.Target, args.User);
    }

    private void OnInteractHand(EntityUid uid, InteractionPopupComponent component, InteractHandEvent args)
    {
        SharedInteract(uid, component, args, args.Target, args.User);
    }

    private void SharedInteract(
        EntityUid uid,
        InteractionPopupComponent component,
        HandledEntityEventArgs args,
        EntityUid target,
        EntityUid user)
    {
        if (args.Handled || user == target)
            return;

        //Handling does nothing and this thing annoyingly plays way too often.
        // HUH? What does this comment even mean?

        if (HasComp<SleepingComponent>(uid))
            return;

        if (TryComp<MobStateComponent>(uid, out var state)
            && !_mobStateSystem.IsAlive(uid, state))
        {
            return;
        }

        args.Handled = true;

        var curTime = _gameTiming.CurTime;

        if (curTime < component.LastInteractTime + component.InteractDelay)
            return;

        component.LastInteractTime = curTime;

        // TODO: Should be an attempt event
        // TODO: Need to handle pausing with an accumulator.

        var msg = ""; // Stores the text to be shown in the popup message
        SoundSpecifier? sfx = null; // Stores the filepath of the sound to be played

        var predict = component.SuccessChance is 0 or 1
                      && component.InteractSuccessSpawn == null
                      && component.InteractFailureSpawn == null;

        if (_netMan.IsClient && !predict)
            return;

        if (_random.Prob(component.SuccessChance))
        {
            if (component.InteractSuccessString != null)
                msg = Loc.GetString(component.InteractSuccessString, ("target", Identity.Entity(uid, EntityManager))); // Success message (localized).

            if (component.InteractSuccessSound != null)
                sfx = component.InteractSuccessSound;

            if (component.InteractSuccessSpawn != null)
                Spawn(component.InteractSuccessSpawn, _transform.GetMapCoordinates(uid));

            var ev = new InteractionSuccessEvent(user);
            RaiseLocalEvent(target, ref ev);
        }
        else
        {
            if (component.InteractFailureString != null)
                msg = Loc.GetString(component.InteractFailureString, ("target", Identity.Entity(uid, EntityManager))); // Failure message (localized).

            if (component.InteractFailureSound != null)
                sfx = component.InteractFailureSound;

            if (component.InteractFailureSpawn != null)
                Spawn(component.InteractFailureSpawn, _transform.GetMapCoordinates(uid));

            var ev = new InteractionFailureEvent(user);
            RaiseLocalEvent(target, ref ev);
        }

        if (!string.IsNullOrEmpty(component.MessagePerceivedByOthers))
        {
            var msgOthers = Loc.GetString(component.MessagePerceivedByOthers,
                ("user", Identity.Entity(user, EntityManager)), ("target", Identity.Entity(uid, EntityManager)));
            _popupSystem.PopupEntity(msgOthers, uid, Filter.PvsExcept(user, entityManager: EntityManager), true);
        }

        if (!predict)
        {
            _popupSystem.PopupEntity(msg, uid, user);

            if (component.SoundPerceivedByOthers)
                _audio.PlayPvs(sfx, target);
            else
                _audio.PlayEntity(sfx, Filter.Entities(user, target), target, false);
            return;
        }

        _popupSystem.PopupClient(msg, uid, user);

        if (sfx == null)
            return;

        if (component.SoundPerceivedByOthers)
        {
            _audio.PlayPredicted(sfx, target, user);
            return;
        }

        if (_netMan.IsClient)
        {
            if (_gameTiming.IsFirstTimePredicted)
                _audio.PlayEntity(sfx, Filter.Local(), target, true);
        }
        else
        {
            _audio.PlayEntity(sfx, Filter.Empty().FromEntities(target), target, false);
        }
    }

    /// <summary>
    /// Sets <see cref="InteractionPopupComponent.InteractSuccessString"/>.
    /// </summary>
    /// <para>
    /// This field is not networked automatically, so this method must be called on both sides of the network.
    /// </para>
    public void SetInteractSuccessString(Entity<InteractionPopupComponent> ent, string str)
    {
        ent.Comp.InteractSuccessString = str;
    }

    /// <summary>
    /// Sets <see cref="InteractionPopupComponent.InteractFailureString"/>.
    /// </summary>
    /// <para>
    /// This field is not networked automatically, so this method must be called on both sides of the network.
    /// </para>
    public void SetInteractFailureString(Entity<InteractionPopupComponent> ent, string str)
    {
        ent.Comp.InteractFailureString = str;
    }
}