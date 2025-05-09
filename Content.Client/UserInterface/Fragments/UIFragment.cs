// SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface;

namespace Content.Client.UserInterface.Fragments;

/// <summary>
/// Specific ui fragments need to inherit this class. The subclass is then used in yaml to tell a main ui to use it as a ui fragment.
/// </summary>
/// <example>
/// This is an example from the yaml definition from the notekeeper ui
/// <code>
/// - type: CartridgeUi
///     ui: !type:NotekeeperUi
/// </code>
/// </example>
[ImplicitDataDefinitionForInheritors]
public abstract partial class UIFragment
{
    public abstract Control GetUIFragmentRoot();

    public abstract void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner);

    public abstract void UpdateState(BoundUserInterfaceState state);

}