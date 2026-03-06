using Content.Goobstation.Shared.Magic;
using Content.Shared._Shitcode.Heretic.Components;

namespace Content.Goobstation.Server.Magic.IncantationRequirements;

public sealed partial class MagicRequireStability : IncantationRequirement
{
    public override bool Valid(EntityUid ent, IEntityManager entMan, out string reason)
    {
        reason = Loc.GetString("magic-requirements-stability");

        // add more here
        return (!entMan.HasComponent<RustChargeComponent>(ent));
    }
}
