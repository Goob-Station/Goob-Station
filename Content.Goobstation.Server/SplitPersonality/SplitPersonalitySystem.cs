// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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

    private void OnInit(Entity<SplitPersonalityComponent> ent, ref MapInitEvent args)
    {
        if (!_mind.TryGetMind(ent, out var hostMindId, out var hostMind))
            return;

        ent.Comp.NextSwapAttempt = _timing.CurTime + ent.Comp.SwapAttemptDelay;
        ent.Comp.MindsContainer = _container.EnsureContainer<Container>(ent, "SplitPersonalityContainer");

        if (ent.Comp.DoStartFlavor)
        {
            var popup = Loc.GetString("split-personality-start-popup");
            _popup.PopupEntity(popup, ent, ent, PopupType.LargeCaution);
            _stun.TryParalyze(ent, TimeSpan.FromSeconds(6), true);
        }

        ent.Comp.OriginalMind = hostMindId;
        ent.Comp.MindRoles.AddRange(hostMind.MindRoles);
        ent.Comp.Objectives.AddRange(hostMind.Objectives);

        ent.Comp.AdditionalMindsCount = Math.Max(1, ent.Comp.AdditionalMindsCount);

        for (var i = 0; i < ent.Comp.AdditionalMindsCount; i++)
            SpawnDummy(ent);
    }
    private void OnGhostRoleTaken(Entity<SplitPersonalityDummyComponent> dummy, ref TakeGhostRoleEvent args)
    {
        // Transfer the mind to visit the host entity
        if (!TryComp<SplitPersonalityComponent>(dummy.Comp.Host, out var hostComp)
            || !_mind.TryGetMind(args.Player, out var dummyMind, out var mindComponent))
            return;

        _mind.ControlMob(args.Player.UserId, dummy);

        mindComponent.MindRoles.AddRange(hostComp.MindRoles);

        foreach (var objective in hostComp.Objectives)
            _mind.AddObjective(dummyMind, mindComponent, objective);
    }

    private void OnRemove(Entity<SplitPersonalityComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.OriginalMind is { } originalMind)
            _mind.TransferTo(originalMind, ent);

        _container.CleanContainer(ent.Comp.MindsContainer);
        ent.Comp.GhostRoleDummies.Clear();
    }
    private bool TryAlternateMind(SplitPersonalityComponent comp)
    {
        if (!_random.Prob(comp.SwapProbability))
            return false;

        var eligibleDummies = comp.GhostRoleDummies // evil linq
            .Where(dummy => dummy != null
             && _mind.TryGetMind(dummy.Value, out _, out _))
            .Select(dummy => dummy!.Value)
            .ToList();

        if (eligibleDummies.Count == 0)
            return false;

        var selectedDummy = _random.Pick(eligibleDummies);

        if (!TryComp<SplitPersonalityDummyComponent>(selectedDummy, out var selectedDummyComponent)
            || selectedDummyComponent.Host is not { } host)
            return false;

        return TrySwapMinds(host, selectedDummy);
    }

    #region Helper Methods

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

        foreach (var dummyNullable in comp.GhostRoleDummies)
        {
            if (dummyNullable is not { } dummy
                || !_mind.TryGetMind(dummy, out var dummyMindId, out _)
                || dummyMindId != originalMind)
                continue;

            if (TryComp<SplitPersonalityDummyComponent>(dummy, out var dummyComp)
                && dummyComp.Host is { } hostResolved)
                return TrySwapMinds(hostResolved, dummy);
        }
        return false;
    }

    private void SpawnDummy(Entity<SplitPersonalityComponent> host)
    {
        var dummy = Spawn("SplitPersonalityDummy", MapCoordinates.Nullspace);
        _container.Insert(dummy, host.Comp.MindsContainer);
        host.Comp.GhostRoleDummies.Add(dummy);

        var ghostRole = EnsureComp<GhostRoleComponent>(dummy);

        var name = Loc.GetString("split-personality-dummy-name", ("host", Name(host)));
        var desc = Loc.GetString("split-personality-dummy-description");

        ghostRole.RoleName = name;
        ghostRole.RoleDescription = desc;
        _meta.SetEntityName(dummy, name);

        EnsureComp<SplitPersonalityDummyComponent>(dummy).Host = host;
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
