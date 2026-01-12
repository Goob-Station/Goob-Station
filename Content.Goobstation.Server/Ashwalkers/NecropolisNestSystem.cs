using System;
using System.Numerics;
using Content.Goobstation.Common.AshWalkers;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Pinpointer;
using Content.Shared.Destructible;
using Content.Shared.Destructible;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.Humanoid;
using Content.Shared.Maps;
using Content.Shared.Database;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Destructible;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;
using Content.Server.Administration.Logs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Item;
using Content.Server.Body.Systems;

namespace Content.Goobstation.Server.AshWalkers;

public sealed class NecropolisNestSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly StepTriggerSystem _step = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ITileDefinitionManager _tiledef = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;

    private EntityQuery<NecropolisNestComponent> _query;

    private readonly List<Entity<NecropolisNestComponent>> _nests = new();

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<NecropolisNestComponent>();

        SubscribeLocalEvent<NecropolisNestComponent, StepTriggeredOffEvent>(OnStepTriggeredOff);
        SubscribeLocalEvent<NecropolisNestComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<NecropolisNestComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<NecropolisNestComponent, DestructionEventArgs>(OnNestDestruction);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    private void OnInit(EntityUid uid, NecropolisNestComponent component, MapInitEvent args)
    {
        if (!Transform(uid).Coordinates.IsValid(EntityManager))
            QueueDel(uid);

        var coords = Transform(uid).Coordinates;
        for (var i = 0; i < component.StartingEggs; i++)
            Spawn(component.ObjectToSpawn, coords);
    }

    private void OnStepTriggeredOff(EntityUid uid, NecropolisNestComponent component, ref StepTriggeredOffEvent args)
    {

        // We're not a bingle pit.
        if (HasComp<ItemComponent>(args.Tripper))
            return;

        if (!_mob.IsDead(args.Tripper))
           return;

        AbsorbCorpse(uid, component, args.Tripper);

        if (component.TendrilPoints >= component.SpawnNewAt)
        {
            SpawnEgg(uid, component);
            component.TendrilPoints -= component.SpawnNewAt;
        }
    }

    private void AbsorbCorpse(EntityUid uid, NecropolisNestComponent component, EntityUid victim, bool playSound = true)
    {
        component.TendrilPoints++;

        if (HasComp<HumanoidAppearanceComponent>(victim))
            component.TendrilPoints += component.AdditionalPointsForHuman;

        if (TryComp<PullableComponent>(victim, out var pullable) && pullable.BeingPulled)
            _pulling.TryStopPull(victim, pullable, ignoreGrab: true);

        if (playSound)
            _audio.PlayPvs(component.AbsorbingSound, uid);

       // Timer is there so the console doesn't get flooded with errors.
       Timer.Spawn(TimeSpan.FromMilliseconds(50), () => _body.GibBody(victim));
       var logImpact = HasComp<HumanoidAppearanceComponent>(victim) ? LogImpact.Extreme : LogImpact.Medium;
       _adminLogger.Add(LogType.Gib, logImpact, $"{ToPrettyString(victim):victim} was gibbed by a necropolis nest!");
    }

    private void OnStepTriggerAttempt(EntityUid uid, NecropolisNestComponent component, ref StepTriggerAttemptEvent args)
    => args.Continue = true;

    public void SpawnEgg(EntityUid uid, NecropolisNestComponent component)
    {
        Spawn(component.ObjectToSpawn, Transform(uid).Coordinates);
    }

    private void OnNestDestruction(EntityUid uid, NecropolisNestComponent comp, DestructionEventArgs args)
    {
        var coords = Transform(uid).Coordinates;
        var delay = comp.ChasmDelay;

        _popup.PopupCoordinates(Loc.GetString("tendril-destroyed-warning-message"), coords, PopupType.LargeCaution);

        Timer.Spawn(TimeSpan.FromSeconds(delay),
            () =>
        {
            SpawnChasm(coords, comp.ChasmRadius);
        });
    }

    private void SpawnChasm(EntityCoordinates coords, int radius)
    {
        Spawn("FloorChasmEntity", coords);
        for (var i = 1; i <= radius; i++)
        {
            // shitcode
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X + i, coords.Y));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X - i, coords.Y));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X, coords.Y + i));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X, coords.Y - i));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X + i, coords.Y + i));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X - i, coords.Y + i));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X + i, coords.Y - i));
            Spawn("FloorChasmEntity", new EntityCoordinates(coords.EntityId, coords.X - i, coords.Y - i));
        }
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        var query = AllEntityQuery<NecropolisNestComponent>();

        _nests.Clear();
        while (query.MoveNext(out var uid, out var comp))
            _nests.Add((uid, comp));

        if (_nests.Count == 0)
            return;

        ev.AddLine("");

        foreach (var ent in _nests)
        {
            var (uid, comp) = ent;

            var points = comp.TendrilPoints;

            ev.AddLine(Loc.GetString("necropolis-nest-end-of-round",
                ("points", points)));
        }

        ev.AddLine("");
    }
}
