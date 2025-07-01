// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Hannah Giovanna Dawson <karakkaraz@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Research.Components;
using Content.Shared.Xenoarchaeology.Equipment.Components;
using Robust.Client.UserInterface;
using JetBrains.Annotations;

namespace Content.Client.Xenoarchaeology.Ui;

/// <summary>
/// BUI for artifact analysis console, proxies server-provided UI updates
/// (related to device, connected artifact analyzer, and artifact lying on it).
/// </summary>
[UsedImplicitly]
public sealed class AnalysisConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private AnalysisConsoleMenu? _consoleMenu;

    /// <inheritdoc />
    protected override void Open()
    {
        base.Open();

        _consoleMenu = this.CreateWindow<AnalysisConsoleMenu>();
        _consoleMenu.SetOwner(Owner);

        _consoleMenu.OnClose += Close;
        _consoleMenu.OpenCentered();

        _consoleMenu.OnServerSelectionButtonPressed += () =>
        {
            SendMessage(new ConsoleServerSelectionMessage());
        };
        _consoleMenu.OnExtractButtonPressed += () =>
        {
            SendMessage(new AnalysisConsoleExtractButtonPressedMessage());
        };
    }

    /// <summary>
    /// Update UI state based on corresponding component.
    /// </summary>
    public void Update(Entity<AnalysisConsoleComponent> ent)
    {
        _consoleMenu?.Update(ent);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _consoleMenu?.Dispose();
    }
}
