using System.Linq;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

// todo
// speaking only to host
namespace Content.Goobstation.Server.SplitPersonality;
public sealed partial class SplitPersonalitySystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SplitPersonalityComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<SplitPersonalityComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<SplitPersonalityDummyComponent, TakeGhostRoleEvent>(OnGhostRoleTaken);
    }

    private void OnInit(EntityUid uid, SplitPersonalityComponent comp, MapInitEvent args)
    {
        comp.NextSwapAttempt = _timing.CurTime + comp.SwapAttemptDelay;
        comp.MindsContainer = _container.EnsureContainer<Container>(uid, "SplitPersonalityContainer");

        var popup = Loc.GetString("split-personality-start-popup");
        _popup.PopupEntity(popup, uid, uid, PopupType.LargeCaution);
        _stun.TryParalyze(uid, TimeSpan.FromSeconds(6), true);

        if (!_mind.TryGetMind(uid, out var hostMindId, out var hostMind))
            return;

        comp.OriginalMind = hostMindId;
        comp.MindRoles.AddRange(hostMind.MindRoles);
        comp.Objectives.AddRange(hostMind.Objectives);

        comp.AdditionalMindsCount = Math.Max(1, comp.AdditionalMindsCount);

        for (var i = 0; i < comp.AdditionalMindsCount; i++)
            SpawnDummy(uid, comp);
    }
    private void OnGhostRoleTaken(EntityUid dummy, SplitPersonalityDummyComponent comp, TakeGhostRoleEvent args)
    {
        // Transfer the mind to visit the host entity
        if (!TryComp<SplitPersonalityComponent>(comp.Host, out var hostComp))
            return;

        if (!_mind.TryGetMind(args.Player, out var dummyMind, out var mindComponent))
            return;

        _mind.ControlMob(args.Player.UserId, dummy);

        mindComponent.MindRoles.AddRange(hostComp.MindRoles);

        foreach (var objective in hostComp.Objectives)
            _mind.AddObjective(dummyMind, mindComponent, objective);
    }

    private void OnRemove(EntityUid uid, SplitPersonalityComponent comp, ComponentRemove args)
    {
        if (comp.OriginalMind is { } originalMind)
            _mind.TransferTo(originalMind, uid);

        foreach (var dummy in comp.GhostRoleDummies.Where(dummy => !TerminatingOrDeleted(dummy)))
            QueueDel(dummy);

        comp.GhostRoleDummies.Clear();
    }
    private bool TryAlternateMind(SplitPersonalityComponent comp)
    {
        if (!_random.Prob(comp.SwapProbability))
            return false;

        var eligibleDummies = // linq monstrosity
            comp.GhostRoleDummies.Where
            (dummy => !TerminatingOrDeleted(dummy)
            && dummy is { } dummyResolved
            && _mind.TryGetMind(dummyResolved, out _, out _))
            .ToList();

        if (eligibleDummies.Count == 0)
            return false;

        var selectedDummy = _random.Pick(eligibleDummies);

        if (!TryComp<SplitPersonalityDummyComponent>(selectedDummy, out var selectedDummyComponent)
            || selectedDummyComponent.Host is not { } dummyHostResolved
            || selectedDummy is not { } selectedDummyResolved)
            return false;

        return TrySwapMinds(dummyHostResolved, selectedDummyResolved);
    }

    #region helper Methods

    private bool TrySwapMinds(EntityUid host, EntityUid dummy)
    {
        if (!_mind.TryGetMind(host, out var hostMindId, out _)
            || !_mind.TryGetMind(dummy, out var dummyMindId, out _))
            return false;

        var popup = Loc.GetString("split-personality-swap-popup");
        _popup.PopupEntity(popup, dummy, dummy, PopupType.LargeCaution);

        _mind.TransferTo(hostMindId, dummy);
        _mind.TransferTo(dummyMindId, host);
        return true;
    }

    private bool TryReturnMind(SplitPersonalityComponent comp)
    {
        if (comp.OriginalMind is not { } originalMind)
            return false;

        foreach (var dummy in comp.GhostRoleDummies)
        {
            if (dummy is not { } dummyResolved
                || !_mind.TryGetMind(dummyResolved, out var dummyMindId, out _)
                || dummyMindId != originalMind)
                continue;

            if (TryComp<SplitPersonalityDummyComponent>(dummy, out var dummyComp)
                && dummyComp.Host is { } hostResolved)
                return TrySwapMinds(hostResolved, dummyResolved);
        }
        return false;
    }

    private void SpawnDummy(EntityUid hostUid, SplitPersonalityComponent comp)
    {
        var dummy = Spawn("SplitPersonalityDummy", MapCoordinates.Nullspace);
        _container.Insert(dummy, comp.MindsContainer);
        comp.GhostRoleDummies.Add(dummy);

        var ghostRole = EnsureComp<GhostRoleComponent>(dummy);
        ghostRole.RoleName = $"Split Personality of {Name(hostUid)}";
        ghostRole.RoleDescription = "A fragmented piece of the host's psyche.";

        _meta.SetEntityName(dummy, $"Split Personality of {Name(hostUid)}");
        EnsureComp<SplitPersonalityDummyComponent>(dummy).Host = hostUid;
    }

    #endregion
    public override void Update(float frametime)
    {
        base.Update(frametime);

        var query = EntityQueryEnumerator<SplitPersonalityComponent>();
        while (query.MoveNext(out var comp))
        {
            if (_timing.CurTime < comp.NextSwapAttempt)
                continue;

            if (TryReturnMind(comp))
                continue;

            TryAlternateMind(comp);

            comp.NextSwapAttempt = _timing.CurTime + comp.SwapAttemptDelay;
        }
    }
}
