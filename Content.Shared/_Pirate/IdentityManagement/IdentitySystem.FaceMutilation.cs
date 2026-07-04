using System.Linq;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.IdentityManagement.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;

namespace Content.Shared.IdentityManagement;

public sealed partial class IdentitySystem
{
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;

    private partial void InitializePirateFaceMutilation()
    {
        SubscribeLocalEvent<IdentityComponent, TransformSpeakerNameEvent>(OnPirateTransformSpeakerName);
        SubscribeLocalEvent<WoundableComponent, TraumaInducedEvent>(OnPirateTraumaChanged);
        SubscribeLocalEvent<WoundableComponent, TraumaBeingRemovedEvent>(OnPirateTraumaChanged);
    }

    private partial bool TryGetPirateFaceMutilationIdentity(EntityUid target, IdentityRepresentation representation, out string identity)
    {
        identity = string.Empty;
        if (!IsFaceMutilated(target))
            return false;

        if (TryComp<HumanoidAppearanceComponent>(target, out var appearance))
        {
            var species = _humanoid.GetSpeciesRepresentation(appearance.Species).ToLowerInvariant();
            identity = $"{representation.AgeString} {species}";
            return true;
        }

        identity = new IdentityRepresentation(representation.TrueName, representation.TrueGender, representation.AgeString).ToStringUnknown();
        return true;
    }

    private void OnPirateTraumaChanged(EntityUid uid, WoundableComponent component, ref TraumaInducedEvent args)
    {
        HandleFaceMutilationTraumaChanged(uid, args.TraumaType);
    }

    private void OnPirateTraumaChanged(EntityUid uid, WoundableComponent component, ref TraumaBeingRemovedEvent args)
    {
        HandleFaceMutilationTraumaChanged(uid, args.TraumaType);
    }

    private void HandleFaceMutilationTraumaChanged(EntityUid uid, TraumaType traumaType)
    {
        if (traumaType != TraumaType.FaceMutilation)
            return;

        QueueIdentityUpdateForWoundable(uid);
    }

    private void QueueIdentityUpdateForWoundable(EntityUid woundable)
    {
        if (!TryComp<BodyPartComponent>(woundable, out var partComp)
            || !partComp.Body.HasValue)
            return;

        QueueIdentityUpdate(partComp.Body.Value);
    }

    private bool IsFaceMutilated(EntityUid uid, BodyComponent? body = null)
    {
        if (!Resolve(uid, ref body, false))
            return false;

        return _bodySystem.GetBodyChildrenOfType(uid, BodyPartType.Head, body)
            .Any(head => _trauma.HasWoundableTrauma(head.Id, TraumaType.FaceMutilation));
    }

    private void OnPirateTransformSpeakerName(EntityUid uid, IdentityComponent component, ref TransformSpeakerNameEvent args)
    {
        if (!IsFaceMutilated(uid))
            return;

        args.VoiceName = GetEntityIdentity(uid);
    }
}
