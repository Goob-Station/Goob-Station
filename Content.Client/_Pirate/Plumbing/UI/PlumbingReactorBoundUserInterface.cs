using Content.Shared._Pirate.Plumbing;
using Content.Goobstation.Maths.FixedPoint;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Pirate.Plumbing.UI;

[UsedImplicitly]
public sealed class PlumbingReactorBoundUserInterface : BoundUserInterface
{
    private PlumbingReactorWindow? _window;

    public PlumbingReactorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<PlumbingReactorWindow>();

        _window.OnToggle += OnToggle;
        _window.OnSetTarget += OnSetTarget;
        _window.OnRemoveTarget += OnRemoveTarget;
        _window.OnClearTargets += OnClearTargets;
        _window.OnSetTemperature += OnSetTemperature;
    }

    private void OnToggle(bool enabled)
        => SendMessage(new PlumbingReactorToggleMessage(enabled));

    private void OnSetTarget(string reagentId, FixedPoint2 quantity)
        => SendMessage(new PlumbingReactorSetTargetMessage(reagentId, quantity));

    private void OnRemoveTarget(string reagentId)
        => SendMessage(new PlumbingReactorRemoveTargetMessage(reagentId));

    private void OnClearTargets()
        => SendMessage(new PlumbingReactorClearTargetsMessage());

    private void OnSetTemperature(float temperature)
        => SendMessage(new PlumbingReactorSetTemperatureMessage(temperature));

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not PlumbingReactorBoundUserInterfaceState cast)
            return;

        _window.UpdateState(cast);
    }
}
