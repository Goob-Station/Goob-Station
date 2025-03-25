using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Blob.NPC.BlobPod;

public abstract class SharedBlobPodSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobs = default!;


    private EntityQuery<HumanoidAppearanceComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobPodComponent, GetVerbsEvent<InnateVerb>>(AddDrainVerb);
        SubscribeLocalEvent<BlobPodComponent, BeingUnequippedAttemptEvent>(OnUnequipAttempt);
        SubscribeLocalEvent<BlobPodComponent, CanDropTargetEvent>(OnCanDragDropOn);
        SubscribeLocalEvent<BlobPodComponent, DragDropTargetEvent>(OnBlobPodDragDrop);

        _query = GetEntityQuery<HumanoidAppearanceComponent>();
    }

    private void OnBlobPodDragDrop(Entity<BlobPodComponent> ent, ref DragDropTargetEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = NpcStartZombify(ent, args.Dragged, ent);
    }

    private void OnCanDragDropOn(Entity<BlobPodComponent> ent, ref CanDropTargetEvent args)
    {
        if (args.Handled)
            return;
        if (args.User == args.Dragged)
            return;
        if (!_query.HasComponent(args.Dragged))
            return;
        if (_mobs.IsAlive(args.Dragged))
            return;

        args.CanDrop = true;
        if (!HasComp<HandsComponent>(args.User))
            args.CanDrop = false;

        if (ent.Comp.IsZombifying)
            args.CanDrop = false;

        args.Handled = true;
    }

    private void OnUnequipAttempt(Entity<BlobPodComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        if (args.Unequipee == args.UnEquipTarget)
        {
            args.Cancel();
            return;
        }
        if (!TryComp<MobStateComponent>(args.UnEquipTarget, out var mobStateComponent))
            return;
        if (_mobs.IsDead(args.UnEquipTarget,mobStateComponent) || _mobs.IsCritical(args.UnEquipTarget,mobStateComponent))
            return;
        if (!HasComp<ZombieBlobComponent>(args.UnEquipTarget))
            return;
        args.Cancel();
    }

    private void AddDrainVerb(EntityUid uid, BlobPodComponent component, GetVerbsEvent<InnateVerb> args)
    {
        if (args.User == args.Target)
            return;
        if (!args.CanAccess)
            return;
        if (!_query.HasComponent(args.Target))
            return;
        if (_mobs.IsAlive(args.Target))
            return;

        InnateVerb verb = new()
        {
            Act = () =>
            {
                NpcStartZombify(uid, args.Target, component);
            },
            Text = Loc.GetString("blob-pod-verb-zombify"),
            // Icon = new SpriteSpecifier.Texture(new ("/Textures/")),
            Priority = 2
        };
        args.Verbs.Add(verb);
    }

    public abstract bool NpcStartZombify(EntityUid uid, EntityUid argsTarget, BlobPodComponent component);
}

[Serializable, NetSerializable]
public sealed partial class BlobPodZombifyDoAfterEvent : SimpleDoAfterEvent
{
}
