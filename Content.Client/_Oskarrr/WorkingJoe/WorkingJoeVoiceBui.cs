// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Oskarrr.WorkingJoe;
using Robust.Client.UserInterface;
using Robust.Shared.ContentPack;
using Robust.Shared.Prototypes;

namespace Content.Client._Oskarrr.WorkingJoe;

public sealed class WorkingJoeVoiceBui : BoundUserInterface
{
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IResourceManager _resource = default!;

    private WorkingJoeVoiceFavorites? _favorites;
    private WorkingJoeVoiceWindow? _window;

    public WorkingJoeVoiceBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _favorites ??= new WorkingJoeVoiceFavorites(_resource);
        _window = new WorkingJoeVoiceWindow(_favorites);
        _window.OnClose += Close;
        _window.OnLineSelected += OnLineSelected;

        var lines = new List<WorkingJoeVoiceLineData>();
        foreach (var line in _proto.EnumeratePrototypes<WorkingJoeVoiceLinePrototype>())
        {
            lines.Add(new WorkingJoeVoiceLineData
            {
                LineId = line.ID,
                DisplayName = _loc.GetString(line.Name),
                Category = line.Category
            });
        }

        _window.SetLines(lines);
        _window.OpenCentered();
    }

    private void OnLineSelected(string lineId)
    {
        SendMessage(new WorkingJoePlayLineMessage(lineId));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        if (_window != null)
        {
            _window.OnClose -= Close;
            _window.OnLineSelected -= OnLineSelected;
            _window.Close();
            _window = null;
        }
    }
}
