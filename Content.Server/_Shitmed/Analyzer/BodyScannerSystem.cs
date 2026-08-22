using Content.Server.Medical;
using Content.Server.Medical.Components;
using Content.Shared._Shitmed.Analyzer;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Buckle.Components;
using Content.Shared.DeviceLinking.Events;

namespace Content.Server._Shitmed.Analyzer;

public sealed partial class BodyScannerSystem : EntitySystem
{
    [Dependency] HealthAnalyzerSystem _healthAnalyzer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyScannerComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<BodyScannerComponent, PortDisconnectedEvent>(OnPortDisconnected);

        SubscribeLocalEvent<OperatingTableComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<OperatingTableComponent, UnstrappedEvent>(OnUnstrapped);
    }

    private void OnStrapped(Entity<OperatingTableComponent> ent, ref StrappedEvent args)
    {
        if (ent.Comp.LinkedScanner is { } linkedScanner && TryComp(linkedScanner, out HealthAnalyzerComponent? analyzer))
            _healthAnalyzer.BeginAnalyzingEntity((linkedScanner, analyzer), args.Buckle);
    }
    private void OnUnstrapped(Entity<OperatingTableComponent> ent, ref UnstrappedEvent args)
    {
        if (ent.Comp.LinkedScanner is { } linkedScanner && TryComp(linkedScanner, out HealthAnalyzerComponent? analyzer))
            _healthAnalyzer.StopAnalyzingEntity((linkedScanner, analyzer), args.Buckle);
    }

    private void OnNewLink(Entity<BodyScannerComponent> ent, ref NewLinkEvent args)
    {
        if (args.SinkPort == ent.Comp.OperatingTablePort
            && TryComp(args.Source, out OperatingTableComponent? tableComp))
        {
            ent.Comp.OperatingTable = args.Source;
            tableComp.LinkedScanner = ent;
            Dirty(ent);
            Dirty(args.Source, tableComp);
        }
    }
    private void OnPortDisconnected(Entity<BodyScannerComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port != ent.Comp.OperatingTablePort)
            return;

        if (TryComp(ent.Comp.OperatingTable, out OperatingTableComponent? table))
        {
            table.LinkedScanner = null;
            Dirty(ent, table);
        }

        ent.Comp.OperatingTable = null;
        Dirty(ent);

        if (TryComp(ent, out HealthAnalyzerComponent? analyzer) && analyzer.ScannedEntity != null)
            _healthAnalyzer.StopAnalyzingEntity((ent, analyzer), analyzer.ScannedEntity.Value);
    }
}