using Content.Shared.VoiceMask;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Goobstation.VoiceModulator;

public sealed class VoiceModulatorBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _protomanager = default!;

    [ViewVariables]
    private VoiceModulatorNameChangeWindow? _window;

    public VoiceModulatorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<VoiceModulatorNameChangeWindow>();
        //_window.ReloadVerbs(_protomanager);
        //_window.AddVerbs();

        _window.OnNameChange += OnNameSelected;
        //_window.OnVerbChange += verb => SendMessage(new VoiceMaskChangeVerbMessage(verb));
    }

    private void OnNameSelected(string name)
    {
        SendMessage(new VoiceMaskChangeNameMessage(name));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not VoiceMaskBuiState cast || _window == null)
        {
            return;
        }

        _window.UpdateState(cast.Name, cast.Verb);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Close();
    }
}
