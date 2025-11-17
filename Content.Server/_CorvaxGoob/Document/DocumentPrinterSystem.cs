using Content.Server.Station.Systems;
using Content.Shared._CorvaxGoob.Documents;
using Content.Shared.Access.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.GameTicking;
using Content.Shared.Paper;
using Robust.Shared.Timing;

namespace Content.Server._CorvaxGoob.Document;

public sealed class DocumentPrinterSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedGameTicker _gameTicker = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DocumentPrinterComponent, LatheGetResultEvent>(SetContentDocument);
    }

    private void SetContentDocument(Entity<DocumentPrinterComponent> ent, ref LatheGetResultEvent result)
    {
        var paperComp = EnsureComp<PaperComponent>(result.ResultItem);

        var station = _station.GetOwningStation(result.ResultItem);
        var stationName = station != null ? Name(station.Value) : null;

        if (_itemSlots.TryGetSlot(ent.Owner, ent.Comp.SlotName, out var slot) && slot.Item is { Valid: true } idCardEntity
            && TryComp<IdCardComponent>(idCardEntity, out var idCard))
        {
            _paper.SetContent(result.ResultItem, FormatString(Loc.GetString(paperComp.Content), stationName, idCard));
        }
        else
        {
            _paper.SetContent(result.ResultItem, FormatString(Loc.GetString(paperComp.Content), stationName));
        }
    }

    private string FormatString(string content, string? station, IdCardComponent? idCard = null)
    {
        var stationTime = GetTimeStation();

        content = content
            .Replace(Loc.GetString("doc-var-date"), stationTime)
            .Replace(Loc.GetString("doc-var-station"), station ?? Loc.GetString("doc-text-printer-default-station"));

        content = content
            .Replace(Loc.GetString("doc-var-name"), idCard?.FullName ?? Loc.GetString("doc-text-printer-default-name"))
            .Replace(Loc.GetString("doc-var-job"), idCard?.LocalizedJobTitle ?? Loc.GetString("doc-text-printer-default-job"));

        return content;
    }

    private string GetTimeStation()
    {
        var time = _gameTicker.RoundDuration().ToString("hh\\:mm\\:ss");
        return time + " " + DateTime.Now.AddYears(1000).ToShortDateString();
    }
}

[Serializable]
public sealed partial class LatheGetResultEvent : EntityEventArgs
{
    public readonly EntityUid ResultItem;
    public LatheGetResultEvent(EntityUid result)
    {
        ResultItem = result;
    }
}
