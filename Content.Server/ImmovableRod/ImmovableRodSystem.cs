// SPDX-FileCopyrightText: 2022 Andreas Kämper <andreas.kaemper@5minds.de>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 SpaceManiac <tad@platymuus.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 TsjipTsjip <19798667+TsjipTsjip@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Server.Polymorph.Components;
using Content.Server.Popups;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Server.Stunnable;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.ImmovableRod;

public sealed class ImmovableRodSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!; // Goobstation
    [Dependency] private readonly TagSystem _tag = default!; // Goobstation
    [Dependency] private readonly StunSystem _stun = default!; // Goobstation

    private static readonly ProtoId<TagPrototype> IgnoreTag = "IgnoreImmovableRod"; // Goobstation

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // we are deliberately including paused entities. rod hungers for all
        foreach (var (rod, trans) in EntityManager.EntityQuery<ImmovableRodComponent, TransformComponent>(true))
        {
            if (!rod.DestroyTiles)
                continue;

            if (!TryComp<MapGridComponent>(trans.GridUid, out var grid))
                continue;

            _map.SetTile(trans.GridUid.Value, grid, trans.Coordinates, Tile.Empty);
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ImmovableRodComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<ImmovableRodComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ImmovableRodComponent, ExaminedEvent>(OnExamined);
    }

    private void OnMapInit(EntityUid uid, ImmovableRodComponent component, MapInitEvent args)
    {
        if (EntityManager.TryGetComponent(uid, out PhysicsComponent? phys))
        {
            _physics.SetLinearDamping(uid, phys, 0f);
            _physics.SetFriction(uid, phys, 0f);
            _physics.SetBodyStatus(uid, phys, BodyStatus.InAir);

            var xform = Transform(uid);
            var (worldPos, worldRot) = _transform.GetWorldPositionRotation(uid);
            var vel = worldRot.ToWorldVec() * component.MaxSpeed;

            if (component.RandomizeVelocity)
            {
                vel = component.DirectionOverride.Degrees switch
                {
                    0f => _random.NextVector2(component.MinSpeed, component.MaxSpeed),
                    _ => worldRot.RotateVec(component.DirectionOverride.ToVec()) * _random.NextFloat(component.MinSpeed, component.MaxSpeed)
                };
            }

            _physics.ApplyLinearImpulse(uid, vel, body: phys);
            xform.LocalRotation = (vel - worldPos).ToWorldAngle() + MathHelper.PiOver2;
        }
    }

    private void OnCollide(EntityUid uid, ImmovableRodComponent component, ref StartCollideEvent args)
    {
        var ent = args.OtherEntity;

        // Goobstation start
        if (component.DamagedEntities.Contains(ent))
            return;

        if (!component.DestroyTiles && _tag.HasTag(ent, IgnoreTag))
            return;
        // Goobstation end

        if (_random.Prob(component.HitSoundProbability))
        {
            _audio.PlayPvs(component.Sound, uid);
        }

        if (HasComp<ImmovableRodComponent>(ent))
        {
            // oh god.
            var coords = Transform(uid).Coordinates;
            _popup.PopupCoordinates(Loc.GetString("immovable-rod-collided-rod-not-good"), coords, PopupType.LargeCaution);

            Del(uid);
            Del(ent);
            Spawn("Singularity", coords);

            return;
        }

        // dont delete/hurt self if polymoprhed into a rod
        if (TryComp<PolymorphedEntityComponent>(uid, out var polymorphed))
        {
            if (polymorphed.Parent == ent)
                return;
        }

        // gib or damage em
        if (TryComp<BodyComponent>(ent, out var body))
        {
            component.MobCount++;
            _popup.PopupEntity(Loc.GetString("immovable-rod-penetrated-mob", ("rod", uid), ("mob", ent)), uid, PopupType.LargeCaution);

            if (!component.ShouldGib)
            {
                if (component.Damage == null)
                    return;

                component.DamagedEntities.Add(ent); // Goobstation
                _damageable.TryChangeDamage(ent, component.Damage, component.IgnoreResistances, origin: uid, partMultiplier: component.PartDamageMultiplier); // Goob edit
                if (component.KnockdownTime > TimeSpan.Zero) // Goobstation
                    _stun.KnockdownOrStun(ent, component.KnockdownTime, true);
                return;
            }

            _bodySystem.GibBody(ent, body: body);
            return;
        }

        _entityStorage.EmptyContents(ent); // Goobstation

        QueueDel(ent);
    }

    private void OnExamined(EntityUid uid, ImmovableRodComponent component, ExaminedEvent args)
    {
        if (component.MobCount == 0)
        {
            args.PushText(Loc.GetString("immovable-rod-consumed-none", ("rod", uid)));
        }
        else
        {
            args.PushText(Loc.GetString("immovable-rod-consumed-souls", ("rod", uid), ("amount", component.MobCount)));
        }
    }
}