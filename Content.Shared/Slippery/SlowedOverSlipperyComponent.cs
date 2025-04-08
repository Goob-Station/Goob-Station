// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Slippery;

/// <summary>
/// Slows down the user when passing over an entity with <see cref="SlipperyComponent"/>. Does not prevent slipping, see <see cref="NoSlipComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SlipperySystem))]
public sealed partial class SlowedOverSlipperyComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public float SlowdownModifier = 1f;
}