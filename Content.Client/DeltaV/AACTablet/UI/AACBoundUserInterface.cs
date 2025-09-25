using Content.Shared.DeltaV.AACTablet;
using Content.Shared.DeltaV.QuickPhrase;
using Robust.Shared.Prototypes;

namespace Content.Client.DeltaV.AACTablet.UI;

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
        _window = new AACWindow();
        _window.OpenCentered();

        _window.PhraseButtonPressed += OnPhraseButtonPressed;
        _window.OnClose += Close;
    }

    private void OnPhraseButtonPressed(ProtoId<QuickPhrasePrototype> phraseId)
    {
        SendMessage(new AACTabletSendPhraseMessage(phraseId));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _window?.Orphan();
    }
}
