using System.Linq;
using Content.Shared._Shitmed.BodyEffects;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Goobstation.Shared.Cybernetics;

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
        SubscribeLocalEvent<PartUpgraderComponent, ItemToggleDeactivateAttemptEvent>(OnDeactivated);
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

        if (_netManager.IsClient)
            return;

        var sound = _audio.PlayPvs(ent.Comp.Sound, ent);
        if (sound.HasValue)
            ent.Comp.ActiveSound = sound.Value.Entity;
    }

    private void OnDeactivated(Entity<PartUpgraderComponent> ent, ref ItemToggleDeactivateAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnDoAfter(Entity<PartUpgraderComponent> ent, ref PartUpgraderDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
        {
            Del(ent.Comp.ActiveSound);
            _toggle.TryDeactivate(ent.Owner);
            args.Handled = true;
            return;
        }

        var part = ent.Comp.TargetOrgan == null
            ? _body.GetBodyChildrenOfType(args.Target.Value, ent.Comp.TargetBodyPart, symmetry: ent.Comp.TargetBodyPartSymmetry).FirstOrDefault().Id
            : _body.GetBodyOrgans(args.Target).FirstOrDefault(organ => organ.Component.SlotId == ent.Comp.TargetOrgan).Id;

        if (!part.Valid)
        {
            Del(ent.Comp.ActiveSound);
            _toggle.TryDeactivate(ent.Owner);
            args.Handled = true;
            return;
        }

        AddComponents(part, ent.Comp.ComponentsToPart);

        if (ent.Comp.TargetOrgan == null)
            HandleBodyPart(args.Target.Value, part, ent.Comp.ComponentsToUser);
        else
            HandleOrgan(args.Target.Value, part, ent.Comp.ComponentsToUser);

        Del(ent.Comp.ActiveSound);
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
        if (ent.Comp.Used)
        {
            args.PushMarkup(Loc.GetString("gun-cartridge-spent"));
        }
        else
        {
            args.PushMarkup(Loc.GetString("gun-cartridge-unspent"));
        }
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
            var temp = (object)newComp;
            _serializationManager.CopyTo(data.Component, ref temp);
            EntityManager.AddComponent(ent, (Component)temp!);

            result.Add(name, data);
        }

        return result;
    }
}
