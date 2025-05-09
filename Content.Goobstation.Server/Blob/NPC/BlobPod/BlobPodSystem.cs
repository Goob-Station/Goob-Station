// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Blob.Components;
using Content.Goobstation.Shared.Blob.NPC.BlobPod;
using Content.Server.DoAfter;
using Content.Server.Explosion.EntitySystems;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Popups;
using Content.Shared.ActionBlocker;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Rejuvenate;
using Content.Shared.Tag;
using Robust.Server.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Blob.NPC.BlobPod;

public sealed class BlobPodSystem : SharedBlobPodSystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobs = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly PopupSystem _popups = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobPodComponent, BlobPodZombifyDoAfterEvent>(OnZombify);
        SubscribeLocalEvent<BlobPodComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<BlobPodComponent, EntGotRemovedFromContainerMessage>(OnUnequip);
        SubscribeLocalEvent<BlobPodComponent, BeforeDamageChangedEvent>(OnGetDamage);
    }



    private void OnGetDamage(Entity<BlobPodComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (ent.Comp.ZombifiedEntityUid == null || TerminatingOrDeleted(ent.Comp.ZombifiedEntityUid.Value))
            return;
        // relay damage
        args.Cancelled = true;
        _damageableSystem.TryChangeDamage(ent.Comp.ZombifiedEntityUid.Value, args.Damage, origin: args.Origin);
    }

    private void OnUnequip(Entity<BlobPodComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if(args.Container.ID != "head")
            return;

        if (!HasComp<HumanoidAppearanceComponent>(args.Container.Owner) || !HasComp<ZombieBlobComponent>(args.Container.Owner))
            return;

        if (!TryComp<ZombieBlobComponent>(args.Container.Owner, out var zombieBlob))
            return;

        _tag.RemoveTag(args.Container.Owner, zombieBlob.TagAdded);

        RemCompDeferred<ZombieBlobComponent>(args.Container.Owner);
    }

    private void OnDestruction(EntityUid uid, BlobPodComponent component, DestructionEventArgs args)
    {
        if (!TryComp<BlobCoreComponent>(component.Core, out var blobCoreComponent))
            return;
        if (blobCoreComponent.CurrentChem == BlobChemType.ExplosiveLattice)
        {
            _explosionSystem.QueueExplosion(uid, blobCoreComponent.BlobExplosive, 4, 1, 2, maxTileBreak: 0);
        }
    }

    public bool Zombify(Entity<BlobPodComponent> ent, EntityUid target)
    {
        _inventory.TryGetSlotEntity(target, "head", out var headItem);
        if (HasComp<BlobMobComponent>(headItem))
            return false;

        _inventory.TryUnequip(target, "head", true, true);
        var equipped = _inventory.TryEquip(target, ent, "head", true, true);

        if (!equipped)
            return false;

        _popups.PopupEntity(Loc.GetString("blob-mob-zombify-second-end", ("pod", ent.Owner)),
            target,
            target,
            Content.Shared.Popups.PopupType.LargeCaution);
        _popups.PopupEntity(
            Loc.GetString("blob-mob-zombify-third-end", ("pod", ent.Owner), ("target", target)),
            target,
            Filter.PvsExcept(target),
            true,
            Content.Shared.Popups.PopupType.LargeCaution);

        RemComp<CombatModeComponent>(ent);
        RemComp<HTNComponent>(ent);
        RemComp<UnremoveableComponent>(ent);

        _audioSystem.PlayPvs(ent.Comp.ZombifyFinishSoundPath, ent);

        var rejEv = new RejuvenateEvent();
        RaiseLocalEvent(target, rejEv);

        ent.Comp.ZombifiedEntityUid = target;

        var zombieBlob = EnsureComp<ZombieBlobComponent>(target);
        _tag.AddTag(target, ent.Comp.HostTag);
        zombieBlob.TagAdded = ent.Comp.HostTag;
        zombieBlob.BlobPodUid = ent;
        if (HasComp<ActorComponent>(ent))
        {
            _npc.SleepNPC(target);
            _mover.SetRelay(ent, target);
        }

        return true;
    }

    private void OnZombify(EntityUid uid, BlobPodComponent component, BlobPodZombifyDoAfterEvent args)
    {
        component.IsZombifying = false;
        if (args.Handled || args.Args.Target == null)
        {
            _audioSystem.Stop(component.ZombifyStingStream, component.ZombifyStingStream);
            return;
        }

        if (args.Cancelled)
        {
            return;
        }

        Zombify((uid, component), args.Args.Target.Value);
    }


    public override bool NpcStartZombify(EntityUid uid, EntityUid target, BlobPodComponent? component = null)
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
        _popups.PopupEntity(Loc.GetString("blob-mob-zombify-second-start", ("pod", uid)), target, target,
            Content.Shared.Popups.PopupType.LargeCaution);
        _popups.PopupEntity(Loc.GetString("blob-mob-zombify-third-start", ("pod", uid), ("target", target)), target,
            Filter.PvsExcept(target), true, Content.Shared.Popups.PopupType.LargeCaution);

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
}