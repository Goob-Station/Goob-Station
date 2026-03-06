using Content.Goobstation.Shared.Magic;
using Content.Shared.Speech.Muting;

namespace Content.Goobstation.Server.Magic.IncantationRequirements;

public sealed partial class MagicRequireSpeech : IncantationRequirement
{
    public override bool Valid(EntityUid ent, IEntityManager entMan, out string reason)
    {
        reason = Loc.GetString("magic-requirements-muted");
        return !entMan.HasComponent<MutedComponent>(ent);
    }
}
