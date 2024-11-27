using System.Diagnostics.CodeAnalysis;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Robust.Shared.Containers;

// Shitmed Change

using Content.Shared.Damage;
using Content.Shared._Shitmed.BodyEffects;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared._Shitmed.Body.Part;

namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem
{
    // Shitmed Change Start

    private void InitializeOrgans()
    {
        SubscribeLocalEvent<OrganComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<OrganComponent, OrganEnableChangedEvent>(OnOrganEnableChanged);

        SubscribeLocalEvent<BodyComponent, ComponentRemove>(OnBodyRemoved);
    }

    private void OnMapInit(Entity<OrganComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.OnAdd is not null || ent.Comp.OnRemove is not null)
            EnsureComp<OrganEffectComponent>(ent);
    }

    // Shitmed Change End

    // handles OrganComponent, BodyPartComponent, and BodyPartAppearanceComponent registers, kinda messy to put them ALL here, but better than having three events
    private void OnBodyRemoved(Entity<BodyComponent> ent, ref ComponentRemove args)
    {
        foreach (var organComp in ent.Comp.OriginalOrgans)
        {
            if (organComp is not null && organComp.OriginalBody == ent.Owner) // should the equality check be an assert? maybe...
            {
                organComp.OriginalBody = null;
            }
        }
        foreach (var partComp in ent.Comp.OriginalBodyParts)
        {
            if (partComp is not null && partComp.OriginalBody == ent.Owner)
            {
                partComp.OriginalBody = null;
            }
        }
        foreach (var appearComp in ent.Comp.OriginalAppearances)
        {
            if (appearComp is not null && appearComp.OriginalBody == ent.Owner)
            {
                appearComp.OriginalBody = null;
            }
        }
    }

    private void SetOrganOriginalBody(OrganComponent organComp, EntityUid? nextBodyUid)
    {
        // unset previous
        BodyComponent? prevBody = null;
        if (organComp.OriginalBody is { Valid: true } prevBodyUid && Resolve(prevBodyUid, ref prevBody))
        {
            prevBody.OriginalOrgans.Remove(organComp);
        }
        organComp.OriginalBody = null;

        if (nextBodyUid is { Valid: true } validBodyUid)
        {
            BodyComponent? nextBody = null;
            if (Resolve(validBodyUid, ref nextBody)) // fuck me if this fails
            {
                nextBody.OriginalOrgans.Add(organComp);
            }
        }

        // yes I know its fucking deprecated, do you REALLY want to track Entity<OrganComponent> changes as well !?
        Dirty(organComp.Owner, organComp);
    }

    private void AddOrgan(
        Entity<OrganComponent> organEnt,
        EntityUid bodyUid,
        EntityUid parentPartUid)
    {
        organEnt.Comp.Body = bodyUid;
        var addedEv = new OrganAddedEvent(parentPartUid);
        RaiseLocalEvent(organEnt, ref addedEv);

        if (organEnt.Comp.Body is not null)
        {
        // Shitmed Change Start
            SetOrganOriginalBody(organEnt.Comp, organEnt.Comp.Body);
            var addedInBodyEv = new OrganAddedToBodyEvent(bodyUid, parentPartUid);
            RaiseLocalEvent(organEnt, ref addedInBodyEv);
            var organEnabledEv = new OrganEnableChangedEvent(true);
            RaiseLocalEvent(organEnt, ref organEnabledEv);
        }

        if (TryComp(parentPartUid, out DamageableComponent? damageable)
            && damageable.TotalDamage > 200)
            TrySetOrganUsed(organEnt, true, organEnt.Comp);
        // Shitmed Change End

        Dirty(organEnt, organEnt.Comp);
    }

    private void RemoveOrgan(Entity<OrganComponent> organEnt, EntityUid parentPartUid)
    {
        var removedEv = new OrganRemovedEvent(parentPartUid);
        RaiseLocalEvent(organEnt, ref removedEv);

        if (organEnt.Comp.Body is { Valid: true } bodyUid)
        {
            // Shitmed Change Start
            SetOrganOriginalBody(organEnt.Comp, organEnt.Comp.Body);
            var organDisabledEv = new OrganEnableChangedEvent(false);
            RaiseLocalEvent(organEnt, ref organDisabledEv);
            // Shitmed Change End
            var removedInBodyEv = new OrganRemovedFromBodyEvent(bodyUid, parentPartUid);
            RaiseLocalEvent(organEnt, ref removedInBodyEv);
        }

        organEnt.Comp.Body = null;
        Dirty(organEnt, organEnt.Comp);
    }

    /// <summary>
    /// Creates the specified organ slot on the parent entity.
    /// </summary>
    private OrganSlot? CreateOrganSlot(Entity<BodyPartComponent?> parentEnt, string slotId)
    {
        if (!Resolve(parentEnt, ref parentEnt.Comp, logMissing: false))
            return null;

        Containers.EnsureContainer<ContainerSlot>(parentEnt, GetOrganContainerId(slotId));
        var slot = new OrganSlot(slotId);
        parentEnt.Comp.Organs.Add(slotId, slot);
        return slot;
    }

    /// <summary>
    /// Attempts to create the specified organ slot on the specified parent if it exists.
    /// </summary>
    public bool TryCreateOrganSlot(
        EntityUid? parent,
        string slotId,
        [NotNullWhen(true)] out OrganSlot? slot,
        BodyPartComponent? part = null)
    {
        slot = null;

        if (parent is null || !Resolve(parent.Value, ref part, logMissing: false))
        {
            return false;
        }

        Containers.EnsureContainer<ContainerSlot>(parent.Value, GetOrganContainerId(slotId));
        slot = new OrganSlot(slotId);
        return part.Organs.TryAdd(slotId, slot.Value);
    }

    /// <summary>
    /// Returns whether the slotId exists on the partId.
    /// </summary>
    public bool CanInsertOrgan(
        EntityUid partId,
        string slotId,
        BodyPartComponent? part = null)
    {
        return Resolve(partId, ref part) && part.Organs.ContainsKey(slotId);
    }

    /// <summary>
    /// Returns whether the specified organ slot exists on the partId.
    /// </summary>
    public bool CanInsertOrgan(
        EntityUid partId,
        OrganSlot slot,
        BodyPartComponent? part = null)
    {
        return CanInsertOrgan(partId, slot.Id, part);
    }

    public bool InsertOrgan(
        EntityUid partId,
        EntityUid organId,
        string slotId,
        BodyPartComponent? part = null,
        OrganComponent? organ = null)
    {
        if (!Resolve(organId, ref organ, logMissing: false)
            || !Resolve(partId, ref part, logMissing: false)
            || !CanInsertOrgan(partId, slotId, part))
        {
            return false;
        }

        var containerId = GetOrganContainerId(slotId);

        return Containers.TryGetContainer(partId, containerId, out var container)
            && Containers.Insert(organId, container);
    }

    /// <summary>
    /// Removes the organ if it is inside of a body part.
    /// </summary>
    public bool RemoveOrgan(EntityUid organId, OrganComponent? organ = null)
    {
        if (!Containers.TryGetContainingContainer((organId, null, null), out var container))
            return false;

        var parent = container.Owner;

        return HasComp<BodyPartComponent>(parent)
            && Containers.Remove(organId, container);
    }

    /// <summary>
    /// Tries to add this organ to any matching slot on this body part.
    /// </summary>
    public bool AddOrganToFirstValidSlot(
        EntityUid partId,
        EntityUid organId,
        BodyPartComponent? part = null,
        OrganComponent? organ = null)
    {
        if (!Resolve(partId, ref part, logMissing: false)
            || !Resolve(organId, ref organ, logMissing: false))
        {
            return false;
        }

        foreach (var slotId in part.Organs.Keys)
        {
            InsertOrgan(partId, organId, slotId, part, organ);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns a list of Entity<<see cref="T"/>, <see cref="OrganComponent"/>>
    /// for each organ of the body
    /// </summary>
    /// <typeparam name="T">The component that we want to return</typeparam>
    /// <param name="entity">The body to check the organs of</param>
    public List<Entity<T, OrganComponent>> GetBodyOrganEntityComps<T>(
        Entity<BodyComponent?> entity)
        where T : IComponent
    {
        if (!Resolve(entity, ref entity.Comp))
            return new List<Entity<T, OrganComponent>>();

        var query = GetEntityQuery<T>();
        var list = new List<Entity<T, OrganComponent>>(3);
        foreach (var organ in GetBodyOrgans(entity.Owner, entity.Comp))
        {
            if (query.TryGetComponent(organ.Id, out var comp))
                list.Add((organ.Id, comp, organ.Component));
        }

        return list;
    }

    /// <summary>
    ///     Tries to get a list of ValueTuples of <see cref="T"/> and OrganComponent on each organs
    ///     in the given body.
    /// </summary>
    /// <param name="uid">The body entity id to check on.</param>
    /// <param name="comps">The list of components.</param>
    /// <param name="body">The body to check for organs on.</param>
    /// <typeparam name="T">The component to check for.</typeparam>
    /// <returns>Whether any were found.</returns>
    public bool TryGetBodyOrganEntityComps<T>(
        Entity<BodyComponent?> entity,
        [NotNullWhen(true)] out List<Entity<T, OrganComponent>>? comps)
        where T : IComponent
    {
        if (!Resolve(entity.Owner, ref entity.Comp))
        {
            comps = null;
            return false;
        }

        comps = GetBodyOrganEntityComps<T>(entity);

        if (comps.Count != 0)
            return true;

        comps = null;
        return false;
    }

    // Shitmed Change Start

    public bool TrySetOrganUsed(EntityUid organId, bool used, OrganComponent? organ = null)
    {
        if (!Resolve(organId, ref organ)
            || organ.Used == used)
            return false;

        organ.Used = used;
        Dirty(organId, organ);
        return true;
    }

    private void OnOrganEnableChanged(Entity<OrganComponent> organEnt, ref OrganEnableChangedEvent args)
    {
        if (!organEnt.Comp.CanEnable && args.Enabled)
            return;

        organEnt.Comp.Enabled = args.Enabled;

        if (args.Enabled)
            EnableOrgan(organEnt);
        else
            DisableOrgan(organEnt);

        if (organEnt.Comp.Body is { Valid: true } bodyEnt)
            RaiseLocalEvent(organEnt, new OrganComponentsModifyEvent(bodyEnt, args.Enabled));

        Dirty(organEnt, organEnt.Comp);
    }

    private void EnableOrgan(Entity<OrganComponent> organEnt)
    {
        if (!TryComp(organEnt.Comp.Body, out BodyComponent? body))
            return;

        // I hate having to hardcode these checks so much.
        if (HasComp<EyesComponent>(organEnt))
        {
            var ev = new OrganEnabledEvent(organEnt);
            RaiseLocalEvent(organEnt, ref ev);
        }
    }

    private void DisableOrgan(Entity<OrganComponent> organEnt)
    {
        if (!TryComp(organEnt.Comp.Body, out BodyComponent? body))
            return;

        // I hate having to hardcode these checks so much.
        if (HasComp<EyesComponent>(organEnt))
        {
            var ev = new OrganDisabledEvent(organEnt);
            RaiseLocalEvent(organEnt, ref ev);
        }
    }

    // Shitmed Change End
}
