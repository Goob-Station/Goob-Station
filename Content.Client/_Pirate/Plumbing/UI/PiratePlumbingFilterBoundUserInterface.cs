using Content.Shared._Pirate.Plumbing;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Pirate.Plumbing.UI;

[UsedImplicitly]
public sealed class PiratePlumbingFilterBoundUserInterface : BoundUserInterface
{
    private PiratePlumbingFilterWindow? _window;

    public PiratePlumbingFilterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<PiratePlumbingFilterWindow>();

        _window.OnToggle += OnToggle;
        _window.OnAddReagent += OnAddReagent;
        _window.OnRemoveReagent += OnRemoveReagent;
        _window.OnClear += OnClear;
    }

    private void OnToggle(bool enabled)
        => SendMessage(new PiratePlumbingFilterToggleMessage(enabled));

    private void OnAddReagent(string reagentId)
        => SendMessage(new PiratePlumbingFilterAddReagentMessage(reagentId));

    private void OnRemoveReagent(string reagentId)
        => SendMessage(new PiratePlumbingFilterRemoveReagentMessage(reagentId));

    private void OnClear()
        => SendMessage(new PiratePlumbingFilterClearMessage());

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not PiratePlumbingFilterBoundUserInterfaceState cast)
            return;

        _window.UpdateState(cast);
    }
}
