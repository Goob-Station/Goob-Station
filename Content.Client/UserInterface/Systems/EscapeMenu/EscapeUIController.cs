// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Guidebook;
using Content.Client.UserInterface.Systems.Info;
using Content.Shared.CCVar;
using JetBrains.Annotations;
using Robust.Client.Console;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BaseButton;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;  // RMC - Patreon
using Content.Client._RMC14.LinkAccount; // RMC - Patreon
// Goobstation - Character customization in escape menu
using Content.Client.Lobby;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;
using Robust.Client.UserInterface.CustomControls;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Preferences;
using Content.Client.Guidebook;
using Content.Client.Lobby.UI;
using Content.Client.Players.PlayTimeTracking;

namespace Content.Client.UserInterface.Systems.EscapeMenu;

[UsedImplicitly]
public sealed class EscapeUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [Dependency] private readonly IClientConsoleHost _console = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ChangelogUIController _changelog = default!;
    [Dependency] private readonly InfoUIController _info = default!;
    [Dependency] private readonly OptionsUIController _options = default!;
    [Dependency] private readonly GuidebookUIController _guidebook = default!;
    [Dependency] private readonly LinkAccountManager _linkAccount = default!; // RMC - Patreon
    // Goobstation - Character customization in escape menu
    [Dependency] private readonly IClientPreferencesManager _preferencesManager = default!;
    [Dependency] private readonly IFileDialogManager _dialogManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IUriOpener _uri = default!;
    [Dependency] private readonly JobRequirementsManager _requirements = default!;
    [Dependency] private readonly MarkingManager _markings = default!;
    [UISystemDependency] private readonly GuidebookSystem? _guide = default!;

    private Options.UI.EscapeMenu? _escapeWindow;
    // Goobstation - Character customization in escape menu
    private DefaultWindow? _characterWindow;
    private CharacterSetupGui? _characterSetup;
    private HumanoidProfileEditor? _profileEditor;

    private MenuButton? EscapeButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.EscapeButton; // RMC - Patreon

    public override void Initialize()  // RMC - Patreon
    {
        _linkAccount.Updated += () =>
        {
            if (_escapeWindow != null)
                _escapeWindow.PatronPerksButton.Visible = _linkAccount.CanViewPatronPerks();
        };
    }

    public void UnloadButton()
    {
        if (EscapeButton == null)
        {
            return;
        }

        EscapeButton.Pressed = false;
        EscapeButton.OnPressed -= EscapeButtonOnOnPressed;
    }

    public void LoadButton()
    {
        if (EscapeButton == null)
        {
            return;
        }

        EscapeButton.OnPressed += EscapeButtonOnOnPressed;
    }

    private void ActivateButton() => EscapeButton!.SetClickPressed(true);
    private void DeactivateButton() => EscapeButton!.SetClickPressed(false);

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_escapeWindow == null);

        _escapeWindow = UIManager.CreateWindow<Options.UI.EscapeMenu>();

        _escapeWindow.OnClose += DeactivateButton;
        _escapeWindow.OnOpen += ActivateButton;

        _escapeWindow.ChangelogButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _changelog.ToggleWindow();
        };

        _escapeWindow.PatronPerksButton.Visible = _linkAccount.CanViewPatronPerks(); // RMC - Patreon
        _escapeWindow.PatronPerksButton.OnPressed += _ => // RMC - Patreon
        {
            CloseEscapeWindow();
            UIManager.GetUIController<LinkAccountUIController>().TogglePatronPerksWindow();
        };

        _escapeWindow.RulesButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _info.OpenWindow();
        };

        _escapeWindow.DisconnectButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _console.ExecuteCommand("disconnect");
        };

        _escapeWindow.OptionsButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _options.OpenWindow();
        };

        _escapeWindow.CharacterButton.OnPressed += _ => // Goobstation - Character customization in escape menu
        {
            CloseEscapeWindow();
            OpenCharacterSetup();
        };

        _escapeWindow.QuitButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _console.ExecuteCommand("quit");
        };

        _escapeWindow.WikiButton.OnPressed += _ =>
        {
            _uri.OpenUri(_cfg.GetCVar(CCVars.InfoLinksWiki));
        };

        _escapeWindow.GuidebookButton.OnPressed += _ =>
        {
            _guidebook.ToggleGuidebook();
        };

        // Hide wiki button if we don't have a link for it.
        _escapeWindow.WikiButton.Visible = _cfg.GetCVar(CCVars.InfoLinksWiki) != "";

        CommandBinds.Builder
            .Bind(EngineKeyFunctions.EscapeMenu,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<EscapeUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        if (_escapeWindow != null)
        {
            _escapeWindow.Dispose();
            _escapeWindow = null;
        }

        CloseCharacterSetup(); // Goobstation - Character customization in escape menu

        CommandBinds.Unregister<EscapeUIController>();
    }

    private void EscapeButtonOnOnPressed(ButtonEventArgs obj)
    {
        ToggleWindow();
    }

    private void CloseEscapeWindow()
    {
        _escapeWindow?.Close();
    }

    /// <summary>
    /// Toggles the game menu.
    /// </summary>
    public void ToggleWindow()
    {
        if (_escapeWindow == null)
            return;

        if (_escapeWindow.IsOpen)
        {
            CloseEscapeWindow();
            EscapeButton!.Pressed = false;
        }
        else
        {
            _escapeWindow.OpenCentered();
            EscapeButton!.Pressed = true;
        }
    }

    // Goobstation - Character customization in escape menu

    private void OpenCharacterSetup()
    {
        if (_characterWindow is { IsOpen: true })
        {
            _characterWindow.MoveToFront();
            return;
        }

        _profileEditor = new HumanoidProfileEditor(
            _preferencesManager,
            _cfg,
            EntityManager,
            _dialogManager,
            LogManager,
            _playerManager,
            _prototypeManager,
            _resourceCache,
            _requirements,
            _markings);

        if (_guide != null)
            _profileEditor.OnOpenGuidebook += _guide.OpenHelp;

        _profileEditor.Save += SaveCharacterProfile;

        _characterSetup = new CharacterSetupGui(_profileEditor);

        _characterSetup.CloseButton.OnPressed += _ =>
        {
            if (_profileEditor.Profile != null && _profileEditor.IsDirty)
            {
                OpenCharacterSavePanel();
                return;
            }

            CloseCharacterSetup();
        };

        _characterSetup.SelectCharacter += slot =>
        {
            _preferencesManager.SelectCharacter(slot);
            ReloadCharacterSetup();
        };

        _characterSetup.DeleteCharacter += slot =>
        {
            _preferencesManager.DeleteCharacter(slot);

            if (_profileEditor.CharacterSlot == slot)
                ReloadCharacterSetup();
            else
                _characterSetup.ReloadCharacterPickers();
        };

        _characterWindow = new DefaultWindow
        {
            Title = Loc.GetString("ui-escape-character"),
            Resizable = true,
            MinWidth = 1050,
            MinHeight = 550,
            SetWidth = 1050,
            SetHeight = 700
        };

        _characterWindow.Contents.AddChild(_characterSetup);

        if (_preferencesManager.ServerDataLoaded)
        {
            _characterSetup.ReloadCharacterPickers();
            _profileEditor.SetProfile(
                (HumanoidCharacterProfile?) _preferencesManager.Preferences?.SelectedCharacter,
                _preferencesManager.Preferences?.SelectedCharacterIndex);
        }

        _characterWindow.OpenCentered();
    }

    private void CloseCharacterSetup()
    {
        _characterWindow?.Dispose();
        _characterWindow = null;
        _characterSetup = null;
        _profileEditor = null;
    }

    private void SaveCharacterProfile()
    {
        if (_profileEditor?.Profile == null || _profileEditor.CharacterSlot == null)
            return;

        _preferencesManager.UpdateCharacter(_profileEditor.Profile, _profileEditor.CharacterSlot.Value);
        ReloadCharacterSetup();
    }

    private void ReloadCharacterSetup()
    {
        _characterSetup?.ReloadCharacterPickers();
        _profileEditor?.SetProfile(
            (HumanoidCharacterProfile?) _preferencesManager.Preferences?.SelectedCharacter,
            _preferencesManager.Preferences?.SelectedCharacterIndex);
    }

    private void OpenCharacterSavePanel()
    {
        var savePanel = new CharacterSetupGuiSavePanel();

        savePanel.SaveButton.OnPressed += _ =>
        {
            SaveCharacterProfile();
            savePanel.Close();
            CloseCharacterSetup();
        };

        savePanel.NoSaveButton.OnPressed += _ =>
        {
            savePanel.Close();
            CloseCharacterSetup();
        };

        savePanel.OpenCentered();
    }
}
