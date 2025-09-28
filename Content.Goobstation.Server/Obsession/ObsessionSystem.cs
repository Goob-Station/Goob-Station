using System.Linq;
using Content.Goobstation.Server.Photo;
using Content.Goobstation.Shared.Obsession;
using Content.Server.Antag;
using Content.Server.Interaction;
using Content.Server.Jittering;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Server.Speech;
using Content.Server.Stunnable;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.SSDIndicator;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Obsession;

public sealed class ObsessionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly InteractionSystem _interaction = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly JitteringSystem _jittering = default!;
    [Dependency] private readonly RoleSystem _role = default!;

    private int _lastId = 0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ObsessedComponent, MapInitEvent>(OnObsessedMapInit);
        SubscribeLocalEvent<ObsessedComponent, PhotographedEvent>(OnObsessedPhotograph);
        SubscribeLocalEvent<ObsessedComponent, GrabStageChangedEvent>(OnObsessedGrab);
        SubscribeLocalEvent<ObsessedComponent, ListenEvent>(OnObsessedListen);

        SubscribeLocalEvent<ObsessionTargetComponent, InteractionSuccessEvent>(OnTargetInteract);
        SubscribeLocalEvent<ObsessionTargetComponent, PhotographedTargetEvent>(OnTargetPhotographed);

        SubscribeLocalEvent<ObsessionTargetPhotoComponent, BoundUIOpenedEvent>(OnPhotoOpen);
        SubscribeLocalEvent<ObsessionTargetPhotoComponent, BoundUIClosedEvent>(OnPhotoClose);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ObsessedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextUpdate > _timing.CurTime)
                continue;

            comp.NextUpdate = _timing.CurTime + TimeSpan.FromSeconds(12);

            var xform = Transform(uid);
            var ents = _lookup.GetEntitiesInRange<ObsessionTargetComponent>(xform.Coordinates, 4f);
            foreach (var item in ents)
            {
                if (item.Comp.Id != comp.TargetId)
                    continue;

                if (_interaction.InRangeUnobstructed(uid, item.Owner, 4))
                    comp.Sanity += comp.InteractionRecovery[ObsessionInteraction.StayNear];
            }

            DoPassiveSanityDamage((uid, comp));

            if (comp.Sanity <= 70 && _random.Prob((comp.MaxSanity - comp.Sanity) / 100f + 0.1f * comp.SanityLossStage))
                DoRandomEffect((uid, comp));
        }

        var photoQuery = EntityQueryEnumerator<ObsessionTargetPhotoComponent>();
        while (photoQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.NextUpdate > _timing.CurTime)
                continue;

            comp.NextUpdate = _timing.CurTime + TimeSpan.FromSeconds(5);

            foreach (var item in comp.Actors)
            {
                if (!TryComp<ObsessedComponent>(item, out var obsessed) || !comp.Ids.Contains(obsessed.TargetId))
                    continue;

                obsessed.Sanity += obsessed.InteractionRecovery[ObsessionInteraction.PhotoLook];
            }
        }
    }

    private void OnObsessedMapInit(Entity<ObsessedComponent> ent, ref MapInitEvent args)
    {
        var target = _random.Pick(EntityManager.AllEntities<HumanoidAppearanceComponent>().Where(x => x.Owner != ent.Owner).ToList());
        if (TryComp<ObsessionTargetComponent>(target.Owner, out var targComp))
            ent.Comp.TargetId = targComp.Id;
        else
        {
            _lastId++;
            var comp = EnsureComp<ObsessionTargetComponent>(target);
            comp.Id = _lastId;
            ent.Comp.TargetId = _lastId;

            Dirty(target.Owner, comp);
        }

        Dirty(ent);
    }

    private void OnObsessedPhotograph(Entity<ObsessedComponent> ent, ref PhotographedEvent args)
    {
        foreach (var item in args.OnPhoto)
        {
            if (!TryComp<ObsessionTargetComponent>(item, out var comp) || comp.Id != ent.Comp.TargetId)
                continue;

            TryRecoverSanity(ent, ObsessionInteraction.Photo);
        }
    }

    private void OnObsessedGrab(Entity<ObsessedComponent> ent, ref GrabStageChangedEvent args)
    {
        if (!TryComp<ObsessionTargetComponent>(args.Target, out var comp) || comp.Id != ent.Comp.TargetId)
            return;

        if (args.Stage <= args.PrevStage)
            return;

        TryRecoverSanity(ent, ObsessionInteraction.Grab);
    }

    private void OnObsessedListen(Entity<ObsessedComponent> ent, ref ListenEvent args)
    {
        if (!TryComp<ObsessionTargetComponent>(args.Source, out var comp) || comp.Id != ent.Comp.TargetId)
            return;

        if (args.Message.Length < 3)
            return;

        TryRecoverSanity(ent, ObsessionInteraction.Hear);
    }

    private void OnTargetInteract(Entity<ObsessionTargetComponent> ent, ref InteractionSuccessEvent args)
    {
        if (!TryComp<ObsessedComponent>(args.User, out var comp) || comp.TargetId != ent.Comp.Id)
            return;

        TryRecoverSanity((args.User, comp), ObsessionInteraction.Touch);
    }

    private void OnTargetPhotographed(Entity<ObsessionTargetComponent> ent, ref PhotographedTargetEvent args)
    {
        var comp = EnsureComp<ObsessionTargetPhotoComponent>(args.Photo);
        comp.Ids.Add(ent.Comp.Id);
    }

    private void OnPhotoOpen(Entity<ObsessionTargetPhotoComponent> ent, ref BoundUIOpenedEvent args)
    {
        ent.Comp.Actors.Add(args.Actor);
    }

    private void OnPhotoClose(Entity<ObsessionTargetPhotoComponent> ent, ref BoundUIClosedEvent args)
    {
        ent.Comp.Actors.Remove(args.Actor);
    }

    public void TryRecoverSanity(Entity<ObsessedComponent> ent, ObsessionInteraction interaction)
    {
        int sameInteractions = 0;
        for (var i = 0; i < ent.Comp.LastRecoveries.Count; i++)
        {
            if (ent.Comp.LastRecoveries[i] == interaction)
                sameInteractions++;
        }

        var recovery = ent.Comp.InteractionRecovery[interaction];
        ent.Comp.Sanity += sameInteractions switch
        {
            <= 0 => recovery,
            2 => recovery / 2,
            _ => 0
        };

        if (ent.Comp.LastRecoveries.Count >= 3)
            ent.Comp.LastRecoveries.RemoveAt(0);

        ent.Comp.LastRecoveries.Add(interaction);
    }

    private void DoPassiveSanityDamage(Entity<ObsessedComponent> ent)
    {
        ent.Comp.Sanity -= ObsessedComponent.PassiveSanityLoss;

        var stage = ent.Comp.Sanity switch
        {
            > 55 => 0,
            > 35 => 1,
            > 15 => 2,
            _ => 3
        };

        if (stage != ent.Comp.SanityLossStage)
        {
            ent.Comp.SanityLossStage = stage;
            Dirty(ent);
        }
    }

    private void DoRandomEffect(Entity<ObsessedComponent> ent)
    {
        switch (_random.Next(0, 2))
        {
            case 0:
                _popup.PopupEntity(Loc.GetString($"popup-obsessed-{_random.Next(1, 3)}"), ent, ent, Content.Shared.Popups.PopupType.SmallCaution);
                break;
            case 1:
                _stun.TrySlowdown(ent, TimeSpan.FromSeconds(1.5f), false);
                break;
            case 2:
                _jittering.DoJitter(ent, TimeSpan.FromSeconds(2), true);
                break;
        }
    }
}
