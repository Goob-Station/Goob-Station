// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Autodoc;
using Robust.Client.Player;

namespace Content.Client._Shitmed.Autodoc;

public sealed class AutodocBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    [ViewVariables]
    private AutodocWindow? _window;

    public AutodocBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _window = new AutodocWindow(owner, _entMan, _player);

        _window.OnCreateProgram += title => SendMessage(new AutodocCreateProgramMessage(title));
        _window.OnToggleProgramSafety += program => SendMessage(new AutodocToggleProgramSafetyMessage(program));
        _window.OnRemoveProgram += program => SendMessage(new AutodocRemoveProgramMessage(program));

        _window.OnAddStep += (program, step, index) => SendMessage(new AutodocAddStepMessage(program, step, index));
        _window.OnRemoveStep += (program, stepIndex) => SendMessage(new AutodocRemoveStepMessage(program, stepIndex));

        _window.OnImportProgram += (program) => SendMessage(new AutodocImportProgramMessage(program));

        _window.OnStart += program => SendMessage(new AutodocStartMessage(program));
        _window.OnStop += () => SendMessage(new AutodocStopMessage());

        _window.OnClose += () => Close();

        _window.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _window?.Dispose();
    }
}