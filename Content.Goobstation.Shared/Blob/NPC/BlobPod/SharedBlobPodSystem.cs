// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Blob.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Blob.NPC.BlobPod;

public abstract class SharedBlobPodSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly MobStateSystem _mobs = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;

    private EntityQuery<HumanoidAppearanceComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobPodComponent, GetVerbsEvent<InnateVerb>>(AddDrainVerb);
        SubscribeLocalEvent<BlobPodComponent, BlobPodZombifyDoAfterEvent>(OnZombify);
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

    public bool NpcStartZombify(EntityUid uid, EntityUid target, BlobPodComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;
        if (!HasComp<HumanoidAppearanceComponent>(target))
            return false;
        if (_mobs.IsAlive(target))
            return false;
        if (!_actionBlocker.CanInteract(uid, target))
            return false;

        StartZombify(uid, target, component);
        return true;
    }


    public void StartZombify(EntityUid uid, EntityUid target, BlobPodComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.ZombifyTarget = target;
        _popups.PopupClient();

        _popups.PopupEntity(Loc.GetString("blob-mob-zombify-second-start", ("pod", uid)), target, target, PopupType.LargeCaution);
        _popups.PopupEntity(Loc.GetString("blob-mob-zombify-third-start", ("pod", uid), ("target", target)), target,
            Filter.PvsExcept(target), true, PopupType.LargeCaution);

        component.ZombifyStingStream = _audioSystem.PlayPvs(component.ZombifySoundPath, target);
        component.IsZombifying = true;

        var ev = new BlobPodZombifyDoAfterEvent();
        var args = new DoAfterArgs(EntityManager, uid, component.ZombifyDelay, ev, uid, target: target)
        {
            BreakOnMove = true,
            DistanceThreshold = 2f,
            NeedHand = false,
            MultiplyDelay = false
        };

        _doAfter.TryStartDoAfter(args);
    }

    private void OnZombify(EntityUid uid, BlobPodComponent component, BlobPodZombifyDoAfterEvent args)
    {
        component.IsZombifying = false;
        if (args.Handled || args.Target == null)
        {
            _audioSystem.Stop(component.ZombifyStingStream, component.ZombifyStingStream);
            return;
        }

        if (args.Cancelled)
            return;

        Zombify((uid, component), args.Args.Target.Value);
    }
}

[Serializable, NetSerializable]
public sealed partial class BlobPodZombifyDoAfterEvent : SimpleDoAfterEvent;
