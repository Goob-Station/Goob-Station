// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Antags.Abductor;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;
using static Content.Shared.Pinpointer.SharedNavMapSystem;
using static Robust.Client.UserInterface.Control;

namespace Content.Client._Shitmed.Antags.Abductor;

[UsedImplicitly]
public sealed class AbductorConsoleBui : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entities = default!;

    [ViewVariables]
    private AbductorConsoleWindow? _window;

    [ViewVariables]
    private bool _armorDisabled = false;

    [ViewVariables]
    private bool _armorLocked = false;

    [ViewVariables]
    private AbductorArmorModeType _armorMode = AbductorArmorModeType.Stealth;

    public AbductorConsoleBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }
    protected override void Open() => UpdateState(State);
    protected override void UpdateState(BoundUserInterfaceState? state)
    {
        if (state is AbductorConsoleBuiState s)
            Update(s);
    }

    private void Update(AbductorConsoleBuiState state)
    {
        TryInitWindow();

        View(ViewType.Teleport);

        RefreshUI();

        if (!_window!.IsOpen)
            _window.OpenCentered();
    }

    private void TryInitWindow()
    {
        if (_window != null) return;
        _window = new AbductorConsoleWindow();
        _window.OnClose += Close;
        _window.Title = "console";

        _window.TeleportTabButton.OnPressed += _ => View(ViewType.Teleport);

        _window.ExperimentTabButton.OnPressed += _ => View(ViewType.Experiment);

        _window.ArmorControlTabButton.OnPressed += _ => View(ViewType.ArmorControl);

        _window.CombatModeButton.OnPressed += _ =>
        {
            _window.StealthModeButton.Disabled = false;
            _window.CombatModeButton.Disabled = true;
            SendMessage(new AbductorVestModeChangeBuiMsg()
            {
                Mode = AbductorArmorModeType.Combat,
            });
        };

        _window.StealthModeButton.OnPressed += _ =>
        {
            _window.StealthModeButton.Disabled = true;
            _window.CombatModeButton.Disabled = false;
            SendMessage(new AbductorVestModeChangeBuiMsg()
            {
                Mode = AbductorArmorModeType.Stealth,
            });
        };

        if (_armorMode == AbductorArmorModeType.Combat)
        {
            _window.CombatModeButton.Disabled = true;
            _window.StealthModeButton.Disabled = false;
        }
        else
        {
            _window.CombatModeButton.Disabled = false;
            _window.StealthModeButton.Disabled = true;
        }

        _window.LockArmorButton.OnPressed += _ =>
        {
            SendMessage(new AbductorLockBuiMsg());

            _armorLocked = !_armorLocked;

            if (!_armorLocked)
                _window.LockArmorButton.Text = Loc.GetString("abductors-ui-lock-armor");
            else
                _window.LockArmorButton.Text = Loc.GetString("abductors-ui-unlock-armor");
        };
    }

    private void RefreshUI()
    {
        if (_window == null || State is not AbductorConsoleBuiState state)
            return;

        // teleportTab
        _window.TargetLabel.Children.Clear();

        var padMsg = new FormattedMessage();
        padMsg.AddMarkupOrThrow(state.AlienPadFound ? Loc.GetString("abductor-ui-pad-found") : Loc.GetString("abductor-ui-pad-not-found"));
        _window.PadLabel.SetMessage(padMsg);

        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(state.Target == null ? Loc.GetString("abductor-ui-target-none") : Loc.GetString("abductor-ui-target-found", ("target", state.TargetName ?? "")));
        _window.TeleportButton.Disabled = state.Target == null || !state.AlienPadFound;
        _window.TeleportButton.OnPressed += _ =>
        {
            SendMessage(new AbductorAttractBuiMsg());
            Close();
        };
        _window.TargetLabel.SetMessage(msg, new Type[1] { typeof(ColorTag) });

        // experiment tab

        var experimentatorMsg = new FormattedMessage();
        experimentatorMsg.AddMarkupOrThrow(state.AlienPadFound ? Loc.GetString("abductor-ui-experimentator-connected") : Loc.GetString("abductor-ui-experimentator-not-found"));
        _window.ExperimentatorLabel.SetMessage(experimentatorMsg);

        var victimMsg = new FormattedMessage();
        victimMsg.AddMarkupOrThrow(state.VictimName == null ? Loc.GetString("abductor-ui-victim-none") : Loc.GetString("abductor-ui-victim-found", ("victim", state.VictimName ?? "")));
        _window.VictimLabel.SetMessage(victimMsg);

        _window.CompleteExperimentButton.Disabled = state.VictimName == null;
        _window.CompleteExperimentButton.OnPressed += _ =>
        {
            SendMessage(new AbductorCompleteExperimentBuiMsg());
            Close();
        };

        // armor tab
        _armorLocked = state.ArmorLocked;

        if (!_armorLocked)
            _window.LockArmorButton.Text = Loc.GetString("abductors-ui-lock-armor");
        else
            _window.LockArmorButton.Text = Loc.GetString("abductors-ui-unlock-armor");

        _armorDisabled = state.ArmorFound;
        _armorMode = state.CurrentArmorMode;

        if (_armorMode == AbductorArmorModeType.Combat)
        {
            _window.CombatModeButton.Disabled = true;
            _window.StealthModeButton.Disabled = false;
        }
        else
        {
            _window.CombatModeButton.Disabled = false;
            _window.StealthModeButton.Disabled = true;
        }
        UpdateDisabledPanel(_armorDisabled);
    }

    private void UpdateDisabledPanel(bool disable)
    {
        if (_window == null)
            return;

        if (disable || !_window.ArmorControlTab.Visible)
        {
            _window.DisabledPanel.Visible = false;
            _window.DisabledPanel.MouseFilter = MouseFilterMode.Ignore;
            return;
        }

        _window.DisabledPanel.Visible = true;
        if (_window.DisabledLabel.GetMessage() is null)
        {
            var text = new FormattedMessage();
            text.AddMarkupOrThrow(Loc.GetString("abductor-ui-armor-plug-in"));
            _window.DisabledLabel.SetMessage(text);
        }

        _window.DisabledPanel.MouseFilter = MouseFilterMode.Stop;
    }

    private void View(ViewType type)
    {
        if (_window == null)
            return;

        _window.TeleportTabButton.Parent!.Margin = new Thickness(0, 0, 0, 10);

        _window.TeleportTabButton.Disabled = type == ViewType.Teleport;
        _window.ArmorControlTabButton.Disabled = type == ViewType.ArmorControl;
        _window.ExperimentTabButton.Disabled = type == ViewType.Experiment;
        _window.TeleportTab.Visible = type == ViewType.Teleport;
        _window.ExperimentTab.Visible = type == ViewType.Experiment;
        _window.ArmorControlTab.Visible = type == ViewType.ArmorControl;

        UpdateDisabledPanel(_armorDisabled);
    }

    private enum ViewType
    {
        Teleport,
        Experiment,
        ArmorControl
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _window?.Dispose();
    }
}