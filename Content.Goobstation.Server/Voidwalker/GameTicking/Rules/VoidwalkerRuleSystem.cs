using Content.Goobstation.Server.Voidwalker.Roles;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Roles;
using Content.Shared.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Localizations;
using Content.Shared.NPC.Systems;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.Voidwalker.GameTicking.Rules;

public sealed class VoidwalkerRuleSystem : GameRuleSystem<VoidwalkerRuleComponent>
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidwalkerRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
        SubscribeLocalEvent<VoidwalkerRoleComponent, GetBriefingEvent>(UpdateBriefing);
    }

    private void UpdateBriefing(Entity<VoidwalkerRoleComponent> voidwalker, ref GetBriefingEvent args)
    {
        var ent = args.Mind.Comp.OwnedEntity;

        if (ent is not { } entity)
            return;

        args.Append(MakeBriefing(entity));
    }

    private void OnSelectAntag(Entity<VoidwalkerRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        var target = args.EntityUid;

        if (!_mind.TryGetMind(target, out var mindId, out _)
            || !_roleSystem.MindHasRole<VoidwalkerRoleComponent>(mindId))
            return;

        _antag.SendBriefing(target, MakeBriefing(target), Color.DarkCyan, ent.Comp.BriefingSound);
    }

    private string MakeBriefing(EntityUid voidwalker)
    {
        var direction = string.Empty;
        var voidwalkerXform = Transform(voidwalker);

        EntityUid? stationGrid = null;
        if (_station.GetStationInMap(voidwalkerXform.MapID) is { } station)
            stationGrid = _station.GetLargestGrid(station);

        if (stationGrid is not null)
        {
            var stationPosition = _transform.GetWorldPosition((EntityUid) stationGrid);
            var dragonPosition = _transform.GetWorldPosition(voidwalker);

            var vectorToStation = stationPosition - dragonPosition;
            direction = ContentLocalizationManager.FormatDirection(vectorToStation.GetDir());
        }

        var briefing = Loc.GetString("voidwalker-role-briefing", ("direction", direction));

        return briefing;
    }
}
