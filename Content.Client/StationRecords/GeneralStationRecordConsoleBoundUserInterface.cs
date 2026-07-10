// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.StationRecords;
using Robust.Client.UserInterface;
using Robust.Client.GameObjects; // Pirate: records photos
using Content.Shared.Access.Systems; // Pirate: records photos
using Robust.Client.Player; // Pirate: records photos

namespace Content.Client.StationRecords;

public sealed class GeneralStationRecordConsoleBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPlayerManager _playerManager = default!; // Pirate: records photos
    private readonly AccessReaderSystem _accessReader; // Pirate: records photos
    [ViewVariables]
    private GeneralStationRecordConsoleWindow? _window = default!;

    public GeneralStationRecordConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _accessReader = EntMan.System<AccessReaderSystem>(); // Pirate: records photos
    }

    protected override void Open()
    {
        base.Open();

        _window = new(Owner, _playerManager, _accessReader); // Pirate: records photos
        _window.OnClose += Close; // Pirate: records photos
        var uiSystem = EntMan.System<UserInterfaceSystem>(); // Pirate: records photos
        uiSystem.RegisterControl(this, _window); // Pirate: records photos
        if (uiSystem.TryGetPosition(Owner, UiKey, out var position)) // Pirate: records photos
            _window.Open(position); // Pirate: records photos
        else
            _window.OpenCentered(); // Pirate: records photos
        _window.OnKeySelected += key =>
            SendMessage(new SelectStationRecord(key));
        _window.OnFiltersChanged += (type, filterValue) =>
            SendMessage(new SetStationRecordFilter(type, filterValue));
        #region Pirate: records photos
        _window.OnDeleteRecord += id => SendMessage(new DeleteStationRecord(id));
        _window.OnCreateRecord += name => SendMessage(new GeneralRecordCreateRecord(name));
        _window.OnIdentityInfoChanged += (species, nationality, employer, age, gender) =>
            SendMessage(new GeneralRecordEditIdentity(species, nationality, employer, age, gender));
        _window.OnForensicsInfoChanged += (fingerprint, dna) =>
            SendMessage(new GeneralRecordEditForensics(fingerprint, dna));
        #endregion
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Close(); // Pirate: records photos
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not GeneralStationRecordConsoleState cast)
            return;

        _window?.UpdateState(cast);
    }
}
