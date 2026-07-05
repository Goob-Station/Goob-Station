// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Ghost;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.Follower;
using Content.Shared.Follower.Components;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.PDA;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StationAi;
using Content.Shared.StatusIcon;
using Content.Shared.Warps;
using Content.Shared.Body.Organ;
using Content.Shared.Bed.Cryostorage;
using Content.Goobstation.Shared.Blob.Components;
using Robust.Server.Player;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Ghost;

#region DOWNSTREAM-TPirates: ghost follow menu update
public sealed partial class GhostSystem
{
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;

    private static readonly HashSet<ProtoId<NpcFactionPrototype>> ImportantNpcFactions = new()
    {
        "AllHostile",
        "Blob",
        "Changeling",
        "DevilFaction",
        "Dragon",
        "Heretic",
        "PirateFaction",
        "Revolutionary",
        "SAN",
        "Syndicate",
        "Vampire",
        "Wizard",
        "Wraith",
        "Xeno",
        "Zombie"
    };
    private readonly Dictionary<NetEntity, int> _lastBroadcastObserverCounts = new();

    private void OnFollowChanged(FollowEvent ev)
    {
        BroadcastObserverCount(ev.Following);
    }

    private void BroadcastObserverCount(EntityUid followedEntity)
    {
        var count = _followerSystem.GetGhostFollowerCount(followedEntity);
        var followedNet = GetNetEntity(followedEntity);
        if (_lastBroadcastObserverCounts.TryGetValue(followedNet, out var previousCount) && previousCount == count)
            return;

        _lastBroadcastObserverCounts[followedNet] = count;

        var ev = new GhostWarpObserverCountChangedEvent(followedNet, count);
        var sentSessions = new HashSet<ICommonSession>();
        var ghostQuery = EntityQueryEnumerator<GhostComponent, ActorComponent>();
        while (ghostQuery.MoveNext(out _, out _, out var actor))
        {
            var session = actor.PlayerSession;
            if (!sentSessions.Add(session))
                continue;

            RaiseNetworkEvent(ev, session.Channel);
        }
    }

    private void OnWarpObserverEntityTerminating(EntityUid uid, MetaDataComponent component, ref EntityTerminatingEvent args)
    {
        if (!HasComp<FollowedComponent>(uid))
            return;

        _lastBroadcastObserverCounts.Remove(GetNetEntity(uid));
    }

    private void OnWarpObserverRoundRestart(RoundRestartCleanupEvent ev)
    {
        _lastBroadcastObserverCounts.Clear();
    }

    private IEnumerable<GhostWarp> GetLocationWarps()
    {
        var allQuery = AllEntityQuery<WarpPointComponent>();

        while (allQuery.MoveNext(out var uid, out var warp))
        {
            // Player-controlled entities (including AI eyes) should appear in "Players", not "Places".
            if (HasComp<ActorComponent>(uid) || IsStationAiEntity(uid))
                continue;

            yield return new GhostWarp(GetNetEntity(uid), warp.Location ?? Name(uid), GhostWarpType.Location, _followerSystem.GetGhostFollowerCount(uid));
        }
    }

    // Matches ShowJobIconsSystem (job/status icon HUD): default to JobIconNoId when no ID/PDA found.
    private static readonly ProtoId<JobIconPrototype> JobIconNoId = "JobIconNoId";
    private static readonly ProtoId<JobIconPrototype> JobIconBorg = "JobIconBorg";

    private bool TryGetIdCard(EntityUid uid, [NotNullWhen(true)] out IdCardComponent? idCard)
    {
        idCard = null;
        if (!_accessReader.FindAccessItemsInventory(uid, out var items))
            return false;

        foreach (var item in items)
        {
            if (TryComp(item, out idCard))
                return true;

            if (TryComp<PdaComponent>(item, out var pda) && pda.ContainedId is { Valid: true } idUid && TryComp(idUid, out idCard))
                return true;
        }

        return false;
    }

    private string GetJobIconFor(EntityUid uid)
    {
        // Borgs and AI don't carry ID/PDA; use shared silicon icon for ghost menu.
        if (HasComp<BorgChassisComponent>(uid) || IsStationAiEntity(uid))
            return JobIconBorg.Id;

        return TryGetIdCard(uid, out var idCard) ? idCard.JobIcon.Id : JobIconNoId.Id;
    }

    private bool IsStationAiEntity(EntityUid uid)
    {
        if (HasComp<StationAiHeldComponent>(uid) ||
            HasComp<StationAiOverlayComponent>(uid) ||
            HasComp<StationAiHoloComponent>(uid) ||
            HasComp<StationAiCoreComponent>(uid))
            return true;
        return false;
    }

    /// <summary>
    /// Gets job title from an entity's ID card or PDA (inventory).
    /// used for dead entities so profession persists after mind leaves (respawn).
    /// </summary>
    private string GetJobTitleFromEntity(EntityUid uid)
    {
        return TryGetIdCard(uid, out var idCard) ? idCard.LocalizedJobTitle ?? string.Empty : string.Empty;
    }

    /// <summary>
    /// Gets department prototype ID from an entity's ID card (for department-based chip color).
    /// </summary>
    private string GetDepartmentIdFromEntity(EntityUid uid)
    {
        if (!TryGetIdCard(uid, out var idCard) || idCard.JobDepartments.Count == 0)
            return string.Empty;
        return idCard.JobDepartments[0].Id;
    }

    /// <summary>
    /// Gets department prototype ID from a mind's job. Returns empty string when mind is null or has no primary department.
    /// </summary>
    private string GetDepartmentIdFromMind(EntityUid? mindId)
    {
        if (mindId is not { } id)
            return string.Empty;
        if (!_jobs.MindTryGetJobId(id, out var jobIdVal) || !jobIdVal.HasValue || !_jobs.TryGetPrimaryDepartment(jobIdVal.Value.Id, out var dept))
            return string.Empty;
        return dept.ID;
    }

    // Matches medical HUD / crew monitoring: SuitSensorSystem uses CheckVitalDamage and Critical threshold
    // for DamagePercentage; crew monitor uses index = round(4 * damage/critThreshold) -> 0.25 is first bucket boundary.
    // See Content.Server.Medical.SuitSensors.SuitSensorSystem and Content.Client.Overlays.EntityHealthBarOverlay.
    private const float WoundedDamageRatio = 0.25f;

    private GhostWarpHealthState GetGhostWarpHealthState(EntityUid uid, MobState mobState)
    {
        switch (mobState)
        {
            case MobState.Dead:
                return GhostWarpHealthState.Dead;
            case MobState.Critical:
                return GhostWarpHealthState.Critical;
            case MobState.Alive:
                if (!TryComp<DamageableComponent>(uid, out var damageable) ||
                    !_mobThresholdSystem.TryGetThresholdForState(uid, MobState.Critical, out var critThreshold))
                    return GhostWarpHealthState.Healthy;
                if (critThreshold.Value <= 0)
                    return GhostWarpHealthState.Healthy;
                var totalDamage = _mobThresholdSystem.CheckVitalDamage(uid, damageable);
                var ratio = (float)(totalDamage / critThreshold).Value;
                return ratio >= WoundedDamageRatio ? GhostWarpHealthState.Wounded : GhostWarpHealthState.Healthy;
            default:
                return GhostWarpHealthState.Unknown;
        }
    }

    /// <summary>
    /// Returns true if this entity is a borg brain (e.g. positronic brain) that is currently inside a cyborg chassis.
    /// Used to hide the brain from the ghost warp list when it is installed so we only show the cyborg.
    /// When the borg is disassembled and the brain is on its own, this returns false and the brain is shown.
    /// </summary>
    private bool IsBorgBrainInsideChassis(EntityUid uid)
    {
        if (!HasComp<BorgBrainComponent>(uid))
            return false;
        if (!_container.TryGetContainingContainer(uid, out var container))
            return false;
        return HasComp<BorgChassisComponent>(container.Owner);
    }

    /// <summary>
    /// Returns true if this warp target is an organ/brain currently attached inside a body.
    /// These are internal implementation entities and should not appear as separate player entries.
    /// </summary>
    private bool IsAttachedOrganInBody(EntityUid uid)
    {
        return TryComp<OrganComponent>(uid, out var organ) && organ.Body is { Valid: true };
    }

    private EntityUid ResolvePlayerWarpTarget(EntityUid attached)
    {
        if (!TryComp<StationAiHeldComponent>(attached, out _))
            return attached;

        if (!_container.TryGetContainingContainer(attached, out var container))
            return attached;

        if (!TryComp<StationAiCoreComponent>(container.Owner, out var core))
            return attached;

        return core.RemoteEntity is { Valid: true } remote && EntityManager.EntityExists(remote) ? remote : attached;
    }

    private IEnumerable<GhostWarp> GetPlayerWarps(IReadOnlyList<(EntityUid Target, EntityUid? MindId, MobState MobState)> playerOwnedTargets)
    {
        foreach (var (target, mindId, mobState) in playerOwnedTargets)
        {
            if (mobState == MobState.Dead)
                continue;

            var entityName = Comp<MetaDataComponent>(target).EntityName;
            var jobName = mindId is { } id
                ? _jobs.MindTryGetJobName(id)
                : GetJobTitleFromEntity(target);
            var jobIconId = GetJobIconFor(target);
            var healthState = mobState == MobState.Invalid
                ? GhostWarpHealthState.Unknown
                : GetGhostWarpHealthState(target, mobState);
            var departmentId = GetDepartmentIdFromMind(mindId);
            if (string.IsNullOrEmpty(departmentId))
                departmentId = GetDepartmentIdFromEntity(target);
            yield return new GhostWarp(GetNetEntity(target), entityName, GhostWarpType.Player, _followerSystem.GetGhostFollowerCount(target), jobIconId, mobState, jobName, healthState, departmentId);
        }
    }

    /// <summary>
    /// All dead mobs for the "Dead" section.
    /// Job/icon from entity's ID card so profession persists after respawn when mind leaves the corpse.
    /// Limited by ghost.warp_max_dead to avoid performance issues when many corpses exist.
    /// </summary>
    private IEnumerable<GhostWarp> GetDeadPlayerWarps(IReadOnlyList<(EntityUid Target, EntityUid? MindId, MobState MobState)> playerOwnedTargets)
    {
        var maxDead = _configurationManager.GetCVar(CCVars.GhostWarpMaxDead);
        var count = 0;

        foreach (var (target, mindId, mobState) in playerOwnedTargets)
        {
            if (mobState != MobState.Dead)
                continue;

            count++;
            var entityName = Comp<MetaDataComponent>(target).EntityName;
            var jobName = mindId is { } id
                ? _jobs.MindTryGetJobName(id)
                : GetJobTitleFromEntity(target);
            var jobIconId = GetJobIconFor(target);
            var healthState = GhostWarpHealthState.Dead;
            var departmentId = GetDepartmentIdFromMind(mindId);
            if (string.IsNullOrEmpty(departmentId))
                departmentId = GetDepartmentIdFromEntity(target);
            yield return new GhostWarp(GetNetEntity(target), entityName, GhostWarpType.Dead, _followerSystem.GetGhostFollowerCount(target), jobIconId, mobState, jobName, healthState, departmentId);

            if (count >= maxDead)
                break;
        }
    }

    /// <summary>
    /// Builds all player-owned entities for ghost warp classification.
    /// This is keyed by real player minds (has user id) and then resolved to the visible target
    /// (e.g. AI eye remote entity) to avoid session-only blind spots and ad-hoc SSD handling.
    /// </summary>
    private List<(EntityUid Target, EntityUid? MindId, MobState MobState)> BuildPlayerOwnedWarpIndex(EntityUid except)
    {
        var seenTargets = new HashSet<EntityUid>();
        var historicallyPlayerOwned = new HashSet<EntityUid>();
        var result = new List<(EntityUid Target, EntityUid? MindId, MobState MobState)>();
        var mindQuery = AllEntityQuery<MindComponent>();
        while (mindQuery.MoveNext(out _, out var knownMind))
        {
            // Only treat minds that are/were associated with a player as ownership evidence.
            if (knownMind.UserId == null && knownMind.OriginalOwnerUserId == null)
                continue;

            if (knownMind.OwnedEntity is { Valid: true } ownedEntity)
                historicallyPlayerOwned.Add(ownedEntity);

            if (knownMind.OriginalOwnedEntity is { } originalNet)
            {
                var originalEntity = GetEntity(originalNet);
                if (Exists(originalEntity))
                    historicallyPlayerOwned.Add(originalEntity);
            }
        }

        var ownerQuery = AllEntityQuery<MindContainerComponent>();

        while (ownerQuery.MoveNext(out var ownerUid, out var mindContainer))
        {
            // Include "catatonic" bodies that have a mind container intended for player examine,
            // even when no mind is currently linked.
            EntityUid? mindId = null;
            if (mindContainer.HasMind)
            {
                if (!TryComp<MindComponent>(mindContainer.Mind, out var mindComp))
                    continue;
                if (mindComp.UserId == null && mindComp.OriginalOwnerUserId == null)
                    continue;
                mindId = mindContainer.Mind!.Value;
            }
            else if (!HasComp<MindExaminableComponent>(ownerUid))
            {
                continue;
            }
            else if (!historicallyPlayerOwned.Contains(ownerUid))
            {
                continue;
            }

            var target = ResolvePlayerWarpTarget(ownerUid);
            if (target == except || !seenTargets.Add(target))
                continue;

            // Cryosleeped players are moved to a paused map and should not appear in the warp menu.
            if (HasComp<CryostorageContainedComponent>(target) && IsPaused(target))
                continue;

            // Hide internal attached organs/brains to avoid duplicate entries (body + its internal brain).
            if (IsAttachedOrganInBody(target))
                continue;

            // Regular observer ghosts are listed in the "Ghosts" section, not "Players"/"Dead".
            if (_ghostQuery.HasComp(target) && !IsStationAiEntity(target))
                continue;

            if (IsBorgBrainInsideChassis(target))
                continue;

            var mobState = TryComp<MobStateComponent>(target, out var mob) ? mob.CurrentState : MobState.Invalid;
            if (mobState is not (MobState.Alive or MobState.Critical or MobState.Dead or MobState.Invalid))
                continue;

            result.Add((target, mindId, mobState));
        }

        return result;
    }

    /// <summary>
    /// Other ghost entities for the "Ghosts" section.
    /// </summary>
    private IEnumerable<GhostWarp> GetGhostWarps(EntityUid except)
    {
        var query = AllEntityQuery<GhostComponent, MetaDataComponent>();
        while (query.MoveNext(out var uid, out var _, out var meta))
        {
            if (uid == except)
                continue;

            var name = meta.EntityName;
            if (string.IsNullOrWhiteSpace(name))
                continue;

            yield return new GhostWarp(GetNetEntity(uid), name, GhostWarpType.Ghost, _followerSystem.GetGhostFollowerCount(uid));
        }
    }

    /// <summary>
    /// Living mobs without a mind (NPCs) for the "Mobs" section.
    /// Dead mindless mobs are in "Dead", so we exclude dead here.
    /// </summary>
    private IEnumerable<GhostWarp> GetMobWarps(IReadOnlyList<(EntityUid Target, EntityUid? MindId, MobState MobState)> playerOwnedTargets)
    {
        var maxNpcs = _configurationManager.GetCVar(CCVars.GhostWarpMaxMobs);
        var count = 0;
        var playerOwnedTargetSet = new HashSet<EntityUid>(playerOwnedTargets.Select(x => x.Target));
        var query = AllEntityQuery<MobStateComponent, MetaDataComponent>();
        while (query.MoveNext(out var uid, out var mobState, out var meta) && count < maxNpcs)
        {
            // Already represented in Players/Dead sections.
            if (playerOwnedTargetSet.Contains(uid))
                continue;

            if (_ghostQuery.HasComp(uid))
                continue;

            if (_mobState.IsDead(uid))
                continue;

            if (TryComp<MindContainerComponent>(uid, out var mind) && mind.HasMind)
                continue;

            // Blob tiles are extremely noisy in warp list; keep only the core tile visible.
            if (TryComp<BlobTileComponent>(uid, out var blobTile) &&
                blobTile.BlobTileType != BlobTileType.Core)
                continue;

            // Hide ambient mindless mobs by default; keep only "interesting" NPC targets.
            var hasGhostRoleTargeting = HasComp<GhostRoleComponent>(uid) || HasComp<GhostTakeoverAvailableComponent>(uid);
            var hasImportantFaction = TryComp<NpcFactionMemberComponent>(uid, out var factionComp) &&
                                      _npcFaction.IsMemberOfAny((uid, factionComp), ImportantNpcFactions);
            if (!hasGhostRoleTargeting && !hasImportantFaction)
                continue;

            // Don't show positronic brain in list when it's inside a cyborg; show the cyborg only.
            if (IsBorgBrainInsideChassis(uid))
                continue;

            var name = meta.EntityName;
            if (string.IsNullOrWhiteSpace(name))
                continue;

            count++;
            var mobStateValue = mobState.CurrentState;
            var healthState = GetGhostWarpHealthState(uid, mobStateValue);
            var jobIconId = GetJobIconFor(uid);
            // Try to get department/job from ID so NPCs with ID cards get department colors.
            var professionTitle = GetJobTitleFromEntity(uid);
            var departmentId = GetDepartmentIdFromEntity(uid);
            yield return new GhostWarp(GetNetEntity(uid), name, GhostWarpType.Mob, _followerSystem.GetGhostFollowerCount(uid), jobIconId, mobStateValue, professionTitle, healthState, departmentId);
        }
    }
}
#endregion
