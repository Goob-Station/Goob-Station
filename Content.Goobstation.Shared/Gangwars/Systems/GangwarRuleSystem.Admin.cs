using Content.Goobstation.Shared.Gangwars.Administration;
using Content.Goobstation.Shared.Gangwars.Components;
using Content.Goobstation.Shared.Gangwars.Events;
using Content.Shared.Actions;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Gangwars.Systems;

public sealed partial class GangwarRuleSystem
{
    #region Admin tools

    /// <summary>
    /// Builds a snapshot of every gang for the admin panel.
    /// Gangs with no members still appear as long as the rule remembers their name.
    /// </summary>
    public IReadOnlyCollection<GangAdminInfo> BuildGangList()
    {
        var gangs = new Dictionary<Color, GangAdminInfo>();

        if (TryGetRule(out _, out var rule))
        {
            foreach (var (color, name) in rule.GangNames)
                gangs[color] = new GangAdminInfo { Color = color, Name = name, Members = new() };
        }

        var query = AllEntityQuery<GangMemberComponent>();
        while (query.MoveNext(out var uid, out var member))
        {
            if (member.Gang is not { } color)
                continue;

            if (!gangs.TryGetValue(color, out var info))
            {
                info = new GangAdminInfo { Color = color, Name = member.GangName ?? color.ToHex(), Members = new() };
                gangs[color] = info;
            }

            info.Members.Add(new GangAdminMemberInfo
            {
                Entity = GetNetEntity(uid),
                Name = Name(uid),
                IsLeader = HasComp<GangLeaderComponent>(uid),
                Points = member.GangPoints,
            });
        }

        return gangs.Values;
    }

    /// <summary>
    /// Lists every player-controlled entity that isn't a ghost, so an admin can pick someone to add to a gang.
    /// </summary>
    public List<GangAdminPlayerInfo> BuildPlayerList()
    {
        var players = new List<GangAdminPlayerInfo>();

        var query = AllEntityQuery<ActorComponent>();
        while (query.MoveNext(out var uid, out var actor))
        {
            if (HasComp<GhostComponent>(uid))
                continue;

            players.Add(new GangAdminPlayerInfo
            {
                Entity = GetNetEntity(uid),
                Name = $"{Name(uid)} ({actor.PlayerSession.Name})",
                InGang = HasComp<GangMemberComponent>(uid),
            });
        }

        return players;
    }

    public bool AddMember(Color gangColor, EntityUid player)
    {

        if (HasComp<GangMemberComponent>(player)
            || !TryGetRule(out _, out var rule)
            || !rule.GangNames.TryGetValue(gangColor, out var gangName))
            return false;

        var gangMember = EnsureComp<GangMemberComponent>(player);
        gangMember.Gang = gangColor;
        gangMember.GangName = gangName;
        Dirty(player, gangMember);

        if (_mind.TryGetMind(player, out var mindId, out _))
            _role.MindAddRole(mindId, GangMemberMindRole, null, true);

        RaiseLocalEvent(player, new GangMemberRecruitedEvent());
        return true;
    }

    /// <summary>
    /// Reassigns a gangs name and colour, updating every member, locker, painted structure,
    /// piece of gang clothing and the gamerule.
    /// </summary>
    public void MigrateGang(Color oldColor, Color newColor, string newName)
    {
        var recolor = oldColor != newColor;

        var memberQuery = AllEntityQuery<GangMemberComponent>();
        while (memberQuery.MoveNext(out var uid, out var member))
        {
            if (member.Gang != oldColor)
                continue;

            if (recolor)
                member.Gang = newColor;
            member.GangName = newName;
            Dirty(uid, member);
        }

        if (recolor)
        {
            var lockerQuery = AllEntityQuery<GangLockerComponent>();
            while (lockerQuery.MoveNext(out var uid, out var locker))
            {
                if (locker.GangColor != oldColor)
                    continue;
                locker.GangColor = newColor;
                Dirty(uid, locker);
            }

            var colorQuery = AllEntityQuery<GangColorComponent>();
            while (colorQuery.MoveNext(out var uid, out var color))
            {
                if (color.GangColor != oldColor)
                    continue;
                color.GangColor = newColor;
                Dirty(uid, color);
            }

            var territoryQuery = AllEntityQuery<GangTerritoryComponent>();
            while (territoryQuery.MoveNext(out var uid, out var territory))
            {
                if (territory.GangColor != oldColor)
                    continue;
                territory.GangColor = newColor;
                Dirty(uid, territory);
            }

            var clothingQuery = AllEntityQuery<GangClothingComponent>();
            while (clothingQuery.MoveNext(out var uid, out var clothing))
            {
                if (clothing.Gang != oldColor)
                    continue;
                clothing.Gang = newColor;
                Dirty(uid, clothing);
            }
        }

        if (TryGetRule(out var ruleEntity, out var rule))
        {
            if (recolor)
            {
                rule.GangNames.Remove(oldColor);
                if (rule.GangScores.Remove(oldColor, out var score))
                    rule.GangScores[newColor] = score;
            }

            rule.GangNames[newColor] = newName;
            Dirty(ruleEntity, rule);
        }
    }

    /// <summary>
    /// Sends the gang's leader back to the colour-pick screen. Their next choice migrates the existing gang.
    /// </summary>
    public bool ForceRemake(Color color)
    {
        if (!TryGetGangLeader(color, out var leader)
            || !TryComp<ActorComponent>(leader.Owner, out var actor))
            return false;

        leader.Comp.PendingRemakeColor = color;
        Dirty(leader);

        TryGetRule(out _, out var rule);
        RaiseNetworkEvent(new GangLeaderNeedsColorPickEvent(rule.GangNames), actor.PlayerSession);
        return true;
    }

    public bool KickMember(EntityUid member)
    {
        if (!HasComp<GangMemberComponent>(member))
            return false;

        if (HasComp<GangLeaderComponent>(member))
            RemCompDeferred<GangLeaderComponent>(member);

        RemCompDeferred<GangMemberComponent>(member);

        if (_mind.TryGetMind(member, out var mindId, out var mind))
        {
            _role.MindRemoveRole(mindId, GangMemberMindRole.Id);
            _role.MindRemoveRole(mindId, GangLeaderMindRole.Id);
            RemoveGangObjectives(mindId, mind);
        }

        return true;
    }

    private void RemoveGangObjectives(EntityUid mindId, MindComponent mind)
    {
        for (var i = mind.Objectives.Count - 1; i >= 0; i--)
        {
            var id = MetaData(mind.Objectives[i]).EntityPrototype?.ID;
            if (id != null && Array.IndexOf(GangLeaderObjectives, id) != -1)
                _mind.TryRemoveObjective(mindId, mind, i);
        }
    }

    public bool SetLeader(EntityUid member)
    {
        if (!TryComp<GangMemberComponent>(member, out var memberComp) || memberComp.Gang is not { } color)
            return false;

        EntityUid? locker = null;
        var hadPreviousLeader = false;
        var invitesUsed = 0;
        var maxInvites = 0;
        if (TryGetGangLeader(color, out var currentLeader))
        {
            if (currentLeader.Owner == member)
                return true;

            hadPreviousLeader = true;
            locker = currentLeader.Comp.GangLocker;
            invitesUsed = currentLeader.Comp.MembersRecruited;
            maxInvites = currentLeader.Comp.MaxRecruits;
            RemCompDeferred<GangLeaderComponent>(currentLeader);

            // Demote the old leader back to a regular member's role and objectives.
            SwapGangRole(currentLeader.Owner, GangLeaderMindRole, GangMemberMindRole, GangMemberObjectives);
        }

        var newLeader = EnsureComp<GangLeaderComponent>(member);
        newLeader.GangLocker = locker;
        if (hadPreviousLeader)
        {
            newLeader.MembersRecruited = invitesUsed;
            newLeader.MaxRecruits = maxInvites;
        }
        _actions.AddAction(member, ref newLeader.ActionEnt, newLeader.SummonLockerAction);
        if (newLeader.MembersRecruited < newLeader.MaxRecruits)
            _actions.AddAction(member, ref newLeader.MemberOfferActionEnt, newLeader.MemberOfferAction);
        Dirty(member, newLeader);

        if (locker is { } lockerUid && TryComp<GangLockerComponent>(lockerUid, out var lockerComp))
        {
            lockerComp.OwnerLeader = member;
            Dirty(lockerUid, lockerComp);
        }

        SwapGangRole(member, GangMemberMindRole, GangLeaderMindRole, GangLeaderObjectives);

        return true;
    }

    /// <summary>
    /// Swaps a gangster's mind role and gang objectives, used when promoting a member to leader or demoting back.
    /// </summary>
    private void SwapGangRole(EntityUid gangster, EntProtoId oldRole, EntProtoId newRole, string[] newObjectives)
    {
        if (!_mind.TryGetMind(gangster, out var mindId, out var mind))
            return;

        _role.MindRemoveRole(mindId, oldRole.Id);
        _role.MindAddRole(mindId, newRole, mind, true);

        var kept = new HashSet<string>();
        for (var i = mind.Objectives.Count - 1; i >= 0; i--)
        {
            var id = MetaData(mind.Objectives[i]).EntityPrototype?.ID;
            if (id == null
                || Array.IndexOf(GangLeaderObjectives, id) == -1
                || Array.IndexOf(newObjectives, id) != -1 && kept.Add(id))
                continue;

            _mind.TryRemoveObjective(mindId, mind, i);
        }

        foreach (var objective in newObjectives)
            if (kept.Add(objective))
                _mind.TryAddObjective(mindId, mind, objective);
    }

    public bool IsColorOrNameTakenByOther(Color? ownColor, Color newColor, string newName)
    {
        if (!TryGetRule(out _, out var rule))
            return false;

        foreach (var (color, name) in rule.GangNames)
        {
            if (color == ownColor)
                continue;

            if (ColorsAreTooSimilar(newColor, color)
                || string.Equals(name, newName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public bool SetMemberPoints(EntityUid member, int points)
    {
        if (!TryComp<GangMemberComponent>(member, out var memberComp))
            return false;

        memberComp.GangPoints = points;
        Dirty(member, memberComp);
        return true;
    }

    private bool TryGetGangLeader(Color color, out Entity<GangLeaderComponent> leader)
    {
        var query = AllEntityQuery<GangLeaderComponent, GangMemberComponent>();
        while (query.MoveNext(out var uid, out var leaderComp, out var member))
        {
            if (member.Gang != color)
                continue;

            leader = (uid, leaderComp);
            return true;
        }

        leader = default;
        return false;
    }

    #endregion
}
