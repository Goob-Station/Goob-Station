using Content.Goobstation.Server.Cult.GameTicking;
using Content.Goobstation.Shared.Cult;
using Content.Server.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using System.Linq;
using Content.Server.Body.Systems;

namespace Content.Goobstation.Server.Cult.Runes;

public sealed partial class CultRuneBehaviorSacrifice : CultRuneBehavior
{
    private BloodCultRuleSystem _cultRuleSystem = default!;
    private MindSystem _mind = default!;
    private BodySystem _body = default!;

    private Entity<BloodCultRuleComponent>? _rule;

    public override void Initialize(IEntityManager ent)
    {
        base.Initialize(ent);

        _cultRuleSystem = ent.System<BloodCultRuleSystem>();
        _mind = ent.System<MindSystem>();
        _body = ent.System<BodySystem>();

        _cultRuleSystem.TryGetRule(out _rule);
    }

    public override bool IsValid(IEntityManager ent, List<EntityUid> invokers, List<EntityUid> targets, out string invalidReason)
    {
        if (!base.IsValid(ent, invokers, targets, out invalidReason))
            return false;

        if (targets.Count == 0)
        {
            invalidReason = Loc.GetString("rune-invoke-fail-notarget");
            return false;
        }

        var target = targets.First();
        if (ent.TryGetComponent<MobStateComponent>(target, out var mob) && mob.CurrentState != MobState.Dead)
            return false;

        return true;
    }

    public override void Invoke(IEntityManager ent, List<EntityUid> invokers, List<EntityUid> targets, EntityUid? owner = null)
    {
        var target = targets.First();

        // TODO move mind to soulstone
        // gib body

        if (_cultRuleSystem.TryGetRule(out var rule))
            rule!.Value.Comp.ReviveRuneCharges += 1;
    }
}
