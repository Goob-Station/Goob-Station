// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Antag;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Server.Stunnable;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.SplitPersonality;
public sealed partial class SplitPersonalitySystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem           _mind       = null!;
    [Dependency] private readonly ContainerSystem            _container  = null!;
    [Dependency] private readonly MetaDataSystem             _meta       = null!;
    [Dependency] private readonly IGameTiming                _timing     = null!;
    [Dependency] private readonly IRobustRandom              _random     = null!;
    [Dependency] private readonly PopupSystem                _popup      = null!;
    [Dependency] private readonly StunSystem                 _stun       = null!;
    [Dependency] private readonly CollectiveMindUpdateSystem _collective = null!;
    [Dependency] private readonly ISharedAdminLogManager     _admin      = null!;
    [Dependency] private readonly AntagSelectionSystem       _antag      = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SplitPersonalityComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<SplitPersonalityComponent, ComponentRemove>(OnRemove);

        SubscribeLocalEvent<SplitPersonalityComponent, BeforePolymorphedEvent>(OnBeforePolymorphed);

        SubscribeLocalEvent<SplitPersonalityDummyComponent, TakeGhostRoleEvent>(OnGhostRoleTaken);
        SubscribeLocalEvent<SplitPersonalityDummyComponent, SpeakAttemptEvent>(OnSpeakAttempt);
    }

    private void OnInit(Entity<SplitPersonalityComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.DoStartFlavor)
        {
            var popup = Loc.GetString("split-personality-start-popup");
            _popup.PopupEntity(popup, ent, ent, PopupType.LargeCaution);
            _stun.TryParalyze(ent, TimeSpan.FromSeconds(6), true);
        }

        SplitMind(ent);
    }
    private void OnRemove(Entity<SplitPersonalityComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.OriginalMind is not { } originalMind
            || TerminatingOrDeleted(ent.Comp.OriginalMind)
            || TerminatingOrDeleted(ent))
            return;

        _mind.TransferTo(originalMind, ent);
        _container.CleanContainer(ent.Comp.MindsContainer);

        foreach (var dummyNullable in ent.Comp.GhostRoleDummies)
        {
            if (dummyNullable is { } dummy)
                QueueDel(dummy);
        }

        ent.Comp.GhostRoleDummies.Clear();

        if (TryComp<CollectiveMindComponent>(ent, out var collective))
            collective.Channels.Remove(ent.Comp.CollectiveMind);
    }

    private void OnGhostRoleTaken(Entity<SplitPersonalityDummyComponent> dummy, ref TakeGhostRoleEvent args)
    {
        // Transfer the mind to visit the host entity
        if (dummy.Comp.Host is not { } host
            || TerminatingOrDeleted(host)
            || !TryComp<SplitPersonalityComponent>(host, out var hostComp)
            || !_mind.TryGetMind(args.Player, out var dummyMind, out var mindComponent))
            return;

        _mind.ControlMob(args.Player.UserId, dummy);

        mindComponent.MindRoles.AddRange(hostComp.MindRoles);

        foreach (var objective in hostComp.Objectives)
            _mind.AddObjective(dummyMind, mindComponent, objective);

        var briefing = Loc.GetString("split-personality-role-greeting", ("hostName", Name(host)));
        _antag.SendBriefing(dummy, briefing, Color.CornflowerBlue, null);
    }

    private bool TryAlternateMind(SplitPersonalityComponent comp)
    {
        if (!_random.Prob(comp.SwapProbability))
            return false;

        var eligibleDummies = comp.GhostRoleDummies
            .Where(dummy => dummy != null && _mind.TryGetMind(dummy.Value, out _, out _))
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

    private void OnBeforePolymorphed(Entity<SplitPersonalityComponent> ent, ref BeforePolymorphedEvent args) =>
        RemCompDeferred(ent, ent.Comp); // surely this won't be abused :clueless:

    private void OnSpeakAttempt(Entity<SplitPersonalityDummyComponent> ent, ref SpeakAttemptEvent args) =>
        args.Cancel();

    #region Helper Methods

    private Entity<SplitPersonalityComponent>? SplitMind(EntityUid uid, SplitPersonalityComponent? comp = null)
    {
        if (!Resolve(uid, ref comp)
            || !_mind.TryGetMind(uid, out var hostMindId, out var hostMind))
            return null;

        var collectiveMind = EnsureComp<CollectiveMindComponent>(uid);
        collectiveMind.Channels.Add(comp.CollectiveMind);
        collectiveMind.DefaultChannel = comp.CollectiveMind;

        _collective.CreateOrJoinWeb(uid, comp.CollectiveMind);

        comp.NextSwapAttempt = _timing.CurTime + comp.SwapAttemptDelay;
        comp.MindsContainer  = _container.EnsureContainer<Container>(uid, "SplitPersonalityContainer");

        comp.OriginalMind = hostMindId;
        comp.MindRoles.AddRange(hostMind.MindRoles);
        comp.Objectives.AddRange(hostMind.Objectives);

        for (var i = 0; i < comp.AdditionalMindsCount; i++)
            SpawnDummy((uid, comp));

        return (uid, comp);
    }

    private bool TrySwapMinds(EntityUid controlled, EntityUid controlling)
    {
        if (!_mind.TryGetMind(controlled, out var controlledMindId, out _)
            || !_mind.TryGetMind(controlling, out var controllingMindId, out _))
            return false;

        var popup = Loc.GetString("split-personality-swap-popup");
        _popup.PopupEntity(popup, controlled, controlled, PopupType.LargeCaution);

        _admin.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(controlling)} has taken control of: {ToPrettyString(controlled)}");

        _mind.TransferTo(controlledMindId, controlling);
        _mind.TransferTo(controllingMindId, controlled);
        return true;
    }

    private bool TryReturnMind(SplitPersonalityComponent comp)
    {
        if (comp.OriginalMind is not { } originalMind)
            return false;

        foreach (var dummyNullable in comp.GhostRoleDummies)
        {
            if (dummyNullable is not { } dummy
                || TerminatingOrDeleted(dummy)
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
        if (dummy == EntityUid.Invalid)
            return;

        if (!TryInsertDummy(host, dummy))
            return;

        var name = Loc.GetString("split-personality-dummy-name", ("host", Name(host)), ("count", host.Comp.GhostRoleDummies.Count));
        _meta.SetEntityName(dummy, name);

        var ghostRole = EnsureComp<GhostRoleComponent>(dummy);

        ghostRole.RoleName = name;

        if (!TryComp<CollectiveMindComponent>(host, out var hostCollective)
            || !hostCollective.WebMemberships.TryGetValue(host.Comp.CollectiveMind.Id, out var hostMembership))
            return;

        var collectiveMind = EnsureComp<CollectiveMindComponent>(dummy);

        collectiveMind.Channels.Add(host.Comp.CollectiveMind);
        collectiveMind.DefaultChannel = host.Comp.CollectiveMind;

        _collective.CreateOrJoinWeb(dummy, host.Comp.CollectiveMind, hostMembership.WebId);
    }

    private bool TryInsertDummy(Entity<SplitPersonalityComponent> host, EntityUid dummy)
    {
        if (_container.Insert(dummy, host.Comp.MindsContainer))
        {
            host.Comp.GhostRoleDummies.Add(dummy);
            EnsureComp<SplitPersonalityDummyComponent>(dummy).Host = host;

            return true;
        }

        QueueDel(dummy);
        return false;
    }

    #endregion
    public override void Update(float frametime)
    {
        base.Update(frametime);

        var query = EntityQueryEnumerator<SplitPersonalityComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (TerminatingOrDeleted(uid)
                ||_timing.CurTime < comp.NextSwapAttempt
                || TryReturnMind(comp))
                continue;

            TryAlternateMind(comp);
            comp.NextSwapAttempt = _timing.CurTime + comp.SwapAttemptDelay;
        }
    }
}
