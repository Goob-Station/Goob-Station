using Content.Shared.Access.Systems;
using Content.Shared.PsionicsRecords;
using Content.Shared.PsionicsRecords.Components;
using Content.Shared.Psionics;
using Content.Shared.StationRecords;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

/// <summary>
/// EVERYTHING HERE IS A MODIFIED VERSION OF CRIMINAL RECORDS
/// </summary>

namespace Content.Pirate.Client._EinsteinEngines.PsionicsRecords;

public sealed class PsionicsRecordsConsoleBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    private readonly AccessReaderSystem _accessReader;

    private PsionicsRecordsConsoleWindow? _window;

    public PsionicsRecordsConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _accessReader = EntMan.System<AccessReaderSystem>();
    }

    protected override void Open()
    {
        base.Open();

        var comp = EntMan.GetComponent<PsionicsRecordsConsoleComponent>(Owner);

        _window = new(Owner, comp.MaxStringLength, _playerManager, _proto, _random, _accessReader);
        _window.OnKeySelected += key =>
            SendMessage(new SelectStationRecord(key));
        _window.OnFiltersChanged += (type, filterValue) =>
            SendMessage(new SetStationRecordFilter(type, filterValue));
        _window.OnStatusSelected += status =>
            SendMessage(new PsionicsRecordChangeStatus(status, null));
        _window.OnDialogConfirmed += (status, reason) =>
            SendMessage(new PsionicsRecordChangeStatus(status, reason));
        _window.OnClose += Close;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not PsionicsRecordsConsoleState cast)
            return;

        _window?.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Close();
    }
}
