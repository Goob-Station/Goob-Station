using Content.Server.Ghost.Roles.Components;
using Content.Server.Speech.Components;
using Content.Shared.EntityEffects;
using Content.Shared.Mind.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class MakeSentientFreeAgent : EntityEffect
{
    [Dependency] private readonly PopupSystem _popup = default!;
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-make-sentient-freeagent", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var entityManager = args.EntityManager;
        var uid = args.TargetEntity;

        // Let affected entities speak normally to make this effect different from, say, the "random sentience" event
        // This also works on entities that already have a mind
        // We call this before the mind check to allow things like player-controlled mice to be able to benefit from the effect
        entityManager.RemoveComponent<ReplacementAccentComponent>(uid);
        entityManager.RemoveComponent<MonkeyAccentComponent>(uid);

        // Stops from adding a ghost role to things like people who already have a mind
        if (entityManager.TryGetComponent<MindContainerComponent>(uid, out var mindContainer) && mindContainer.HasMind)
        {
            return;
        }

        // Convert pre-existing ghost roles (if present) into Free Agents
        if (entityManager.TryGetComponent(uid, out GhostRoleComponent? ghostRole))
        {
            // silly workaround, make this proper if/when MindRoles get added
            if (ghostRole.RoleRules == Loc.GetString("ghost-role-information-freeagent-rules"))
            {
                return;
            }

            ghostRole.RoleDescription += " " + Loc.GetString("ghost-role-information-emancizine-converted-description");
            ghostRole.RoleRules = Loc.GetString("ghost-role-information-freeagent-rules");

            return;
        }

        ghostRole = entityManager.AddComponent<GhostRoleComponent>(uid);
        entityManager.EnsureComponent<GhostTakeoverAvailableComponent>(uid);

        var entityData = entityManager.GetComponent<MetaDataComponent>(uid);
        ghostRole.RoleName = entityData.EntityName;
        ghostRole.RoleDescription = Loc.GetString("ghost-role-information-emancizine-description");
        ghostRole.RoleRules = Loc.GetString("ghost-role-information-freeagent-rules");
    }
}
