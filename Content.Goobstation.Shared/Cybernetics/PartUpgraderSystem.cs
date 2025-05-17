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
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Goobstation.Shared.Cybernetics;

// There might be some goidacode inside, I warned you.
public sealed class PartUpgraderSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PartUpgraderComponent, ItemToggleActivateAttemptEvent>(OnActivated);
        SubscribeLocalEvent<PartUpgraderComponent, PartUpgraderDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<PartUpgraderComponent, ExaminedEvent>(OnExamined);
    }

    private void OnActivated(Entity<PartUpgraderComponent> ent, ref ItemToggleActivateAttemptEvent args)
    {
        if (args.User == null || ent.Comp.Used || !_doAfter.TryStartDoAfter(new DoAfterArgs(
                    EntityManager,
                    ent.Owner,
                    ent.Comp.DoAfterTime,
                    new PartUpgraderDoAfterEvent(),
                    ent.Owner,
                    args.User,
                    ent.Owner)
                {
                    MovementThreshold = 0.2f,
                    BreakOnMove = true,
                }))
        {
            args.Cancelled = true;
            return;
        }

        if (_netManager.IsClient) // Fuck sound networking
            return;

        var sound = _audio.PlayPvs(ent.Comp.Sound, ent);
        if (sound.HasValue)
            ent.Comp.ActiveSound = sound.Value.Entity;
    }

    private void OnDoAfter(Entity<PartUpgraderComponent> ent, ref PartUpgraderDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
        {
            _audio.Stop(ent.Comp.ActiveSound);
            _toggle.TryDeactivate(ent.Owner);
            args.Handled = true;
            return;
        }

        var part = ent.Comp.TargetOrgan == null
            ? _body.GetBodyChildrenOfType(args.Target.Value, ent.Comp.TargetBodyPart, symmetry: ent.Comp.TargetBodyPartSymmetry).FirstOrDefault().Id
            : _body.GetBodyOrgans(args.Target).FirstOrDefault(organ => organ.Component.SlotId == ent.Comp.TargetOrgan).Id;

        if (!part.Valid)
        {
            _audio.Stop(ent.Comp.ActiveSound);
            _toggle.TryDeactivate(ent.Owner);
            args.Handled = true;
            return;
        }

        var addedToPart = AddComponents(part, ent.Comp.ComponentsToPart); // if none were actually added the part is probably already modified
        if (addedToPart != null && !addedToPart.Any()) // null indicates there were no components to add in the first place, so it's fine
        {
            _audio.Stop(ent.Comp.ActiveSound);
            _toggle.TryDeactivate(ent.Owner);
            args.Handled = true;
            return;
        }

        if (ent.Comp.TargetOrgan == null)
            HandleBodyPart(args.Target.Value, part, ent.Comp.ComponentsToUser);
        else
            HandleOrgan(args.Target.Value, part, ent.Comp.ComponentsToUser);

        _audio.Stop(ent.Comp.ActiveSound);
        _toggle.TryDeactivate(ent.Owner);
        args.Handled = true;

        if (ent.Comp.OneTimeUse)
            ent.Comp.Used = true;
        Dirty(ent);
    }

    private void HandleBodyPart(EntityUid user, EntityUid part, ComponentRegistry? comps)
    {
        if (!TryComp<BodyPartComponent>(part, out var partComp) || comps == null)
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
        {
            partEffectComp.Active.TryAdd(name, data);
        }
    }

    private void HandleOrgan(EntityUid user, EntityUid organ, ComponentRegistry? comps)
    {
        if (!TryComp<OrganComponent>(organ, out var organComp) || comps == null)
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
        {
            organEffectComp.Active.TryAdd(name, data);
        }
    }

    private void OnExamined(Entity<PartUpgraderComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(ent.Comp.Used ? Loc.GetString("gun-cartridge-spent") : Loc.GetString("gun-cartridge-unspent")); // Yes gun locale, and?
    }

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
