using Content.Shared._Pirate.Plumbing;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Pirate.Plumbing.UI;

[UsedImplicitly]
public sealed class PlumbingPillPressBoundUserInterface : BoundUserInterface
{
    private PlumbingPillPressWindow? _window;

    public PlumbingPillPressBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<PlumbingPillPressWindow>();

        _window.OnToggle += enabled =>
            SendMessage(new PlumbingPillPressToggleMessage(enabled));

        _window.OnSetDosage += dosage =>
            SendMessage(new PlumbingPillPressSetDosageMessage(dosage));

        _window.OnSetLabel += label =>
            SendMessage(new PlumbingPillPressSetLabelMessage(label));

        _window.OnSetOutputMode += mode =>
            SendMessage(new PlumbingPillPressSetOutputModeMessage(mode));

        _window.OnSetPillType += pillType =>
            SendMessage(new PlumbingPillPressSetPillTypeMessage(pillType));

        _window.OnSetMixing += mixingEnabled =>
            SendMessage(new PlumbingPillPressSetMixingMessage(mixingEnabled));

        _window.OnSetInletRatio += (inlet, ratio) =>
            SendMessage(new PlumbingPillPressSetInletRatioMessage(inlet, ratio));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not PlumbingPillPressBoundUserInterfaceState cast)
            return;

        _window.UpdateState(cast);
    }
}
