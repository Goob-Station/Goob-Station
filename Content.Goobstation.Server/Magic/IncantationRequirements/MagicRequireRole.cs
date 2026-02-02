using Content.Goobstation.Shared.Magic;
using Content.Shared.Mind;
using Content.Shared.Roles;

namespace Content.Goobstation.Server.Magic.IncantationRequirements;

public sealed partial class MagicRequireRole : IncantationRequirement
{
    [DataField] public Type RoleType;

    public override bool Valid(EntityUid ent, IEntityManager entMan, out string reason)
    {
        reason = Loc.GetString("magic-requirements-role");

        var roleSystem = entMan.System<SharedRoleSystem>();
        var mindSystem = entMan.System<SharedMindSystem>();

        return mindSystem.TryGetMind(ent, out var mindId, out _)
            && roleSystem.MindHasRole(mindId, RoleType, out _);
    }
}
