using Content.Server.Abilities.Mime;
using Content.Server.Bible.Components;
using Content.Server.Implants;
using Content.Shared._NF.Implants.Components;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Paper;
using Robust.Shared.Containers;

namespace Content.Server._NF.Implants;

public sealed class NFImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BibleUserImplantComponent, ImplantImplantedEvent>(OnBibleInserted);
        // Need access to the implant, this has to run before the implant is removed.
        SubscribeLocalEvent<BibleUserImplantComponent, EntGotRemovedFromContainerMessage>(OnBibleRemoved, before: [typeof(SubdermalImplantSystem)]);
        SubscribeLocalEvent<MimePowersImplantComponent, ImplantImplantedEvent>(OnMimeInserted);
        // Need access to the implant, this has to run before the implant is removed.
        SubscribeLocalEvent<MimePowersImplantComponent, EntGotRemovedFromContainerMessage>(OnMimeRemoved, before: [typeof(SubdermalImplantSystem)]);
    }

    private void OnBibleInserted(EntityUid uid, BibleUserImplantComponent component, ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue)
            return;

        EnsureComp<BibleUserComponent>(args.Implanted.Value);
    }

    // Currently permanent, but should support removal if/when a viable solution is found.
    private void OnBibleRemoved(EntityUid uid, BibleUserImplantComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (!TryComp<SubdermalImplantComponent>(uid, out var implanted) || implanted.ImplantedEntity == null)
            return;

        RemComp<BibleUserComponent>(implanted.ImplantedEntity.Value);
    }

    private void OnMimeInserted(EntityUid uid, MimePowersImplantComponent component, ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue)
            return;

        EnsureComp<MimePowersComponent>(args.Implanted.Value, out var mimeComp);
        mimeComp.PreventWriting = false; // Explicit in case upstream changes its mind on this.
    }

    private void OnMimeRemoved(EntityUid uid, MimePowersImplantComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (!TryComp<SubdermalImplantComponent>(uid, out var implanted) || implanted.ImplantedEntity == null)
            return;

        RemComp<MimePowersComponent>(implanted.ImplantedEntity.Value);
    }
}
