// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Rotatable;

/// <summary>
/// Allows an entity to be rotated by using a verb.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RotatableComponent : Component
{
    /// <summary>
    /// If true, this entity can be rotated even while anchored.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RotateWhileAnchored;

    /// <summary>
    /// If true, will rotate entity in players direction when pulled
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RotateWhilePulling = true;

    /// <summary>
    /// The angular value to change when using the rotate verbs.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Angle Increment = Angle.FromDegrees(90);
}
