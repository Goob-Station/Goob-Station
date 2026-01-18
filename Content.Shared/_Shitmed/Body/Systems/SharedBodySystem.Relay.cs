// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kurokoTurbo <92106367+kurokoTurbo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Trest <144359854+trest100@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Kayzel <43700376+KayzelW@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Knowledge;
using Content.Shared._Shitmed.DoAfter;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Weapons.Melee.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;

namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem
{
    private void InitializeRelay()
    {
        SubscribeLocalEvent<BodyComponent, GetDoAfterDelayMultiplierEvent>(RelayBodyPartEvent);
        SubscribeLocalEvent<BodyPartComponent, GetDoAfterDelayMultiplierEvent>(RelayBoneEvent);
        SubscribeLocalEvent<BodyComponent, AttemptHandsMeleeEvent>(RelayBodyPartEvent);
        SubscribeLocalEvent<BodyPartComponent, AttemptHandsMeleeEvent>(RelayBoneEvent);
        SubscribeLocalEvent<BodyComponent, KnowledgeContainerRelayEvent>(RelayGeneralEvent);
        SubscribeLocalEvent<BodyPartComponent, KnowledgeContainerRelayEvent>(RelayGeneralEvent);
    }

    protected void RefRelayBodyPartEvent<T>(EntityUid uid, BodyComponent component, ref T args) where T : IBodyPartRelayEvent
    {
        RelayEvent((uid, component), ref args);
    }

    protected void RelayBodyPartEvent<T>(EntityUid uid, BodyComponent component, T args) where T : IBodyPartRelayEvent
    {
        RelayEvent((uid, component), args);
    }

    protected void RefRelayBoneEvent<T>(EntityUid uid, BodyPartComponent component, ref T args) where T : IBoneRelayEvent
    {
        RelayEvent((uid, component), ref args);
    }

    protected void RelayBoneEvent<T>(EntityUid uid, BodyPartComponent component, T args) where T : IBoneRelayEvent
    {
        RelayEvent((uid, component), args);
    }

    public void RelayEvent<T>(Entity<BodyComponent> body, ref T args) where T : IBodyPartRelayEvent
    {
        // this copies the by-ref event if it is a struct
        var ev = new BodyPartRelayedEvent<T>(args);
        foreach (var part in GetBodyChildrenOfType(body.Owner, args.TargetBodyPart, body.Comp))
        {
            RaiseLocalEvent(part.Id, ev);

            if (args.RaiseOnParent
                && TryGetParentBodyPart(part.Id, out var parentUid, out var _)
                && parentUid.HasValue)
                RaiseLocalEvent(parentUid.Value, ev);
        }

        // and now we copy it back
        args = ev.Args;
    }

    public void RelayEvent<T>(Entity<BodyComponent> body, T args) where T : IBodyPartRelayEvent
    {
        var ev = new BodyPartRelayedEvent<T>(args);

        foreach (var part in GetBodyChildrenOfType(body.Owner, args.TargetBodyPart, body.Comp))
        {
            RaiseLocalEvent(part.Id, ev);

            if (args.RaiseOnParent
                && TryGetParentBodyPart(part.Id, out var parentUid, out var _)
                && parentUid.HasValue)
                RaiseLocalEvent(parentUid.Value, ev);
        }
    }

    public void RelayEvent<T>(Entity<BodyPartComponent> bodyPart, ref T args) where T : IBoneRelayEvent
    {
        var ev = new BoneRelayedEvent<T>(args);

        if (!TryComp<WoundableComponent>(bodyPart.Owner, out var woundable))
            return;

        if (woundable.Bone.ContainedEntities.Count > 0)
            foreach (var bone in woundable.Bone.ContainedEntities)
                RaiseLocalEvent(bone, ev);

        // Now we run it on the parent (i.e. Arm or Leg)
        if (args.RaiseOnParent
            && woundable.ParentWoundable != null
            && TryComp<WoundableComponent>(woundable.ParentWoundable, out var parentWoundable)
            && parentWoundable.Bone.ContainedEntities.Count > 0)
            foreach (var bone in parentWoundable.Bone.ContainedEntities)
                RaiseLocalEvent(bone, ev);

        args = ev.Args;
    }

    public void RelayEvent<T>(Entity<BodyPartComponent> bodyPart, T args) where T : IBoneRelayEvent
    {
        var ev = new BoneRelayedEvent<T>(args);

        if (!TryComp<WoundableComponent>(bodyPart.Owner, out var woundable))
            return;

        if (woundable.Bone.ContainedEntities.Count > 0)
            foreach (var bone in woundable.Bone.ContainedEntities)
                RaiseLocalEvent(bone, ev);

        // Now we run it on the parent (i.e. Arm or Leg)
        if (args.RaiseOnParent
            && woundable.ParentWoundable != null
            && TryComp<WoundableComponent>(woundable.ParentWoundable, out var parentWoundable)
            && parentWoundable.Bone.ContainedEntities.Count > 0)
            foreach (var bone in parentWoundable.Bone.ContainedEntities)
                RaiseLocalEvent(bone, ev);

    }

    /// <summary>
    /// Relays an event to all body parts.
    /// Should be used as the last resort, for example if your event is located in a Common module.
    /// </summary>
    private void RelayGeneralEvent<T>(Entity<BodyComponent> ent, ref T args) where T : notnull
    {
        var children = GetBodyChildren(ent.Owner, ent.Comp).ToList();
        foreach (var (part, _) in children)
        {
            RaiseLocalEvent(part, ref args);
        }
    }

    /// <summary>
    /// Relays an event to all organs.
    /// Should be used as the last resort, for example if your event is located in a Common module.
    /// </summary>
    private void RelayGeneralEvent<T>(Entity<BodyPartComponent> ent, ref T args) where T : notnull
    {
        var children = GetPartOrgans(ent.Owner, ent.Comp).ToList();
        foreach (var (part, _) in children)
        {
            RaiseLocalEvent(part, ref args);
        }
    }
}

public sealed class BodyPartRelayedEvent<TEvent> : EntityEventArgs
{
    public TEvent Args;

    public BodyPartRelayedEvent(TEvent args)
    {
        Args = args;
    }
}

public sealed class BoneRelayedEvent<TEvent> : EntityEventArgs
{
    public TEvent Args;

    public BoneRelayedEvent(TEvent args)
    {
        Args = args;
    }
}

/// <summary>
///     Events that should be relayed to body parts should implement this interface.
/// </summary>
public interface IBodyPartRelayEvent
{
    /// <summary>
    ///     What body part should this event be relayed to, if any?
    /// </summary>
    public BodyPartType TargetBodyPart { get; }

    public BodyPartSymmetry? TargetBodyPartSymmetry { get; }

    public bool RaiseOnParent { get; }
}

/// <summary>
///     Events that should be relayed to bones should implement this interface.
/// </summary>
public interface IBoneRelayEvent
{
    /// <summary>
    ///     Whether to raise the event on the parent body part as well.
    /// </summary>
    public bool RaiseOnParent { get; }
}
