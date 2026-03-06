using Content.Client.Eui;
using Content.Goobstation.Shared.ImmortalSnail;
using Content.Shared.Eui;
using JetBrains.Annotations;

namespace Content.Goobstation.Client.ImmortalSnail.UI;

[UsedImplicitly]
public sealed class AcceptImmortalSnailEui : BaseEui
{
    private readonly AcceptImmortalSnailWindow _window;

    public AcceptImmortalSnailEui()
    {
        _window = new AcceptImmortalSnailWindow();

        _window.DenyButton.OnPressed += _ =>
        {
            SendMessage(new AcceptImmortalSnailChoiceMessage(AcceptImmortalSnailUiButton.Deny));
        };

        _window.AcceptButton.OnPressed += _ =>
        {
            SendMessage(new AcceptImmortalSnailChoiceMessage(AcceptImmortalSnailUiButton.Accept));
        };

        _window.OnClose += () => SendMessage(new CloseEuiMessage());
    }

    public override void Opened()
    {
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not AcceptImmortalSnailEuiState snailState)
            return;

        _window.SetEndTime(snailState.EndTime);
    }
}
