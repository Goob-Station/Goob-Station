using Content.Goobstation.Server.Disease;
using Content.Goobstation.Shared.Disease;
using Content.Goobstation.Shared.Virology;
using Content.Server.Power.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Paper;
using Robust.Server.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using System.Text;

namespace Content.Goobstation.Server.Virology;

public sealed partial class DiseaseAnalyzerSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly DiseaseSystem _disease = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DiseaseAnalyzerComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<DiseaseAnalyzerComponent, EntInsertedIntoContainerMessage>(OnSwabInserted);
        SubscribeLocalEvent<DiseaseAnalyzerComponent, EntRemovedFromContainerMessage>(OnSwabRemoved);
    }

    private void OnComponentInit(Entity<DiseaseAnalyzerComponent> ent, ref ComponentInit args)
    {
        _itemSlots.AddItemSlot(ent, DiseaseAnalyzerComponent.SwabSlotId, ent.Comp.SwabSlot);
    }

    private void OnSwabInserted(Entity<DiseaseAnalyzerComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != DiseaseAnalyzerComponent.SwabSlotId)
            return;

        if (!_power.IsPowered(ent))
            return;

        if (!TryComp<DiseaseSwabComponent>(args.Entity, out var swab))
            return;

        AnalyzeSwab((ent.Owner, ent.Comp), (args.Entity, swab));
    }

    private void OnSwabRemoved(Entity<DiseaseAnalyzerComponent> ent, ref EntRemovedFromContainerMessage args)
    {
       // Clear any pending analysis? Stop DoAfter? Depends on implementation.
    }

    private void AnalyzeSwab(Entity<DiseaseAnalyzerComponent?> analyzer, Entity<DiseaseSwabComponent?> swab)
    {
        if (!Resolve(analyzer, ref analyzer.Comp) || !Resolve(swab, ref swab.Comp))
            return;

        if (!TryComp<DiseaseComponent>(swab.Comp.DiseaseUid, out var disease))
            return;

        // Build the report
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
            if (TryComp<MetaDataComponent>(effectUid, out var meta) && TryComp<DiseaseEffectComponent>(effectUid, out var effectComp))
            {
                var effectName = meta.EntityName; // Use EntityName as a fallback
                var effectDescription = Loc.GetString("disease-analyzer-unknown-effect");
                if (meta.EntityPrototype != null)
                {
                    effectName = Loc.GetString(meta.EntityPrototype.Name); // Use localized prototype name if available
                    effectDescription = Loc.GetString(meta.EntityPrototype.Description);
                }

                report.AppendLine(Loc.GetString("disease-analyzer-report-effect-line",
                    ("effect", effectName),
                    ("description", effectDescription),
                    ("severity", effectComp.Severity))); // Severity as percentage
            }
        }

        // Print the report
        var printed = Spawn(analyzer.Comp.PaperPrototype, Transform(analyzer).Coordinates);
        _paper.SetContent((printed, EnsureComp<PaperComponent>(printed)), report.ToString());

        _itemSlots.TryEject(analyzer, analyzer.Comp.SwabSlot, null, out _);
        _audio.PlayPvs(analyzer.Comp.AnalyzedSound, analyzer);
    }
}
