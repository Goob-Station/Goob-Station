using Content.Goobstation.Shared.CustomLawboard;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

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

        _window.LawsChangedEvent += (laws, popup) => OnLawsChanged(laws, popup);

        _window.Entity = Owner;
        var customLawboard = EntMan.EnsureComponent<CustomLawboardComponent>(Owner);
        if (customLawboard.Laws != null)
        {
            _window.LawboardComponent = customLawboard;
            _window.SetLaws(customLawboard.Laws);
        }
        Update();
    }

    private void OnLawsChanged(List<SiliconLaw> value, bool popup)
    {
        SendPredictedMessage(new CustomLawboardChangeLawsMessage(value, popup));
    }
}
