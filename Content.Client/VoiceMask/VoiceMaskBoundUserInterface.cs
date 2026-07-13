// SPDX-License-Identifier: AGPL-3.0-or-later

// Goobstation
using Content.Goobstation.Shared.IntrinsicVoiceModulator.VoiceMask; // Goobstation
using Content.Shared.StatusIcon; // Goobstation
using Content.Shared.VoiceMask;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.VoiceMask;

public sealed class VoiceMaskBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _protomanager = default!;

    [ViewVariables]
    private VoiceMaskNameChangeWindow? _window;

    public VoiceMaskBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<VoiceMaskNameChangeWindow>();
        _window.ReloadVerbs(_protomanager);
        _window.AddVerbs();

        // GabyStation start Radio icons
        _window.ReloadJobIcons();
        _window.AddJobIcons();
        // GabyStation end Radio icons

        _window.OnNameChange += OnNameSelected;
        _window.OnVerbChange += verb => SendMessage(new VoiceMaskChangeVerbMessage(verb));
        _window.OnToggle += OnToggle;
        _window.OnAccentToggle += OnAccentToggle;
        _window.OnJobIconChanged += OnJobIconChanged; // GabyStation -> Radio icons
    }

    private void OnNameSelected(string name)
    {
        SendMessage(new VoiceMaskChangeNameMessage(name));
    }

    private void OnToggle()
    {
        SendMessage(new VoiceMaskToggleMessage());
    }

    private void OnAccentToggle()
    {
        SendMessage(new VoiceMaskAccentToggleMessage());
    }

    // GabyStation Radio icons start
    public void OnJobIconChanged(ProtoId<JobIconPrototype> newJobIconId)
    {
        SendMessage(new VoiceMaskChangeJobIconMessage(newJobIconId));
    }
    // GabyStation Radio icons end

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not VoiceMaskBuiState cast || _window == null)
        {
            return;
        }

        _window.UpdateState(cast.Name, cast.Verb, cast.Active, cast.AccentHide);

        _window.SetCurrentJobIcon(cast.JobIcon); // GabyStation -> Radio icons
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Close();
    }
}
