using Content.Shared._DV.AACTablet;
using Content.Shared._DV.QuickPhrase;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.AACTablet.UI;

public sealed class AACBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private AACWindow? _window;

    public AACBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window?.Close();
        _window = this.CreateWindow<AACWindow>();
        _window.PhraseButtonPressed += OnPhraseButtonPressed;
    }

    private void OnPhraseButtonPressed(List<ProtoId<QuickPhrasePrototype>> phraseId)
    {
        SendMessage(new AACTabletSendPhraseMessage(phraseId));
    }
}
