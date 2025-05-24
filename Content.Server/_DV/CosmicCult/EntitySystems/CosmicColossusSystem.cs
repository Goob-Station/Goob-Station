using System.Linq;
using Content.Server.Doors.Systems;
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Audio;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.EntitySystems;

public sealed class CosmicColossusSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DoorSystem _door = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSound = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicColossusComponent, MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<CosmicColossusComponent, EventCosmicColossusIngress>(OnColossusIngress);
        SubscribeLocalEvent<CosmicColossusComponent, EventCosmicColossusIngressDoAfter>(OnColossusIngressDoAfter);
        SubscribeLocalEvent<CosmicColossusComponent, EventCosmicColossusSunder>(OnColossusSunder);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var colossusQuery = EntityQueryEnumerator<CosmicColossusComponent>();
        while (colossusQuery.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime >= comp.AttackHoldTimer && comp.Attacking)
            {
                _appearance.SetData(ent, ColossusVisuals.Status, ColossusStatus.Alive); _transform.Unanchor(ent);
                comp.Attacking = false;
            }
        }
    }

    private void OnMobStateChanged(Entity<CosmicColossusComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
            return;
        if (!TryComp<PhysicsComponent>(ent, out var physComp))
            return;
        _appearance.SetData(ent, ColossusVisuals.Status, ColossusStatus.Dead);
        _ambientSound.SetAmbience(ent, false);
        _audio.PlayPvs(ent.Comp.DeathSfx, ent);
        _physics.SetBodyStatus(ent, physComp, BodyStatus.OnGround, true);
        _popup.PopupCoordinates(
            Loc.GetString("cosmiccult-colossus-death"),
            Transform(ent).Coordinates,
            PopupType.Large);
        RemComp<PointLightComponent>(ent);
    }

    private void OnColossusIngress(Entity<CosmicColossusComponent> ent, ref EventCosmicColossusIngress args)
    {
        var doargs = new DoAfterArgs(EntityManager, ent, ent.Comp.IngressDoAfter, new EventCosmicColossusIngressDoAfter(), ent, args.Target)
        {
            DistanceThreshold = 2f,
            Hidden = false,
            BreakOnMove = true,
        };
        args.Handled = true;
        _audio.PlayPvs(ent.Comp.DoAfterSfx, ent);
        _doAfter.TryStartDoAfter(doargs);
    }

    private void OnColossusIngressDoAfter(Entity<CosmicColossusComponent> ent, ref EventCosmicColossusIngressDoAfter args)
    {
        if (args.Args.Target is not { } target)
            return;
        if (args.Cancelled || args.Handled)
            return;
        args.Handled = true;
        var comp = ent.Comp;

        if (TryComp<DoorBoltComponent>(target, out var doorBolt))
            _door.SetBoltsDown((target, doorBolt), false);
        _door.StartOpening(target);
        _audio.PlayPvs(comp.IngressSfx, ent);
        Spawn(comp.CultVfx, Transform(target).Coordinates);
    }

    private void OnColossusSunder(Entity<CosmicColossusComponent> ent, ref EventCosmicColossusSunder args)
    {
        args.Handled = true;

        var comp = ent.Comp;
        _appearance.SetData(ent, ColossusVisuals.Status, ColossusStatus.Attacking);
        _transform.SetCoordinates(ent, args.Target);
        _transform.AnchorEntity(ent);

        comp.Attacking = true;
        comp.AttackHoldTimer = comp.AttackWait + _timing.CurTime;
        Spawn(comp.Attack1Vfx, args.Target);

        var detonator = Spawn(comp.TileDetonations, args.Target);
        EnsureComp<CosmicTileDetonatorComponent>(detonator, out var detonateComp);
        detonateComp.DetonationTimer = _timing.CurTime;
    }
}
