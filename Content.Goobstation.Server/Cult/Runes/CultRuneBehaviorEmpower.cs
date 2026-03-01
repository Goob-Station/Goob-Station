using Content.Goobstation.Server.Cult.GameTicking;
using Content.Goobstation.Shared.Cult;
using Content.Goobstation.Shared.Cult.Magic;
using Content.Goobstation.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using System.Data;
using System.Linq;

namespace Content.Goobstation.Server.Cult.Runes;

public sealed partial class CultRuneBehaviorEmpower : CultRuneBehavior
{
    private UserInterfaceSystem _ui = default!;
    private BloodCultRuleSystem _cultRule = default!;

    public override void Initialize(IEntityManager ent)
    {
        base.Initialize(ent);

        _ui = ent.System<UserInterfaceSystem>();
        _cultRule = ent.System<BloodCultRuleSystem>();
    }

    public override bool IsValid(IEntityManager ent, List<EntityUid> invokers, List<EntityUid> targets, out string invalidReason)
    {
        if (!base.IsValid(ent, invokers, targets, out invalidReason))
            return false;

        return true;
    }

    public override void Invoke(IEntityManager ent, List<EntityUid> invokers, List<EntityUid> targets, EntityUid? owner = null)
    {
        var invoker = invokers.First();

        if (!owner.HasValue
        || !_cultRule.TryGetRule(out var rule)
        || !ent.TryGetComponent<BloodMagicProviderComponent>(invoker, out var magic))
            return;

        var tiers = magic.Spells.Where(q => q.Key >= rule!.Value.Comp.CurrentTier).ToList();
        var spells = new List<EntProtoId>();
        foreach (var tier in tiers) spells.AddRange(tier.Value);

        _ui.TryOpenUi(owner.Value, EntityRadialMenuKey.Key, invoker);
        _ui.SetUiState(owner.Value, EntityRadialMenuKey.Key, new EntityRadialMenuState(spells));
    }
}
