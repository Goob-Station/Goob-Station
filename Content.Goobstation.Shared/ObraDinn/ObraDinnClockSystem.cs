using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Damage;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Nyanotrasen.Holograms;
using Content.Shared.Popups;
using Content.Shared.SSDIndicator;
using Content.Shared.Strip.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.ObraDinn;

/// <summary>
/// This handles the clock item
/// </summary>
public sealed class ObraDinnClockSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobstate = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ObraDinnClockComponent,UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<ObraDinnClockComponent,AfterInteractEvent>(OnInteract);
    }

    private void OnUseInHand(Entity<ObraDinnClockComponent> ent, ref UseInHandEvent args)
    {
        if (_timing.CurTime < ent.Comp.Cooldown)
            return;
        ent.Comp.Cooldown = _timing.CurTime + ent.Comp.CooldownTime;

        if(_net.IsClient)// temp because i cant figure out how to not get the right popups
            return;

        if (ent.Comp.Witnesses.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("obradinn-activate-fail-case"),args.User,args.User);
            return;
        }
        if (ent.Comp.Map == null ||  ent.Comp.Map.Value != Transform(args.User).MapID)
        {
            _popup.PopupEntity(Loc.GetString("obradinn-activate-fail-map"),args.User,args.User);
            return;
        }
        if (ent.Comp.Location == null  || !Transform(args.User).Coordinates.TryDistance(EntityManager, ent.Comp.Location.Value, out var distance))
        {
            _popup.PopupEntity(Loc.GetString("obradinn-activate-fail-no-distance"),args.User,args.User);
            return;
        }

        if (distance > ent.Comp.DistanceFromCrimeScene)
        {
            _popup.PopupEntity(Loc.GetString("obradinn-activate-fail-distance", ("distance",Math.Round(distance))),args.User,args.User);
            return;
        }

        _popup.PopupEntity(Loc.GetString("obradinn-activate-success"), args.User,args.User);


        WatchActivates(ent, args.User);

        ent.Comp.Location = null;
        ent.Comp.Map = null;
        ent.Comp.Witnesses.Clear();
        ent.Comp.Cooldown = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.Lifetime);// clock cant be used as long as the holograms are active
    }
    private void OnInteract(Entity<ObraDinnClockComponent> ent, ref AfterInteractEvent args)
    {
        if (_timing.CurTime < ent.Comp.Cooldown)
            return;
        ent.Comp.Cooldown = _timing.CurTime + ent.Comp.CooldownTime;

        if (args.Target == null)
            return;

        if(_net.IsClient) // temp because i cant figure out how to not get the right popups
            return;

        if (!_mobstate.IsDead(args.Target.Value) || !TryComp<ObraDinnBodyComponent>(args.Target, out var body))
        {
            _popup.PopupEntity(Loc.GetString("obradinn-interact-fail-target"), args.User, args.User);
            return;
        }

        if (body.Witnesses.Count.Equals(0)) // probably never gonna trigger because the victim is 1
        {
            _popup.PopupEntity(Loc.GetString("obradinn-interact-fail-witness"), args.User, args.User);
            return;
        }

        ent.Comp.Location = body.Location;
        ent.Comp.Map = body.Map;
        foreach (var witness in body.Witnesses) //if we just copy the list it gets deleted on clear.
            ent.Comp.Witnesses.Add(witness);

        _popup.PopupEntity(Loc.GetString("obradinn-interact-success"), args.User,args.User);
    }


    private void WatchActivates(Entity<ObraDinnClockComponent> ent, EntityUid user)
    {
        foreach (var witness in ent.Comp.Witnesses)
        {
            if (TerminatingOrDeleted(witness.Uid))
                continue;

            var proto = MetaData(witness.Uid).EntityPrototype;
            if(proto == null)
                continue;

            var newUid = SpawnAtPosition(proto.ID, witness.Loc);

            _humanoidAppearance.CloneAppearance(witness.Uid, newUid );

            _metaData.SetEntityName(newUid, Loc.GetString("obradinn-hologram-name"));
            _mobstate.ChangeMobState(newUid, witness.MobState);

            //add despawn
            var despawn = EnsureComp<TimedDespawnComponent>(newUid);
            despawn.Lifetime = ent.Comp.Lifetime;

            EnsureComp<HologramVisualsComponent>(newUid);

            var hologram = EnsureComp<ObraDinnHologramComponent>(newUid);
            hologram.RealName = witness.Name;

            // comps we dont want the hologram to have
            RemCompDeferred<PullableComponent>(newUid);
            RemCompDeferred<WoundableComponent>(newUid);
            RemCompDeferred<ActorComponent>(newUid);
            RemCompDeferred<MindContainerComponent>(newUid);
            RemCompDeferred<FixturesComponent>(newUid);
            RemCompDeferred<PhysicsComponent>(newUid);
            RemCompDeferred<StrippableComponent>(newUid);
            RemCompDeferred<SSDIndicatorComponent>(newUid);
            RemCompDeferred<GhostRoleMobSpawnerComponent>(newUid);
            RemCompDeferred<DamageableComponent>(newUid);
            RemCompDeferred<MobMoverComponent>(newUid);
        }
    }

}
