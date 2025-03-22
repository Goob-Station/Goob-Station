using System.Diagnostics.CodeAnalysis;
using Content.Server.Access.Systems;
using Content.Server.Actions;
using Content.Server.Administration.Managers;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Polymorph.Systems;
using Content.Server.Revolutionary.Components;
using Content.Shared._Goobstation.MisandryBox;
using Content.Shared.Access;
using Content.Shared.Access.Systems;
using Content.Shared.Actions;
using Content.Shared.Administration.Components;
using Content.Shared.Body.Components;
using Content.Shared.Clumsy;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Database;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.MisandryBox.BSOEnforcement;

public sealed partial class BSOAutoFailSystem
{
    [Dependency] private readonly ActionsSystem _act = default!;
    [Dependency] private readonly AccessSystem _access = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IBanManager _ban = default!;

    private void ThresholdReached(EntityUid key)
    {
        _apostles.Remove(key);
        RemComp<CommandStaffComponent>(key);

        if ((_punishment & BSOEnforcementPunishmentEnum.Lifeline) != 0)
            RemoveFromRound(key);

        if ((_punishment & BSOEnforcementPunishmentEnum.AccessStrip) != 0)
            StripAccesses(key);

        if ((_punishment & BSOEnforcementPunishmentEnum.Pacifist) != 0)
            ApplyPacifist(key);

        if ((_punishment & BSOEnforcementPunishmentEnum.Clumsy) != 0)
            ApplyClumsy(key);

        if ((_punishment & BSOEnforcementPunishmentEnum.DisarmProne) != 0)
            ApplyDisarmProne(key);

        if ((_punishment & BSOEnforcementPunishmentEnum.Monkey) != 0)
            Monkey(key);

        if ((_punishment & BSOEnforcementPunishmentEnum.RoleBan) != 0)
            ApplyRoleBan(key);
    }

    private void ApplyRoleBan(EntityUid key)
    {
        if (!_player.TryGetSessionByEntity(key, out var session))
            // welp
            return;

        _ban.CreateRoleBan(session.UserId,
            null,
            null,
            null,
            null,
            _apostleJobProto,
            (uint)_bantime,
            NoteSeverity.None,
            "[AUTOMATED] BSO Enforcement - failure to perform duties.",
            DateTimeOffset.Now);
    }

    private void ApplyDisarmProne(EntityUid victim)
    {
        EnsureComp<DisarmProneComponent>(victim);
    }

    private void ApplyClumsy(EntityUid victim)
    {
        EnsureComp<ClumsyComponent>(victim);
    }

    private void StripAccesses(EntityUid victim)
    {
        foreach (var entity in _accessReader.FindPotentialAccessItems(victim))
        {
            _access.TrySetTags(entity, new List<ProtoId<AccessLevelPrototype>>());
        }
    }

    private void ApplyPacifist(EntityUid victim)
    {
        EnsureComp<PacifiedComponent>(victim);
    }

    private void RemoveFromRound(EntityUid victim)
    {
        if (TryComp<BloodstreamComponent>(victim, out var bloodstream))
        {
            _damage.SetDamage(victim, Comp<DamageableComponent>(victim), new DamageSpecifier(_prototype.Index<DamageGroupPrototype>("Genetic"), 1984));
            _bloodstream.SpillAllSolutions(victim, bloodstream);
        }
        else
        {
            // Okay fuck you
            Monkey(victim);
            RemoveFromRound(victim);
        }
    }

    private void Monkey(EntityUid victim)
    {
        _polymorph.PolymorphEntity(victim, "AdminMonkeySmite");
    }

    private bool TryGetLifelineAction(EntityUid victim,
        ActionsComponent comp,
        [NotNullWhen(true)] out EntityUid? actionUid,
        [NotNullWhen(true)] out BaseActionComponent? actionComp)
    {
        actionUid = null;
        actionComp = null;

        var e = _act.GetActions(victim, comp);

        foreach (var a in e)
        {
            if (MetaData(a.Id).EntityPrototype?.ID != "ActionActivateBluespaceLifeline")
                continue;

            actionUid = a.Id;
            actionComp = a.Comp;
        }

        return actionUid is not null;
    }
}
