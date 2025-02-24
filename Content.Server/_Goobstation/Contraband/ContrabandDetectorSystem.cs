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
using Content.Server.DeviceLinking.Systems;
using Robust.Server.GameObjects;

namespace Content.Server._Goobstation.Contraband;

public sealed class ContrabandDetectorSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedIdCardSystem _id = default!;
    [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;
    [Dependency] private readonly PointLightSystem _pointLight = default!;

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

        if (list.Count > 0 ||
            FalseDetection(component.FalseDetectingChance) &&    //false positive
            FalseDetection(100 - component.FalseDetectingChance))     //false negative
        {
            _popupSystem.PopupCoordinates(
                Loc.GetString("contraband-detector-popup-detected"),
                Transform(uid).Coordinates,
                PopupType.SmallCaution);

            _audioSystem.PlayPvs(component.Detect, uid);
            _deviceLink.SendSignal(uid, "SignalContrabandDetected", true);
            _pointLight.SetEnabled(uid, true);
        }
        else
        {
            _audioSystem.PlayPvs(component.NoDetect, uid);
            _deviceLink.SendSignal(uid, "SignalContrabandDetected", false);
            _pointLight.SetEnabled(uid, false);
        }
    }

    private static void HandleStepTriggerAttempt(EntityUid uid, ContrabandDetectorComponent component, ref StepTriggerAttemptEvent args)
        => args.Continue = component.IsPowered;


    private List<EntityUid> RecursiveFindContraband(EntityUid uid, int depth)
    {
        List<EntityUid> listOfContraband = new();
        if (TryComp<ContainerManagerComponent>(uid, out var containerManager) && depth < 10)// added dept tracker to break infinite loop
            foreach (var (_, container) in containerManager.Containers)
                foreach (var ent in container.ContainedEntities)
                    if (!HasComp<HideContrabandContentComponent>(ent)&& ent != null)
                        listOfContraband.AddRange(RecursiveFindContraband(ent, depth+1));

        if (HasComp<ContrabandComponent>(uid) && !HasComp<UndetectableContrabandComponent>(uid))
            listOfContraband.Add(uid);

        return listOfContraband;
    }

    private List<EntityUid> RemovePermitedItems(EntityUid target, ref List<EntityUid> listOfFoundContraband)
    {
        List<EntityUid> nonApprovedlist = new();
        List<ProtoId<DepartmentPrototype>>? departments = null;

        if (_id.TryFindIdCard(target, out var id))
            departments = id.Comp.JobDepartments;

        foreach (var uid in listOfFoundContraband)
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
    {
        component.IsPowered = args.Powered;
        _pointLight.SetEnabled(uid, false);
    }

    private bool FalseDetection(int chance)
    {
        return chance > new Random().Next(0, 100);
    }
}
