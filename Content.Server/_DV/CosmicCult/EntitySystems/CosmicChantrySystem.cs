using Content.Server.Antag;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._DV.CosmicCult.EntitySystems;

public sealed class CosmicChantrySystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;

    /// <summary>
    /// Mind role to add to colossi.
    /// </summary>
    public static readonly EntProtoId MindRole = "MindRoleCosmicColossus";
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
                if (!_mind.TryGetMind(comp.InternalVictim, out var mindEnt, out var mind))
                    return;
                mind.PreventGhosting = false;
                var tgtpos = Transform(uid).Coordinates;
                var colossus = Spawn(comp.Colossus, tgtpos);
                _mind.TransferTo(mindEnt, colossus);
                _mind.TryAddObjective(mindEnt, mind, "CosmicFinalityObjective");
                _role.MindAddRole(mindEnt, MindRole, mind, true);
                _antag.SendBriefing(colossus, Loc.GetString("cosmiccult-silicon-colossus-briefing"), Color.FromHex("#4cabb3"), null);
                Spawn(comp.SpawnVFX, tgtpos);
                QueueDel(comp.InternalVictim);
                QueueDel(uid);
            }
        }
    }

    private void OnChantryStarted(Entity<CosmicChantryComponent> ent, ref ComponentInit args)
    {
        var indicatedLocation = FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString((ent, Transform(ent))));
        var comp = ent.Comp;

        comp.SpawnTimer = _timing.CurTime + comp.SpawningTime;
        comp.CountdownTimer = _timing.CurTime + comp.EventTime;

        _sound.PlayGlobalOnStation(ent, _audio.ResolveSound(comp.ChantryAlarm));
        _chatSystem.DispatchStationAnnouncement(ent,
        Loc.GetString("cosmiccult-chantry-location", ("location", indicatedLocation)),
        null, false, null,
        Color.FromHex("#cae8e8"));

        if (_mind.TryGetMind(comp.InternalVictim, out _, out var mind))
            mind.PreventGhosting = true;
    }

    private void OnChantryDestroyed(Entity<CosmicChantryComponent> ent, ref ComponentShutdown args)
    {
        var comp = ent.Comp;
        if (!_mind.TryGetMind(comp.InternalVictim, out var mindId, out var mind))
            return;

        mind.PreventGhosting = false;
        _mind.TransferTo(mindId, comp.VictimBody);
        QueueDel(comp.InternalVictim);
    }
}
