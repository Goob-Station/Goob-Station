using Content.Server._DV.CosmicCult.Components;
using Content.Server.Actions;
using Content.Server.Popups;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Warps;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.EntitySystems;

public sealed class CosmicColossusSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSound = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CosmicColossusComponent, ComponentInit>(OnSpawn);
        SubscribeLocalEvent<CosmicColossusComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var colossusQuery = EntityQueryEnumerator<CosmicColossusComponent>();
        while (colossusQuery.MoveNext(out var ent, out var comp))
        {
            if (comp.Attacking && _timing.CurTime >= comp.AttackHoldTimer)
            {
                _appearance.SetData(ent, ColossusVisuals.Status, ColossusStatus.Alive);
                _appearance.SetData(ent, ColossusVisuals.Sunder, ColossusAction.Stopped);
                _transform.Unanchor(ent);
                comp.Attacking = false;
            }
            if (comp.Hibernating && _timing.CurTime >= comp.HibernationTimer)
            {
                _appearance.SetData(ent, ColossusVisuals.Status, ColossusStatus.Alive);
                _appearance.SetData(ent, ColossusVisuals.Hibernation, ColossusAction.Stopped);
                _transform.Unanchor(ent);
                _audio.PlayPvs(comp.ReawakenSfx, ent);
                comp.Hibernating = false;
                Spawn(comp.CultBigVfx, Transform(ent).Coordinates);
                if (!TryComp<DamageableComponent>(ent, out var damageable))
                    continue;
                _damage.TryChangeDamage(ent, damageable.Damage / 2 * -1, true);
            }
            if (comp.Timed && _timing.CurTime >= comp.DeathTimer)
            {
                if (!_threshold.TryGetThresholdForState(ent, MobState.Dead, out var damage))
                    return;
                DamageSpecifier dspec = new();
                dspec.DamageDict.Add("Heat", damage.Value);
                _damage.TryChangeDamage(ent, dspec, true);
            }
        }
    }

    private void OnSpawn(Entity<CosmicColossusComponent> ent, ref ComponentInit args) // I WANT THIS BIG GUY HURLED TOWARDS THE STATION
    {
        ent.Comp.DeathTimer = _timing.CurTime + ent.Comp.DeathWait;
        var station = _station.GetStationInMap(Transform(ent).MapID);
        if (TryComp<StationDataComponent>(station, out var stationData))
        {
            var stationGrid = _station.GetLargestGrid(stationData);
            _throw.TryThrow(ent, Transform(stationGrid!.Value).Coordinates, baseThrowSpeed: 30, null, 0, 0, false, false, false, false, false);
        }
        if (ent.Comp.Timed)
            _actions.AddAction(ent, ref ent.Comp.EffigyPlaceActionEntity, ent.Comp.EffigyPlaceAction, ent);
    }

    private void OnMobStateChanged(Entity<CosmicColossusComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
            return;
        if (!TryComp<PhysicsComponent>(ent, out var physComp))
            return;
        ent.Comp.Hibernating = false;
        ent.Comp.Attacking = false;
        _appearance.SetData(ent, ColossusVisuals.Status, ColossusStatus.Dead);
        _appearance.SetData(ent, ColossusVisuals.Hibernation, ColossusAction.Stopped);
        _appearance.SetData(ent, ColossusVisuals.Sunder, ColossusAction.Stopped);
        _ambientSound.SetAmbience(ent, false);
        _audio.PlayPvs(ent.Comp.DeathSfx, ent);
        _physics.SetBodyStatus(ent, physComp, BodyStatus.OnGround, true);
        _popup.PopupCoordinates(
            Loc.GetString("ghost-role-colossus-death"),
            Transform(ent).Coordinates,
            PopupType.Large);
        RemComp<PointLightComponent>(ent);
        RemComp<WarpPointComponent>(ent);
        RemComp<CosmicCorruptingComponent>(ent);
    }
}
