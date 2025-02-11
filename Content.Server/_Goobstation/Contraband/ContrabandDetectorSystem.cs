using System.Linq;
using Content.Shared.Popups;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Audio.Systems;
using Content.Shared.Contraband;
using Robust.Shared.Containers;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Content.Shared.Access.Systems;
using Content.Shared.Power;

namespace Content.Server._Goobstation.Contraband;

public sealed class ContrabandDetectorSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedIdCardSystem _id = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContrabandDetectorComponent, StepTriggeredOnEvent>(HandleStepOnTriggered);
        SubscribeLocalEvent<ContrabandDetectorComponent, StepTriggerAttemptEvent>(HandleStepTriggerAttempt);
        SubscribeLocalEvent<ContrabandDetectorComponent, PowerChangedEvent>(OnPowerchange);
    }

    private void HandleStepOnTriggered(EntityUid uid, ContrabandDetectorComponent component, ref StepTriggeredOnEvent args)
    {
        var list = RecursiveFindContraband(args.Tripper, 0);
        list = RemovePermitedItems(args.Tripper, ref list);

        if (list.Count > 0)
        {
            _popupSystem.PopupCoordinates(
                "Contraband found",
                Transform(uid).Coordinates,
                PopupType.LargeCaution);

            _audioSystem.PlayPvs(component.Detect, uid);
        }
        else
             _audioSystem.PlayPvs(component.NoDetect, uid);
    }

    private static void HandleStepTriggerAttempt(EntityUid uid, ContrabandDetectorComponent component, ref StepTriggerAttemptEvent args)
    {
        args.Continue = component.IsPowered;
    }

    private List<EntityUid> RecursiveFindContraband(EntityUid uid, int depth)
    {
        List<EntityUid> theList = new();
        if (TryComp<ContainerManagerComponent>(uid, out var containerManager) && depth < 10)// added dept tracker to break infinite loop
            foreach (var (id, container) in containerManager.Containers)
                foreach (var ent in container.ContainedEntities)
                    if (!HasComp<HideContrabandContentComponent>(ent)&& ent != null)
                        theList.AddRange(RecursiveFindContraband(ent, depth+1));

        if (HasComp<ContrabandComponent>(uid) && !HasComp<UndetectableContrabandComponent>(uid))
            theList.Add(uid);

        return theList;
    }

    private List<EntityUid> RemovePermitedItems(EntityUid target, ref List<EntityUid> theList)
    {
        List<EntityUid> nonApprovedlist = new();
        List<ProtoId<DepartmentPrototype>>? departments = null;

        if (_id.TryFindIdCard(target, out var id))
            departments = id.Comp.JobDepartments;

        foreach (var uid in theList)
        {
            if (!uid.IsValid() ||
                !TryComp<ContrabandComponent>(uid, out var contraband))
                continue;

            if (contraband.AllowedDepartments is not null &&
                departments is not null &&
                departments.Intersect(contraband.AllowedDepartments).Any())
                continue;

            nonApprovedlist.Add(uid);
        }

        return nonApprovedlist;
    }

    private void OnPowerchange(EntityUid uid, ContrabandDetectorComponent component, PowerChangedEvent args)
        => component.IsPowered = args.Powered;

}
