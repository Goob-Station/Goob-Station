using System.Numerics;
using Content.Goobstation.Common.Weapons.Ranged;
using Content.Shared.Audio;
using Content.Shared.Projectiles;
using Content.Shared.Random.Helpers;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    protected System.Random PredictedRandom(EntityUid uid)
    {
        var netEntity = GetNetEntity(uid);
        var seed = HashCode.Combine((int) Timing.CurTick.Value, netEntity.Id, 0x50495241);
        return new System.Random(seed);
    }

    protected Angle GetPredictedRecoilAngle(TimeSpan curTime, Entity<GunComponent> ent, Angle direction, EntityUid? user = null)
    {
        var (uid, component) = ent;
        var timeSinceLastFire = (curTime - component.LastFire).TotalSeconds;
        var minTheta = Math.Min(component.MinAngleModified.Theta, component.MaxAngleModified.Theta);
        var maxTheta = Math.Max(component.MinAngleModified.Theta, component.MaxAngleModified.Theta);
        var newTheta = MathHelper.Clamp(
            component.CurrentAngle.Theta + component.AngleIncreaseModified.Theta - component.AngleDecayModified.Theta * timeSinceLastFire,
            minTheta,
            maxTheta);

        component.CurrentAngle = new Angle(newTheta);
        component.LastFire = component.NextFire;

        var random = PredictedRandom(uid).NextFloat(-0.5f, 0.5f);

        var angleEv = new GetRecoilModifiersEvent
        {
            Gun = uid,
            User = user ?? uid,
        };

        if (user != null)
            RaiseLocalEvent(user.Value, angleEv);

        RaiseLocalEvent(uid, angleEv);
        random *= angleEv.Modifier;

        var spread = component.CurrentAngle.Theta * random;
        var angle = new Angle(direction.Theta + spread);
        DebugTools.Assert(Math.Abs(spread) <= maxTheta);
        return angle;
    }

    protected Angle[] LinearSpreadPredicted(Angle start, Angle end, int intervals)
    {
        var angles = new Angle[intervals];
        DebugTools.Assert(intervals > 1);

        for (var i = 0; i <= intervals - 1; i++)
        {
            angles[i] = new Angle(start + (end - start) * i / (intervals - 1));
        }

        return angles;
    }

    protected void ShootOrThrowPredicted(EntityUid uid, Vector2 mapDirection, Vector2 gunVelocity, GunComponent gun, EntityUid gunUid, EntityUid? user, Vector2? targetCoordinates = null)
    {
        if (gun.Target is { } target && !TerminatingOrDeleted(target))
            SetTarget(uid, target, out _);

        if (!HasComp<ProjectileComponent>(uid))
        {
            RemoveShootable(uid);
            ThrowingSystem.TryThrow(uid, mapDirection, gun.ProjectileSpeedModified, user);
            return;
        }

        ShootProjectile(uid, mapDirection, gunVelocity, gunUid, user, gun.ProjectileSpeedModified, targetCoordinates);
    }

    protected void CycleBallisticPredicted(EntityUid uid, BallisticAmmoProviderComponent component, MapCoordinates coordinates, EntityUid? user = null)
    {
        if (component.Entities.Count > 0)
        {
            var existing = component.Entities[^1];
            component.Entities.RemoveAt(component.Entities.Count - 1);
            DirtyField(uid, component, nameof(BallisticAmmoProviderComponent.Entities));

            Containers.Remove(existing, component.Container);
            EnsureShootable(existing);
        }
        else if (component.UnspawnedCount > 0)
        {
            component.UnspawnedCount--;
            DirtyField(uid, component, nameof(BallisticAmmoProviderComponent.UnspawnedCount));
            var ent = EntityManager.PredictedSpawn(component.Proto, coordinates);
            EnsureShootable(ent);
            EjectCartridgePredicted(PredictedRandom(uid), user, ent);
        }

        var cycledEvent = new GunCycledEvent();
        RaiseLocalEvent(uid, ref cycledEvent);
    }

    protected void EjectCartridgePredicted(System.Random rand, EntityUid? user, EntityUid entity, Angle? angle = null, bool playSound = true)
    {
        var offsetPos = rand.NextAngle().RotateVec(new Vector2(rand.NextFloat(0, EjectOffset), 0));
        var xform = Transform(entity);

        var coordinates = xform.Coordinates.Offset(offsetPos);
        TransformSystem.SetLocalRotation(entity, rand.NextAngle(), xform);
        TransformSystem.SetCoordinates(entity, xform, coordinates);

        if (angle != null)
        {
            var ejectAngle = angle.Value + 3.7f;
            ThrowingSystem.TryThrow(entity, ejectAngle.ToVec().Normalized() / 100, 5f);
        }

        if (playSound && TryComp<CartridgeAmmoComponent>(entity, out var cartridge))
        {
            Audio.PlayPredicted(
                cartridge.EjectSound,
                entity,
                user,
                AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation).WithVolume(-1f));
        }
    }
}
