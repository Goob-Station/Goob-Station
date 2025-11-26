using Content.Goobstation.Server.Disease;
using Content.Goobstation.Shared.Disease;
using Content.Goobstation.Shared.Virology;
using Content.Server.Power.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Paper;
using Robust.Server.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Text;

namespace Content.Goobstation.Server.Virology;

public sealed partial class VirologyMachinesSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly DiseaseSystem _disease = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VirologyMachineComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<VirologyMachineComponent, EntInsertedIntoContainerMessage>(OnSwabInserted);
        SubscribeLocalEvent<VirologyMachineComponent, VirologyMachineCheckEvent>(OnAnalyzerCheck);
        SubscribeLocalEvent<VirologyMachineComponent, VirologyMachineDoneEvent>(OnMachineDone);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ActiveVirologyMachineComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var checkEv = new VirologyMachineCheckEvent();
            RaiseLocalEvent(uid, ref checkEv);
            if (checkEv.Cancelled)
            {
                SetAppearance(uid, false);
                var doneEv = new VirologyMachineDoneEvent(false);
                RaiseLocalEvent(uid, doneEv);
                continue;
            }

            if (!_power.IsPowered(uid))
            {
                SetAppearance(uid, false);
                comp.EndTime += TimeSpan.FromSeconds(frameTime);
                continue;
            }

            if (_timing.CurTime > comp.EndTime)
            {
                SetAppearance(uid, false);
                var doneEv = new VirologyMachineDoneEvent(true);
                RaiseLocalEvent(uid, doneEv);
                continue;
            }

            SetAppearance(uid, true);
        }
    }

    private void OnSwabInserted(Entity<VirologyMachineComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != VirologyMachineComponent.SwabSlotId)
            return;

        if (!TryComp<DiseaseSwabComponent>(args.Entity, out _))
            return;

        EnsureComp<ActiveVirologyMachineComponent>(ent, out var active);
        active.EndTime = _timing.CurTime + ent.Comp.AnalysisDuration;
    }

    private void OnComponentInit(Entity<VirologyMachineComponent> ent, ref ComponentInit args)
    {
        if (_itemSlots.TryGetSlot(ent, VirologyMachineComponent.SwabSlotId, out var slot))
            ent.Comp.SwabSlot = slot;
        else
            _itemSlots.AddItemSlot(ent, VirologyMachineComponent.SwabSlotId, ent.Comp.SwabSlot);
    }

    private void OnAnalyzerCheck(Entity<VirologyMachineComponent> ent, ref VirologyMachineCheckEvent args)
    {
        args.Cancelled = !_itemSlots.TryGetSlot(ent, VirologyMachineComponent.SwabSlotId, out var slot) || slot.Item == null;
    }

    private void OnMachineDone(Entity<VirologyMachineComponent> ent, ref VirologyMachineDoneEvent args)
    {
        if (!args.Success)
            return;

        if (!_itemSlots.TryGetSlot(ent, VirologyMachineComponent.SwabSlotId, out var slot) || slot.Item == null)
            return;

        if(TryComp<DiseaseAnalyzerComponent>(ent, out var diseaseAnalyzerComp))
            AnalyzeSwab((ent, diseaseAnalyzerComp), (slot.Item.Value, null), ent);
        else if (TryComp<VaccinatorComponent>(ent, out var vaccinatorComp))
            CreateVaccine(ent);
    }

    private void CreateVaccine(Entity<VirologyMachineComponent> ent)
    {
        // create a vaccine

    }

    private void AnalyzeSwab(Entity<DiseaseAnalyzerComponent?> analyzer, Entity<DiseaseSwabComponent?> swab, Entity<VirologyMachineComponent> machine)
    {
        if (!Resolve(analyzer, ref analyzer.Comp) || !Resolve(swab, ref swab.Comp))
            return;

        if (!TryComp<DiseaseComponent>(swab.Comp.DiseaseUid, out var disease))
            return;

        // build the report
        var report = new StringBuilder();
        report.AppendLine(Loc.GetString("disease-analyzer-report-title"));
        report.AppendLine(Loc.GetString("disease-analyzer-report-genotype", ("genotype", disease.Genotype)));
        report.AppendLine(Loc.GetString("disease-analyzer-report-type", ("type", Loc.GetString(_proto.Index(disease.DiseaseType).LocalizedName))));
        report.AppendLine(Loc.GetString("disease-analyzer-report-infection-rate", ("rate", disease.InfectionRate)));
        report.AppendLine(Loc.GetString("disease-analyzer-report-immunity-gain", ("rate", disease.ImmunityGainRate)));
        report.AppendLine(Loc.GetString("disease-analyzer-report-mutation-rate", ("rate", disease.MutationRate)));
        report.AppendLine(Loc.GetString("disease-analyzer-report-complexity", ("complexity", disease.Complexity)));

        report.AppendLine(Loc.GetString("disease-analyzer-report-effects-header"));
        foreach (var effectUid in disease.Effects)
        {
            var meta = MetaData(effectUid);
            if (TryComp<DiseaseEffectComponent>(effectUid, out var effectComp) && meta.EntityPrototype != null)
            {
                report.AppendLine(Loc.GetString("disease-analyzer-report-effect-line",
                    ("effect", Loc.GetString(meta.EntityPrototype.Name)),
                    ("description", Loc.GetString(meta.EntityPrototype.Description)),
                    ("severity", effectComp.Severity)));
            }
        }
        // print the report
        var printed = Spawn(machine.Comp.PaperPrototype, Transform(analyzer).Coordinates);
        _paper.SetContent((printed, EnsureComp<PaperComponent>(printed)), report.ToString());

        _itemSlots.TryEject(analyzer, machine.Comp.SwabSlot, null, out _);
        _audio.PlayPvs(machine.Comp.AnalyzedSound, analyzer);
    }

    private void SetAppearance(EntityUid uid, bool state)
    {
        _appearance.SetData(uid, DiseaseMachineVisuals.IsRunning, state);
    }
}
