// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CombatMode;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Console;

namespace Content.Client.Weapons.Melee;

public sealed class MeleeSpreadCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MeleeWeaponSystem _meleeSystem = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override string Command => "showmeleespread";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (_overlay.RemoveOverlay<MeleeArcOverlay>())
            return;

        _overlay.AddOverlay(new MeleeArcOverlay(
            EntityManager,
            _eyeManager,
            _inputManager,
            _playerManager,
            _meleeSystem,
            _combatSystem,
            _transformSystem));
    }
}