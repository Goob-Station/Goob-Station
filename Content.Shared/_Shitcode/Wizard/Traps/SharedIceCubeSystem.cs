// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Emoting;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.Throwing;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Wizard.Traps;

public abstract class SharedIceCubeSystem : EntitySystem
{
    [Dependency] protected readonly SharedPhysicsSystem Physics = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;

    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IceCubeComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<IceCubeComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<IceCubeComponent, TileFrictionEvent>(OnTileFriction);
        SubscribeLocalEvent<IceCubeComponent, MoveInputEvent>(OnMoveInput);
        SubscribeLocalEvent<IceCubeComponent, BreakFreeDoAfterEvent>(OnBreakFree);
        SubscribeLocalEvent<IceCubeComponent, UseAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, PickupAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, ThrowAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, InteractionAttemptEvent>(OnInteractAttempt);
        SubscribeLocalEvent<IceCubeComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<IceCubeComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<IceCubeComponent, AttackAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, EmoteAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, SpeakAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, StandAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, DownAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, ChangeDirectionAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, DamageModifyEvent>(OnModify);
    }

    private void OnModify(Entity<IceCubeComponent> ent, ref DamageModifyEvent args)
    {
        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, ent.Comp.DamageReduction);
    }

    private void OnStartCollide(Entity<IceCubeComponent> ent, ref StartCollideEvent args)
    {
        var lenSquared = args.OtherBody.LinearVelocity.LengthSquared();
        if (lenSquared < 0.01f || !lenSquared.IsValid()) // Tests heisenfail without this since an engine issue causes it to return NaN randomly
            return;

        var xform = Transform(args.OtherEntity);

        var ray = new CollisionRay(_transform.GetWorldPosition(xform),
            args.OtherBody.LinearVelocity.Normalized(),
            args.OurBody.CollisionLayer);

        if (ent.Owner != Physics.IntersectRay(xform.MapID, ray, 1f, args.OtherEntity).FirstOrNull()?.HitEntity)
            return;

        Physics.ApplyLinearImpulse(ent,
            args.OtherBody.LinearVelocity * args.OtherBody.Mass * ent.Comp.VelocityMultiplier,
            body: args.OurBody);
    }

    private void OnMobStateChanged(Entity<IceCubeComponent> ent, ref MobStateChangedEvent args)
    {
        RemCompDeferred(ent.Owner, ent.Comp);
    }

    private void OnTileFriction(Entity<IceCubeComponent> ent, ref TileFrictionEvent args)
    {
        args.Modifier *= ent.Comp.TileFriction;
    }

    private void OnBreakFree(Entity<IceCubeComponent> ent, ref BreakFreeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        RemCompDeferred(ent.Owner, ent.Comp);
    }

    private void OnMoveInput(Entity<IceCubeComponent> ent, ref MoveInputEvent args)
    {
        var (uid, comp) = ent;

        var doArgs = new DoAfterArgs(EntityManager, uid, comp.BreakFreeDelay, new BreakFreeDoAfterEvent(), uid)
        {
            Hidden = true,
            RequireCanInteract = false,
            MultiplyDelay = false,
            CancelDuplicate = false,
        };

        if (_doAfter.TryStartDoAfter(doArgs) && _net.IsServer)
            Popup.PopupEntity(Loc.GetString("ice-cube-break-free-start"), uid, uid);
    }

    private void OnInteractAttempt(Entity<IceCubeComponent> ent, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnAttempt(EntityUid uid, IceCubeComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    private void OnPullAttempt(EntityUid uid, IceCubeComponent component, PullAttemptEvent args)
    {
        if (args.PullerUid == uid)
            args.Cancelled = true;
    }

    private void OnUpdateCanMove(Entity<IceCubeComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.LifeStage > ComponentLifeStage.Running)
            return;

        args.Cancel();
    }
}

[Serializable, NetSerializable]
public sealed partial class BreakFreeDoAfterEvent : SimpleDoAfterEvent;
