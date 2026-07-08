// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Components;
using Content.Server.Medical.Components;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.MedicalScanner;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.PowerCell;
using Content.Shared.Temperature.Components;
using Content.Shared.Traits.Assorted;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Content.Server.Body.Systems;

// Shitmed Change
using Content.Shared._Shitmed.Medical.HealthAnalyzer;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Goobstation.Maths.FixedPoint;
using System.Linq;
using Content.Shared.Mobs.Systems; // Goobstation

namespace Content.Server.Medical;

public sealed class HealthAnalyzerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PowerCellSystem _cell = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!; // Shitmed Change
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly WoundSystem _woundSystem = default!; // Shitmed Change
    [Dependency] private readonly TraumaSystem _trauma = default!; // Shitmed Change
    [Dependency] private readonly MobThresholdSystem _threshold = default!; // Goobstation

    public override void Initialize()
    {
        SubscribeLocalEvent<HealthAnalyzerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<HealthAnalyzerComponent, HealthAnalyzerDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<HealthAnalyzerComponent, EntGotInsertedIntoContainerMessage>(OnInsertedIntoContainer);
        SubscribeLocalEvent<HealthAnalyzerComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<HealthAnalyzerComponent, DroppedEvent>(OnDropped);
        // Shitmed Change Start
        Subs.BuiEvents<HealthAnalyzerComponent>(HealthAnalyzerUiKey.Key, subs =>
        {
            subs.Event<HealthAnalyzerPartMessage>(OnHealthAnalyzerPartSelected);
            subs.Event<HealthAnalyzerModeSelectedMessage>(OnHealthAnalyzerModeSelected);
        });
        // Shitmed Change End
    }

    public override void Update(float frameTime)
    {
        var analyzerQuery = EntityQueryEnumerator<HealthAnalyzerComponent, TransformComponent>();
        while (analyzerQuery.MoveNext(out var uid, out var component, out var transform))
        {
            //Update rate limited to 1 second
            if (component.NextUpdate > _timing.CurTime)
                continue;

            if (component.ScannedEntity is not {} patient)
                continue;

            if (Deleted(patient))
            {
                StopAnalyzingEntity((uid, component), patient);
                continue;
            }

            // Shitmed Change Start
            if (component.CurrentBodyPart != null
                && (Deleted(component.CurrentBodyPart)
                || TryComp(component.CurrentBodyPart, out BodyPartComponent? bodyPartComponent)
                && bodyPartComponent.Body is null))
            {
                BeginAnalyzingEntity((uid, component), patient, null);
                continue;
            }
            // Shitmed Change End

            component.NextUpdate = _timing.CurTime + component.UpdateInterval;

            //Get distance between health analyzer and the scanned entity
            //null is infinite range
            var patientCoordinates = Transform(patient).Coordinates;
            if (component.MaxScanRange != null && !_transformSystem.InRange(patientCoordinates, transform.Coordinates, component.MaxScanRange.Value))
            {
                //Range too far, disable updates
                StopAnalyzingEntity((uid, component), patient);
                continue;
            }

            UpdateScannedUser(uid, patient, true, component.CurrentMode, component.CurrentBodyPart); // Shitmed Change
        }
    }

    /// <summary>
    /// Trigger the doafter for scanning
    /// </summary>
    private void OnAfterInteract(Entity<HealthAnalyzerComponent> uid, ref AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach || !HasComp<MobStateComponent>(args.Target) || !_cell.HasDrawCharge(uid.Owner, user: args.User))
            return;

        _audio.PlayPvs(uid.Comp.ScanningBeginSound, uid);

        var doAfterCancelled = !_doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, uid.Comp.ScanDelay, new HealthAnalyzerDoAfterEvent(), uid, target: args.Target, used: uid)
        {
            NeedHand = true,
            BreakOnMove = true,
        });

        if (args.Target == args.User || doAfterCancelled || uid.Comp.Silent)
            return;

        var msg = Loc.GetString("health-analyzer-popup-scan-target", ("user", Identity.Entity(args.User, EntityManager)));
        _popupSystem.PopupEntity(msg, args.Target.Value, args.Target.Value, PopupType.Medium);
    }

    private void OnDoAfter(Entity<HealthAnalyzerComponent> uid, ref HealthAnalyzerDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null || !_cell.HasDrawCharge(uid.Owner, user: args.User))
            return;

        if (!uid.Comp.Silent)
            _audio.PlayPvs(uid.Comp.ScanningEndSound, uid);

        OpenUserInterface(args.User, uid);
        BeginAnalyzingEntity(uid, args.Target.Value);
        args.Handled = true;
    }

    /// <summary>
    /// Turn off when placed into a storage item or moved between slots/hands
    /// </summary>
    private void OnInsertedIntoContainer(Entity<HealthAnalyzerComponent> uid, ref EntGotInsertedIntoContainerMessage args)
    {
        if (uid.Comp.ScannedEntity is { } patient)
            _toggle.TryDeactivate(uid.Owner);
    }

    /// <summary>
    /// Disable continuous updates once turned off
    /// </summary>
    private void OnToggled(Entity<HealthAnalyzerComponent> ent, ref ItemToggledEvent args)
    {
        if (!args.Activated && ent.Comp.ScannedEntity is { } patient)
            StopAnalyzingEntity(ent, patient);
    }

    /// <summary>
    /// Turn off the analyser when dropped
    /// </summary>
    private void OnDropped(Entity<HealthAnalyzerComponent> uid, ref DroppedEvent args)
    {
        if (uid.Comp.ScannedEntity is { } patient)
            _toggle.TryDeactivate(uid.Owner);
    }

    private void OpenUserInterface(EntityUid user, EntityUid analyzer)
    {
        if (!_uiSystem.HasUi(analyzer, HealthAnalyzerUiKey.Key))
            return;

        _uiSystem.OpenUi(analyzer, HealthAnalyzerUiKey.Key, user);
    }

    /// <summary>
    /// Mark the entity as having its health analyzed, and link the analyzer to it
    /// </summary>
    /// <param name="healthAnalyzer">The health analyzer that should receive the updates</param>
    /// <param name="target">The entity to start analyzing</param>
    /// <param name="part">Shitmed Change: The body part to analyze, if any</param>
    private void BeginAnalyzingEntity(Entity<HealthAnalyzerComponent> healthAnalyzer, EntityUid target, EntityUid? part = null)
    {
        //Link the health analyzer to the scanned entity
        healthAnalyzer.Comp.ScannedEntity = target;
        healthAnalyzer.Comp.CurrentBodyPart = part; // Shitmed Change

        _toggle.TryActivate(healthAnalyzer.Owner);

        UpdateScannedUser(healthAnalyzer, target, true, healthAnalyzer.Comp.CurrentMode, part); // Shitmed Change
    }

    /// <summary>
    /// Remove the analyzer from the active list, and remove the component if it has no active analyzers
    /// </summary>
    /// <param name="healthAnalyzer">The health analyzer that's receiving the updates</param>
    /// <param name="target">The entity to analyze</param>
    private void StopAnalyzingEntity(Entity<HealthAnalyzerComponent> healthAnalyzer, EntityUid target)
    {
        //Unlink the analyzer
        healthAnalyzer.Comp.ScannedEntity = null;
        healthAnalyzer.Comp.CurrentBodyPart = null; // Shitmed Change
        _toggle.TryDeactivate(healthAnalyzer.Owner);

        UpdateScannedUser(healthAnalyzer, target, false, healthAnalyzer.Comp.CurrentMode);
    }

    // Shitmed Change Start
    /// <summary>
    /// Shitmed Change: Handle the selection of a body part on the health analyzer
    /// </summary>
    /// <param name="healthAnalyzer">The health analyzer that's receiving the updates</param>
    /// <param name="args">The message containing the selected part</param>
    private void OnHealthAnalyzerPartSelected(Entity<HealthAnalyzerComponent> healthAnalyzer, ref HealthAnalyzerPartMessage args)
    {
        if (!TryGetEntity(args.Owner, out var owner))
            return;

        healthAnalyzer.Comp.CurrentMode = HealthAnalyzerMode.Body; // If you press a part ye get redirected bozo.
        if (args.BodyPart == null)
        {
            BeginAnalyzingEntity(healthAnalyzer, owner.Value, null);
        }
        else
        {
            var (targetType, targetSymmetry) = _bodySystem.ConvertTargetBodyPart(args.BodyPart.Value);
            if (_bodySystem.GetBodyChildrenOfType(owner.Value, targetType, symmetry: targetSymmetry) is { } part)
                BeginAnalyzingEntity(healthAnalyzer, owner.Value, part.FirstOrDefault().Id);
        }
    }

    /// <summary>
    /// Shitmed Change: Handle the selection of a different health analyzer mode
    /// </summary>
    /// <param name="healthAnalyzer">The health analyzer that's receiving the updates</param>
    /// <param name="args">The message containing the selected mode</param>
    private void OnHealthAnalyzerModeSelected(Entity<HealthAnalyzerComponent> healthAnalyzer, ref HealthAnalyzerModeSelectedMessage args)
    {
        if (!TryGetEntity(args.Owner, out var owner))
            return;

        healthAnalyzer.Comp.CurrentMode = args.Mode; // If you press a part ye get redirected bozo.
        BeginAnalyzingEntity(healthAnalyzer, owner.Value);
    }
    // Shitmed Change End

    /// <summary>
    /// Send an update for the target to the healthAnalyzer
    /// </summary>
    /// <param name="healthAnalyzer">The health analyzer</param>
    /// <param name="target">The entity being scanned</param>
    /// <param name="scanMode">True makes the UI show ACTIVE, False makes the UI show INACTIVE</param>
    /// <param name="part">Shitmed Change: The body part being scanned, if any</param>
    public void UpdateScannedUser(EntityUid healthAnalyzer, EntityUid target, bool scanMode, HealthAnalyzerMode mode, EntityUid? part = null)
    {
        if (!_uiSystem.HasUi(healthAnalyzer, HealthAnalyzerUiKey.Key)
            || !TryComp<BodyComponent>(target, out var body))
            return;

        var bodyTemperature = float.NaN;

        if (TryComp<TemperatureComponent>(target, out var temp))
            bodyTemperature = temp.CurrentTemperature;

        var bloodAmount = float.NaN;
        var unrevivable = false;
        var bloodLow = false; // Goobstation

        if (TryComp<BloodstreamComponent>(target, out var bloodstream) &&
            _solutionContainerSystem.ResolveSolution(target, bloodstream.BloodSolutionName,
                ref bloodstream.BloodSolution, out var bloodSolution))
        {
            bloodAmount = bloodSolution.FillFraction;
            bloodLow = bloodAmount < bloodstream.BloodlossThreshold; // Goobstation
        }

        // Goobstation start
        var bodyStatus = _woundSystem.GetDamageableStatesOnBody(target); // Goob
        Dictionary<TargetBodyPart, bool> bleeding; // Goobstation - removed unnecessary allocation

        var vitalDamage = FixedPoint2.Zero;
        if (TryComp<DamageableComponent>(target, out var damageableComponent))
            vitalDamage = _threshold.CheckVitalDamage(target, damageableComponent);
        // Goobstation end

        switch (mode)
        {
            case HealthAnalyzerMode.Body:
                unrevivable = false;
                FetchBodyData(target, body, out var traumas, out var pain, out bleeding);
                if (TryComp<UnrevivableComponent>(target, out var unrevivableComp) && unrevivableComp.Analyzable)
                    unrevivable = true;

                _uiSystem.ServerSendUiMessage(healthAnalyzer, HealthAnalyzerUiKey.Key, new HealthAnalyzerBodyMessage(
                    GetNetEntity(target),
                    bodyTemperature,
                    bloodAmount,
                    scanMode,
                    unrevivable,
                    bodyStatus,
                    bleeding,
                    vitalDamage, // Goobstation
                    traumas,
                    pain,
                    bloodLow, // Goobstation
                    part != null ? GetNetEntity(part) : null
                ));
                break;

            case HealthAnalyzerMode.Organs:
                bleeding = FetchBleedData(body);
                var organs = FetchOrganData(target);
                _uiSystem.ServerSendUiMessage(healthAnalyzer, HealthAnalyzerUiKey.Key, new HealthAnalyzerOrgansMessage(
                    GetNetEntity(target),
                    bodyTemperature,
                    bloodAmount,
                    scanMode,
                    bleeding,
                    vitalDamage, // Goobstation
                    bodyStatus,
                    organs
                ));
                break;

            case HealthAnalyzerMode.Chemicals:
                bleeding = FetchBleedData(body);
                var chemicals = FetchChemicalData(target);
                _uiSystem.ServerSendUiMessage(healthAnalyzer, HealthAnalyzerUiKey.Key, new HealthAnalyzerChemicalsMessage(
                    GetNetEntity(target),
                    bodyTemperature,
                    bloodAmount,
                    scanMode,
                    bleeding,
                    vitalDamage, // Goobstation
                    bodyStatus,
                    chemicals
                ));
                break;
        }
    }

    private void FetchBodyData(EntityUid target,
        BodyComponent body,
        out Dictionary<NetEntity, List<WoundableTraumaData>> traumas,
        out Dictionary<NetEntity, FixedPoint2> pain,
        out Dictionary<TargetBodyPart, bool> bleeding)
    {
        traumas = new();
        pain = new();
        bleeding = new();

        if (body.RootContainer.ContainedEntity is not { } rootPart)
            return;

        foreach (var (woundable, component) in _woundSystem.GetAllWoundableChildren(rootPart))
        {
            traumas.Add(GetNetEntity(woundable), FetchTraumaData(woundable, component));
            pain.Add(GetNetEntity(woundable), FetchPainData(woundable, component));
            bleeding.Add(_bodySystem.GetTargetBodyPart(woundable), component.Bleeds > 0);
        }
    }

    private Dictionary<TargetBodyPart, bool> FetchBleedData(BodyComponent body)
    {
        var bleeding = new Dictionary<TargetBodyPart, bool>();

        if (body.RootContainer.ContainedEntity is not { } rootPart)
            return bleeding;

        foreach (var (woundable, component) in _woundSystem.GetAllWoundableChildren(rootPart))
            bleeding.Add(_bodySystem.GetTargetBodyPart(woundable), component.Bleeds > 0);

        return bleeding;
    }

    private List<WoundableTraumaData> FetchTraumaData(EntityUid target,
        WoundableComponent woundable)
    {
        var traumasList = new List<WoundableTraumaData>();

        if (_trauma.TryGetWoundableTrauma(target, out var traumasFound))
        {
            foreach (var trauma in traumasFound)
            {
                if (trauma.Comp.TraumaType == TraumaType.BoneDamage
                    && trauma.Comp.TraumaTarget is { } boneWoundable
                    && TryComp(boneWoundable, out BoneComponent? boneComp))
                {
                    traumasList.Add(new WoundableTraumaData(ToPrettyString(target),
                        trauma.Comp.TraumaType.ToString(), trauma.Comp.TraumaSeverity, boneComp.BoneSeverity.ToString(), trauma.Comp.TargetType));

                    continue;
                }

                traumasList.Add(new WoundableTraumaData(ToPrettyString(trauma),
                        trauma.Comp.TraumaType.ToString(), trauma.Comp.TraumaSeverity, targetType: trauma.Comp.TargetType));
            }
        }

        return traumasList;
    }

    private FixedPoint2 FetchPainData(EntityUid target,
        WoundableComponent woundable)
    {
        var pain = FixedPoint2.Zero;

        if (!TryComp<NerveComponent>(target, out var nerve))
            return pain;

        return nerve.PainFeels;
    }

    private Dictionary<NetEntity, OrganTraumaData> FetchOrganData(EntityUid target)
    {
        var organs = new Dictionary<NetEntity, OrganTraumaData>();
        if (!TryComp<BodyComponent>(target, out var body))
            return organs;

        foreach (var (organId, organComp) in _bodySystem.GetBodyOrgans(target))
        {
            organs.Add(GetNetEntity(organId), new OrganTraumaData(organComp.OrganIntegrity,
                organComp.IntegrityCap,
                organComp.OrganSeverity,
                organComp.IntegrityModifiers
                    .Select(x => (x.Key.Item1, x.Value))
                    .ToList()));
        }

        return organs;
    }

    private Dictionary<NetEntity, Solution> FetchChemicalData(EntityUid target)
    {
        var solutionsList = new Dictionary<NetEntity, Solution>();

        if (!TryComp(target, out SolutionContainerManagerComponent? container) || container.Containers.Count == 0)
            return solutionsList;

        foreach (var (name, solution) in _solutionContainerSystem.EnumerateSolutions((target, container)))
        {
            if (name is null
                || name == BloodstreamComponent.DefaultBloodTemporarySolutionName
                || name == "print" // I hate this so fucking much.
                || !TryGetNetEntity(solution, out var netSolution))
                continue;

            solutionsList.Add(netSolution.Value, solution.Comp.Solution);
        }

        if (TryComp<BodyComponent>(target, out var body)
            && _bodySystem.TryGetBodyOrganEntityComps<StomachComponent>((target, body), out var stomachs))
        {
            foreach (var stomach in stomachs)
            {
                if (stomach.Comp1.Solution is null
                    || !TryGetNetEntity(stomach.Comp1.Solution, out var netSolution))
                    continue;

                solutionsList.Add(netSolution.Value, stomach.Comp1.Solution.Value.Comp.Solution); // This is horrible.
            }
        }

        return solutionsList;
    }
}
