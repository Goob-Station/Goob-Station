using Content.Server.Body.Components;
using Content.Server.Forensics;
using Content.Server.Objectives.Components;
using Content.Shared._DV.Recruiter;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared._DV.Paper;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Recruiter;

/// <summary>
/// Handles bloodstream related code since that isn't in shared.
/// </summary>
public sealed class RecruiterPenSystem : SharedRecruiterPenSystem
{
    [Dependency] private readonly ForensicsSystem _forensics = default!;
    [Dependency] private readonly SolutionTransferSystem _transfer = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    protected override void DrawBlood(EntityUid uid, Entity<SolutionComponent> dest, EntityUid user)
    {
        // how did you even use this mr plushie...
        if (CompOrNull<BloodstreamComponent>(user)?.BloodSolution is not {} blood)
            return;

        var desired = dest.Comp.Solution.AvailableVolume;
        // TODO: when bloodstream is shared put the transfer in shared so PopupClient is actually used, and this popup isnt needed
        if (desired == 0)
        {
            Popup.PopupEntity(Loc.GetString("recruiter-pen-prick-full", ("pen", uid)), user, user);
            return;
        }

        if (_transfer.Transfer(user, user, blood, uid, dest, desired) != desired)
            return;

        // this is why you have to keep the pen safe, it has the dna of everyone you recruited!
        _forensics.TransferDna(uid, user, canDnaBeCleaned: false);

        if (TryComp<SignatureWriterComponent>(uid, out var signatureComp))
        {
            var bloodColor = blood.Comp.Solution.GetColor(_proto);
            signatureComp.Color = bloodColor;
        }

        Popup.PopupEntity(Loc.GetString("recruiter-pen-pricked", ("pen", uid)), user, user, PopupType.LargeCaution);
    }

    protected override void Recruit(Entity<RecruiterPenComponent> ent, EntityUid user)
    {
        // only increment count once if 1 person signs multiple papers
        if (!ent.Comp.Recruited.Add(user))
            return;

        if (!Mind.TryGetMind(user, out var userMindId, out _))
            return;

        if (ent.Comp.RecruiterMind is {} mindId &&
            mindId != userMindId &&
            Mind.TryGetObjectiveComp<RecruitingConditionComponent>(mindId, out var obj, null))
        {
            obj.Recruited++;
        }
    }
}
