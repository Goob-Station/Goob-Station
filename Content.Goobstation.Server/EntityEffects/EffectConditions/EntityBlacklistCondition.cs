using Content.Shared.EntityEffects;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects.EffectConditions;

public sealed partial class EntityBlacklistCondition : EntityEffectCondition
{
    [DataField] public EntityWhitelist? Whitelist;
    [DataField] public EntityWhitelist? Blacklist;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        var target = args.TargetEntity;
        var whitelist = args.EntityManager.System<EntityWhitelistSystem>();
        return whitelist.IsWhitelistPass(Whitelist, target) && !whitelist.IsBlacklistPass(Blacklist, target);
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
        => string.Empty;
}
