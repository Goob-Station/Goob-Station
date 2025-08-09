// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Shitmed.BodyEffects;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Forensics;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Goobstation.Shared.Autosurgeon;

// There might be some goidacode inside, I warned you.
// It should also maybe be in _Shitmed instead of here, but who cares.
public sealed class AutoSurgeonSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutoSurgeonComponent, ItemToggleActivateAttemptEvent>(OnActivated);
        SubscribeLocalEvent<AutoSurgeonComponent, AutoSurgeonDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<AutoSurgeonComponent, ExaminedEvent>(OnExamined);
    }

    private void OnActivated(Entity<AutoSurgeonComponent> ent, ref ItemToggleActivateAttemptEvent args)
    {
        _audio.Stop(ent.Comp.ActiveSound);
        args.Cancelled = true;

        if (ent.Comp.Used || args.User == null)
            return;

        if (!_doAfter.TryStartDoAfter(new DoAfterArgs(
                EntityManager,
                ent.Owner,
                ent.Comp.DoAfterTime,
                new AutoSurgeonDoAfterEvent(),
                ent.Owner,
                args.User,
                ent.Owner)
            {
                BreakOnMove = true,
                DistanceThreshold = 0.1f,
                MovementThreshold = 0.1f,
            }))
            return;

        var ev = new TransferDnaEvent { Donor = args.User.Value, Recipient = ent };
        RaiseLocalEvent(args.User.Value, ref ev);

        if (_netManager.IsClient) // Fuck sound networking
            return;

        var sound = _audio.PlayPvs(ent.Comp.Sound, ent);
        if (sound.HasValue)
            ent.Comp.ActiveSound = sound.Value.Entity;
    }

    private void OnDoAfter(Entity<AutoSurgeonComponent> ent, ref AutoSurgeonDoAfterEvent args)
    {
        if (args.Cancelled
        || ent.Comp.Used
        || args.Target == null)
        {
            _audio.Stop(ent.Comp.ActiveSound);
            return;
        }

        var isBodyPart = ent.Comp.TargetOrgan == null;

        // Handle replacing the part
        if (ent.Comp.NewPartProto != null)
        {
            var parent = _body.GetBodyChildrenOfType(args.Target.Value, ent.Comp.TargetBodyPart, symmetry: ent.Comp.TargetBodyPartSymmetry)
                .FirstOrDefault()
                .Id;

            if (!parent.Valid
            || !TryComp<BodyPartComponent>(parent, out var parentComp))
            {
                _audio.Stop(ent.Comp.ActiveSound);
                return;
            }

            var newPart = Spawn(ent.Comp.NewPartProto, Transform(args.Target.Value).Coordinates);

            if (isBodyPart)
            {
                if (!TryComp<BodyPartComponent>(newPart, out var newPartComp)
                || !parentComp.Children.ContainsKey(_body.GetSlotFromBodyPart(newPartComp))) // why is there no method for this
                {
                    Del(newPart);
                    _audio.Stop(ent.Comp.ActiveSound);
                    return;
                }

                var oldPart = _body.GetBodyChildrenOfType(args.Target.Value, newPartComp.PartType, symmetry: newPartComp.Symmetry)
                    .FirstOrDefault()
                    .Id;

                if (oldPart.Valid)
                    _body.DetachPart(parent, _body.GetSlotFromBodyPart(newPartComp), oldPart);

                _body.AttachPart(parent, _body.GetSlotFromBodyPart(newPartComp), newPart);
            }
            else
            {
                if (!TryComp<OrganComponent>(newPart, out var newOrganComp)
                || !_body.CanInsertOrgan(parent, newOrganComp.SlotId))
                {
                    Del(newPart);
                    _audio.Stop(ent.Comp.ActiveSound);
                    return;
                }

                var oldOrgan = _body.GetPartOrgans(parent)
                    .FirstOrDefault(organ => organ.Component.SlotId == newOrganComp.SlotId)
                    .Id;

                if (!_body.AddOrganToFirstValidSlot(parent, newPart) && oldOrgan.Valid)
                {
                    _body.RemoveOrgan(oldOrgan);
                    _body.InsertOrgan(parent, newPart, newOrganComp.SlotId);
                }
            }

            _audio.Stop(ent.Comp.ActiveSound);

            if (ent.Comp.OneTimeUse)
                ent.Comp.Used = true;
            Dirty(ent);
            return;
        }

        // If we didn't replace it, then we upgrade it.
        var part = isBodyPart
            ? _body.GetBodyChildrenOfType(args.Target.Value, ent.Comp.TargetBodyPart, symmetry: ent.Comp.TargetBodyPartSymmetry)
                .FirstOrDefault()
                .Id
            : _body.GetBodyOrgans(args.Target)
                .FirstOrDefault(organ => organ.Component.SlotId == ent.Comp.TargetOrgan)
                .Id;

        if (!part.Valid)
        {
            _audio.Stop(ent.Comp.ActiveSound);
            return;
        }

        var addedToPart = AddComponents(part, ent.Comp.ComponentsToPart); // if none were actually added the part is probably already upgraded

        if (addedToPart != null // null indicates there were no components to add in the first place, so it's fine
        && !addedToPart.Any())
        {
            _audio.Stop(ent.Comp.ActiveSound);
            return;
        }

        if (ent.Comp.TargetOrgan == null)
            HandleBodyPart(args.Target.Value, part, ent.Comp.ComponentsToUser);
        else
            HandleOrgan(args.Target.Value, part, ent.Comp.ComponentsToUser);

        _audio.Stop(ent.Comp.ActiveSound);

        if (ent.Comp.OneTimeUse)
            ent.Comp.Used = true;
        Dirty(ent);
    }

    private void HandleBodyPart(EntityUid user, EntityUid part, ComponentRegistry? comps)
    {
        if (!TryComp<BodyPartComponent>(part, out var partComp)
        || comps == null)
            return;

        var addedToOnAdd = new ComponentRegistry();
        foreach (var (name, data) in comps)
        {
            if (partComp.OnAdd != null)
            {
                if (partComp.OnAdd.TryAdd(name, data))
                    addedToOnAdd.Add(name, data);
            }
            else
            {
                partComp.OnAdd = comps;
                addedToOnAdd = comps;
            }
        }

        var addedToUser = AddComponents(user, addedToOnAdd);
        if (addedToUser == null)
            return;

        var partEffectComp = EnsureComp<BodyPartEffectComponent>(part);
        foreach (var (name, data) in addedToUser)
            partEffectComp.Active.TryAdd(name, data);
    }

    private void HandleOrgan(EntityUid user, EntityUid organ, ComponentRegistry? comps)
    {
        if (!TryComp<OrganComponent>(organ, out var organComp)
        || comps == null)
            return;

        var addedToOnAdd = new ComponentRegistry();
        foreach (var (name, data) in comps)
        {
            if (organComp.OnAdd != null)
            {
                if (organComp.OnAdd.TryAdd(name, data))
                    addedToOnAdd.Add(name, data);
            }
            else
            {
                organComp.OnAdd = comps;
                addedToOnAdd = comps;
            }
        }

        var addedToUser = AddComponents(user, addedToOnAdd);
        if (addedToUser == null)
            return;

        var organEffectComp = EnsureComp<OrganEffectComponent>(organ);
        foreach (var (name, data) in addedToUser)
            organEffectComp.Active.TryAdd(name, data);
    }

    private void OnExamined(Entity<AutoSurgeonComponent> ent, ref ExaminedEvent args) =>
        args.PushMarkup(ent.Comp.Used ? Loc.GetString("gun-cartridge-spent") : Loc.GetString("gun-cartridge-unspent")); // Yes gun locale, and?

    private ComponentRegistry? AddComponents(EntityUid ent, ComponentRegistry? comps) // Returns actually added components
    {
        if (comps == null)
            return null;

        var result = new ComponentRegistry();

        foreach (var (name, data) in comps)
        {
            var newComp = (Component)_componentFactory.GetComponent(name);
            if (HasComp(ent, newComp.GetType()))
                continue;

            newComp.Owner = ent;
            object? temp = newComp;
            _serializationManager.CopyTo(data.Component, ref temp);
            EntityManager.AddComponent(ent, (Component)temp!);

            result.Add(name, data);
        }

        return result;
    }
}
