using System.Numerics;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModMagneticHarnessSystem : VirtualController
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedHandsSystem _hands = default!;

    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly List<EntityUid> _forget = new();

    [SubscribeLocalEvent]
    private void OnDidEquip(Entity<ModMagneticHarnessComponent> ent, ref DidEquipEvent args)
    {
        if (_timing.ApplyingState
            || !ent.Comp.Slots.Contains(args.Slot)
            || !HasComp<GunComponent>(args.Equipment))
            return;

        var magnetized = EnsureComp<ModMagnetizedGunComponent>(args.Equipment);
        magnetized.Wearer = ent;
        magnetized.Slot = args.Slot;
        magnetized.LooseSince = null;
        magnetized.Launched = false;
        Dirty(args.Equipment, magnetized);
        PhysicsSystem.UpdateIsPredicted(args.Equipment);
    }

    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        var now = _timing.CurTime;
        _forget.Clear();

        var query = EntityQueryEnumerator<ModMagnetizedGunComponent, PhysicsComponent>();
        while (query.MoveNext(out var gun, out var magnetized, out var body))
        {
            if (prediction && !body.Predict)
                continue;

            var owner = magnetized.Wearer;
            if (TerminatingOrDeleted(owner))
            {
                if (_net.IsServer)
                    _forget.Add(gun);
                continue;
            }

            if (_container.IsEntityInContainer(gun)
                || Transform(gun).Anchored
                || !TryComp<ModMagneticHarnessComponent>(owner, out var harness))
            {
                Reset((gun, magnetized), body, null);
                continue;
            }

            var gunPos = TransformSystem.GetMapCoordinates(gun);
            var ownerPos = TransformSystem.GetMapCoordinates(owner);
            var offset = ownerPos.Position - gunPos.Position;
            var distance = offset.Length();
            if (gunPos.MapId != ownerPos.MapId || distance > harness.Range)
            {
                Reset((gun, magnetized), body, null);
                continue;
            }

            if (magnetized.LooseSince is not { } since)
            {
                magnetized.LooseSince = now;
                Dirty(gun, magnetized);
                PhysicsSystem.UpdateIsPredicted(gun);
                continue;
            }

            var elapsed = now - since;
            if (elapsed < harness.Delay)
                continue;

            if (distance <= harness.CatchDistance)
            {
                PhysicsSystem.SetLinearVelocity(gun, Vector2.Zero, body: body);
                if (_net.IsClient)
                    continue;

                if (!TryCatch(gun, magnetized, owner, harness))
                    Reset((gun, magnetized), body, now + RetryDelay);

                continue;
            }

            if (elapsed > harness.Delay + harness.PullTime + harness.GiveUpTime)
            {
                Reset((gun, magnetized), body, now + RetryDelay);
                continue;
            }

            var launching = elapsed >= harness.Delay + harness.PullTime;
            if (launching && !magnetized.Launched)
            {
                magnetized.Launched = true;
                Dirty(gun, magnetized);
                _audio.PlayPredicted(harness.LaunchSound, gun, owner);
            }

            if (body.BodyStatus != BodyStatus.InAir)
                PhysicsSystem.SetBodyStatus(gun, body, BodyStatus.InAir);

            var speed = launching ? harness.LaunchSpeed : harness.PullSpeed;
            PhysicsSystem.SetLinearVelocity(gun, offset / distance * speed, body: body);
            PhysicsSystem.SetAngularVelocity(gun, 0f, body: body);
        }

        foreach (var gun in _forget)
        {
            RemComp<ModMagnetizedGunComponent>(gun);
        }
    }

    private bool TryCatch(EntityUid gun, ModMagnetizedGunComponent magnetized, EntityUid owner, ModMagneticHarnessComponent harness)
    {
        if (TryComp<PhysicsComponent>(gun, out var body))
            PhysicsSystem.SetBodyStatus(gun, body, BodyStatus.OnGround);

        var caught = _inventory.TryEquip(owner, gun, magnetized.Slot, silent: true);
        if (!caught)
        {
            foreach (var slot in harness.Slots)
            {
                if (slot != magnetized.Slot && _inventory.TryEquip(owner, gun, slot, silent: true))
                {
                    caught = true;
                    break;
                }
            }
        }

        if (!caught)
            caught = _hands.TryPickupAnyHand(owner, gun, checkActionBlocker: false);

        if (caught)
            _audio.PlayPvs(harness.CatchSound, owner);

        return caught;
    }

    private void Reset(Entity<ModMagnetizedGunComponent> ent, PhysicsComponent body, TimeSpan? looseSince)
    {
        if (ent.Comp.LooseSince == looseSince && !ent.Comp.Launched)
            return;

        if (ent.Comp.LooseSince != null && body.BodyStatus == BodyStatus.InAir && !_container.IsEntityInContainer(ent))
        {
            PhysicsSystem.SetLinearVelocity(ent, Vector2.Zero, body: body);
            PhysicsSystem.SetBodyStatus(ent, body, BodyStatus.OnGround);
        }

        ent.Comp.LooseSince = looseSince;
        ent.Comp.Launched = false;
        Dirty(ent);
        PhysicsSystem.UpdateIsPredicted(ent);
    }
}
