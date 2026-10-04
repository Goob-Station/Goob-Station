using Content.Goobstation.Shared.Teleportation.Systems;
using Content.Shared.Teleportation;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Teleport;


public sealed class RandomTeleportSystem : SharedRandomTeleportSystem
{
    public override bool TeleportBodyPart(EntityUid victim, EntityUid uid, RandomTeleportOnUseComponent? rtp = null)
    {
        if (!Resolve(uid, ref rtp))
            return false;

        var random = new RobustRandom();

        var parts = _body.GetBodyChildren(victim);
        var organs = _body.GetBodyOrgans(victim);

        if (!random.Prob(rtp.BodyPartTeleportChance))
            return false;

        EntityUid? bodyPartToRemove = EntityUid.Invalid; // Either an organ or limb

        // This whole thing basically randomly picking available organ/limb to remove
        if (random.Prob(rtp.OrganOrLimb))
        {
            foreach (var organ in organs)
            {
                if (random.Prob(rtp.OrganPickerChance))
                {
                    bodyPartToRemove = organ.Id;
                    break;
                }
            }
        }
        else
        {
            foreach (var part in parts)
            {
                if (random.Prob(rtp.LimbPickerChance))
                {
                    bodyPartToRemove = part.Id;
                    break;
                }
            }
        }

        if (!_body.TryDetachPart(bodyPartToRemove.Value) || !_body.TryRemoveOrgan(bodyPartToRemove.Value, logMissing: false))
        {
            if (!RandomTeleport(victim, rtp, out var _)) // Teleport when detach or remove fails
                return false;
        }

        RandomTeleport(victim, rtp.Radius, rtp.TeleportAttempts, rtp.ForceSafeTeleport, rtp.TeleportPulled, other: bodyPartToRemove);

        return true;
    }

}
