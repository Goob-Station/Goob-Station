using Content.Shared._Pirate.Plumbing;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Pirate.Plumbing.UI;

[UsedImplicitly]
public sealed class PlumbingSynthesizerBoundUserInterface : BoundUserInterface
{
    private PlumbingSynthesizerWindow? _window;

    public PlumbingSynthesizerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<PlumbingSynthesizerWindow>();

        _window.OnToggle += OnToggle;
        _window.OnSelectReagent += OnSelectReagent;
    }

    private void OnToggle(bool enabled)
        => SendMessage(new PlumbingSynthesizerToggleMessage(enabled));

    private void OnSelectReagent(string? reagentId)
        => SendMessage(new PlumbingSynthesizerSelectReagentMessage(reagentId));

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not PlumbingSynthesizerBoundUserInterfaceState cast)
            return;

        _window.UpdateState(cast);
    }
}
