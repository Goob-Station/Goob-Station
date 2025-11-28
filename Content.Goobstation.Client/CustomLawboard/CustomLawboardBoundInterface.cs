using Content.Goobstation.Shared.CustomLawboard;
using Content.Shared.Silicons.Laws;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.CustomLawboard;

/// <summary>
/// Initializes a <see cref="LawboardSiliconLawUi"/> and updates it when new server messages are received.
/// </summary>
[UsedImplicitly]
public sealed class CustomLawboardBoundInterface : BoundUserInterface
{
    [ViewVariables]
    private LawboardSiliconLawUi? _window;

    public CustomLawboardBoundInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<LawboardSiliconLawUi>();

        _window.LawsChangedEvent += args => OnLawsChanged(args);
        _window.Entity = Owner;
        Update();
    }

    private void OnLawsChanged(List<SiliconLaw> value)
    {
        SendPredictedMessage(new CustomLawboardChangeLawsMessage(value));
    }
}
