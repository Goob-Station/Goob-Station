using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Pinpointer;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Mind;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._DV.CosmicCult.EntitySystems;

public sealed class CosmicChantrySystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicChantryComponent, ComponentInit>(OnChantryStarted);
        SubscribeLocalEvent<CosmicChantryComponent, ComponentShutdown>(OnChantryDestroyed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var chantryQuery = EntityQueryEnumerator<CosmicChantryComponent>();
        while (chantryQuery.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.SpawnTimer && !comp.Spawned)
            {
                _appearance.SetData(uid, ChantryVisuals.Status, ChantryStatus.On);
                _popup.PopupCoordinates(Loc.GetString("cosmiccult-chantry-powerup"), Transform(uid).Coordinates, Shared.Popups.PopupType.LargeCaution);
                comp.Spawned = true;
            }
            if (_timing.CurTime >= comp.CountdownTimer)
            {
                if (!_mind.TryGetMind(comp.PolyVictim, out var mindEnt, out var mind))
                    return;
                mind.PreventGhosting = false;
                var tgtpos = Transform(uid).Coordinates;
                var colossus = Spawn(comp.Colossus, tgtpos);
                _mind.TransferTo(mindEnt, colossus);
                Spawn(comp.SpawnVFX, tgtpos);
                QueueDel(comp.PolyVictim);
                QueueDel(uid);
            }
        }
    }

    private void OnChantryStarted(Entity<CosmicChantryComponent> ent, ref ComponentInit args)
    {
        var indicatedLocation = FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString((ent, Transform(ent))));

        ent.Comp.SpawnTimer = _timing.CurTime + TimeSpan.FromSeconds(2.4);
        ent.Comp.CountdownTimer = _timing.CurTime + TimeSpan.FromSeconds(15);

        _sound.PlayGlobalOnStation(ent, _audio.ResolveSound(ent.Comp.ChantryAlarm));
        _chatSystem.DispatchStationAnnouncement(ent,
        Loc.GetString("cosmiccult-chantry-location", ("location", indicatedLocation)),
        null, false, null,
        Color.FromHex("#cae8e8"));

        if (_mind.TryGetMind(ent.Comp.PolyVictim, out _, out var mind))
            mind.PreventGhosting = true;
    }

    private void OnChantryDestroyed(Entity<CosmicChantryComponent> ent, ref ComponentShutdown args)
    {
        if (!_mind.TryGetMind(ent.Comp.PolyVictim, out _, out var mind) || !_polymorph.TryGetNetEntity(ent.Comp.PolyVictim, out _))
            return;
        mind.PreventGhosting = false;
        _polymorph.Revert(ent.Comp.PolyVictim);
    }
}
