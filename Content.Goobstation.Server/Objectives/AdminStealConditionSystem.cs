using Content.Shared.Interaction;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Objectives;

public sealed class AdminStealConditionSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AdminStealConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<AdminStealConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    /// <summary>
    /// Sets the entity to steal. Call before the objective is assigned so the title and icon get filled in.
    /// </summary>
    public void SetTarget(EntityUid uid, EntityUid target, AdminStealConditionComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.Target = target;
    }

    private void OnAfterAssign(Entity<AdminStealConditionComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        if (ent.Comp.Target is not { } target || !TryComp<MetaDataComponent>(target, out var targetMeta))
            return;

        var itemName = FormattedMessage.EscapeText(targetMeta.EntityName);
        _metaData.SetEntityName(ent, Loc.GetString("objective-condition-steal-title-no-owner", ("itemName", itemName)), args.Meta);
        _metaData.SetEntityDescription(ent, Loc.GetString("objective-condition-steal-description", ("itemName", itemName)), args.Meta);

        // use the item's own sprite as the icon, keeping the prototype's default if it has none
        if (targetMeta.EntityPrototype is { } proto)
            _objectives.SetIcon(ent, new SpriteSpecifier.EntityPrototype(proto.ID), args.Objective);
    }

    private void OnGetProgress(Entity<AdminStealConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        if (ent.Comp.Target is not { } item || TerminatingOrDeleted(item))
        {
            args.Progress = 0f;
            return;
        }

        var stolen = IsCarried(item, args.Mind.OwnedEntity) ||
                     ent.Comp.CheckStealAreas && IsInStealArea(item, args.MindId);
        args.Progress = stolen ? 1f : 0f;
    }

    /// <summary>
    /// The target counts as stolen if it is somewhere inside the owner (hands, inventory, bags, implants...)
    /// or is, or is inside, the entity the owner is pulling.
    /// </summary>
    private bool IsCarried(EntityUid item, EntityUid? owner)
    {
        if (owner is not { } body || !Exists(body))
            return false;

        EntityUid? pulled = null;
        if (TryComp<PullerComponent>(body, out var puller))
            pulled = puller.Pulling;

        // container contents are parented to the container's owner, so walking the parent chain covers nesting
        var current = item;
        while (current.IsValid())
        {
            if (current == pulled)
                return true;

            var parent = Transform(current).ParentUid;
            if (parent == body)
                return true;

            current = parent;
        }

        return false;
    }

    /// <summary>
    /// Like <see cref="Content.Server.Objectives.Systems.StealConditionSystem"/>, the target counts as stolen if it
    /// (or whatever contains it) is within range of a steal area, such as a thief beacon, that the mind has linked.
    /// </summary>
    private bool IsInStealArea(EntityUid item, EntityUid mindId)
    {
        // find the top level entity holding the item, so lockers and crates next to the beacon count
        var root = item;
        var parent = Transform(root).ParentUid;
        while (parent.IsValid() && !HasComp<MapComponent>(parent) && !HasComp<MapGridComponent>(parent))
        {
            // the inventories of anything that can hold a mind aren't searched, same as normal steal objectives
            if (HasComp<MindContainerComponent>(parent))
                return false;

            root = parent;
            parent = Transform(root).ParentUid;
        }

        var rootXform = Transform(root);
        var query = EntityQueryEnumerator<StealAreaComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var area, out var xform))
        {
            if (!area.Owners.Contains(mindId))
                continue;

            if (_interaction.InRangeUnobstructed((uid, xform), (root, rootXform), range: area.Range))
                return true;
        }

        return false;
    }
}
